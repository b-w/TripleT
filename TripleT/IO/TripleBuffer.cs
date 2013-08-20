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
    /// Represents a buffer, containing a number of pages on which triples can be stored.
    /// </summary>
    public class TripleBuffer
    {
        private readonly int m_numPages;
        private readonly int m_pageSize;
        private readonly TriplePage[] m_pages;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleBuffer"/> class.
        /// </summary>
        /// <param name="numberOfPages">The number of pages the buffer consists of.</param>
        /// <param name="triplesPerPage">The size of each page, denoting the number of triples that can be stored.</param>
        public TripleBuffer(int numberOfPages, int triplesPerPage)
        {
            m_numPages = numberOfPages;
            m_pageSize = triplesPerPage;
            m_pages = new TriplePage[numberOfPages];

            for (int i = 0; i < numberOfPages; i++) {
                m_pages[i] = new TriplePage(triplesPerPage);
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
        /// Gets the size of each page, denoting the number of triples that can be stored.
        /// </summary>
        public int TriplesPerPage
        {
            get { return m_pageSize; }
        }

        /// <summary>
        /// Gets the <see cref="TripleT.IO.TriplePage"/> at the specified index.
        /// </summary>
        public TriplePage this[int index]
        {
            get { return m_pages[index]; }
        }
    }
}
