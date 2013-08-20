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
    using TripleT.Datastructures;

    /// <summary>
    /// Class containing functionality for position-based comparisons of triples.
    /// </summary>
    public sealed class TripleComparer : IComparer<Triple<Atom, Atom, Atom>>
    {
        private readonly TriplePosition m_primary;
        private readonly TriplePosition m_secondary;
        private readonly TriplePosition m_tertiary;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleComparer"/> class.
        /// </summary>
        /// <param name="primary">The primary comparison position.</param>
        /// <param name="secondary">The secondary comparison position.</param>
        /// <param name="tertiary">The tertiary comparison position.</param>
        public TripleComparer(TriplePosition primary, TriplePosition secondary, TriplePosition tertiary)
        {
            m_primary = primary;
            m_secondary = secondary;
            m_tertiary = tertiary;
        }

        /// <summary>
        /// Compares the given triples to each other.
        /// </summary>
        /// <param name="x">The first triple.</param>
        /// <param name="y">The second triple.</param>
        /// <returns>
        /// <c>-1</c> if the first triple is smaller than the second triple, <c>1</c> if the first
        /// triple is larger than the second triple, and <c>0</c> if the two are equal.
        /// </returns>
        public int Compare(Triple<Atom, Atom, Atom> x, Triple<Atom, Atom, Atom> y)
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
            // perform the comparisons based on the specified positions

            var c = x[m_primary].InternalValue.CompareTo(y[m_primary].InternalValue);
            if (c != 0)
                return c;

            c = x[m_secondary].InternalValue.CompareTo(y[m_secondary].InternalValue);
            if (c != 0)
                return c;

            return x[m_tertiary].InternalValue.CompareTo(y[m_tertiary].InternalValue);
        }
    }
}
