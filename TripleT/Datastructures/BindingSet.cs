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
    using System.Collections.Generic;

    /// <summary>
    /// Represents a set of (variable : atom) bindings, indexed by variable.
    /// </summary>
    public class BindingSet : IEquatable<BindingSet>
    {
        private readonly Dictionary<long, Binding> m_varTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingSet"/> class.
        /// </summary>
        /// <param name="bindings">The bindings belonging to this set.</param>
        public BindingSet(params Binding[] bindings)
        {
            m_varTable = new Dictionary<long, Binding>();

            for (int i = 0; i < bindings.Length; i++) {
                Add(bindings[i]);
            }
        }

        /// <summary>
        /// Adds the specified binding to the set.
        /// </summary>
        /// <param name="binding">The binding to add.</param>
        public void Add(Binding binding)
        {
            if (!m_varTable.ContainsKey(binding.Variable.InternalValue)) {
                m_varTable.Add(binding.Variable.InternalValue, binding);
            }
        }

        /// <summary>
        /// Removes the specified binding from the set.
        /// </summary>
        /// <param name="binding">The binding to remove.</param>
        public void Remove(Binding binding)
        {
            m_varTable.Remove(binding.Variable.InternalValue);
        }

        /// <summary>
        /// Gets the number of bindings in the set.
        /// </summary>
        public int Count
        {
            get { return m_varTable.Count; }
        }

        /// <summary>
        /// Gets the <see cref="TripleT.Datastructures.Binding"/> from this set for the specified variable.
        /// </summary>
        public Binding this[Variable index]
        {
            get
            {
                if (m_varTable.ContainsKey(index.InternalValue)) {
                    return m_varTable[index.InternalValue];
                } else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets an enumerator for all bindings in the set.
        /// </summary>
        public IEnumerable<Binding> Bindings
        {
            get
            {
                foreach (var item in m_varTable.Values) {
                    yield return item;
                }
            }
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given binding set.
        /// </summary>
        /// <param name="other">The other binding set to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given binding set is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(BindingSet other)
        {
            //
            // edge case for null values

            if ((object)other == null)
                return false;

            //
            // a quick way of ruling out equality is by looking at the binding counts of both sets

            if (other.m_varTable.Count != this.m_varTable.Count)
                return false;

            //
            // to be equal, both sets need to contain the exact same bindings

            foreach (var b1 in this.m_varTable.Values) {
                var b2 = other[b1.Variable];
                if (b2 == null) {
                    return false;
                } else if (!b1.Value.Equals(b2.Value)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is BindingSet)
                return this.Equals((BindingSet)obj);
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var h = m_varTable.Count;
            foreach (var item in m_varTable.Values) {
                h ^= item.GetHashCode();
            }

            return h;
        }

        #endregion
    }
}
