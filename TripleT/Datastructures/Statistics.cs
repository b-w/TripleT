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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using BerkeleyDB;
    using TripleT.Algorithms;
    using TripleT.Datastructures.JoinGraph;
    using TripleT.Datastructures.Queries;
    using TripleT.IO;
    using TripleT.Util;

    /// <summary>
    /// Represents the statistics database belonging to a TripleT database.
    /// </summary>
    public class Statistics : IDisposable
    {
        private readonly HashDatabase m_db;
        private readonly TripleT.Database m_context;
        private const long KEY_COUNT_RECORDS_TOTAL = -1;
        private const long KEY_COUNT_ATOMS_UNIQUE_TOTAL = -2;
        private const long KEY_COUNT_ATOMS_UNIQUE_S = -3;
        private const long KEY_COUNT_ATOMS_UNIQUE_P = -4;
        private const long KEY_COUNT_ATOMS_UNIQUE_O = -5;
        private const long KEY_PREFIX_JOINSTATS = -1;
        private const long KEY_PREFIX_2ATOM_SAPS = -2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Statistics"/> class.
        /// </summary>
        /// <param name="databaseName">Name of the database this statistics database belongs to.</param>
        /// <param name="context">The database context.</param>
        public Statistics(string databaseName, TripleT.Database context)
        {
            m_context = context;

            //
            // the statistics database is stored in a BerkeleyDB hash database. configuration for
            // this database is hardcoded here.

            var nameDb = String.Format("{0}.stats.dat", databaseName);
            var config = new HashDatabaseConfig();
            config.Duplicates = DuplicatesPolicy.NONE;
            config.CacheSize = new CacheInfo(0, 256 * 1024 * 1024, 4);
            config.PageSize = 512;
            config.Creation = CreatePolicy.IF_NEEDED;

            //
            // open the database

            m_db = HashDatabase.Open(nameDb, config);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //
            // close the database

            m_db.Close(true);
        }

        /// <summary>
        /// Builds the statistics database.
        /// </summary>
        internal void Build()
        {
#if DEBUG
            Logger.WriteLine("Start general stats...");
#endif
            var cursor = m_context.Index.GetDbCursor();
            var pAtoms = new List<Tuple<long, Atom>>();
            long tCount = 0, sCount = 0, pCount = 0, oCount = 0;

            //
            // go through the index, computing aggregate statistics and gathering all unique
            // p-atoms in the dataset

            while (cursor.MoveNext()) {
                var payload = new IndexPayload(cursor.Current.Value.Data);
                if (payload.SStart > -1) {
                    sCount++;
                }
                if (payload.PStart > -1) {
                    var pAtom = new Atom(Encoding.DbDecodeInt64(cursor.Current.Key.Data));
                    pAtom.SetTextValue(m_context.Dictionary);
                    pCount++;

                    pAtoms.Add(Tuple.Create(payload.PCount, pAtom));
                }
                if (payload.OStart > -1) {
                    oCount++;
                }
                tCount++;
            }
            cursor.Close();

            //
            // quickly computing the total number of records using a triple cursor

            long rCount = 0;
            var tripleCursor = m_context.Index.SBucket.OpenRead();
            rCount = tripleCursor.Length;
            tripleCursor.Dispose();

            DatabaseEntry key, value;

            //
            // total number of records (i.e. triples)

            key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_RECORDS_TOTAL));
            value = new DatabaseEntry(Encoding.DbEncode(rCount));
            m_db.Put(key, value);

            //
            // total number of unique atoms

            key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_TOTAL));
            value = new DatabaseEntry(Encoding.DbEncode(tCount));
            m_db.Put(key, value);

            // total number of unique s-atoms
            key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_S));
            value = new DatabaseEntry(Encoding.DbEncode(sCount));
            m_db.Put(key, value);

            //
            // total number of unique p-atoms

            key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_P));
            value = new DatabaseEntry(Encoding.DbEncode(pCount));
            m_db.Put(key, value);

            //
            // total number of unique o-atoms

            key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_O));
            value = new DatabaseEntry(Encoding.DbEncode(oCount));
            m_db.Put(key, value);

            //
            // start computation of join statistics on predicates collected earlier
#if DEBUG
            Logger.WriteLine("Start join stats...");
            Console.WriteLine("p-count (pre-filter): {0}", pAtoms.Count);
#endif
            //
            // for the sake of time, we need a threshold. for now set it to > 0.1% of the dataset
            // size.

            if (pAtoms.Count > 100) {
                var threshold = tCount / 1000;
                pAtoms.RemoveAll(t => t.Item1 < threshold);
            }

#if DEBUG
            Console.WriteLine("p-count (post-filter): {0}", pAtoms.Count);
