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

namespace TripleT.Compatibility
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents a reader used for reading triples from a simple split-line formatted data source.
    /// </summary>
    public class SplitlineTripleReader : TripleReader
    {
        private readonly StreamReader m_input;
        private readonly string[] m_splitValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitlineTripleReader"/> class.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="splitValues">The values used for splitting the lines.</param>
        public SplitlineTripleReader(Stream input, params string[] splitValues)
        {
            m_input = new StreamReader(input);
            m_splitValues = splitValues;

            //
            // attempt to read the first triple

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_input.Close();
        }

        /// <summary>
        /// Attempts to reads the next triple from the underlying data source.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        protected override bool TryReadNext()
        {
            if (m_input.EndOfStream) {
                m_hasNext = false;
                return false;
            } else {
                while (!m_input.EndOfStream) {
                    var line = m_input.ReadLine().Trim();

                    var next = ParseLine(line);
                    if (next != null) {
                        m_next = next;
                        m_hasNext = true;
                        return true;
                    }
                }

                m_hasNext = false;
                return false;
            }
        }

        /// <summary>
        /// Parses the line and extracts the triple.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        private Tuple<string, string, string> ParseLine(string line)
        {
            //
            // we simple split the line using the given split values

            var parts = line.Split(m_splitValues, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3) {
                return Tuple.Create(parts[0], parts[1], parts[2]);
            } else {
                return null;
            }
        }
    }
}
