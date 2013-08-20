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
    /// Represents a scan operator for use in a descriptive query plan. This operator does not
    /// perform any physical operations.
    /// </summary>
    public class Scan : Operator
    {
        private readonly TriplePosition m_bucket;
        private readonly Pattern m_pattern;
        private readonly string m_startAtom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scan"/> class.
        /// </summary>
        /// <param name="bucket">The triple bucket to scan from.</param>
        /// <param name="pattern">The SAP to scan for.</param>
        public Scan(TriplePosition bucket, Pattern pattern)
            : this(bucket, pattern, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scan"/> class.
        /// </summary>
        /// <param name="bucket">The triple bucket to scan from.</param>
        /// <param name="pattern">The SAP to scan for.</param>
        /// <param name="startAtom">The atom to start scanning at.</param>
        public Scan(TriplePosition bucket, Pattern pattern, string startAtom)
        {
            m_bucket = bucket;
            m_pattern = pattern;
            m_startAtom = startAtom;
        }

        /// <summary>
        /// Gets the triple bucket to scan from.
        /// </summary>
        public TriplePosition Bucket
        {
            get { return m_bucket; }
        }

        /// <summary>
        /// Gets the Simple Access Pattern to scan for.
        /// </summary>
        public Pattern SelectPattern
        {
            get { return m_pattern; }
        }

        /// <summary>
        /// Gets the atom to start scanning at.
        /// </summary>
        public string StartAtom
        {
            get { return m_startAtom; }
        }
    }
}