#endif

            //
            // start computation of the join statistics. we do this in the simplest way possible:
            // by just feeding the relevant queries to our database context and counting the number
            // of results.

            for (int i = 0; i < pAtoms.Count; i++) {
#if DEBUG
                Console.Write("[");
#endif
                for (int j = i + 1; j < pAtoms.Count; j++) {
                    long cSS = 0, cSO = 0, cOS = 0, cOO = 0;
                    var p1 = pAtoms[i].Item2;
                    var p2 = pAtoms[j].Item2;

                    var patternSS = new Pattern[]
                    {
                        new Pattern(1, p1.TextValue, 2),
                        new Pattern(1, p2.TextValue, 3)
                    };
                    var patternSO = new Pattern[]
                    {
                        new Pattern(1, p1.TextValue, 2),
                        new Pattern(3, p2.TextValue, 1)
                    };
                    var patternOS = new Pattern[]
                    {
                        new Pattern(1, p1.TextValue, 2),
                        new Pattern(2, p2.TextValue, 3)
                    };
                    var patternOO = new Pattern[]
                    {
                        new Pattern(1, p1.TextValue, 2),
                        new Pattern(3, p2.TextValue, 2)
                    };

                    var planSS = m_context.GetQueryPlan(patternSS);
                    var planSO = m_context.GetQueryPlan(patternSO);
                    var planOS = m_context.GetQueryPlan(patternOS);
                    var planOO = m_context.GetQueryPlan(patternOO);

                    foreach (var item in m_context.Query(planSS, false)) {
                        cSS++;
                    }

                    foreach (var item in m_context.Query(planSO, false)) {
                        cSO++;
                    }

                    foreach (var item in m_context.Query(planOS, false)) {
                        cOS++;
                    }

                    foreach (var item in m_context.Query(planOO, false)) {
                        cOO++;
                    }

                    var keyBytes = Encoding.DbEncode(new long[] { KEY_PREFIX_JOINSTATS, p1.InternalValue, p2.InternalValue });
                    var valueBytes = Encoding.DbEncode(new long[] { cSS, cSO, cOS, cOO });
                    key = new DatabaseEntry(keyBytes);
                    value = new DatabaseEntry(valueBytes);
                    if (!m_db.Exists(key)) {
                        m_db.Put(key, value);
                    }
#if DEBUG
                    Console.Write("-");
#endif
                }
#if DEBUG
                Console.WriteLine("]");
#endif
            }

            //
            // start computing the 2-atom SAP statistics
            //
            // we create three new tmp buckets and sort them in the remaining orders: SPO, POS, OPS.
            // we then walk through the six buckets and count how many of each 2-atom pairs we have,
            // where the 2 atoms we look at are the first two in the sort order.
            // this is better than the alternative, which is executing a 2-atom SAP query for each
            // pair of atoms in the DB (which is a huge, huge amount of queries)

#if DEBUG
            Logger.WriteLine("Start 2-atom stats...");
            Logger.WriteLine("Creating tmp buckets...");
#endif

            var bucketSPOFile = String.Format("~{0}.dat", Generator.GetRandomFilename(12));
            var bucketPOSFile = String.Format("~{0}.dat", Generator.GetRandomFilename(12));
            var bucketOPSFile = String.Format("~{0}.dat", Generator.GetRandomFilename(12));
            File.Copy(m_context.SBucket.FileName, bucketSPOFile);
            File.Copy(m_context.PBucket.FileName, bucketPOSFile);
            File.Copy(m_context.OBucket.FileName, bucketOPSFile);
#if DEBUG
            Logger.WriteLine("Sorting tmp buckets...");
#endif
            GC.Collect();
            ExternalSort.SortFile(bucketSPOFile, 3, TripleT.Database.MAX_TRIPLES_IN_MEMORY, 0, 1, 2);
            GC.Collect();
            ExternalSort.SortFile(bucketPOSFile, 3, TripleT.Database.MAX_TRIPLES_IN_MEMORY, 1, 2, 0);
            GC.Collect();
            ExternalSort.SortFile(bucketOPSFile, 3, TripleT.Database.MAX_TRIPLES_IN_MEMORY, 2, 1, 0);
            GC.Collect();

            Triple<Atom, Atom, Atom> current = null;
            long cCurrent;

            //
            // SPO bucket
#if DEBUG
            Logger.WriteLine("Starting SPO pass...");
#endif
            cCurrent = 0;
            var cursorSPO = new TripleCursor(File.Open(bucketSPOFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            while (cursorSPO.HasNext) {
                var next = cursorSPO.Next();
                if (current == null) {
                    cCurrent++;
                } else {
                    if (current.S.InternalValue == next.S.InternalValue &&
                        current.P.InternalValue == next.P.InternalValue) {
                        cCurrent++;
                    } else {
                        //
                        // insert (SP, cCurrent) into db (or update/append if exists)
                        // db: key consists of the 2 atom values, value contains counts for all
                        // possible positions

                        var keyBytes = current.S.InternalValue < current.P.InternalValue
                            ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.P.InternalValue })
                            : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.S.InternalValue })
                            ;
                        key = new DatabaseEntry(keyBytes);

                        long[] payload;
                        if (m_db.Exists(key)) {
                            payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                        } else {
                            payload = new long[6];
                        }
                        payload[0] = cCurrent;

                        var valueBytes = Encoding.DbEncode(payload);
                        value = new DatabaseEntry(valueBytes);

                        m_db.Put(key, value);

                        cCurrent = 1;
                    }
                }
                current = next;
            }
            if (cCurrent > 0) {
                //
                // insert (SP, cCurrent) into db (or update/append if exists)
                // db: key consists of the 2 atom values, value contains counts for all
                // possible positions

                var keyBytes = current.S.InternalValue < current.P.InternalValue
                    ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.P.InternalValue })
                    : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.S.InternalValue })
                    ;
                key = new DatabaseEntry(keyBytes);

                long[] payload;
                if (m_db.Exists(key)) {
                    payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                } else {
                    payload = new long[6];
                }
                payload[0] = cCurrent;

                var valueBytes = Encoding.DbEncode(payload);
                value = new DatabaseEntry(valueBytes);

                m_db.Put(key, value);
            }
            cursorSPO.Dispose();

            GC.Collect();

            //
            // SOP bucket
#if DEBUG
            Logger.WriteLine("Starting SOP pass...");
