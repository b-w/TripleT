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
    using TripleT.Datastructures;

    /// <summary>
    /// Represents a one-directional cursor for reading triples from an underlying binary stream.
    /// </summary>
    public class TripleCursor : IDisposable
    {
        private readonly long m_tripleSize;
        private readonly BinaryReader m_reader;
        private bool m_hasNext;
        private Triple<Atom, Atom, Atom> m_next;
        private readonly TriplePosition m_miniBucket;
        private readonly long m_miniBucketValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleCursor"/> class.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="seekToTriple">The optional position offset (measured in triples) to seek to before reading starts.</param>
        public TripleCursor(Stream input, long seekToTriple = 0)
        {
            m_miniBucket = TriplePosition.None;
            m_tripleSize = 24;

            m_reader = new BinaryReader(input);
            if (seekToTriple > 0) {
                m_reader.BaseStream.Seek(seekToTriple * m_tripleSize, SeekOrigin.Begin);
            }

            //
            // prepare the next triple to output

            TryReadNext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleCursor"/> class.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="miniBucket">The mini-bucket position.</param>
        /// <param name="miniBucketValue">The mini-bucket value.</param>
        /// <param name="seekToTriple">The optional position offset (measured in triples) to seek to before reading starts.</param>
        public TripleCursor(Stream input, TriplePosition miniBucket, long miniBucketValue, long seekToTriple = 0)
        {
            m_miniBucket = miniBucket;
            m_miniBucketValue = miniBucketValue;
            m_tripleSize = 16;

            m_reader = new BinaryReader(input);
            if (seekToTriple > 0) {
                m_reader.BaseStream.Seek(seekToTriple * m_tripleSize, SeekOrigin.Begin);
            }
            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            m_reader.Close();
        }

        /// <summary>
        /// Gets the current position in the input steam, measured in triples.
        /// </summary>
        public long Position
        {
            get { return (m_reader.BaseStream.Position / m_tripleSize) - 1L; }
        }

        /// <summary>
        /// Gets the size of the input steam, measured in triples.
        /// </summary>
        public long Length
        {
            get { return m_reader.BaseStream.Length / m_tripleSize; }
        }

        /// <summary>
        /// Gets a value indicating whether the cursor has a next triple available to output.
        /// </summary>
        public bool HasNext
        {
            get { return m_hasNext; }
        }

        /// <summary>
        /// Attempts to read the next available triple from the underlying input stream.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a new triple was successfully read; otherwise, false.
        /// </returns>
        private bool TryReadNext()
        {
            m_hasNext = false;

            if (m_reader.BaseStream.Position + m_tripleSize > m_reader.BaseStream.Length) {
                return false;
            }

            m_hasNext = true;
            if (m_miniBucket != TriplePosition.None) {
                m_next = TripleSerializer.Read(m_reader, m_miniBucket, m_miniBucketValue);
            } else {
                m_next = TripleSerializer.Read(m_reader);
            }
            return true;
        }

        /// <summary>
        /// Gets the next available triple, and advances the underlying input stream.
        /// </summary>
        /// <returns>
        /// A sinlge triple.
        /// </returns>
        public Triple<Atom, Atom, Atom> Next()
        {
            if (!m_hasNext) {
                return null;
            } else {
                var t = m_next;
                TryReadNext();
                return t;
            }
        }

        /// <summary>
        /// Gets the next available triple, but does not advance the underlying input stream.
        /// </summary>
        /// <returns>
        /// A single triple.
        /// </returns>
        public Triple<Atom, Atom, Atom> Peek()
        {
            if (!m_hasNext) {
                return null;
            } else {
                return m_next;
            }
        }

        /// <summary>
        /// Attempts to advance the underlying input stream by skipping the specified number of
        /// triples.
        /// </summary>
        /// <param name="numberOfTriples">The number of triples to skip.</param>
        public void Skip(long numberOfTriples)
        {
            if (numberOfTriples > 0) {
                m_reader.BaseStream.Seek(numberOfTriples * m_tripleSize, SeekOrigin.Current);
            }
        }
    }
}