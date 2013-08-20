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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a reader used for reading triples from a Notation-3 formatted data source.
    /// </summary>
    public class Notation3TripleReader : TripleReader
    {
        private readonly StreamReader m_input;
        private readonly HashSet<char> m_nameChars;
        private readonly Dictionary<string, string> m_namespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notation3TripleReader"/> class.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        public Notation3TripleReader(Stream input)
        {
            m_input = new StreamReader(input, Encoding.ASCII);

            //
            // set containing the characters that can be used in names/identifiers

            m_nameChars = new HashSet<char>();
            var c = 'a';
            for (int i = 0; i < 26; i++) {
                m_nameChars.Add(c);
                c++;
            }
            c = 'A';
            for (int i = 0; i < 26; i++) {
                m_nameChars.Add(c);
                c++;
            }
            c = '0';
            for (int i = 0; i < 10; i++) {
                m_nameChars.Add(c);
                c++;
            }
            m_nameChars.Add('_');
            m_nameChars.Add('-');

            m_namespaces = new Dictionary<string, string>();

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
            string line;
            var foundTriple = false;

            do {
                //
                // check if we're at EOF yet

                if (m_input.EndOfStream) {
                    m_hasNext = false;
                    return false;
                }

                //
                // read the next line

                line = m_input.ReadLine().Trim();

                if (line.StartsWith("@prefix ")) {
                    //
                    // slightly ugly code for handling lines containing prefixes. basically, we
                    // just extract the necessary info and add it to the namespaces map

                    var parts = line.Replace("  ", " ").Split(' ');
                    var key = parts[1].Trim().Substring(0, parts[1].Trim().Length - 1);
                    var value = parts[2].Trim().Substring(1, parts[2].Trim().Length - (parts[2].Trim().EndsWith(".") ? 3 : 2));
                    if (!m_namespaces.ContainsKey(key)) {
                        m_namespaces.Add(key, value);
                    }
                } else if (!line.StartsWith("#") && !String.IsNullOrWhiteSpace(line)) {
                    //
                    // assuming there's no garbage in the file, if we're here then we have a line
                    // containing a triple

                    foundTriple = true;
                }
            } while (!foundTriple);

            //
            // if we are here, then we've found a line containing a triple. parse it and return.

            m_next = ParseLine(line);
            m_hasNext = true;
            return true;
        }

        /// <summary>
        /// Parses the given line and extracts the triple.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The extracted triple (as a 3-tuple of strings).</returns>
        private Tuple<string, string, string> ParseLine(string line)
        {
            var mode = ReadingMode.None;
            var c = ' ';
            var tmpString = new StringBuilder();
            var esc = false;
            var tripleValues = new string[3];
            var triplePosition = 0;

            //
            // we parse the line character by character. it doesn't handle the full N3 spec, but
            // it should be good for most simple cases.

            for (int i = 0; i < line.Length; i++) {
                c = line[i];
                switch (mode) {
                    case ReadingMode.None:
                        if (c == '<') {
                            mode = ReadingMode.UriRef;
                        } else if (c == '_' && line[i + 1] == ':') {
                            mode = ReadingMode.NamedNode;
                            i++;
                        } else if (c == '"') {
                            mode = ReadingMode.Literal;
                        } else if (c == '^' && line[i + 1] == '^') {
                            mode = ReadingMode.Suffix;
                            i++;
                        } else if (m_nameChars.Contains(c)) {
                            mode = ReadingMode.Namespace;
                            i--;
                        }
                        break;
                    case ReadingMode.UriRef:
                        if (c == '>') {
                            tripleValues[triplePosition] = tmpString.ToString();
                            tmpString.Clear();
                            triplePosition++;

                            mode = ReadingMode.None;
                        } else {
                            tmpString.Append(c);
                        }
                        break;
                    case ReadingMode.NamedNode:
                        if (m_nameChars.Contains(c)) {
                            tmpString.Append(c);
                        } else {
                            tripleValues[triplePosition] = tmpString.ToString();
                            tmpString.Clear();
                            triplePosition++;

                            mode = ReadingMode.None;
                        }
                        break;
                    case ReadingMode.Literal:
                        if (c == '"' && !esc) {
                            tripleValues[triplePosition] = tmpString.ToString();
                            tmpString.Clear();
                            triplePosition++;

                            mode = ReadingMode.None;
                        } else {
                            if (esc) {
                                switch (c) {
                                    case 'n':
                                        tmpString.Append('\n');
                                        break;
                                    case 'r':
                                        tmpString.Append('\r');
                                        break;
                                    case 't':
                                        tmpString.Append('\t');
                                        break;
                                    case '"':
                                        tmpString.Append('"');
                                        break;
                                    case '\\':
                                        tmpString.Append('\\');
                                        break;
                                    default:
                                        tmpString.Append(c);
                                        break;
                                }

                                esc = false;
                            } else if (c == '\\') {
                                esc = true;
                            } else {
                                tmpString.Append(c);
                            }
                        }
                        break;
                    case ReadingMode.Namespace:
                        if (m_nameChars.Contains(c) || c == ':') {
                            tmpString.Append(c);
                        } else {
                            var str = tmpString.ToString();
                            var parts = str.Split(':');

                            if (m_namespaces.ContainsKey(parts[0])) {
                                str = m_namespaces[parts[0]] + parts[1];
                            }

                            tripleValues[triplePosition] = str;
                            tmpString.Clear();
                            triplePosition++;

                            mode = ReadingMode.None;
                        }
                        break;
                    case ReadingMode.Suffix:
                        if (m_nameChars.Contains(c) || c == ':') {
                            //
                            // ignore...

                        } else {
                            mode = ReadingMode.None;
                        }
                        break;
                    default:
                        break;
                }

                if (triplePosition >= tripleValues.Length) {
                    break;
                }
            }

            return Tuple.Create(tripleValues[0], tripleValues[1], tripleValues[2]);
        }

        /// <summary>
        /// Reading mode for the line parser.
        /// </summary>
        private enum ReadingMode
        {
            None = 0,
            UriRef,
            NamedNode,
            Literal,
            Namespace,
            Suffix
        }
    }
}