#endif
            cCurrent = 0;
            var cursorSOP = m_context.SBucket.OpenRead();
            while (cursorSOP.HasNext) {
                var next = cursorSOP.Next();
                if (current == null) {
                    cCurrent++;
                } else {
                    if (current.S.InternalValue == next.S.InternalValue &&
                        current.O.InternalValue == next.O.InternalValue) {
                        cCurrent++;
                    } else {
                        //
                        // insert (SO, cCurrent) into db (or update/append if exists)
                        // db: key consists of the 2 atom values, value contains counts for all
                        // possible positions

                        var keyBytes = current.S.InternalValue < current.O.InternalValue
                            ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.O.InternalValue })
                            : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.S.InternalValue })
                            ;
                        key = new DatabaseEntry(keyBytes);

                        long[] payload;
                        if (m_db.Exists(key)) {
                            payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                        } else {
                            payload = new long[6];
                        }
                        payload[1] = cCurrent;

                        var valueBytes = Encoding.DbEncode(payload);
                        value = new DatabaseEntry(valueBytes);

                        m_db.Put(key, value);

                        cCurrent = 1;
                    }
                }
                current = next;
            }
            if (cCurrent > 0) {
                //
                // insert (SO, cCurrent) into db (or update/append if exists)
                // db: key consists of the 2 atom values, value contains counts for all
                // possible positions

                var keyBytes = current.S.InternalValue < current.O.InternalValue
                    ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.O.InternalValue })
                    : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.S.InternalValue })
                    ;
                key = new DatabaseEntry(keyBytes);

                long[] payload;
                if (m_db.Exists(key)) {
                    payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                } else {
                    payload = new long[6];
                }
                payload[1] = cCurrent;

                var valueBytes = Encoding.DbEncode(payload);
                value = new DatabaseEntry(valueBytes);

                m_db.Put(key, value);
            }
            cursorSOP.Dispose();

            GC.Collect();

            //
            // PSO bucket
#if DEBUG
            Logger.WriteLine("Starting PSO pass...");
#endif
            cCurrent = 0;
            var cursorPSO = m_context.PBucket.OpenRead();
            while (cursorPSO.HasNext) {
                var next = cursorPSO.Next();
                if (current == null) {
                    cCurrent++;
                } else {
                    if (current.P.InternalValue == next.P.InternalValue &&
                        current.S.InternalValue == next.S.InternalValue) {
                        cCurrent++;
                    } else {
                        //
                        // insert (SP, cCurrent) into db (or update/append if exists)
                        // db: key consists of the 2 atom values, value contains counts for all
                        // possible positions

                        var keyBytes = current.S.InternalValue < current.P.InternalValue
                            ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.P.InternalValue })
                            : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.S.InternalValue })
                            ;
                        key = new DatabaseEntry(keyBytes);

                        long[] payload;
                        if (m_db.Exists(key)) {
                            payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                        } else {
                            payload = new long[6];
                        }
                        payload[2] = cCurrent;

                        var valueBytes = Encoding.DbEncode(payload);
                        value = new DatabaseEntry(valueBytes);

                        m_db.Put(key, value);

                        cCurrent = 1;
                    }
                }
                current = next;
            }
            if (cCurrent > 0) {
                //
                // insert (SP, cCurrent) into db (or update/append if exists)
                // db: key consists of the 2 atom values, value contains counts for all
                // possible positions

                var keyBytes = current.S.InternalValue < current.P.InternalValue
                    ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.P.InternalValue })
                    : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.S.InternalValue })
                    ;
                key = new DatabaseEntry(keyBytes);

                long[] payload;
                if (m_db.Exists(key)) {
                    payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                } else {
                    payload = new long[6];
                }
                payload[2] = cCurrent;

                var valueBytes = Encoding.DbEncode(payload);
                value = new DatabaseEntry(valueBytes);

                m_db.Put(key, value);
            }
            cursorPSO.Dispose();

            GC.Collect();

            //
            // POS bucket
#if DEBUG
            Logger.WriteLine("Starting POS pass...");
#endif
            cCurrent = 0;
            var cursorPOS = new TripleCursor(File.Open(bucketPOSFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            while (cursorPOS.HasNext) {
                var next = cursorPOS.Next();
                if (current == null) {
                    cCurrent++;
                } else {
                    if (current.P.InternalValue == next.P.InternalValue &&
                        current.O.InternalValue == next.O.InternalValue) {
                        cCurrent++;
                    } else {
                        //
                        // insert (PO, cCurrent) into db (or update/append if exists)
                        // db: key consists of the 2 atom values, value contains counts for all
                        // possible positions

                        var keyBytes = current.P.InternalValue < current.O.InternalValue
                            ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.O.InternalValue })
                            : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.P.InternalValue })
                            ;
                        key = new DatabaseEntry(keyBytes);

                        long[] payload;
                        if (m_db.Exists(key)) {
                            payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                        } else {
                            payload = new long[6];
                        }
                        payload[3] = cCurrent;

                        var valueBytes = Encoding.DbEncode(payload);
                        value = new DatabaseEntry(valueBytes);

                        m_db.Put(key, value);

                        cCurrent = 1;
                    }
                }
                current = next;
            }
            if (cCurrent > 0) {
                //
                // insert (PO, cCurrent) into db (or update/append if exists)
                // db: key consists of the 2 atom values, value contains counts for all
                // possible positions

                var keyBytes = current.P.InternalValue < current.O.InternalValue
                    ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.O.InternalValue })
                    : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.P.InternalValue })
                    ;
                key = new DatabaseEntry(keyBytes);

                long[] payload;
                if (m_db.Exists(key)) {
                    payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                } else {
                    payload = new long[6];
                }
                payload[3] = cCurrent;

                var valueBytes = Encoding.DbEncode(payload);
                value = new DatabaseEntry(valueBytes);

                m_db.Put(key, value);
            }
            cursorPOS.Dispose();

            GC.Collect();

            //
            // OSP bucket
#if DEBUG
            Logger.WriteLine("Starting OSP pass...");
