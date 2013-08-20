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

namespace TripleT.Algorithms
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class containing functionality for fast equality comparisons of byte arrays.
    /// </summary>
    public sealed class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        /// <summary>
        /// Determines whether the given byte arrays are equal.
        /// </summary>
        /// <param name="x">The first byte array.</param>
        /// <param name="y">The second byte array.</param>
        /// <returns>
        /// <c>true</c> if the given arrays are equal, <c>false</c> if they are not.
        /// </returns>
        public bool Equals(byte[] x, byte[] y)
        {
            //
            // null check

            if (x == null || y == null) {
                return x == y;
            }

            //
            // if the length is not the same they are automatically unequal

            if (x.Length != y.Length) {
                return false;
            }

            //
            // value-based equality check

            for (int i = 0; i < x.Length; i++) {
                if (x[i] != y[i]) {
                    return false;
                }
            }

            //
            // if we are here, the two arrays must be equal

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The byte array.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(byte[] obj)
        {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            //
            // the hash consists of a simple XOR of all individual byte values

            int h = 0;
            for (int i = 0; i < obj.Length; i++) {
                h ^= obj[i];
            }

            return h;
        }
    }
}
