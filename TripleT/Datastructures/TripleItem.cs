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
    /// <summary>
    /// Represents a triple item: an atomic value occurring in any triple position (s, p, o).
    /// </summary>
    public abstract class TripleItem
    {
        protected readonly long m_internalValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleItem"/> class.
        /// </summary>
        /// <param name="internalValue">The internal value representing this item.</param>
        public TripleItem(long internalValue)
        {
            m_internalValue = internalValue;
        }

        /// <summary>
        /// Gets the internal value representing this item.
        /// </summary>
        public long InternalValue
        {
            get { return m_internalValue; }
        }
    }
}
