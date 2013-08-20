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
    /// Static class containing functions for reading and writing binding sets from and to binary
    /// streams.
    /// </summary>
    public static class BindingSerializer
    {
        /// <summary>
        /// Writes the given binding set to the given binary output stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="values">The binding set.</param>
        public static void Write(BinaryWriter output, BindingSet values)
        {
            output.Write(values.Count);
            foreach (var item in values.Bindings) {
                output.Write(item.Variable.InternalValue);
                output.Write(item.Value.InternalValue);
            }
        }

        /// <summary>
        /// Reads a single binding set from the given binary input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>
        /// A binding set.
        /// </returns>
        public static BindingSet Read(BinaryReader input)
        {
            var count = input.ReadInt32();
            var values = new BindingSet();
            for (int i = 0; i < count; i++) {
                var v = new Variable(input.ReadInt64());
                var a = new Atom(input.ReadInt64());
                values.Add(new Binding(v, a));
            }
            return values;
        }
    }
}
