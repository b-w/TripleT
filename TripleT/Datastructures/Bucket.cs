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
    using System.IO;
    using TripleT.IO;

    /// <summary>
    /// Represents a triple bucket used by the database engine for dataset storage.
    /// </summary>
    public class Bucket
    {
        private readonly string m_fileName;
        private readonly SortOrder m_order;
        private bool m_isBatchInserting;
        private BinaryWriter m_batchWriter;
        private readonly bool m_isMiniBucket;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bucket"/> class.
        /// </summary>
        /// <param name="databaseName">Name of the database this bucket belongs to.</param>
        /// <param name="sortOrder">The sort order for the triples in the bucket.</param>
        /// <param name="isMiniBucket">If set to <c>true</c>, indicates the bucket is a mini bucket.</param>
        public Bucket(string databaseName, SortOrder sortOrder, bool isMiniBucket = false)
        {
            m_order = sortOrder;

            //
            // mini buckets do not contain the triple parts (s, p, or o) belonging to their primary
            // sort order, as this information is implicitly present as part of the TripleT index
            // and thus does not need to be stored here

            if (isMiniBucket) {
                m_fileName = String.Format("{0}.bucket.m.{1}{2}{3}.dat", databaseName, sortOrder.Primary, sortOrder.Secondary, sortOrder.Tertiary);
            } else {
                m_fileName = String.Format("{0}.bucket.{1}{2}{3}.dat", databaseName, sortOrder.Primary, sortOrder.Secondary, sortOrder.Tertiary);
            }

            m_isMiniBucket = isMiniBucket;
        }

        /// <summary>
        /// Gets the name of the file containing the bucket.
        /// </summary>
        public string FileName
        {
            get { return m_fileName; }
        }

        /// <summary>
        /// Gets the sort order for the triples in the bucket.
        /// </summary>
        public SortOrder SortOrder
        {
            get { return m_order; }
        }

        /// <summary>
        /// Gets a value indicating whether the bucket is currently in the process of batch
        /// insertions of new triples.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if bucket is batch inserting; otherwise, <c>false</c>.
        /// </value>
        public bool IsBatchInserting
        {
            get { return m_isBatchInserting; }
        }

        /// <summary>
        /// Gets a value indicating whether this bucket is a mini bucket.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this bucket is a mini bucket; otherwise, <c>false</c>.
        /// </value>
        public bool IsMiniBucket
        {
            get { return m_isMiniBucket; }
        }

        /// <summary>
        /// Indicates that batch insertion of new triples into the bucket is about to start. Opens
        /// a binary writer on the bucket file and keeps it open until batch insertion is finished.
        /// </summary>
        public void BeginBatchInsert()
        {
            m_batchWriter = new BinaryWriter(File.Open(m_fileName, FileMode.Append, FileAccess.Write, FileShare.None));
            m_isBatchInserting = true;
        }

        /// <summary>
        /// Indicates that batch insertion of new triples has finished. Closes the binary writer
        /// used for batch insertion and releases the bucket file.
        /// </summary>
        public void EndBatchInsert()
        {
            if (m_isBatchInserting) {
                m_batchWriter.Flush();
                m_batchWriter.Close();
                m_isBatchInserting = false;
            } else {
                throw new InvalidOperationException("Bucket is not currently batch inserting!");
            }
        }

        /// <summary>
        /// Inserts the given triple into the bucket, appending it to the bucket file.
        /// </summary>
        /// <param name="triple">The triple to insert.</param>
        public void Insert(Triple<Atom, Atom, Atom> triple)
        {
            if (m_isBatchInserting) {
                //
                // if we are batch inserting, then use the binary writer used for that purpose

                if (m_isMiniBucket) {
                    TripleSerializer.Write(m_batchWriter, triple, m_order.Primary);
                } else {
                    TripleSerializer.Write(m_batchWriter, triple);
                }
            } else {
                //
                // for regular insertion, just open the bucket file, append, and close

                using (var sw = new BinaryWriter(File.Open(m_fileName, FileMode.Append, FileAccess.Write, FileShare.None))) {
                    if (m_isMiniBucket) {
                        TripleSerializer.Write(sw, triple, m_order.Primary);
                    } else {
                        TripleSerializer.Write(sw, triple);
                    }
                }
            }
        }

        /// <summary>
        /// Opens the bucket for reading, optionally starting at an offset of a number of triples
        /// from the start of the bucket.
        /// </summary>
        /// <param name="tripleOffset">The offset to start reading from, in number of triples.</param>
        /// <returns>
        /// A triple cursor used for reading triples from this bucket.
        /// </returns>
        public TripleCursor OpenRead(long tripleOffset = 0)
        {
            if (m_isMiniBucket) {
                //
                // this method cannot be used with minibuckets; a mini bucket value is needed
                // because the mini bucket itself does not contain full triples. the mini bucket
                // value is used during reading to complete the partial triples the bucket contains.

                throw new InvalidOperationException("Mini-bucket requires a value for missing triple item!");
            }

            return new TripleCursor(File.Open(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read), tripleOffset);
        }

        /// <summary>
        /// Opens the bucket for reading, optionally starting at an offset of a number of triples
        /// from the start of the bucket.
        /// </summary>
        /// <param name="miniBucketValue">The mini bucket value used to complete partial triples in a mini bucket.</param>
        /// <param name="tripleOffset">The offset to start reading from, in number of triples.</param>
        /// <returns>
        /// A triple cursor used for reading triples from this bucket.
        /// </returns>
        public TripleCursor OpenRead(long miniBucketValue, long tripleOffset = 0)
        {
            if (m_isMiniBucket) {
                return new TripleCursor(File.Open(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read), m_order.Primary, miniBucketValue, tripleOffset);
            } else {
                return new TripleCursor(File.Open(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read), tripleOffset);
            }
        }
    }
}
