/* TripleT: an RDF database engine.
 * Copyright (C) 2012-2013 Eindhoven University of Technology <http://www.tue.nl/>
 * Copyright (C) 2012-2013 Bart Wolff <http://www.bartwolff.com/>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 **/

namespace TripleT.IO.Operators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using BerkeleyDB;
    using TripleT.Algorithms;
    using TripleT.Datastructures;
    using TripleT.Util;
    using qp = TripleT.Datastructures.Queries;

    /// <summary>
    /// Represents a physical operator for performing a naturual join operation via hashing on two
    /// sets of bindings.
    /// </summary>
    public class HashJoin : Operator
    {
        private readonly Operator m_left;
        private readonly Operator m_right;
        private string m_tmpDbLeftFile;
        private string m_tmpDbRightFile;
        private BTreeDatabase m_tmpDbLeft;
        private BTreeDatabase m_tmpDbRight;
        private BTreeCursor m_cursorLeft;
        private BTreeCursor m_cursorRight;
        private bool m_cursorLeftMovePrev;
        private bool m_cursorRightMovePrev;
        private BindingSet m_leftNext;
        private BindingSet m_rightNext;
        private readonly List<Variable> m_joinVariables;
        private bool m_tmpDbsCreated;
        private byte[] m_currentChunk;
        private readonly ByteArrayComparer m_byteComparer;
        private readonly Dictionary<byte[], List<BindingSet>> m_chunkTable;
        private readonly Queue<BindingSet> m_currentChunkList;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashJoin"/> class.
        /// </summary>
        /// <param name="planOperator">The descriptive query plan operator that is the counterpart of this physical operator.</param>
        /// <param name="left">This operator's left input operator.</param>
        /// <param name="right">This operator's right input operator.</param>
        public HashJoin(qp::Operator planOperator, Operator left, Operator right)
        {
            m_planOperator = planOperator;
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            m_left = left;
            m_right = right;
            m_tmpDbsCreated = false;
            m_joinVariables = new List<Variable>();
            m_byteComparer = new ByteArrayComparer(1);
            m_chunkTable = new Dictionary<byte[], List<BindingSet>>(new ByteArrayEqualityComparer());
            m_currentChunkList = new Queue<BindingSet>();
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            //
            // prepare the next set of output bindings

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_left.Dispose();
            m_right.Dispose();

            if (m_tmpDbsCreated) {
                m_tmpDbLeft.Close(true);
                m_tmpDbRight.Close(true);
                File.Delete(m_tmpDbLeftFile);
                File.Delete(m_tmpDbRightFile);
            }
        }

        /// <summary>
        /// Attempts to read from this operator's underlying input stream(s) until the next set of
        /// bindings is found which can be outputted by this operator.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if a next binding set is found available to output; otherwise, <c>false</c>.
        /// </returns>
        internal override bool TryReadNext()
        {
            if (!m_tmpDbsCreated) {
                if (!m_left.HasNext || !m_right.HasNext) {
                    return false;
                }

                CreateTmpDbs();
            }

            if (m_currentChunkList.Count > 0) {
                //
                // if there are still matches available for the current chunk, then simply fetch
                // the next one of those and return it (it's always a join match)
#if DEBUG
                m_planOperator.StartCPUWork();
#endif
                var binding = m_currentChunkList.Dequeue();
                m_next = Merge(binding, m_rightNext);
                m_hasNext = true;
#if DEBUG
                m_planOperator.StopCPUWork();
                m_planOperator.AddResult();
#endif
                return true;
            }

            if (m_chunkTable.Count == 0) {
                //
                // if the current chunk table is empty, try to advance to the next chunk. if we
                // cannot advance then no more matches are possible.

                if (!AdvanceChunk()) {
                    m_hasNext = false;
                    return false;
                }
            }
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            while (AdvanceRight()) {
                //
                // bindings from the left operator are in the chunk table; those from the right
                // operator are then matched against those one by one. as long as we can advance
                // the right input stream, more matches are potentially possible.

                var h = m_cursorRight.Current.Key.Data;
                var c = m_byteComparer.Compare(h, m_currentChunk);

                if (c == 0) {
                    //
                    // if the two hashes are equal it is possible that matches exist in the chunk
                    // table

                    var k = GetChunkTableKey(m_rightNext);

                    if (m_chunkTable.ContainsKey(k)) {
                        //
                        // matches exist in the chunk table. enqueue them in the current chunk
                        // list.

                        foreach (var b in m_chunkTable[k]) {
                            m_currentChunkList.Enqueue(b);
                        }

                        //
                        // same code here for fetching a match from the chunk queue and merging
                        // and returning it

                        var binding = m_currentChunkList.Dequeue();
                        m_next = Merge(binding, m_rightNext);
                        m_hasNext = true;
#if DEBUG
                        m_planOperator.StopCPUWork();
                        m_planOperator.AddResult();
#endif
                        return true;
                    }
                } else if (c > 0) {
                    //
                    // if the right hash succeeds the one from the current chunk, then it is time
                    // to advance to the next chunk. we'll also need to tell the right input stream
                    // cursor to not advance (this is kinda hacky as BerkeleyDB doesn't properly
                    // support moving backwards with a cursor).

                    m_cursorRightMovePrev = true;
                    if (!AdvanceChunk()) {
                        m_hasNext = false;
#if DEBUG
                        m_planOperator.StopCPUWork();
#endif
                        return false;
                    }
                }
            }

            //
            // if we arrive here then no more matches are possible

#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            m_hasNext = false;
            return false;
        }

        /// <summary>
        /// Merges the two specified binding sets together, returning a new binding set which
        /// contains bindings from the two given sets.
        /// </summary>
        /// <param name="left">The left binding set.</param>
        /// <param name="right">The right binding set.</param>
        /// <returns>
        /// A new binding set merging the two given sets.
        /// </returns>
        private BindingSet Merge(BindingSet left, BindingSet right)
        {
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            var b = new BindingSet();

            //
            // binding sets internally disregard (v : a) if they already have some (v : a')

            foreach (var item in left.Bindings) {
                b.Add(item);
            }
            foreach (var item in right.Bindings) {
                b.Add(item);
            }
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            return b;
        }

        /// <summary>
        /// Creates the temporaray databases for storing the input sets, and then reads the input
        /// sets in full and stores them in the databases.
        /// </summary>
        private void CreateTmpDbs()
        {
            //
            // the temporary databases are BerkeleyDB B+ tree databases. configuration for the
            // databases is hardcoded here.

            var config = new BTreeDatabaseConfig();
            config.Duplicates = DuplicatesPolicy.UNSORTED;
            config.CacheSize = new CacheInfo(0, m_planOperator.MemorySize / 2, 4);
            config.PageSize = 512;
            config.Creation = CreatePolicy.ALWAYS;

            m_tmpDbLeftFile = String.Format("~{0}.tmp", Generator.GetRandomFilename(12));
            m_tmpDbRightFile = String.Format("~{0}.tmp", Generator.GetRandomFilename(12));
#if DEBUG
            m_planOperator.StartIOWork();
#endif
            m_tmpDbLeft = BTreeDatabase.Open(m_tmpDbLeftFile, config);
            m_tmpDbRight = BTreeDatabase.Open(m_tmpDbRightFile, config);
#if DEBUG
            m_planOperator.StopIOWork();
#endif

            //
            // peek-a-binding from the left and right input streams, to obtain the shared join
            // variables between them

            var peekLeft = m_left.Peek();
            var peekRight = m_right.Peek();

            foreach (var b1 in peekLeft.Bindings) {
                var b2 = peekRight[b1.Variable];
                if (b2 != null) {
                    m_joinVariables.Add(b1.Variable);
                }
            }

            var barrier = new Barrier(2);

            //
            // do this multi-threaded. because why not?

            Action<Operator, BTreeDatabase> dbBuildAction = (op, db) => {
                    while (op.HasNext) {
                        var next = op.Next();
#if DEBUG
                        lock (m_planOperator) {
                            m_planOperator.StartCPUWork();
                        }
#endif
                        var bKey = new List<Binding>();
                        foreach (var item in m_joinVariables) {
                            bKey.Add(next[item]);
                        }
#if DEBUG
                        lock (m_planOperator) {
                            m_planOperator.StopCPUWork();
                            m_planOperator.StartIOWork();
                        }
#endif
                        var key = new DatabaseEntry(Encoding.DbEncode(Hash(bKey.ToArray())));
                        var data = new DatabaseEntry(Encoding.DbEncode(next));
                        db.Put(key, data);
#if DEBUG
                        lock (m_planOperator) {
                            m_planOperator.StopIOWork();
                        }
#endif
                    }

                    barrier.SignalAndWait();
                };

            //
            // one goes in a seperate thread...

            var t = new Thread(() => dbBuildAction(m_left, m_tmpDbLeft));
            t.Start();

            //
            // ...the other one just in the main thread.

            dbBuildAction(m_right, m_tmpDbRight);

            //
            // create the cursors for moving through the databases

            m_cursorLeft = m_tmpDbLeft.Cursor();
            m_cursorLeftMovePrev = false;
            m_cursorRight = m_tmpDbRight.Cursor();
            m_cursorRightMovePrev = false;

            m_tmpDbsCreated = true;
        }

        /// <summary>
        /// Attempts to advance to the next chunk by advancing the left input stream cursor, fully
        /// reading in all bindings belonging to the next chunk.
        /// </summary>
        /// <returns>
        /// <c>true</c> if bindings for a next chunk are available; otherwise, <c>false</c>.
        /// </returns>
        private bool AdvanceChunk()
        {
            m_chunkTable.Clear();

            if (AdvanceLeft()) {
                //
                // if we can advance the left input stream, then we have new current chunk to deal
                // with

                m_currentChunk = m_cursorLeft.Current.Key.Data;

                //
                // tell the left input stream cursor not to advance, seeing as we've already read
                // one position in this operation which is also needed for the next step

                m_cursorLeftMovePrev = true;
            } else {
                return false;
            }

            while (AdvanceLeft()) {
                //
                // read in all bindings belonging to the next chunk we're now dealing with, and put
                // them in the chunk table

                var h = m_cursorLeft.Current.Key.Data;
                if (m_byteComparer.Compare(h, m_currentChunk) != 0) {
                    //
                    // here we are one over the chunk border (to the next chunk), so break out of
                    // the loop and tell the left input stream cursor not to advance (seeing as
                    // we've just read one item too much)

                    m_cursorLeftMovePrev = true;
                    break;
                } else {
                    //
                    // here we are still in the same chunk, so add the current binding set to the
                    // chunk table

                    var b = Encoding.DbDecodeBindingSet(m_cursorLeft.Current.Value.Data);
                    var k = GetChunkTableKey(b);

                    if (!m_chunkTable.ContainsKey(k)) {
                        m_chunkTable.Add(k, new List<BindingSet>());
                    }

                    m_chunkTable[k].Add(b);
                }
            }

            return true;
        }

        /// <summary>
        /// Advances the left input stream cursor, storing the result in a local variable for
        /// consumption.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the cursor was successfully advanced; otherwise, <c>false</c>.
        /// </returns>
        private bool AdvanceLeft()
        {
            if (m_cursorLeftMovePrev) {
                //
                // BerkeleyDB's support for Cursor.MovePrev() is broken and doesn't work correctly
                // on edge cases, so we have this hacky workaround

                m_cursorLeftMovePrev = false;
                return true;
            }
#if DEBUG
            m_planOperator.StartIOWork();
#endif
            //
            // try to advance the cursor, and store the result in a local variable. if the cursor
            // cannot be advanced then we are done here

            if (m_cursorLeft.MoveNext()) {
#if DEBUG
                m_planOperator.StopIOWork();
#endif
                m_leftNext = Encoding.DbDecodeBindingSet(m_cursorLeft.Current.Value.Data);
                return true;
            } else {
#if DEBUG
                m_planOperator.StopIOWork();
#endif
                return false;
            }
        }

        /// <summary>
        /// Advances the right input stream cursor, storing the result in a local variable for
        /// consumption.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the cursor was successfully advanced; otherwise, <c>false</c>.
        /// </returns>
        private bool AdvanceRight()
        {
            if (m_cursorRightMovePrev) {
                //
                // BerkeleyDB's support for Cursor.MovePrev() is broken and doesn't work correctly
                // on edge cases, so we have this hacky workaround

                m_cursorRightMovePrev = false;
                return true;
            }
#if DEBUG
            m_planOperator.StartIOWork();
#endif
            //
            // try to advance the cursor, and store the result in a local variable. if the cursor
            // cannot be advanced then we are done here

            if (m_cursorRight.MoveNext()) {
#if DEBUG
                m_planOperator.StopIOWork();
#endif
                m_rightNext = Encoding.DbDecodeBindingSet(m_cursorRight.Current.Value.Data);
                return true;
            } else {
#if DEBUG
                m_planOperator.StopIOWork();
#endif
                return false;
            }
        }

        /// <summary>
        /// Get a hash value for the given binding array.
        /// </summary>
        /// <param name="value">The binding array.</param>
        /// <returns>
        /// A hash value for the given binding array.
        /// </returns>
        private short Hash(Binding[] value)
        {
            var h = 0;
            for (int i = 0; i < value.Length; i++) {
                h ^= value[i].GetHashCode();
            }
            return (short)h;
        }

        /// <summary>
        /// Gets the chunk table key for the given binding set.
        /// </summary>
        /// <param name="value">The binding set.</param>
        /// <returns>
        /// A chunk table key for the given binding set.
        /// </returns>
        private byte[] GetChunkTableKey(BindingSet value)
        {
            var key = new byte[m_joinVariables.Count * 8];
            var c = 0;
            foreach (var item in m_joinVariables) {
                var b = value[item];
                var enc = Encoding.DbEncode(b.Value.InternalValue);
                Array.Copy(enc, 0, key, c * 8, 8);
                c++;
            }

            return key;
        }
    }
}
