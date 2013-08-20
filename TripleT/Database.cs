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

namespace TripleT
{
    using System;
    using System.Collections.Generic;
    using TripleT.Algorithms;
    using TripleT.Compatibility;
    using TripleT.Datastructures;
    using TripleT.Datastructures.Queries;
    using TripleT.IO;
    using op = TripleT.IO.Operators;
    using qu = TripleT.Datastructures.Queries;

    /// <summary>
    /// Represents a single TripleT RDF datastore.
    /// </summary>
    public sealed class Database : IDisposable
    {
        /// <summary>
        /// Denotes the current state the database is in.
        /// </summary>
        public enum State
        {
            None = 0,
            Closed,
            Open
        }

        internal const uint MEMORY_SIZE_BINDING = 88;
        internal const uint MEMORY_SIZE_TOTAL = (UInt32.MaxValue / 4) * 3;
        internal const int MAX_TRIPLES_IN_MEMORY = 20000000;

        private readonly string m_name;
        private AtomDictionary m_dict;
        private Index m_index;
        private Statistics m_stats;
        private Bucket m_sBucket;
        private Bucket m_pBucket;
        private Bucket m_oBucket;
        private Bucket m_sBucketMini;
        private Bucket m_oBucketMini;
        private Bucket m_pBucketMini;
        private bool m_isBatchInserting;
        private State m_state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="name">The name for the database.</param>
        public Database(string name)
        {
            m_name = name;
            m_state = State.Closed;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (m_state == State.Open) {
                this.Close();
            }
        }

        /// <summary>
        /// Gets the name of this database.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Gets the S-bucket for this database.
        /// </summary>
        internal Bucket SBucket
        {
            get { return m_sBucket; }
        }

        /// <summary>
        /// Gets the P-bucket for this database.
        /// </summary>
        internal Bucket PBucket
        {
            get { return m_pBucket; }
        }

        /// <summary>
        /// Gets the O-bucket for this database.
        /// </summary>
        internal Bucket OBucket
        {
            get { return m_oBucket; }
        }

        /// <summary>
        /// Gets the mini S-bucket for this database.
        /// </summary>
        internal Bucket SBucketMini
        {
            get { return m_sBucketMini; }
        }

        /// <summary>
        /// Gets the mini P-bucket for this database.
        /// </summary>
        internal Bucket PBucketMini
        {
            get { return m_pBucketMini; }
        }

        /// <summary>
        /// Gets the mini O-bucket for this database.
        /// </summary>
        internal Bucket OBucketMini
        {
            get { return m_oBucketMini; }
        }

        /// <summary>
        /// Opens the database.
        /// </summary>
        public void Open()
        {
            if (m_state == State.Closed) {
                m_dict = new AtomDictionary(m_name);
                m_sBucket = new Bucket(m_name, new SortOrder(TriplePosition.S, TriplePosition.O, TriplePosition.P));
                m_pBucket = new Bucket(m_name, new SortOrder(TriplePosition.P, TriplePosition.S, TriplePosition.O));
                m_oBucket = new Bucket(m_name, new SortOrder(TriplePosition.O, TriplePosition.S, TriplePosition.P));
                m_sBucketMini = new Bucket(m_name, new SortOrder(TriplePosition.S, TriplePosition.O, TriplePosition.P), true);
                m_pBucketMini = new Bucket(m_name, new SortOrder(TriplePosition.P, TriplePosition.S, TriplePosition.O), true);
                m_oBucketMini = new Bucket(m_name, new SortOrder(TriplePosition.O, TriplePosition.S, TriplePosition.P), true);
                m_index = new Index(m_name, m_sBucket, m_pBucket, m_oBucket);
                m_stats = new Statistics(m_name, this);

                m_isBatchInserting = false;

                m_state = State.Open;
            } else {
                throw new InvalidOperationException("Database is not in closed state!");
            }
        }

