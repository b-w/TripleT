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

namespace TripleT.Compatibility
{
    using System;

    /// <summary>
    /// Represents a reader used for reading triples from external data sources.
    /// </summary>
    public abstract class TripleReader : IDisposable
    {
        protected bool m_hasNext;
        protected Tuple<string, string, string> m_next;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleReader"/> class.
        /// </summary>
        public TripleReader()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the reader has a next value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the reader has a next value; otherwise, <c>false</c>.
        /// </value>
        public bool HasNext
        {
            get { return m_hasNext; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Attempts to reads the next triple from the underlying data source.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool TryReadNext();

        /// <summary>
        /// Returns the next available triple from the underlying data source.
        /// </summary>
        /// <returns>
        /// The next triple read from the underlying data source.
        /// </returns>
        public Tuple<string, string, string> Next()
        {
            if (!m_hasNext) {
                return null;
            } else {
                var t = m_next;
                TryReadNext();
                return t;
            }
        }
    }
}
