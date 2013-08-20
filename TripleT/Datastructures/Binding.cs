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
    /// Represents a single (variable : atom) binding that makes up query results.
    /// </summary>
    public class Binding : IEquatable<Binding>
    {
        private readonly Variable m_variable;
        private readonly Atom m_atom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        /// <param name="variable">The variable part of the binding.</param>
        /// <param name="atom">The atom part of the binding.</param>
        public Binding(Variable variable, Atom atom)
        {
            m_variable = variable;
            m_atom = atom;
        }

        /// <summary>
        /// Gets the variable part of the binding.
        /// </summary>
        public Variable Variable
        {
            get { return m_variable; }
        }

        /// <summary>
        /// Gets the atom (or value) part of the binding.
        /// </summary>
        public Atom Value
        {
            get { return m_atom; }
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given binding.
        /// </summary>
        /// <param name="other">The other binding to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given binding is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Binding other)
        {
            //
            // edge case for null values

            if ((object)other == null)
                return false;

            //
            // for two bindings to be equal, both their variables and their atom values must be
            // equal

            return this.Variable.Equals(other.Variable) && this.Value.Equals(other.Value);
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
            if (obj is Binding)
                return this.Equals(obj as Binding);
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
            return m_variable.GetHashCode() ^ m_atom.GetHashCode();
        }

        #endregion

        #region operators

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the two operands are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Binding left, Binding right)
        {
            if (object.ReferenceEquals(left, right))
                return true;
            if ((object)left == null || (object)right == null)
                return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the two operands are unequal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Binding left, Binding right)
        {
            return !(left == right);
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("({0} == {1})", m_variable, m_atom);
        }
    }
}
