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

namespace TripleT.IO
{
    using System;
    using System.IO;
    using TripleT.Algorithms;
    using TripleT.Datastructures;
    using TripleT.Util;
#if DEBUG
    using TripleT.Datastructures.Queries;
#endif

    /// <summary>
    /// Represents a hybrid primary/secondary memory buffer for binding sets, supporting looping
    /// over specified sections of the buffer.
    /// </summary>
    public class BindingBuffer : IDisposable
    {
        private readonly int m_memSize;
        private readonly BindingSet[] m_memBuffer;
        private int m_memRangeMin;
        private int m_memRangeMax;
        private int m_count;
        private bool m_fileCreated;
        private string m_fileName;
        private FileStream m_fileInStream;
        private FileStream m_fileOutStream;
        private BinaryReader m_fileReader;
        private BinaryWriter m_fileWriter;
        private int m_fileRangeMin;
        private int m_fileRangeMax;
        private int m_posCurrent;
        private int m_posMin;
#if DEBUG
        private Operator m_planOperator;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingBuffer"/> class.
        /// </summary>
        /// <param name="memorySize">Size of the memory, in number of binding sets.</param>
        public BindingBuffer(int memorySize)
        {
            m_memSize = memorySize;
            m_memBuffer = new BindingSet[m_memSize];
            m_memRangeMin = 0;
            m_memRangeMax = -1;
            m_count = 0;
            m_fileCreated = false;
            m_fileRangeMin = 0;
            m_fileRangeMax = -1;
            m_posCurrent = 0;
            m_posMin = 0;
        }

#if DEBUG
        /// <summary>
        /// Sets the descriptive query plan operator that is the counterpart of the physical merge
        /// join operator this buffer belongs to.
        /// </summary>
        /// <param name="op">The descriptive query plan operator.</param>
        internal void SetPlanOperator(Operator op)
        {
            m_planOperator = op;
        }
#endif

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DeleteFile();
        }

        /// <summary>
        /// Appends a given binding set to the end of the buffer.
        /// </summary>
        /// <param name="value">The binding set.</param>
        public void Add(BindingSet value)
        {
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            //
            // the position in the memory array where the new binding set will be inserted

            var memPos = m_count % m_memSize;

            if (m_memRangeMax - m_memRangeMin + 1 < m_memSize) {
                //
                // if the entire buffer still fits in main memory, we can simply insert the new
                // binding set and be done with it

                m_memBuffer[memPos] = value;
                m_memRangeMax++;
            } else {
                //
                // if the buffer does not fit in memory (anymore), we need to write the tail of
                // the memory array to a secondary-memory file, and insert the new binding set on
                // the position of the tail

#if DEBUG
                m_planOperator.StartIOWork();
#endif
                if (!m_fileCreated) {
                    m_fileName = String.Format("~{0}.tmp", Generator.GetRandomFilename(12));
                    m_fileOutStream = File.Open(m_fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    m_fileInStream = File.Open(m_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    m_fileWriter = new BinaryWriter(m_fileOutStream);
                    m_fileReader = new BinaryReader(m_fileInStream);
                    m_fileCreated = true;
                    m_fileRangeMin = m_memRangeMin;
                    m_fileRangeMax = m_fileRangeMin - 1;
                }

                BindingSerializer.Write(m_fileWriter, m_memBuffer[memPos]);
                m_memBuffer[memPos] = value;
                m_memRangeMin++;
                m_memRangeMax++;
                m_fileRangeMax++;
#if DEBUG
                m_planOperator.StopIOWork();
#endif
            }

            m_count++;
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
        }

        /// <summary>
        /// Gets a value indicating whether the buffer has a next binding set available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a next binding set is available; otherwise, <c>false</c>.
        /// </value>
        public bool HasNext
        {
            get
            {
                return (m_posCurrent <= m_memRangeMax);
            }
        }

        /// <summary>
        /// Gets the next set of bindings from the buffer stream.
        /// </summary>
        /// <returns>
        /// A set of bindings.
        /// </returns>
        public BindingSet Next()
        {
            if (!HasNext) {
                throw new InvalidOperationException("No new bindings are available!");
            }

            //
            // see if we need to look in the main-memory array, or in the secondary-memory file

            if (m_posCurrent >= m_memRangeMin) {
                return m_memBuffer[m_posCurrent++ % m_memSize];
            } else {
#if DEBUG
                m_planOperator.StartIOWork();
#endif
                //
                // very important to flush the file buffer here: not everything from the writer
                // might be flushed to disk, so the reader potentially does not see the full file

                m_fileWriter.Flush();

                m_posCurrent++;
                var b = BindingSerializer.Read(m_fileReader);
#if DEBUG
                m_planOperator.StopIOWork();
#endif
                return b;
            }
        }

        /// <summary>
        /// Performs a jumpback operation, jumping the reader for the buffer stream back to a
        /// previously set position.
        /// </summary>
        public void Jumpback()
        {
            m_posCurrent = m_posMin;

            if (m_posMin < m_memRangeMin) {
#if DEBUG
                m_planOperator.StartIOWork();
#endif
                m_fileReader.BaseStream.Seek(m_posMin - m_fileRangeMin, SeekOrigin.Begin);
#if DEBUG
                m_planOperator.StopIOWork();
#endif
            }
        }

        /// <summary>
        /// Sets the jumpback position for the reader for the buffer stream to the current position.
        /// </summary>
        public void SetJumpback()
        {
            m_posMin = m_posCurrent - 1;
            if (m_posCurrent >= m_memRangeMin) {
                m_memRangeMin = m_posMin;
                DeleteFile();
            }
        }

        /// <summary>
        /// Deletes the secondary-memory buffer file, if it has been created.
        /// </summary>
        private void DeleteFile()
        {
            if (m_fileCreated) {
                m_fileWriter.Close();
                m_fileReader.Close();
                File.Delete(m_fileName);
                m_fileCreated = false;
                m_fileRangeMin = 0;
                m_fileRangeMax = -1;
            }
        }
    }
}
