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
    using TripleT.Datastructures.Queries;

    /// <summary>
    /// Represents a node in a join graph.
    /// </summary>
    public class Node : IEquatable<Node>
    {
        private readonly TripleItem m_item;
        private readonly Triple<TripleItem, TripleItem, TripleItem> m_sap;
        private Operator m_operator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="item">The atom or variable representing this node.</param>
        /// <param name="pattern">The SAP belonging to this node.</param>
        public Node(TripleItem item, Triple<TripleItem, TripleItem, TripleItem> pattern)
        {
            m_item = item;
            m_sap = pattern;
        }

        /// <summary>
        /// Gets the atom or variable representing this node.
        /// </summary>
        public TripleItem Item
        {
            get { return m_item; }
        }

        /// <summary>
        /// Gets the SAP belonging to this node.
        /// </summary>
        public Triple<TripleItem, TripleItem, TripleItem> SAP
        {
            get { return m_sap; }
        }

        /// <summary>
        /// Gets a value indicating whether this node has an operator attached to it.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has an operator; otherwise, <c>false</c>.
        /// </value>
        public bool HasOperator
        {
            get
            {
                return (m_operator != null);
            }
        }

        /// <summary>
        /// Gets or sets the operator attached to this node.
        /// </summary>
        /// <value>
        /// The new operator to attach to this node.
        /// </value>
        public Operator Operator
        {
            get { return m_operator; }
            set { m_operator = value; }
        }

        #region equality functions

        /// <summary>
        /// Determines whether this instance is equal to another given node.
        /// </summary>
        /// <param name="other">The other node to compare this instance to.</param>
        /// <returns>
        /// <c>true</c> if the given node is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Node other)
        {
            //
            // edge case for null values

            if ((object)other == null) {
                return false;
            }

            //
            // for two nodes to be equal the representing variable or atom needs to be the same for
            // both nodes, they need to contain the same SAP, and they need to contain the same
            // operator. the trick for nodes in a join graph is that they don't always contain all
            // of these values.

            if (m_item == null) {
                if (other.m_item != null) {
                    return false;
                }
            } else if (!m_item.Equals(other.m_item)) {
                return false;
            }

            if (m_sap == null) {
                if (other.m_sap != null) {
                    return false;
                }
            } else if (!m_sap.Equals(other.m_sap)) {
                return false;
            }

            if (m_operator == null) {
                if (other.m_operator != null) {
                    return false;
                }
            } else if (!ReferenceEquals(m_operator, other.m_operator)) {
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
            if (obj is Node)
                return this.Equals(obj as Node);
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
            if (m_item == null)
                return 0;
            return m_item.GetHashCode();
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
        public static bool operator ==(Node left, Node right)
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
        public static bool operator !=(Node left, Node right)
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
            return String.Format("{0}", m_item);
        }
    }
}
