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
    using System;

    /// <summary>
    /// Represents a single buffer page, on which integer arrays can be stored.
    /// </summary>
    public class NPage
    {
        private readonly int m_pageSize;
        private readonly long[][] m_items;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPage"/> class.
        /// </summary>
        /// <param name="pageSize">The size of the page, denoting the number of integer arrays that can be stored.</param>
        public NPage(int pageSize)
        {
            m_pageSize = pageSize;

            m_items = new long[m_pageSize][];
        }

        /// <summary>
        /// Gets the size of the page, denoting the number of integer arrays that can be stored.
        /// </summary>
        public int Size
        {
            get { return m_pageSize; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Int64[]"/> at the specified index.
        /// </summary>
        public long[] this[int index]
        {
            get { return m_items[index]; }
            set { m_items[index] = value; }
        }

        /// <summary>
        /// Clears all buffered items from the page.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_items, 0, m_pageSize);
        }
    }
}
