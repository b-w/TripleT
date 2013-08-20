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
    /// <summary>
    /// Represents a buffer, containing a number of pages on which integer arrays can be stored.
    /// </summary>
    public class NBuffer
    {
        private readonly int m_numPages;
        private readonly int m_pageSize;
        private readonly NPage[] m_pages;

        /// <summary>
        /// Initializes a new instance of the <see cref="NBuffer"/> class.
        /// </summary>
        /// <param name="numberOfPages">The number of pages the buffer consists of.</param>
        /// <param name="pageSize">The size of each page, denoting the number of integer arrays that can be stored.</param>
        public NBuffer(int numberOfPages, int pageSize)
        {
            m_numPages = numberOfPages;
            m_pageSize = pageSize;

            m_pages = new NPage[m_numPages];
            for (int i = 0; i < m_numPages; i++) {
                m_pages[i] = new NPage(m_pageSize);
            }
        }

        /// <summary>
        /// Gets number of pages the buffer consists of.
        /// </summary>
        public int NumPages
        {
            get { return m_numPages; }
        }

        /// <summary>
        /// Gets the size of each page, denoting the number of integer arrays that can be stored.
        /// </summary>
        public int PageSize
        {
            get { return m_pageSize; }
        }

        /// <summary>
        /// Gets the <see cref="TripleT.IO.NPage"/> at the specified index.
        /// </summary>
        public NPage this[int index]
        {
            get { return m_pages[index]; }
        }
    }
}