#endif
            cCurrent = 0;
            var cursorOSP = m_context.OBucket.OpenRead();
            while (cursorOSP.HasNext) {
                var next = cursorOSP.Next();
                if (current == null) {
                    cCurrent++;
                } else {
                    if (current.O.InternalValue == next.O.InternalValue &&
                        current.S.InternalValue == next.S.InternalValue) {
                        cCurrent++;
                    } else {
                        //
                        // insert (SO, cCurrent) into db (or update/append if exists)
                        // db: key consists of the 2 atom values, value contains counts for all
                        // possible positions

                        var keyBytes = current.S.InternalValue < current.O.InternalValue
                            ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.O.InternalValue })
                            : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.S.InternalValue })
                            ;
                        key = new DatabaseEntry(keyBytes);

                        long[] payload;
                        if (m_db.Exists(key)) {
                            payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                        } else {
                            payload = new long[6];
                        }
                        payload[4] = cCurrent;

                        var valueBytes = Encoding.DbEncode(payload);
                        value = new DatabaseEntry(valueBytes);

                        m_db.Put(key, value);

                        cCurrent = 1;
                    }
                }
                current = next;
            }
            if (cCurrent > 0) {
                //
                // insert (SO, cCurrent) into db (or update/append if exists)
                // db: key consists of the 2 atom values, value contains counts for all
                // possible positions

                var keyBytes = current.S.InternalValue < current.O.InternalValue
                    ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.S.InternalValue, current.O.InternalValue })
                    : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.S.InternalValue })
                    ;
                key = new DatabaseEntry(keyBytes);

                long[] payload;
                if (m_db.Exists(key)) {
                    payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                } else {
                    payload = new long[6];
                }
                payload[4] = cCurrent;

                var valueBytes = Encoding.DbEncode(payload);
                value = new DatabaseEntry(valueBytes);

                m_db.Put(key, value);
            }
            cursorOSP.Dispose();

            GC.Collect();

            //
            // OPS bucket
#if DEBUG
            Logger.WriteLine("Starting OPS pass...");
