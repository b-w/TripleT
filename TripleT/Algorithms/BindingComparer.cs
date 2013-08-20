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

namespace TripleT.Algorithms
{
    using System;
    using System.Collections.Generic;
    using TripleT.Datastructures;

    /// <summary>
    /// Class containing functionality for comparisons of single bindings and binding arrays.
    /// </summary>
    public sealed class BindingComparer : IComparer<Binding>, IComparer<Binding[]>
    {
        private readonly bool m_quickComp;
        private readonly Variable[] m_sortOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingComparer"/> class.
        /// </summary>
        public BindingComparer()
        {
            //
            // we don't have a known sort order, so we cannot do a quick comparison

            m_quickComp = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingComparer"/> class.
        /// </summary>
        /// <param name="sortOrder">The sort order of the variables used in binding array comparisons.</param>
        public BindingComparer(Variable[] sortOrder)
        {
            //
            // if we actually have a non-zero length sort order we will be able to perform quicker
            // comparisons on binding arrays

            m_sortOrder = sortOrder;
            if (m_sortOrder != null && m_sortOrder.Length > 0) {
                m_quickComp = true;
            } else {
                m_quickComp = false;
            }
        }

        /// <summary>
        /// Compares the given bindings to each other.
        /// </summary>
        /// <param name="x">The first binding.</param>
        /// <param name="y">The second binding.</param>
        /// <returns>
        /// <c>-1</c> if the first binding is smaller than the second binding, <c>1</c> if the
        /// first binding is larger than the second binding, and <c>0</c> if the two are equal.
        /// </returns>
        public int Compare(Binding x, Binding y)
        {
            //
            // edge cases for null values

            if (x == null) {
                if (y == null) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if (y == null) {
                    return 1;
                }
            }

            //
            // comparing bindings simply involves comparing their variables; comparing values
            // makes no sense

            return x.Variable.CompareTo(y.Variable);
        }

        /// <summary>
        /// Compares the given binding sets to each other.
        /// </summary>
        /// <param name="x">The first binding set.</param>
        /// <param name="y">The second binding set.</param>
        /// <returns>
        /// <c>-1</c> if the first binding set is smaller than the second binding, <c>1</c> if the
        /// first binding is larger than the second binding set, and <c>0</c> if the two are equal.
        /// </returns>
        [Obsolete("Use the BindingSet data structure for sets of bindings, instead of a Binding array.")]
        public int Compare(Binding[] x, Binding[] y)
        {
            //
            // edge cases for null values

            if (x == null) {
                if (y == null) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if (y == null) {
                    return 1;
                }
            }

            if (!m_quickComp) {
                //
                // if we don't know how the binding sets are sorted, we are forced to do a slow
                // comparison which involves first sorting the sets

                Array.Sort(x, this);
                Array.Sort(y, this);

                for (int i = 0; i < Math.Min(x.Length, y.Length); i++) {
                    if (x[i].Variable.InternalValue == y[i].Variable.InternalValue) {
                        var c = x[i].Value.CompareTo(y[i].Value);
                        if (c != 0) {
                            return c;
                        }
                    } else {
                        return 0;
                    }
                }
            } else {
                //
                // if we do know the sort order, we can compare more efficiently by using this
                // information to our benefit. no sorting is required in this case.

                for (int i = 0; i < m_sortOrder.Length; i++) {
                    for (int j = 0; j < x.Length; j++) {
                        if (m_sortOrder[i].InternalValue == x[j].Variable.InternalValue) {
                            for (int k = 0; k < y.Length; k++) {
                                if (m_sortOrder[i].InternalValue == y[k].Variable.InternalValue) {
                                    var c = x[j].Value.CompareTo(y[k].Value);
                                    if (c != 0) {
                                        return c;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }
    }
}
