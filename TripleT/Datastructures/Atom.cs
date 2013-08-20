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
    /// Represents an atom: a string value occurring in the s, p, or o position of a triple.
    /// </summary>
    public class Atom : TripleItem, IEquatable<Atom>, IComparable<Atom>
    {
        private string m_textValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="internalValue">The internal value representing this item.</param>
        public Atom(long internalValue)
            : base(internalValue)
        {
            m_textValue = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="textValue">The string value for this atom.</param>
        /// <param name="dictionary">The dictionary used for translating the string value to the internal value representing this item.</param>
        public Atom(string textValue, AtomDictionary dictionary)
            : base(dictionary.GetInternalRepresentation(textValue))
        {
            m_textValue = textValue;
        }

        /// <summary>
        /// Gets the string value for this atom.
        /// </summary>
        public string TextValue
        {
            get { return m_textValue; }
        }

        /// <summary>
        /// Sets the string value for this atom, based on its current internal value and a given
        /// atom dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary used for translating the string value to the internal value representing this item.</param>
        public void SetTextValue(AtomDictionary dictionary)
        {
            m_textValue = dictionary.GetExternalRepresentation(m_internalValue);
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given atom.
        /// </summary>
        /// <param name="other">The other atom to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given atom is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Atom other)
        {
            //
            // edge case for null values

            if ((object)other == null)
                return false;

            //
            // equality is simply determined by the internal representations of the atoms involved

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
            if (obj is Atom)
                return this.Equals(obj as Atom);
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
        /// Compares this instance to another given atom.
        /// </summary>
        /// <param name="other">The other atom to compare this instance to.</param>
        /// <returns>
        /// <c>-1</c> if this instance is smaller than the given atom, <c>1</c> if this instance is
        /// larger than the given atom, and <c>0</c> if the two are equal.
        /// </returns>
        public int CompareTo(Atom other)
        {
            //
            // edge case for null values. we assume null values precede non-null values.

            if ((object)other == null)
                return 1;

            //
            // atoms are simply compared by their internal representations

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
        public static bool operator ==(Atom left, Atom right)
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
        public static bool operator !=(Atom left, Atom right)
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
            return String.Format("a : {0}", m_internalValue);
        }
    }
}
