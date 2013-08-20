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

namespace TripleT.Datastructures.AtomCollapse
{
    using System;

    /// <summary>
    /// Represents a label belonging to an edge in an atom collapse graph.
    /// </summary>
    public class EdgeLabel : IEquatable<EdgeLabel>
    {
        private readonly Triple<TripleItem, TripleItem, TripleItem> m_left;
        private readonly Triple<TripleItem, TripleItem, TripleItem> m_right;
        private readonly TripleItem m_sharedItem;
        private readonly TriplePosition m_posLeft;
        private readonly TriplePosition m_posRight;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeLabel"/> class.
        /// </summary>
        /// <param name="left">The SAP of the left node connected to the edge.</param>
        /// <param name="right">The SAP of the right node connected to the edge.</param>
        /// <param name="sharedItem">The shared variable value between the two nodes connected by the edge.</param>
        /// <param name="positionLeft">The position of the shared variable in the SAP of the left node connected to the edge.</param>
        /// <param name="positionRight">The position of the shared variable in the SAP of the right node connected to the edge.</param>
        public EdgeLabel(Triple<TripleItem, TripleItem, TripleItem> left, Triple<TripleItem, TripleItem, TripleItem> right, TripleItem sharedItem, TriplePosition positionLeft, TriplePosition positionRight)
        {
            m_left = left;
            m_right = right;
            m_sharedItem = sharedItem;
            m_posLeft = positionLeft;
            m_posRight = positionRight;
        }

        /// <summary>
        /// Gets the SAP of the left node connected to the edge.
        /// </summary>
        public Triple<TripleItem, TripleItem, TripleItem> Left
        {
            get { return m_left; }
        }

        /// <summary>
        /// Gets the SAP of the right node connected to the edge.
        /// </summary>
        public Triple<TripleItem, TripleItem, TripleItem> Right
        {
            get { return m_right; }
        }

        /// <summary>
        /// Gets shared variable value between the two nodes connected by the edge.
        /// </summary>
        public TripleItem SharedItem
        {
            get { return m_sharedItem; }
        }

        /// <summary>
        /// Gets the position of the shared variable in the SAP of the left node connected to the edge.
        /// </summary>
        public TriplePosition PositionLeft
        {
            get { return m_posLeft; }
        }

        /// <summary>
        /// Gets the position of the shared variable in the SAP of the right node connected to the edge.
        /// </summary>
        public TriplePosition PositionRight
        {
            get { return m_posRight; }
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given edge label.
        /// </summary>
        /// <param name="other">The other edge label to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given edge label is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(EdgeLabel other)
        {
            //
            // edge case for null values

            if ((object)other == null) {
                return false;
            }

            //
            // two edge labels are equal if all items in both labels are equal

            if (!m_left.Equals(other.m_left)) {
                return false;
            }

            if (!m_right.Equals(other.m_right)) {
                return false;
            }

            if (!m_sharedItem.Equals(other.m_sharedItem)) {
                return false;
            }

            if (!m_posLeft.Equals(other.m_posLeft)) {
                return false;
            }

            if (!m_posRight.Equals(other.m_posRight)) {
                return false;
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
            if (obj is EdgeLabel)
                return this.Equals(obj as EdgeLabel);
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
            return m_left.GetHashCode() ^ m_right.GetHashCode() ^ m_sharedItem.GetHashCode() ^ m_posLeft.GetHashCode() ^ m_posRight.GetHashCode();
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
        public static bool operator ==(EdgeLabel left, EdgeLabel right)
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
        public static bool operator !=(EdgeLabel left, EdgeLabel right)
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
            return String.Format("({0} : {{{1} , {2}}}", m_sharedItem, m_posLeft, m_posRight);
        }
    }
}
