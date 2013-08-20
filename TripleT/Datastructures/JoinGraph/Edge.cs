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

namespace TripleT.Datastructures.JoinGraph
{
    using System;
    using System.Collections.Generic;
    using ac = TripleT.Datastructures.AtomCollapse;

    /// <summary>
    /// Represents an edge between two nodes in a join graph.
    /// </summary>
    public class Edge : IEquatable<Edge>
    {
        private Node m_left;
        private Node m_right;
        private readonly List<ac::EdgeLabel> m_labels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> class.
        /// </summary>
        /// <param name="left">The left node belonging to this edge.</param>
        /// <param name="right">The right node belonging to this edge.</param>
        public Edge(Node left, Node right)
        {
            m_left = left;
            m_right = right;
            m_labels = new List<ac::EdgeLabel>();
        }

        /// <summary>
        /// Gets or sets the left node belonging to this edge.
        /// </summary>
        /// <value>
        /// The new left node to connect this edge to.
        /// </value>
        public Node Left
        {
            get { return m_left; }
            set { m_left = value; }
        }

        /// <summary>
        /// Gets or sets the right node belonging to this edge.
        /// </summary>
        /// <value>
        /// The new right node to connect this edge to.
        /// </value>
        public Node Right
        {
            get { return m_right; }
            set { m_right = value; }
        }

        /// <summary>
        /// Gets the labels attached to this edge.
        /// </summary>
        public List<ac::EdgeLabel> Labels
        {
            get { return m_labels; }
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given edge.
        /// </summary>
        /// <param name="other">The other edge to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given edge is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Edge other)
        {
            //
            // edge case for null values

            if ((object)other == null) {
                return false;
            }

            //
            // for our purposes, edge equality means two edges connect to the same nodes. edge
            // labels are not included in this equation.

            if (!((m_left.Equals(other.m_left) && m_right.Equals(other.m_right)) ||
                (m_left.Equals(other.m_right) && m_right.Equals(other.m_left)))) {
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
            if (obj is Edge)
                return this.Equals(obj as Edge);
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
            return m_left.GetHashCode() ^ m_right.GetHashCode();
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
        public static bool operator ==(Edge left, Edge right)
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
        public static bool operator !=(Edge left, Edge right)
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
            return String.Format("({0} -- {1}", m_left, m_right);
        }
    }
}