#endif
            cCurrent = 0;
            var cursorOPS = new TripleCursor(File.Open(bucketOPSFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            while (cursorOPS.HasNext) {
                var next = cursorOPS.Next();
                if (current == null) {
                    cCurrent++;
                } else {
                    if (current.O.InternalValue == next.O.InternalValue &&
                        current.P.InternalValue == next.P.InternalValue) {
                        cCurrent++;
                    } else {
                        //
                        // insert (PO, cCurrent) into db (or update/append if exists)
                        // db: key consists of the 2 atom values, value contains counts for all
                        // possible positions

                        var keyBytes = current.P.InternalValue < current.O.InternalValue
                            ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.O.InternalValue })
                            : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.P.InternalValue })
                            ;
                        key = new DatabaseEntry(keyBytes);

                        long[] payload;
                        if (m_db.Exists(key)) {
                            payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                        } else {
                            payload = new long[6];
                        }
                        payload[5] = cCurrent;

                        var valueBytes = Encoding.DbEncode(payload);
                        value = new DatabaseEntry(valueBytes);

                        m_db.Put(key, value);

                        cCurrent = 1;
                    }
                }
                current = next;
            }
            if (cCurrent > 0) {
                //
                // insert (PO, cCurrent) into db (or update/append if exists)
                // db: key consists of the 2 atom values, value contains counts for all
                // possible positions

                var keyBytes = current.P.InternalValue < current.O.InternalValue
                    ? Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.P.InternalValue, current.O.InternalValue })
                    : Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, current.O.InternalValue, current.P.InternalValue })
                    ;
                key = new DatabaseEntry(keyBytes);

                long[] payload;
                if (m_db.Exists(key)) {
                    payload = Encoding.DbDecodeInt64Array(m_db.Get(key).Value.Data);
                } else {
                    payload = new long[6];
                }
                payload[5] = cCurrent;

                var valueBytes = Encoding.DbEncode(payload);
                value = new DatabaseEntry(valueBytes);

                m_db.Put(key, value);
            }
            cursorOPS.Dispose();

            GC.Collect();

            //
            // delete the temporary bucket files

            File.Delete(bucketSPOFile);
            File.Delete(bucketPOSFile);
            File.Delete(bucketOPSFile);
        }

        /// <summary>
        /// Gets the total number of records (or triples) in the database.
        /// </summary>
        public long RecordsTotal
        {
            get
            {
                var key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_RECORDS_TOTAL));
                if (m_db.Exists(key)) {
                    var kvPair = m_db.Get(key);
                    return Encoding.DbDecodeInt64(kvPair.Value.Data);
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets the total number of unique atoms in the database.
        /// </summary>
        public long UniqueAtoms
        {
            get
            {
                var key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_TOTAL));
                if (m_db.Exists(key)) {
                    var kvPair = m_db.Get(key);
                    return Encoding.DbDecodeInt64(kvPair.Value.Data);
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets the number of unique S atoms in the database.
        /// </summary>
        public long UniqueSAtoms
        {
            get
            {
                var key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_S));
                if (m_db.Exists(key)) {
                    var kvPair = m_db.Get(key);
                    return Encoding.DbDecodeInt64(kvPair.Value.Data);
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets the number of unique P atoms in the database.
        /// </summary>
        public long UniquePAtoms
        {
            get
            {
                var key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_P));
                if (m_db.Exists(key)) {
                    var kvPair = m_db.Get(key);
                    return Encoding.DbDecodeInt64(kvPair.Value.Data);
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets the number of unique O atoms in the database.
        /// </summary>
        public long UniqueOAtoms
        {
            get
            {
                var key = new DatabaseEntry(Encoding.DbEncode(KEY_COUNT_ATOMS_UNIQUE_O));
                if (m_db.Exists(key)) {
                    var kvPair = m_db.Get(key);
                    return Encoding.DbDecodeInt64(kvPair.Value.Data);
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets the number of triples in the database with a given atom in a given position.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <param name="position">The triple position.</param>
        /// <returns>
        /// The number of triples with the given atom in the given position.
        /// </returns>
        public long GetRecordCount(Atom atom, TriplePosition position)
        {
            return GetRecordCount(atom.InternalValue, position);
        }

        /// <summary>
        /// Gets the number of triples in the database with a given atom in a given position.
        /// </summary>
        /// <param name="atom">The internal representation of the atom.</param>
        /// <param name="position">The triple position.</param>
        /// <returns>
        /// The number of triples with the given atom in the given position.
        /// </returns>
        public long GetRecordCount(long atom, TriplePosition position)
        {
            //
            // this piece of statistics is not present in the statistics database, but is already
            // implicitly part of the TripleT index. so, we simply use that.

            var payload = m_context.Index.GetPayload(atom);
            if (payload != null) {
                switch (position) {
                    case TriplePosition.None:
                        return 0;
                    case TriplePosition.S:
                        return (payload.SStart > -1 ? payload.SCount : 0);
                    case TriplePosition.P:
                        return (payload.PStart > -1 ? payload.PCount : 0);
                    case TriplePosition.O:
                        return (payload.OStart > -1 ? payload.OCount : 0);
                    default:
                        return 0;
                }
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Gets the number of triples in the database with the given atoms in the given positions.
        /// </summary>
        /// <param name="atom1">The first atom.</param>
        /// <param name="atom2">The second atom.</param>
        /// <param name="atom1Position">The position of the first atom.</param>
        /// <param name="atom2Position">The position of the second atom.</param>
        /// <returns>
        /// The number of triples with the given atoms in the given positions.
        /// </returns>
        public long GetRecordCount(Atom atom1, Atom atom2, TriplePosition atom1Position, TriplePosition atom2Position)
        {
            return GetRecordCount(atom1.InternalValue, atom2.InternalValue, atom1Position, atom2Position);
        }

        /// <summary>
        /// Gets the number of triples in the database with the given atoms in the given positions.
        /// </summary>
        /// <param name="atom1">The internal representaion of the first atom.</param>
        /// <param name="atom2">The internal representaion of the second atom.</param>
        /// <param name="atom1Position">The position of the first atom.</param>
        /// <param name="atom2Position">The position of the second atom.</param>
        /// <returns>
        /// The number of triples with the given atoms in the given positions.
        /// </returns>
        public long GetRecordCount(long atom1, long atom2, TriplePosition atom1Position, TriplePosition atom2Position)
        {
            var keyBytes = Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, atom1, atom2 });
            var key = new DatabaseEntry(keyBytes);
            var keyBytes2 = Encoding.DbEncode(new long[] { KEY_PREFIX_2ATOM_SAPS, atom2, atom1 });
            var key2 = new DatabaseEntry(keyBytes2);

            KeyValuePair<DatabaseEntry, DatabaseEntry> kvPair;

            //
            // we don't know in which order the atoms have been placed in the key used to store
            // this statistic in the BerkeleyDB database, so try them both

            if (m_db.Exists(key)) {
                kvPair = m_db.Get(key);
            } else if (m_db.Exists(key2)) {
                kvPair = m_db.Get(key2);
            } else {
                return 0;
            }

            //
            // the payload contains information for the two atoms in all possible positions.
            // depending on which positions are asked we need to retrieve different parts of the
            // payload.

            var payload = Encoding.DbDecodeInt64Array(kvPair.Value.Data);
            if (atom1Position == TriplePosition.S) {
                if (atom2Position == TriplePosition.P) {
                    return payload[0];
                } else if (atom2Position == TriplePosition.O) {
                    return payload[1];
                } else {
                    return 0;
                }
            } else if (atom1Position == TriplePosition.P) {
                if (atom2Position == TriplePosition.S) {
                    return payload[2];
                } else if (atom2Position == TriplePosition.O) {
                    return payload[3];
                } else {
                    return 0;
                }
            } else if (atom1Position == TriplePosition.O) {
                if (atom2Position == TriplePosition.S) {
                    return payload[4];
                } else if (atom2Position == TriplePosition.P) {
                    return payload[5];
                } else {
                    return 0;
                }
            }

            //
            // if nothing is found we always default to returning 0

            return 0;
        }

        /// <summary>
        /// Gets the join statistic for two SAPs containing the given atoms in their predicate
        /// positions and sharing a join variable between them in the given positions.
        /// </summary>
        /// <param name="leftPredicate">The predicate atom for the left SAP.</param>
        /// <param name="rightPredicate">The predicate atom for the right SAP.</param>
        /// <param name="leftPosition">The position of the shared join variable in the left SAP.</param>
        /// <param name="rightPosition">The position of the shared join variable in the right SAP.</param>
        /// <returns>
        /// The number of triples produced by the join with the given parameters.
        /// </returns>
        public long GetJoinStatistic(Atom leftPredicate, Atom rightPredicate, TriplePosition leftPosition, TriplePosition rightPosition)
        {
            return GetJoinStatistic(leftPredicate.InternalValue, rightPredicate.InternalValue, leftPosition, rightPosition);
        }

        /// <summary>
        /// Gets the join statistic for two SAPs containing the given atoms in their predicate
        /// positions and sharing a join variable between them in the given positions.
        /// </summary>
        /// <param name="leftPredicate">The internal representation of the predicate atom for the left SAP.</param>
        /// <param name="rightPredicate">The internal representation of the predicate atom for the right SAP.</param>
        /// <param name="leftPosition">The position of the shared join variable in the left SAP.</param>
        /// <param name="rightPosition">The position of the shared join variable in the right SAP.</param>
        /// <returns>
        /// The number of triples produced by the join with the given parameters.
        /// </returns>
        public long GetJoinStatistic(long leftPredicate, long rightPredicate, TriplePosition leftPosition, TriplePosition rightPosition)
        {
            var keyBytes = Encoding.DbEncode(new long[] { KEY_PREFIX_JOINSTATS, leftPredicate, rightPredicate });
            var key = new DatabaseEntry(keyBytes);
            var keyBytes2 = Encoding.DbEncode(new long[] { KEY_PREFIX_JOINSTATS, rightPredicate, leftPredicate });
            var key2 = new DatabaseEntry(keyBytes2);

            KeyValuePair<DatabaseEntry, DatabaseEntry> kvPair;

            //
            // we don't know in which order the atoms have been placed in the key used to store
            // this statistic in the BerkeleyDB database, so try them both

            if (m_db.Exists(key)) {
                kvPair = m_db.Get(key);
            } else if (m_db.Exists(key2)) {
                kvPair = m_db.Get(key2);
            } else {
                return 0;
            }

            //
            // the payload contains information for the two join variables in all possible
            // positions. depending on which positions are asked we need to retrieve different
            // parts of the payload.

            var payload = Encoding.DbDecodeInt64Array(kvPair.Value.Data);
            if (leftPosition == TriplePosition.S) {
                if (rightPosition == TriplePosition.S) {
                    return payload[0];
                } else if (rightPosition == TriplePosition.O) {
                    return payload[1];
                } else {
                    return 0;
                }
            } else if (leftPosition == TriplePosition.O) {
                if (rightPosition == TriplePosition.S) {
                    return payload[2];
                } else if (rightPosition == TriplePosition.O) {
                    return payload[3];
                } else {
                    return 0;
                }
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Gets the estimate for the number of triples in the dataset matching the given SAP.
        /// </summary>
        /// <param name="predicate">The Simple Access Pattern.</param>
        /// <returns>
        /// The number of triples estimated to match the given SAP.
        /// </returns>
        public long GetEstimate(Triple<TripleItem, TripleItem, TripleItem> predicate)
        {
            //
            // get the atoms and their positions from the given SAP

            var atomList = new List<Atom>();
            var posList = new List<TriplePosition>();

            if (predicate.S is Atom) {
                atomList.Add(predicate.S as Atom);
                posList.Add(TriplePosition.S);
            }

            if (predicate.P is Atom) {
                atomList.Add(predicate.P as Atom);
                posList.Add(TriplePosition.P);
            }

            if (predicate.O is Atom) {
                atomList.Add(predicate.O as Atom);
                posList.Add(TriplePosition.O);
            }

            //
            // this function doesn't actually do any work itself, but instead redirects to other
            // appropriate functions

            switch (atomList.Count) {
                case 0:
                    return this.RecordsTotal;
                case 1:
                    return this.GetRecordCount(atomList[0], posList[0]);
                case 2:
                    return this.GetRecordCount(atomList[0], atomList[1], posList[0], posList[1]);
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Gets the estimate for the number of results produced by the join between two given SAPs.
        /// </summary>
        /// <param name="left">The left SAP.</param>
        /// <param name="right">The right SAP.</param>
        /// <returns>
        /// The number of results estimated to be produced by the join between the two given SAPs.
        /// </returns>
        public long GetEstimate(Triple<TripleItem, TripleItem, TripleItem> left, Triple<TripleItem, TripleItem, TripleItem> right)
        {
            if (left.P is Atom && right.P is Atom) {
                //
                // if both SAPs have an atom in the predicate position, we can use the precomputed
                // join table to provide the most accurate estimate

                TriplePosition posLeft = TriplePosition.None, posRight = TriplePosition.None;

                var varLeftList = new List<Tuple<Variable, TriplePosition>>();
                var varRightList = new List<Tuple<Variable, TriplePosition>>();
                var atomList = new List<Tuple<Atom, TriplePosition>>();

                //
                // extract the variables and atoms in the SAPs

                if (left.S is Variable) {
                    varLeftList.Add(Tuple.Create(left.S as Variable, TriplePosition.S));
                } else {
                    atomList.Add(Tuple.Create(left.S as Atom, TriplePosition.S));
                }
                if (left.O is Variable) {
                    varLeftList.Add(Tuple.Create(left.O as Variable, TriplePosition.O));
                } else {
                    atomList.Add(Tuple.Create(left.O as Atom, TriplePosition.O));
                }
                if (right.S is Variable) {
                    varRightList.Add(Tuple.Create(right.S as Variable, TriplePosition.S));
                } else {
                    atomList.Add(Tuple.Create(right.S as Atom, TriplePosition.S));
                }
                if (right.O is Variable) {
                    varRightList.Add(Tuple.Create(right.O as Variable, TriplePosition.O));
                } else {
                    atomList.Add(Tuple.Create(right.O as Atom, TriplePosition.O));
                }

                //
                // find the positions of the shared join variable (making the assumption there is
                // either one or zero shared variables)

                for (int i = 0; i < varLeftList.Count; i++) {
                    for (int j = 0; j < varRightList.Count; j++) {
                        if (varLeftList[i].Item1.InternalValue == varRightList[j].Item1.InternalValue) {
                            posLeft = varLeftList[i].Item2;
                            posRight = varRightList[j].Item2;
                        }
                    }
                }

                if (posLeft != TriplePosition.None && posRight != TriplePosition.None) {
                    //
                    // if there is a shared join variable, then the precomputed join table can help
                    // us. we look up the join statistic from this join table, and factor in any
                    // remaining atom values still present in the SAPs.

                    var recTotal = (double)this.RecordsTotal;
                    var joinCount = (double)this.GetJoinStatistic(left.P as Atom, right.P as Atom, posLeft, posRight);
                    foreach (var atom in atomList) {
                        var sel = (double)this.GetRecordCount(atom.Item1, atom.Item2) / recTotal;
                        joinCount *= sel;
                    }

                    return (long)joinCount;
                } else {
                    //
                    // if there is no shared join variable, the result will be a cartesian product,
                    // and the size will thus be the estimation for the left SAP times the
                    // estimation for the right one

                    return this.GetEstimate(left) * this.GetEstimate(right);
                }
            } else {
                //
                // if either of the SAPs does not have an atom in the predicate position, then the
                // join table cannot help us and we need to fall back to less accurate estimation
                // methods

                var leftVars = left.GetVariables();
                var rightVars = right.GetVariables();
                var joinVars = new HashSet<Variable>(leftVars);
                joinVars.IntersectWith(rightVars);

                //
                // estimations for each SAP are computed recursively by other estimation functions

                var sizeLeft = GetEstimate(left);
                var sizeRight = GetEstimate(right);
                var sizeTotal = sizeLeft * sizeRight;

                //
                // here we compute:
                //
                //                             T(Left) * T(Right)
                //  -----------------------------------------------------------------------
                //   max(V(Left, v1), V(Right, v1)) * ... * max(V(Left, vk), V(Right, vk))
                //
                // where V(R, v) indicates the number of unique values for variable v in R,
                // v1 ... vk are the join variables shared between Left and Right, and T(R) is
                // the size of R.

                var denom = 0L;
                var i = 0;
                foreach (var v in joinVars) {
                    long vLeft, vRight;
                    var ests = new List<long>();

                    if (left.S is Variable && left.S.InternalValue == v.InternalValue) {
                        var f = (double)this.UniqueSAtoms / (double)this.RecordsTotal;
                        ests.Add((long)(f * (double)sizeLeft));
                    }
                    if (left.P is Variable && left.P.InternalValue == v.InternalValue) {
                        var f = (double)this.UniquePAtoms / (double)this.RecordsTotal;
                        ests.Add((long)(f * (double)sizeLeft));
                    }
                    if (left.O is Variable && left.O.InternalValue == v.InternalValue) {
                        var f = (double)this.UniqueOAtoms / (double)this.RecordsTotal;
                        ests.Add((long)(f * (double)sizeLeft));
                    }
                    vLeft = ests.Max();
                    ests.Clear();
                    if (right.S is Variable && right.S.InternalValue == v.InternalValue) {
                        var f = (double)this.UniqueSAtoms / (double)this.RecordsTotal;
                        ests.Add((long)(f * (double)sizeRight));
                    }
                    if (right.P is Variable && right.P.InternalValue == v.InternalValue) {
                        var f = (double)this.UniquePAtoms / (double)this.RecordsTotal;
                        ests.Add((long)(f * (double)sizeRight));
                    }
                    if (right.O is Variable && right.O.InternalValue == v.InternalValue) {
                        var f = (double)this.UniqueOAtoms / (double)this.RecordsTotal;
                        ests.Add((long)(f * (double)sizeRight));
                    }
                    vRight = ests.Max();

                    if (i == 0) {
                        denom = Math.Max(vLeft, vRight);
                    } else {
                        denom *= Math.Max(vLeft, vRight);
                    }
                    i++;
                }

                if (denom > 0) {
                    return sizeTotal / denom;
                } else {
                    return sizeTotal;
                }
            }
        }

        /// <summary>
        /// Gets the estimate for the number of results produced by a given operator.
        /// </summary>
        /// <param name="op">The operator.</param>
        /// <returns>
        /// An estimate for the number of results produced by the given operator.
        /// </returns>
        public long GetEstimate(Operator op)
        {
            if (op is Sort) {
                //
                // sort operator produces the same result count as its input operator

                return GetEstimate((op as Sort).Input);
            } else if (op is Scan) {
                //
                // for scan operators, look at the SAP that is used

                return GetEstimate(m_context.PatternToTriple((op as Scan).SelectPattern));
            } else if (op is HashJoin || op is MergeJoin) {
                //
                // for join operators we need to do a rough estimate (which is all we can do seeing
                // as we have no structural information available to use)

                var joinVars = new HashSet<long>();
                Operator left, right;

                //
                // find the shared join variables

                if (op is HashJoin) {
                    var opHash = op as HashJoin;
                    foreach (var v in opHash.GetJoinVariables()) {
                        joinVars.Add(v);
                    }
                    left = opHash.Left;
                    right = opHash.Right;
                } else {
                    var opMerge = op as MergeJoin;
                    foreach (var v in opMerge.GetJoinVariables()) {
                        joinVars.Add(v);
                    }
                    left = opMerge.Left;
                    right = opMerge.Right;
                }

                //
                // estimations for the input operators are computed recursively

                var sizeLeft = GetEstimate(left);
                var sizeRight = GetEstimate(right);
                var sizeTotal = sizeLeft * sizeRight;

                if (sizeTotal > 0) {
                    //
                    // so "ideally" we would like to do this:
                    //
                    //                             T(Left) * T(Right)
                    //  -----------------------------------------------------------------------
                    //   max(V(Left, v1), V(Right, v1)) * ... * max(V(Left, vk), V(Right, vk))
                    //
                    // where V(R, v) indicates the number of unique values for variable v in R,
                    // v1 ... vk are the join variables shared between Left and Right, and T(R) is
                    // the size of R.
                    //
                    // unfortunately, we don't have any structural information at this point that
                    // would allow us to compute V(R, v). so, we are forced to fall back to replacing
                    // V(R, v) with T(R), which is crap but we just can't do any better because we
                    // don't have the triple position of v (it could have been in multiple different
                    // positions throughout the query plan), so we don't know its distribution, and
                    // we have no way of guessing the number of distinct values of v in R.

                    var joinArr = joinVars.ToArray();
                    var denom = 0L;
                    for (int i = 0; i < joinArr.Length; i++) {
                        if (i == 0) {
                            denom = Math.Max(sizeLeft, sizeRight);
                        } else {
                            denom *= Math.Max(sizeLeft, sizeRight);
                        }
                    }

                    if (denom > 0) {
                        return sizeTotal / denom;
                    } else {
                        return sizeTotal;
                    }
                } else {
                    return 0;
                }
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Gets the estimate for the number of results produced by the join denoted by a given
        /// join edge.
        /// </summary>
        /// <param name="joinEdge">The join edge.</param>
        /// <returns>
        /// An estimate for the number of results produced by the join denoted by the given join edge.</returns>
        public long GetEstimate(Edge joinEdge)
        {
            if (!joinEdge.Left.HasOperator && !joinEdge.Right.HasOperator) {
                //
                // full structural information is available. this will produce the highest accuracy
                // in estimating join size, because we can use the precomputed statistics.

                return GetEstimate(joinEdge.Left.SAP, joinEdge.Right.SAP);
            } else {
                //
                // if we do not have full structural information we need to fall back to less
                // accurate estimation methods

                var leftVars = new HashSet<long>();
                var rightVars = new HashSet<long>();

                if (joinEdge.Left.HasOperator) {
                    foreach (var v in joinEdge.Left.Operator.GetOutputVariables()) {
                        leftVars.Add(v);
                    }
                } else {
                    foreach (var v in joinEdge.Left.SAP.GetVariables()) {
                        leftVars.Add(v.InternalValue);
                    }
                }

                if (joinEdge.Right.HasOperator) {
                    foreach (var v in joinEdge.Right.Operator.GetOutputVariables()) {
                        rightVars.Add(v);
                    }
                } else {
                    foreach (var v in joinEdge.Right.SAP.GetVariables()) {
                        rightVars.Add(v.InternalValue);
                    }
                }

                //
                // join variables are the variables shared between the left and right inputs

                var joinVars = new HashSet<long>(leftVars);
                joinVars.IntersectWith(rightVars);

                //
                // get an estimate for output size produced by the left and right inputs,
                // depending on whether the nodes contain SAPs or operators

                var sizeLeft = joinEdge.Left.HasOperator
                    ? GetEstimate(joinEdge.Left.Operator)
                    : GetEstimate(joinEdge.Left.SAP);
                var sizeRight = joinEdge.Right.HasOperator
                    ? GetEstimate(joinEdge.Right.Operator)
                    : GetEstimate(joinEdge.Right.SAP);
                var sizeTotal = sizeLeft * sizeRight;

                if (sizeTotal > 0) {
                    //
                    // here, we do:
                    //
                    //                             T(Left) * T(Right)
                    //  -----------------------------------------------------------------------
                    //   max(V(Left, v1), V(Right, v1)) * ... * max(V(Left, vk), V(Right, vk))
                    //
                    // where V(R, v) indicates the number of unique values for variable v in R,
                    // v1 ... vk are the join variables shared between Left and Right, and T(R) is
                    // the size of R.
                    //
                    // if insufficient structural information is available to compute V(R, v), for
                    // example when one side consists of an operator rather than an SAP, we fall
                    // back to using T(R) instread.

                    var joinArr = joinVars.ToArray();
                    var denom = 0L;
                    for (int i = 0; i < joinArr.Length; i++) {
                        long vLeft, vRight;

                        if (joinEdge.Left.HasOperator) {
                            vLeft = sizeLeft;
                        } else {
                            var ests = new List<long>();
                            if (joinEdge.Left.SAP.S is Variable && joinEdge.Left.SAP.S.InternalValue == joinArr[i]) {
                                var f = (double)this.UniqueSAtoms / (double)this.RecordsTotal;
                                ests.Add((long)(f * (double)sizeLeft));
                            }
                            if (joinEdge.Left.SAP.P is Variable && joinEdge.Left.SAP.P.InternalValue == joinArr[i]) {
                                var f = (double)this.UniquePAtoms / (double)this.RecordsTotal;
                                ests.Add((long)(f * (double)sizeLeft));
                            }
                            if (joinEdge.Left.SAP.O is Variable && joinEdge.Left.SAP.O.InternalValue == joinArr[i]) {
                                var f = (double)this.UniqueOAtoms / (double)this.RecordsTotal;
                                ests.Add((long)(f * (double)sizeLeft));
                            }
                            vLeft = ests.Max();
                        }

                        if (joinEdge.Right.HasOperator) {
                            vRight = sizeRight;
                        } else {
                            var ests = new List<long>();
                            if (joinEdge.Right.SAP.S is Variable && joinEdge.Right.SAP.S.InternalValue == joinArr[i]) {
                                var f = (double)this.UniqueSAtoms / (double)this.RecordsTotal;
                                ests.Add((long)(f * (double)sizeRight));
                            }
                            if (joinEdge.Right.SAP.P is Variable && joinEdge.Right.SAP.P.InternalValue == joinArr[i]) {
                                var f = (double)this.UniquePAtoms / (double)this.RecordsTotal;
                                ests.Add((long)(f * (double)sizeRight));
                            }
                            if (joinEdge.Right.SAP.O is Variable && joinEdge.Right.SAP.O.InternalValue == joinArr[i]) {
                                var f = (double)this.UniqueOAtoms / (double)this.RecordsTotal;
                                ests.Add((long)(f * (double)sizeRight));
                            }
                            vRight = ests.Max();
                        }

                        if (i == 0) {
                            denom = Math.Max(vLeft, vRight);
                        } else {
                            denom *= Math.Max(vLeft, vRight);
                        }
                    }

                    if (denom > 0) {
                        return sizeTotal / denom;
                    } else {
                        return sizeTotal;
                    }
                } else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Prints an overview of the available dataset statistics to the standard output.
        /// </summary>
        public void PrintDbStatistics()
        {
            Console.WriteLine("Statistics for: {0}", m_context.Name);
            Console.Write("================");
            for (int i = 0; i < m_context.Name.Length; i++) {
                Console.Write("=");
            }
            Console.WriteLine();
            Console.WriteLine("Records total: {0}", this.RecordsTotal);
            Console.WriteLine("Unique atoms: {0}", this.UniqueAtoms);
            Console.WriteLine("Unique S-atoms: {0}", this.UniqueSAtoms);
            Console.WriteLine("Unique P-atoms: {0}", this.UniquePAtoms);
            Console.WriteLine("Unique O-atoms: {0}", this.UniqueOAtoms);
        }
    }
}
