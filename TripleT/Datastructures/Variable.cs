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
    /// Represents a variable occurring in the s, p, or o position of a triple.
    /// </summary>
    public class Variable : TripleItem, IEquatable<Variable>, IComparable<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="identifier">The internal value representing this item.</param>
        public Variable(long identifier)
            : base(identifier)
        {
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given variable.
        /// </summary>
        /// <param name="other">The other variable to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given variable is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Variable other)
        {
            //
            // edge case for null values

            if ((object)other == null)
                return false;

            //
            // equality is simply determined by the internal representations of the variable
            // involved

            return this.m_internalValue == other.m_internalValue;
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
            if (obj is Variable)
                return this.Equals(obj as Variable);
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
            return m_internalValue.GetHashCode();
        }

        #endregion

        #region comparison functions

        /// <summary>
        /// Compares this instance to another given variable.
        /// </summary>
        /// <param name="other">The other variable to compare this instance to.</param>
        /// <returns>
        /// <c>-1</c> if this instance is smaller than the given variable, <c>1</c> if this
        /// instance is larger than the given variable, and <c>0</c> if the two are equal.
        /// </returns>
        public int CompareTo(Variable other)
        {
            //
            // edge case for null values. we assume null values precede non-null values.

            if ((object)other == null)
                return 1;

            //
            // variables are simply compared by their internal representations

            return this.m_internalValue.CompareTo(other.m_internalValue);
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
        public static bool operator ==(Variable left, Variable right)
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
        public static bool operator !=(Variable left, Variable right)
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
            return String.Format("v : ?{0}", m_internalValue);
        }
    }
}
