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
    using System.Collections.Generic;

    /// <summary>
    /// Class for comparing 64-bit integer arrays in a specified order.
    /// </summary>
    public sealed class NComparer : IComparer<long[]>
    {
        private readonly int[] m_order;

        /// <summary>
        /// Initializes a new instance of the <see cref="NComparer"/> class.
        /// </summary>
        /// <param name="compareOrder">The order in which to compare the values in the given arrays.</param>
        public NComparer(params int[] compareOrder)
        {
            m_order = compareOrder;
        }

        /// <summary>
        /// Compares the given 64-bit integer arrays to each other.
        /// </summary>
        /// <param name="x">The first array.</param>
        /// <param name="y">The second array.</param>
        /// <returns>
        /// <c>-1</c> if the first array is smaller than the second array, <c>1</c> if the first
        /// array is larger than the second array, and <c>0</c> if the two are equal.
        /// </returns>
        public int Compare(long[] x, long[] y)
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

            for (int i = 0; i < m_order.Length; i++) {
                var j = m_order[i];

                //
                // see if either of the arrays is shorter than the current position specified by
                // the order

                if (j > x.Length) {
                    if (j > y.Length) {
                        return 0;
                    } else {
                        return -1;
                    }
                } else {
                    if (j > y.Length) {
                        return 1;
                    }
                }

                //
                // perform position-based comparison

                var c = x[j].CompareTo(y[j]);

                if (c != 0) {
                    return c;
                }
            }

            return 0;
        }
    }
}
