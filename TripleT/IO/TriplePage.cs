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
    using TripleT.Datastructures;

    /// <summary>
    /// Represents a single buffer page, on which triples can be stored.
    /// </summary>
    public class TriplePage
    {
        private readonly int m_numTriples;
        private readonly Triple<Atom, Atom, Atom>[] m_triples;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriplePage"/> class.
        /// </summary>
        /// <param name="numberOfTriples">The size of the page, denoting the number of triples that can be stored.</param>
        public TriplePage(int numberOfTriples)
        {
            m_numTriples = numberOfTriples;
            m_triples = new Triple<Atom, Atom, Atom>[numberOfTriples];
        }

        /// <summary>
        /// Gets the size of the page, denoting the number of triples that can be stored.
        /// </summary>
        public int Size
        {
            get { return m_numTriples; }
        }

        /// <summary>
        /// Gets or sets the <see cref="TripleT.Datastructures.Triple{T1, T2, T3}"/> at the specified index.
        /// </summary>
        public Triple<Atom, Atom, Atom> this[int index]
        {
            get { return m_triples[index]; }
            set { m_triples[index] = value; }
        }

        /// <summary>
        /// Clears all buffered items from the page.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_triples, 0, m_numTriples);
        }
    }
}
