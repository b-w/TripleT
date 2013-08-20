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
    /// Represents a single (s, p, o) triple.
    /// </summary>
    /// <typeparam name="T1">The subject type (atom or variable).</typeparam>
    /// <typeparam name="T2">The predicate type (atom or variable).</typeparam>
    /// <typeparam name="T3">The object type (atom or variable).</typeparam>
    public class Triple<T1, T2, T3> : IEquatable<Triple<TripleItem, TripleItem, TripleItem>>
        where T1 : TripleItem
        where T2 : TripleItem
        where T3 : TripleItem
    {
        private readonly T1 m_s;
        private readonly T2 m_p;
        private readonly T3 m_o;

        /// <summary>
        /// Initializes a new instance of the <see cref="Triple&lt;T1, T2, T3&gt;"/> class.
        /// </summary>
        /// <param name="s">The subject value.</param>
        /// <param name="p">The predicate value.</param>
        /// <param name="o">The object value.</param>
        public Triple(T1 s, T2 p, T3 o)
        {
            m_s = s;
            m_p = p;
            m_o = o;
        }

        /// <summary>
        /// Gets the subject value.
        /// </summary>
        public T1 S
        {
            get { return m_s; }
        }

        /// <summary>
        /// Gets the predicate value.
        /// </summary>
        public T2 P
        {
            get { return m_p; }
        }

        /// <summary>
        /// Gets the object value.
        /// </summary>
        public T3 O
        {
            get { return m_o; }
        }

        /// <summary>
        /// Gets the <see cref="TripleT.Datastructures.TripleItem"/> at the specified position.
        /// </summary>
        public TripleItem this[TriplePosition index]
        {
            get
            {
                if (index == TriplePosition.S) return m_s;
                if (index == TriplePosition.P) return m_p;
                if (index == TriplePosition.O) return m_o;

                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// Determines whether this instance is equal to another given triple.
        /// </summary>
        /// <param name="other">The other triple to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given triple is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Triple<TripleItem, TripleItem, TripleItem> other)
        {
            var eq = true;

            //
            // equality between triples is determined by the equality of their s, p, and o parts

            if (other.S is Atom) {
                eq &= (other.S as Atom).Equals(this.S);
            } else {
                eq &= (other.S as Variable).Equals(this.S);
            }

            if (other.P is Atom) {
                eq &= (other.P as Atom).Equals(this.P);
            } else {
                eq &= (other.P as Variable).Equals(this.P);
            }

            if (other.O is Atom) {
                eq &= (other.O as Atom).Equals(this.O);
            } else {
                eq &= (other.O as Variable).Equals(this.O);
            }

            return eq;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", m_s, m_p, m_o);
        }
    }
}
