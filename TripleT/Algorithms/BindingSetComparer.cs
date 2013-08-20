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
    using System.Collections.Generic;
    using TripleT.Datastructures;

    /// <summary>
    /// Class containing functionality for comparisons of binding sets.
    /// </summary>
    public sealed class BindingSetComparer : IComparer<BindingSet>
    {
        private readonly bool m_quickComp;
        private readonly Variable[] m_sharedVars;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingSetComparer"/> class.
        /// </summary>
        public BindingSetComparer()
        {
            //
            // we don't have a known number of shared variables, so we cannot do a quick comparison

            m_quickComp = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingSetComparer"/> class.
        /// </summary>
        /// <param name="sharedVars">The variables shared between the binding sets that will be compared by this comparer.</param>
        public BindingSetComparer(Variable[] sharedVars)
        {
            //
            // if we actually have a non-zero length amount of shared variables we will be able to
            // perform quicker comparisons

            m_sharedVars = sharedVars;
            if (m_sharedVars != null && m_sharedVars.Length > 0) {
                m_quickComp = true;
            } else {
                m_quickComp = false;
            }
        }

        /// <summary>
        /// Compares the given binding sets to each other.
        /// </summary>
        /// <param name="x">The first binding set.</param>
        /// <param name="y">The second binding set.</param>
        /// <returns>
        /// <c>-1</c> if the first binding set is smaller than the second binding set, <c>1</c> if
        /// the first binding set is larger than the second binding set, and <c>0</c> if the two
        /// are equal.
        /// </returns>
        public int Compare(BindingSet x, BindingSet y)
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

            if (m_quickComp) {
                //
                // if we know what variables are shared by the two binding sets, we can compare
                // more efficiently by using this information to our benefit: we only need to do a
                // minimal amount of lookups in each of the sets

                for (int i = 0; i < m_sharedVars.Length; i++) {
                    var c = x[m_sharedVars[i]].Value.InternalValue.CompareTo(y[m_sharedVars[i]].Value.InternalValue);
                    if (c != 0) {
                        return c;
                    }
                }
            } else {
                //
                // we don't know what variables are shared between the sets, so we just need to
                // compare all variables from one set to those in the other

                if (x.Count < y.Count) {
                    foreach (var bx in x.Bindings) {
                        var by = y[bx.Variable];
                        if (by != null) {
                            var c = bx.Value.InternalValue.CompareTo(by.Value.InternalValue);
                            if (c != 0) {
                                return c;
                            }
                        }
                    }
                    return 0;
                } else {
                    foreach (var by in y.Bindings) {
                        var bx = x[by.Variable];
                        if (bx != null) {
                            var c = bx.Value.InternalValue.CompareTo(by.Value.InternalValue);
                            if (c != 0) {
                                return c;
                            }
                        }
                    }
                    return 0;
                }
            }

            return 0;
        }
    }
}