        /// <summary>
        /// Closes the database.
        /// </summary>
        public void Close()
        {
            if (m_state == State.Open) {
                m_dict.Dispose();
                m_index.Dispose();
                m_stats.Dispose();

                m_state = State.Closed;
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Indicates that batch insertion of new triples into the database is about to start.
        /// </summary>
        public void BeginBatchInsert()
        {
            if (m_state == State.Open) {
                if (!m_isBatchInserting) {
                    m_sBucket.BeginBatchInsert();
                    m_pBucket.BeginBatchInsert();
                    m_oBucket.BeginBatchInsert();

                    m_isBatchInserting = true;
                }
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Indicates that batch insertion of new triples into the database has finished.
        /// </summary>
        public void EndBatchInsert()
        {
            if (m_state == State.Open) {
                if (m_isBatchInserting) {
                    m_sBucket.EndBatchInsert();
                    m_pBucket.EndBatchInsert();
                    m_oBucket.EndBatchInsert();

                    m_isBatchInserting = false;
                }
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Inserts the given triple into the database.
        /// </summary>
        /// <param name="triple">The triple to insert.</param>
        public void Insert(Tuple<string, string, string> triple)
        {
            if (triple == null) {
                throw new ArgumentNullException("triple");
            }

            this.Insert(triple.Item1, triple.Item2, triple.Item3);
        }

        /// <summary>
        /// Inserts the given triple into the database.
        /// </summary>
        /// <param name="s">The s value of the triple.</param>
        /// <param name="p">The p value of the triple.</param>
        /// <param name="o">The o value of the triple.</param>
        public void Insert(string s, string p, string o)
        {
            if (s == null || String.IsNullOrEmpty(s)) {
                throw new ArgumentOutOfRangeException("s", "Provide a non-empty subject value!");
            }
            if (p == null || String.IsNullOrEmpty(p)) {
                throw new ArgumentOutOfRangeException("p", "Provide a non-empty subject value!");
            }
            if (o == null || String.IsNullOrEmpty(o)) {
                throw new ArgumentOutOfRangeException("o", "Provide a non-empty subject value!");
            }

            if (m_state == State.Open) {
                var sAtom = new Atom(s, m_dict);
                var pAtom = new Atom(p, m_dict);
                var oAtom = new Atom(o, m_dict);
                var triple = new Triple<Atom, Atom, Atom>(sAtom, pAtom, oAtom);

                m_sBucket.Insert(triple);
                m_pBucket.Insert(triple);
                m_oBucket.Insert(triple);
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Reads all triples from the given triple reader and inserts them into the database.
        /// </summary>
        /// <param name="tripleReader">The triple reader to use.</param>
        public void InsertAll(TripleReader tripleReader)
        {
            if (tripleReader == null) {
                throw new ArgumentNullException("tripleReader");
            }

            if (m_state == State.Open) {
                var biState = m_isBatchInserting;
                if (!biState) {
                    BeginBatchInsert();
                }

                while (tripleReader.HasNext) {
                    Insert(tripleReader.Next());
                }

                if (!biState) {
                    EndBatchInsert();
                }
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Builds the indexes over the database. It is assumed that insertion of triples has
        /// finished.
        /// </summary>
        public void BuildIndex()
        {
            if (m_state == State.Open) {
                if (!m_isBatchInserting) {
                    GC.Collect();
                    ExternalSort.SortBucket(m_sBucket, MAX_TRIPLES_IN_MEMORY);
                    GC.Collect();
                    ExternalSort.SortBucket(m_pBucket, MAX_TRIPLES_IN_MEMORY);
                    GC.Collect();
                    ExternalSort.SortBucket(m_oBucket, MAX_TRIPLES_IN_MEMORY);
                    GC.Collect();
                    m_index.Build();
                    GC.Collect();
                    MinifyBuckets();
                    GC.Collect();
                    ComputeStatistics();
                    GC.Collect();
                }
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Creates the mini-buckets from the current full buckets.
        /// </summary>
        private void MinifyBuckets()
        {
            var sCursor = m_sBucket.OpenRead();
            var pCursor = m_pBucket.OpenRead();
            var oCursor = m_oBucket.OpenRead();

            m_sBucketMini.BeginBatchInsert();
            while (sCursor.HasNext) {
                m_sBucketMini.Insert(sCursor.Next());
            }
            m_sBucketMini.EndBatchInsert();

            m_pBucketMini.BeginBatchInsert();
            while (pCursor.HasNext) {
                m_pBucketMini.Insert(pCursor.Next());
            }
            m_pBucketMini.EndBatchInsert();

            m_oBucketMini.BeginBatchInsert();
            while (oCursor.HasNext) {
                m_oBucketMini.Insert(oCursor.Next());
            }
            m_oBucketMini.EndBatchInsert();

            sCursor.Dispose();
            pCursor.Dispose();
            oCursor.Dispose();
        }

        /// <summary>
        /// Computes the statistics database.
        /// </summary>
        private void ComputeStatistics()
        {
            m_stats.Build();
        }

        /// <summary>
        /// Computes a descriptive query plan for a given Basic Graph Pattern.
        /// </summary>
        /// <param name="accessPattern">The Basic Graph Pattern.</param>
        /// <returns>
        /// A descriptive query plan.
        /// </returns>
        public QueryPlan GetQueryPlan(params Pattern[] accessPattern)
        {
            if (accessPattern == null || accessPattern.Length == 0) {
                throw new ArgumentOutOfRangeException("accessPattern", "Provide a non-empty Simple Access Pattern!");
            }

            if (m_state == State.Open) {
                var triplePattern = new Triple<TripleItem, TripleItem, TripleItem>[accessPattern.Length];
                for (int i = 0; i < accessPattern.Length; i++) {
                    triplePattern[i] = PatternToTriple(accessPattern[i]);
                }
                return PlanGenerator.ComputePlan(this, triplePattern);
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Executes the given query plan over the dataset, iterating over and returning the
        /// results.
        /// </summary>
        /// <param name="plan">The descriptive query plan to execute.</param>
        /// <param name="materialize">if set to <c>true</c>, materialize the external representation of the results produced.</param>
        /// <returns>
        /// An iterator over the query results.
        /// </returns>
        public IEnumerable<BindingSet> Query(QueryPlan plan, bool materialize = true)
        {
            if (m_state == State.Open) {
                var rootOperator = GetOperator(plan.Root);
                BindingSet next = null;

                while (rootOperator.HasNext) {
                    next = rootOperator.Next();
                    if (materialize) {
                        foreach (var item in next.Bindings) {
                            item.Value.SetTextValue(m_dict);
                        }
                    }
                    yield return next;
                }

                rootOperator.Dispose();
            } else {
                throw new InvalidOperationException("Database is not in open state!");
            }
        }

        /// <summary>
        /// Gets the statistics database for this dataset.
        /// </summary>
        public Statistics @Statistics
        {
            get { return m_stats; }
        }

        /// <summary>
        /// Gets the index for this dataset.
        /// </summary>
        public Index @Index
        {
            get { return m_index; }
        }

        /// <summary>
        /// Gets the atom dictionary for this dataset.
        /// </summary>
        public AtomDictionary Dictionary
        {
            get { return m_dict; }
        }

        /// <summary>
        /// Recursively converts the given descriptive operator into a physical operator.
        /// </summary>
        /// <param name="oper">The descriptive operator.</param>
        /// <returns>
        /// The physical counterpart of the given descriptive operator.
        /// </returns>
        private op::Operator GetOperator(qu::Operator oper)
        {
            if (oper is qu::Scan) {
                var qScan = (qu::Scan)oper;

                //
                // if it's a scan operator we need to convert the descriptive operator and pattern
                // to a physical operator and triple pattern, which is somewhat tedious but not
                // very difficult

                Bucket bucket = null;
                switch (qScan.Bucket) {
                    case TriplePosition.None:
                        throw new ArgumentOutOfRangeException("oper");
                    case TriplePosition.S:
                        bucket = m_sBucket;
                        break;
                    case TriplePosition.P:
                        bucket = m_pBucket;
                        break;
                    case TriplePosition.O:
                        bucket = m_oBucket;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("oper");
                }

                long startPos = 0;
                long numTriples = -1;
                long miniBucketValue = -1;
                if (!String.IsNullOrEmpty(qScan.StartAtom)) {
                    //
                    // if we have a start atom, then we can use a mini-bucket to retrieve the data.
                    // over here we figure out the values for the mini-bucket.

                    var startAtom = new Atom(qScan.StartAtom, m_dict);
                    var payload = m_index.GetPayload(startAtom);

                    if (payload == null) {
                        throw new ArgumentException(String.Format("No index information found for atom: {0}", startAtom.TextValue));
                    }

                    switch (qScan.Bucket) {
                        case TriplePosition.None:
                            throw new ArgumentOutOfRangeException("oper");
                        case TriplePosition.S:
                            startPos = payload.SStart;
                            if (qScan.SelectPattern.S is string && (qScan.SelectPattern.S as string).Equals(qScan.StartAtom)) {
                                bucket = m_sBucketMini;
                                numTriples = payload.SCount;
                                miniBucketValue = startAtom.InternalValue;
                            }
                            break;
                        case TriplePosition.P:
                            startPos = payload.PStart;
                            if (qScan.SelectPattern.P is string && (qScan.SelectPattern.P as string).Equals(qScan.StartAtom)) {
                                bucket = m_pBucketMini;
                                numTriples = payload.PCount;
                                miniBucketValue = startAtom.InternalValue;
                            }
                            break;
                        case TriplePosition.O:
                            startPos = payload.OStart;
                            if (qScan.SelectPattern.O is string && (qScan.SelectPattern.O as string).Equals(qScan.StartAtom)) {
                                bucket = m_oBucketMini;
                                numTriples = payload.OCount;
                                miniBucketValue = startAtom.InternalValue;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("oper");
                    }
                }

                //
                // see if we're dealing with a mini-bucket or a regular one

                TripleCursor cursor;
                if (numTriples > -1) {
                    cursor = bucket.OpenRead(miniBucketValue, startPos);
                } else {
                    cursor = bucket.OpenRead(startPos);
                }

                var pattern = qScan.SelectPattern;
                var triple = PatternToTriple(pattern);

                if (numTriples > -1) {
                    return new op::Scan(qScan, cursor, bucket.SortOrder, triple, numTriples);
                } else {
                    return new op::Scan(qScan, cursor, bucket.SortOrder, triple);
                }
            } else if (oper is qu::Sort) {
                var qSort = (qu::Sort)oper;

                //
                // if it's a sort operator then the conversion from descriptive to physical
                // operator is pretty easy. we also need to recurse on its input operator.

                var input = GetOperator(qSort.Input);

                var sortVars = new List<Variable>();
                for (int i = 0; i < qSort.SortOrder.Length; i++) {
                    sortVars.Add(new Variable(qSort.SortOrder[i]));
                }

                return new op::Sort(input, sortVars.ToArray());
            } else if (oper is qu::MergeJoin) {
                var qMerge = (qu::MergeJoin)oper;

                //
                // in the case of a merge join, we need to recurse on its input operators

                var left = GetOperator(qMerge.Left);
                var right = GetOperator(qMerge.Right);

                //
                // we also need to conver the sorting order variables

                var sortVars = new List<Variable>();
                for (int i = 0; i < qMerge.SortOrder.Length; i++) {
                    sortVars.Add(new Variable(qMerge.SortOrder[i]));
                }

                return new op::MergeJoin(qMerge, left, right, sortVars.ToArray());
            } else if (oper is qu::HashJoin) {
                var qHash = (qu::HashJoin)oper;

                //
                // in the case of a hash join, we need to recurse on its input operators

                var left = GetOperator(qHash.Left);
                var right = GetOperator(qHash.Right);

                return new op::HashJoin(qHash, left, right);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Converts the given Simple Access Pattern to a triple.
        /// </summary>
        /// <param name="pattern">The Simple Access Pattern.</param>
        /// <returns>
        /// A single triple.
        /// </returns>
        internal Triple<TripleItem, TripleItem, TripleItem> PatternToTriple(Pattern pattern)
        {
            //
            // it's pretty tedious to do this conversion, but not very complicated. it's just a lot
            // of cases we need to go trough.

            Triple<TripleItem, TripleItem, TripleItem> triple = null;
            if (pattern.SType == qu::Pattern.ItemType.Atom && pattern.PType == qu::Pattern.ItemType.Atom && pattern.OType == qu::Pattern.ItemType.Atom) {
                var s = pattern.S as string;
                var p = pattern.P as string;
                var o = pattern.O as string;

                var sAtom = new Atom(s, m_dict);
                var pAtom = new Atom(p, m_dict);
                var oAtom = new Atom(o, m_dict);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else if (pattern.SType == qu::Pattern.ItemType.Variable && pattern.PType == qu::Pattern.ItemType.Atom && pattern.OType == qu::Pattern.ItemType.Atom) {
                var s = (long)pattern.S;
                var p = pattern.P as string;
                var o = pattern.O as string;

                var sAtom = new Variable(s);
                var pAtom = new Atom(p, m_dict);
                var oAtom = new Atom(o, m_dict);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else if (pattern.SType == qu::Pattern.ItemType.Atom && pattern.PType == qu::Pattern.ItemType.Variable && pattern.OType == qu::Pattern.ItemType.Atom) {
                var s = pattern.S as string;
                var p = (long)pattern.P;
                var o = pattern.O as string;

                var sAtom = new Atom(s, m_dict);
                var pAtom = new Variable(p);
                var oAtom = new Atom(o, m_dict);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else if (pattern.SType == qu::Pattern.ItemType.Atom && pattern.PType == qu::Pattern.ItemType.Atom && pattern.OType == qu::Pattern.ItemType.Variable) {
                var s = pattern.S as string;
                var p = pattern.P as string;
                var o = (long)pattern.O;

                var sAtom = new Atom(s, m_dict);
                var pAtom = new Atom(p, m_dict);
                var oAtom = new Variable(o);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else if (pattern.SType == qu::Pattern.ItemType.Variable && pattern.PType == qu::Pattern.ItemType.Variable && pattern.OType == qu::Pattern.ItemType.Atom) {
                var s = (long)pattern.S;
                var p = (long)pattern.P;
                var o = pattern.O as string;

                var sAtom = new Variable(s);
                var pAtom = new Variable(p);
                var oAtom = new Atom(o, m_dict);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else if (pattern.SType == qu::Pattern.ItemType.Variable && pattern.PType == qu::Pattern.ItemType.Atom && pattern.OType == qu::Pattern.ItemType.Variable) {
                var s = (long)pattern.S;
                var p = pattern.P as string;
                var o = (long)pattern.O;

                var sAtom = new Variable(s);
                var pAtom = new Atom(p, m_dict);
                var oAtom = new Variable(o);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else if (pattern.SType == qu::Pattern.ItemType.Atom && pattern.PType == qu::Pattern.ItemType.Variable && pattern.OType == qu::Pattern.ItemType.Variable) {
                var s = pattern.S as string;
                var p = (long)pattern.P;
                var o = (long)pattern.O;

                var sAtom = new Atom(s, m_dict);
                var pAtom = new Variable(p);
                var oAtom = new Variable(o);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            } else {
                var s = (long)pattern.S;
                var p = (long)pattern.P;
                var o = (long)pattern.O;

                var sAtom = new Variable(s);
                var pAtom = new Variable(p);
                var oAtom = new Variable(o);
                triple = new Triple<TripleItem, TripleItem, TripleItem>(sAtom, pAtom, oAtom);
            }
            return triple;
        }
    }
}
