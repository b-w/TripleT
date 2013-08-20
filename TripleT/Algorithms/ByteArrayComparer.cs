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
    /// Class containing functionality for fast (and optionally partial) comparisons of byte arrays.
    /// </summary>
    public sealed class ByteArrayComparer : IComparer<byte[]>
    {
        private readonly int m_count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayComparer"/> class.
        /// </summary>
        public ByteArrayComparer()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayComparer"/> class.
        /// </summary>
        /// <param name="count">The number of positions, starting from 0, to include in the comparison.</param>
        public ByteArrayComparer(int count)
        {
            m_count = count;
        }

        /// <summary>
        /// Compares the given byte arrays to each other.
        /// </summary>
        /// <param name="x">The first byte array.</param>
        /// <param name="y">The second byte array.</param>
        /// <returns>
        /// <c>-1</c> if the first byte array is smaller than the second byte array, <c>1</c> if
        /// the first byte array is larger than the second byte array, and <c>0</c> if the two are
        /// equal.
        /// </returns>
        public int Compare(byte[] x, byte[] y)
        {
            //
            // edge cases for null values

            if (x == null) {
                if (y == null) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if (y == null) {
                    return 1;
                }
            }

            //
            // we compare whichever is smallest: the length of either of the arrays, or the
            // optionally specified number of positions to include

            var c = Math.Min(x.Length, y.Length);
            if (m_count > 0) {
                c = Math.Min(c, m_count);
            }

            for (int i = 0; i < c; i++) {
                if (x[i] < y[i]) {
                    return -1;
                } else if (x[i] > y[i]) {
                    return 1;
                }
            }

            return 0;
        }
    }
}
