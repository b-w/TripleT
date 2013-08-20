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
    /// <summary>
    /// Represents a Simple Access Pattern for use in a descriptive query plan.
    /// </summary>
    public class Pattern
    {
        /// <summary>
        /// The type (atom or variable) for the items (s, p, o) of the SAP.
        /// </summary>
        public enum ItemType
        {
            None = 0,
            Atom,
            Variable
        }

        private readonly object m_s;
        private readonly object m_p;
        private readonly object m_o;
        private readonly ItemType m_sType;
        private readonly ItemType m_pType;
        private readonly ItemType m_oType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value.</param>
        /// <param name="p">The p value.</param>
        /// <param name="o">The o value.</param>
        public Pattern(object s, object p, object o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            if (s is string) {
                m_sType = ItemType.Atom;
            } else {
                m_sType = ItemType.Variable;
            }

            if (p is string) {
                m_pType = ItemType.Atom;
            } else {
                m_pType = ItemType.Variable;
            }

            if (o is string) {
                m_oType = ItemType.Atom;
            } else {
                m_oType = ItemType.Variable;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (atom).</param>
        public Pattern(string s, string p, string o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Atom;
            m_pType = ItemType.Atom;
            m_oType = ItemType.Atom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (atom).</param>
        public Pattern(long s, string p, string o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Variable;
            m_pType = ItemType.Atom;
            m_oType = ItemType.Atom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (atom).</param>
        public Pattern(string s, long p, string o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Atom;
            m_pType = ItemType.Variable;
            m_oType = ItemType.Atom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (variable).</param>
        public Pattern(string s, string p, long o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Atom;
            m_pType = ItemType.Atom;
            m_oType = ItemType.Variable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (atom).</param>
        public Pattern(long s, long p, string o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Variable;
            m_pType = ItemType.Variable;
            m_oType = ItemType.Atom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (variable).</param>
        public Pattern(long s, string p, long o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Variable;
            m_pType = ItemType.Atom;
            m_oType = ItemType.Variable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (variable).</param>
        public Pattern(string s, long p, long o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Atom;
            m_pType = ItemType.Variable;
            m_oType = ItemType.Variable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (variable).</param>
        public Pattern(long s, long p, long o)
        {
            m_s = s;
            m_p = p;
            m_o = o;

            m_sType = ItemType.Variable;
            m_pType = ItemType.Variable;
            m_oType = ItemType.Variable;
        }

        /// <summary>
        /// Gets the s value for this SAP.
        /// </summary>
        public object S
        {
            get { return m_s; }
        }

        /// <summary>
        /// Gets the p value for this SAP.
        /// </summary>
        public object P
        {
            get { return m_p; }
        }

        /// <summary>
        /// Gets the o value for this SAP.
        /// </summary>
        public object O
        {
            get { return m_o; }
        }

        /// <summary>
        /// Gets the type (atom or variable) of the s value for this SAP.
        /// </summary>
        public ItemType SType
        {
            get { return m_sType; }
        }

        /// <summary>
        /// Gets the type (atom or variable) of the p value for this SAP.
        /// </summary>
        public ItemType PType
        {
            get { return m_pType; }
        }

        /// <summary>
        /// Gets the type (atom or variable) of the o value for this SAP.
        /// </summary>
        public ItemType OType
        {
            get { return m_oType; }
        }
    }
}
