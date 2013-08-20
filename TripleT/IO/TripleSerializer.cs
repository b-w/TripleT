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
    using System.IO;
    using TripleT.Datastructures;

    /// <summary>
    /// Static class containing functions for reading and writing triples from and to binary
    /// streams.
    /// </summary>
    public static class TripleSerializer
    {
        /// <summary>
        /// Writes the given triple to the given binary output stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="triple">The triple.</param>
        public static void Write(Stream output, Triple<Atom, Atom, Atom> triple)
        {
            using (var sw = new BinaryWriter(output)) {
                Write(sw, triple);
            }
        }

        /// <summary>
        /// Reads a single triple from the given binary input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>
        /// A single triple.
        /// </returns>
        public static Triple<Atom, Atom, Atom> Read(Stream input)
        {
            using (var sr = new BinaryReader(input)) {
                return Read(sr);
            }
        }

        /// <summary>
        /// Writes the given triple to the given binary output stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="triple">The triple.</param>
        public static void Write(BinaryWriter output, Triple<Atom, Atom, Atom> triple)
        {
            output.Write(triple.S.InternalValue);
            output.Write(triple.P.InternalValue);
            output.Write(triple.O.InternalValue);
        }

        /// <summary>
        /// Reads a single triple from the given binary input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>
        /// A single triple.
        /// </returns>
        public static Triple<Atom, Atom, Atom> Read(BinaryReader input)
        {
            var s = new Atom(input.ReadInt64());
            var p = new Atom(input.ReadInt64());
            var o = new Atom(input.ReadInt64());
            return new Triple<Atom, Atom, Atom>(s, p, o);
        }

        /// <summary>
        /// Writes the given triple to the given binary output stream, not writing the atom in the
        /// mini-bucket position.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="triple">The triple.</param>
        /// <param name="miniBucket">The mini-bucket position.</param>
        public static void Write(BinaryWriter output, Triple<Atom, Atom, Atom> triple, TriplePosition miniBucket)
        {
            if (miniBucket != TriplePosition.S) {
                output.Write(triple.S.InternalValue);
            }
            if (miniBucket != TriplePosition.P) {
                output.Write(triple.P.InternalValue);
            }
            if (miniBucket != TriplePosition.O) {
                output.Write(triple.O.InternalValue);
            }
        }

        /// <summary>
        /// Reads a single triple from the given binary input stream, using the given mini-bucket
        /// value in place of the given mini-bucket position.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="miniBucket">The mini-bucket position.</param>
        /// <param name="miniBucketValue">The mini-bucket value.</param>
        /// <returns>
        /// A single triple.
        /// </returns>
        public static Triple<Atom, Atom, Atom> Read(BinaryReader input, TriplePosition miniBucket, long miniBucketValue)
        {
            Atom s, p, o;
            if (miniBucket != TriplePosition.S) {
                s = new Atom(input.ReadInt64());
            } else {
                s = new Atom(miniBucketValue);
            }
            if (miniBucket != TriplePosition.P) {
                p = new Atom(input.ReadInt64());
            } else {
                p = new Atom(miniBucketValue);
            }
            if (miniBucket != TriplePosition.O) {
                o = new Atom(input.ReadInt64());
            } else {
                o = new Atom(miniBucketValue);
            }
            return new Triple<Atom, Atom, Atom>(s, p, o);
        }
    }
}
