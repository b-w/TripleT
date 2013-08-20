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

namespace TripleT.Datastructures
{
    using System;
    using BerkeleyDB;
    using TripleT.IO;
    using TripleT.Util;

    /// <summary>
    /// Represents the TripleT index used by the database engine.
    /// </summary>
    public class Index : IDisposable
    {
        private readonly HashDatabase m_db;
        private readonly Bucket m_sBucket;
        private readonly Bucket m_pBucket;
        private readonly Bucket m_oBucket;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="databaseName">Name of the database this index belongs to.</param>
        /// <param name="pBucket">The p bucket belonging to this database.</param>
        /// <param name="oBucket">The o bucket belonging to this database.</param>
        /// <param name="sBucket">The s bucket belonging to this database.</param>
        public Index(string databaseName, Bucket sBucket, Bucket pBucket, Bucket oBucket)
        {
            var nameDb = String.Format("{0}.index.dat", databaseName);

            m_sBucket = sBucket;
            m_pBucket = pBucket;
            m_oBucket = oBucket;

            //
            // the index is stored in a BerkeleyDB hash database. configuration for this database
            // is hardcoded here.

            var config = new HashDatabaseConfig();
            config.Duplicates = DuplicatesPolicy.NONE;
            config.CacheSize = new CacheInfo(0, 256 * 1024 * 1024, 4);
            config.PageSize = 512;
            config.Creation = CreatePolicy.IF_NEEDED;

            //
            // open the index database

            m_db = HashDatabase.Open(nameDb, config);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //
            // close the index database

            m_db.Close(true);
        }

        /// <summary>
        /// Gets the s bucket indexed by this index.
        /// </summary>
        public Bucket SBucket
        {
            get { return m_sBucket; }
        }

        /// <summary>
        /// Gets the p bucket indexed by this index.
        /// </summary>
        public Bucket PBucket
        {
            get { return m_pBucket; }
        }

        /// <summary>
        /// Gets the o bucket indexed by this index.
        /// </summary>
        public Bucket OBucket
        {
            get { return m_oBucket; }
        }

        /// <summary>
        /// Builds the index over the buckets that belong to this index.
        /// </summary>
        internal void Build()
        {
            var currentAtom = Int64.MaxValue;
            var start = new long[3];
            var count = new long[3];

            var cursors = new TripleCursor[3];
            cursors[0] = m_sBucket.OpenRead();
            cursors[1] = m_pBucket.OpenRead();
            cursors[2] = m_oBucket.OpenRead();

            //
            // the index building algorithm is fairly straightforward: all three buckets are opened
            // simultaneously, and we go through them such that we are always dealing with the same
            // atom X for all three buckets. at any time we are at (or before) X in the S-bucket
            // where X is in the S-position, at X in the P-bucket where it is in the P-position,
            // and at X in the O-bucket where it is in the O-position. when we are done with X we
            // move on to the next atom and never see a relevant X value again. in this way, we
            // only need one pass over all three buckets to build the index.

            long peek;
            while (cursors[0].HasNext || cursors[1].HasNext || cursors[2].HasNext) {

                //
                // find the next smallest atom value to handle

                if (cursors[0].HasNext) {
                    peek = cursors[0].Peek().S.InternalValue;
                    if (peek < currentAtom) {
                        currentAtom = peek;
                    }
                }
                if (cursors[1].HasNext) {
                    peek = cursors[1].Peek().P.InternalValue;
                    if (peek < currentAtom) {
                        currentAtom = peek;
                    }
                }
                if (cursors[2].HasNext) {
                    peek = cursors[2].Peek().O.InternalValue;
                    if (peek < currentAtom) {
                        currentAtom = peek;
                    }
                }

                //
                // see how much atoms with the selected value there are with that value in the S
                // position

                start[0] = -1;
                count[0] = -1;
                if (cursors[0].HasNext && cursors[0].Peek().S.InternalValue == currentAtom) {
                    start[0] = cursors[0].Position;
                    count[0] = 0;
                    do {
                        count[0]++;
                        cursors[0].Next();
                    } while (cursors[0].HasNext && cursors[0].Peek().S.InternalValue == currentAtom);
                }

                //
                // see how much atoms with the selected value there are with that value in the P
                // position

                start[1] = -1;
                count[1] = -1;
                if (cursors[1].HasNext && cursors[1].Peek().P.InternalValue == currentAtom) {
                    start[1] = cursors[1].Position;
                    count[1] = 0;
                    do {
                        count[1]++;
                        cursors[1].Next();
                    } while (cursors[1].HasNext && cursors[1].Peek().P.InternalValue == currentAtom);
                }

                //
                // see how much atoms with the selected value there are with that value in the O
                // position

                start[2] = -1;
                count[2] = -1;
                if (cursors[2].HasNext && cursors[2].Peek().O.InternalValue == currentAtom) {
                    start[2] = cursors[2].Position;
                    count[2] = 0;
                    do {
                        count[2]++;
                        cursors[2].Next();
                    } while (cursors[2].HasNext && cursors[2].Peek().O.InternalValue == currentAtom);
                }

                //
                // create and insert the index payload for the atom with the current value into the
                // index database

                var payload = new byte[8 * 6];
                var i = 0;
                for (int j = 0; j < start.Length; j++) {
                    Encoding.DbEncode(start[j]).CopyTo(payload, i);
                    i += 8;
                    Encoding.DbEncode(count[j]).CopyTo(payload, i);
                    i += 8;
                }

                var key = new DatabaseEntry(Encoding.DbEncode(currentAtom));
                var value = new DatabaseEntry(payload);
                m_db.Put(key, value);

                currentAtom = Int64.MaxValue;
            }
        }

        /// <summary>
        /// Gets the index payload for a given atom.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <returns>
        /// The index payload for the given atom.
        /// </returns>
        public IndexPayload GetPayload(Atom atom)
        {
            return GetPayload(atom.InternalValue);
        }

        /// <summary>
        /// Gets the index payload for a given atom.
        /// </summary>
        /// <param name="atom">The internal representation belonging to the atom.</param>
        /// <returns>
        /// The index payload for the given atom.
        /// </returns>
        public IndexPayload GetPayload(long atom)
        {
            var key = new DatabaseEntry(Util.Encoding.DbEncode(atom));
            if (m_db.Exists(key)) {
                var kvPair = m_db.Get(key);
                return new IndexPayload(kvPair.Value.Data);
            } else {
                throw new ArgumentOutOfRangeException("atom", "No payload for given atom!");
            }
        }

        /// <summary>
        /// Gets the BerkeleyDB database cursor for the index database.
        /// </summary>
        /// <returns>
        /// A cursor for the index database.
        /// </returns>
        internal HashCursor GetDbCursor()
        {
            return m_db.Cursor();
        }
    }
}
