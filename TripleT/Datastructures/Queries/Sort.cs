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

namespace TripleT.Datastructures.Queries
{
    using System;

    /// <summary>
    /// Represents a sort operator for use in a descriptive query plan. This operator does not
    /// perform any physical operations.
    /// </summary>
    public class Sort : Operator
    {
        private readonly Operator m_input;
        private readonly long[] m_sortOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sort"/> class.
        /// </summary>
        /// <param name="input">The input operator.</param>
        /// <param name="sortOrder">The sorting order.</param>
        public Sort(Operator input, params long[] sortOrder)
        {
            if (sortOrder.Length == 0) {
                throw new ArgumentOutOfRangeException("sortOrder", "Provide a non-empty sort ordering!");
            }

            m_input = input;
            m_sortOrder = sortOrder;
        }

        /// <summary>
        /// Gets the input operator.
        /// </summary>
        public Operator Input
        {
            get { return m_input; }
        }

        /// <summary>
        /// Gets the sorting order.
        /// </summary>
        public long[] SortOrder
        {
            get { return m_sortOrder; }
        }
    }
}
