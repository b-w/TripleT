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

namespace TripleT.Datastructures
{
    using System;

    /// <summary>
    /// Represents a sort order for triples, indicating on which triple positions sorting is done.
    /// </summary>
    public struct SortOrder
    {
        private readonly TriplePosition m_primary;
        private readonly TriplePosition m_secondary;
        private readonly TriplePosition m_tertiary;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortOrder"/> struct.
        /// </summary>
        /// <param name="primary">The primary sorting position.</param>
        /// <param name="secondary">The secondary sorting position.</param>
        /// <param name="tertiary">The tertiary sorting position.</param>
        public SortOrder(TriplePosition primary, TriplePosition secondary, TriplePosition tertiary)
        {
            if (primary == secondary || primary == tertiary || secondary == tertiary) {
                throw new ArgumentException("All sorting positions must be unique!");
            }

            m_primary = primary;
            m_secondary = secondary;
            m_tertiary = tertiary;
        }

        /// <summary>
        /// Gets the primary sorting position.
        /// </summary>
        public TriplePosition Primary
        {
            get { return m_primary; }
        }

        /// <summary>
        /// Gets the secondary sorting position.
        /// </summary>
        public TriplePosition Secondary
        {
            get { return m_secondary; }
        }

        /// <summary>
        /// Gets the tertiary sorting position.
        /// </summary>
        public TriplePosition Tertiary
        {
            get { return m_tertiary; }
        }
    }
}
