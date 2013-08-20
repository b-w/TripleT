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
    using System.IO;
    using TripleT.Algorithms;
    using TripleT.Datastructures;
    using TripleT.Util;

    /// <summary>
    /// Represents a physical operator for sorting a given input set of binding sets.
    /// </summary>
    public class Sort : Operator
    {
        private readonly Operator m_input;
        private readonly Variable[] m_sortOrder;
        private readonly string m_tmpFile;
        private bool m_tmpFileCreated;
        private BinaryReader m_reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sort"/> class.
        /// </summary>
        /// <param name="input">This operator's input operator.</param>
        /// <param name="sortOrder">The sorting order.</param>
        public Sort(Operator input, params Variable[] sortOrder)
        {
            if (sortOrder.Length == 0) {
                throw new ArgumentOutOfRangeException("sortOrder", "Provide a non-empty sort ordering!");
            }

            m_input = input;
            m_sortOrder = sortOrder;

            m_tmpFileCreated = false;
            m_tmpFile = String.Format("~{0}.tmp", Generator.GetRandomFilename(12));

            //
            // prepare the next set of output bindings

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (m_tmpFileCreated) {
                m_reader.Close();
                m_reader.Dispose();

                File.Delete(m_tmpFile);
            }

            m_input.Dispose();
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
            //
            // first creat the sorted temporary file, if it doesn't already exist

            if (!m_tmpFileCreated) {
                CreateTmpFile();
            }

            //
            // one the temporary file is in place, we simply read triples from it in order

            if (m_reader.BaseStream.Position < m_reader.BaseStream.Length) {
                m_next = new BindingSet();
                for (int i = 0; i < m_sortOrder.Length; i++) {
                    var a = new Atom(m_reader.ReadInt64());
                    m_next.Add(new Binding(m_sortOrder[i], a));
                }

                m_hasNext = true;
                return true;
            } else {
                m_hasNext = false;
                return false;
            }
        }

        /// <summary>
        /// Creates the temporary file storing the input set, and sorts this file using the given
        /// sorting order.
        /// </summary>
        private void CreateTmpFile()
        {
            var memSize = 256 * 1024;
            using (var bw = new BinaryWriter(File.Open(m_tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))) {
                if (m_input.HasNext) {
                    var pk = m_input.Peek();
                    var pkSize = (uint)pk.Count * Database.MEMORY_SIZE_BINDING;
                    var m = m_planOperator.MemorySize / pkSize;
                    if (m > Int32.MaxValue) {
                        memSize = Int32.MaxValue;
                    } else {
                        memSize = Convert.ToInt32(m);
                    }
                }

                //
                // the first step involves simply consuming the input stream in full, and writing
                // it to the temporary file. note that it might be better in the future to first
                // attempt to fit the input stream in memory.

                while (m_input.HasNext) {
                    var next = m_input.Next();
                    for (int i = 0; i < m_sortOrder.Length; i++) {
                        var b = next[m_sortOrder[i]];
                        bw.Write(b.Value.InternalValue);
                    }
                }
            }

            //
            // collect the sort order

            var order = new int[m_sortOrder.Length];
            for (int i = 0; i < order.Length; i++) {
                order[i] = i;
            }

            //
            // call the external sorting algorithm to sort the file for us

            GC.Collect();
            ExternalSort.SortFile(m_tmpFile, m_sortOrder.Length, memSize, order);
            GC.Collect();

            //
            // expose a binary reader on the sorted file. we are now ready to start returning
            // triples.

            m_reader = new BinaryReader(File.Open(m_tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            m_tmpFileCreated = true;
        }
    }
}
