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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a node in an atom collapse graph.
    /// </summary>
    public class Node : IEquatable<Node>
    {
        private readonly TripleItem m_item;
        private readonly List<Triple<TripleItem, TripleItem, TripleItem>> m_saps;

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="item">The atom or variable representing this node.</param>
        public Node(TripleItem item)
        {
            m_item = item;
            m_saps = new List<Triple<TripleItem, TripleItem, TripleItem>>();
        }

        /// <summary>
        /// Gets the atom or variable representing this node.
        /// </summary>
        public TripleItem Item
        {
            get { return m_item; }
        }

        /// <summary>
        /// Gets a list of this node's SAPs.
        /// </summary>
        public List<Triple<TripleItem, TripleItem, TripleItem>> SAPs
        {
            get { return m_saps; }
        }

        /// <summary>
        /// Gets the primary SAP for this node.
        /// </summary>
        public Triple<TripleItem, TripleItem, TripleItem> FirstSAP
        {
            get
            {
                if (m_saps.Count > 0) {
                    return m_saps.ElementAt(0);
                } else {
                    return null;
                }
            }
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
            // both nodes, and they need to contain the same SAPs

            if (!m_item.Equals(other.m_item)) {
                return false;
            }

            if (m_saps.Count != other.m_saps.Count) {
                return false;
            }

            foreach (var sap in m_saps) {
                if (!other.m_saps.Contains(sap)) {
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

        /// <summary>
        /// Creates a shallow copy of this instance. The copy will contain the same representing
        /// atom or variable and the same SAPs, all by reference.
        /// </summary>
        /// <returns>
        /// A shallow copy of this instance.
        /// </returns>
        public Node Copy()
        {
            var node = new Node(m_item);
            foreach (var sap in m_saps) {
                node.m_saps.Add(sap);
            }
            return node;
        }
    }
}
