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

namespace TripleT.Util
{
    using System.Collections.Generic;
    using TripleT.Datastructures;

    /// <summary>
    /// Contains extension methods for the <see cref="TripleT.Datastructures.Triple{T1, T2, T3}"/> class.
    /// </summary>
    public static class TripleExtensions
    {
        /// <summary>
        /// Computes the selectivity ranking for this triple, where a lower rank value indicates a
        /// lower degree of selectivity.
        /// </summary>
        /// <param name="triple">The triple instance.</param>
        /// <returns>
        /// An integer indicating the selectivity ranking for this triple.
        /// </returns>
        public static int GetSelectivityRanking(this Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            /*
             * (s, p, o) = 7
             * (s, ?, o) = 6
             * (?, p, o) = 5
             * (s, p, ?) = 4
             * (?, ?, o) = 3
             * (s, ?, ?) = 2
             * (?, p, ?) = 1
             * (?, ?, ?) = 0
             */

            if (triple == null) {
                return -1;
            }

            if (triple.S is Atom) {
                if (triple.O is Atom) {
                    if (triple.P is Atom) {
                        //
                        // (s, p, o)

                        return 7;
                    } else {
                        //
                        // (s, ?, o)

                        return 6;
                    }
                } else {
                    if (triple.P is Atom) {
                        //
                        // (s, p, ?)

                        return 4;
                    } else {
                        //
                        // (s, ?, ?)

                        return 2;
                    }
                }
            } else {
                if (triple.O is Atom) {
                    if (triple.P is Atom) {
                        //
                        // (?, p, o)

                        return 5;
                    } else {
                        //
                        // (?, ?, o)

                        return 3;
                    }
                } else {
                    if (triple.P is Atom) {
                        //
                        // (?, p, ?)

                        return 1;
                    } else {
                        //
                        // (?, ?, ?)

                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Counts the number of variables in this triple.
        /// </summary>
        /// <param name="triple">The triple instance.</param>
        /// <returns>
        /// The number of variables.
        /// </returns>
        public static int CountVariables(this Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            var c = 0;
            if (triple.S is Variable) {
                c++;
            }
            if (triple.P is Variable) {
                c++;
            }
            if (triple.O is Variable) {
                c++;
            }
            return c;
        }

        /// <summary>
        /// Counts the number of atoms in this triple.
        /// </summary>
        /// <param name="triple">The triple instance.</param>
        /// <returns>
        /// The number of atoms.
        /// </returns>
        public static int CountAtoms(this Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            var c = 0;
            if (triple.S is Atom) {
                c++;
            }
            if (triple.P is Atom) {
                c++;
            }
            if (triple.O is Atom) {
                c++;
            }
            return c;
        }

        /// <summary>
        /// Gets all the individual variables in this triple.
        /// </summary>
        /// <param name="triple">The triple instance.</param>
        /// <returns>
        /// The variables in this triple.
        /// </returns>
        public static IEnumerable<Variable> GetVariables(this Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            if (triple.S is Variable) {
                yield return (Variable)triple.S;
            }
            if (triple.P is Variable) {
                yield return (Variable)triple.P;
            }
            if (triple.O is Variable) {
                yield return (Variable)triple.O;
            }
        }

        /// <summary>
        /// Gets all the individual atoms in this triple.
        /// </summary>
        /// <param name="triple">The triple instance.</param>
        /// <returns>
        /// The atoms in this triple.
        /// </returns>
        public static IEnumerable<Atom> GetAtoms(this Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            if (triple.S is Atom) {
                yield return (Atom)triple.S;
            }
            if (triple.P is Atom) {
                yield return (Atom)triple.P;
            }
            if (triple.O is Atom) {
                yield return (Atom)triple.O;
            }
        }

        /// <summary>
        /// Gets the ordering for the variables in the binding sets produced by a scan operator
        /// fetching data matching this triple.
        /// </summary>
        /// <param name="triple">The triple instance.</param>
        /// <returns>
        /// The ordering of the variables produced by a scan matching this triple.
        /// </returns>
        public static long[] GetScanOrdering(this Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            //
            // no matter what bucket we choose to retrieve data matching some triple, we can always
            // know how the data that comes out is sorted

            if (triple.S is Atom) {
                if (triple.P is Atom) {
                    if (triple.O is Atom) {
                        //
                        // (s, p, o)

                        return new long[] { };
                    } else {
                        //
                        // (s, p, ?)

                        return new long[] { triple.O.InternalValue };
                    }
                } else {
                    if (triple.O is Atom) {
                        //
                        // (s, ?, o)

                        return new long[] { triple.P.InternalValue };
                    } else {
                        //
                        // (s, ?, ?)

                        return new long[] { triple.O.InternalValue, triple.P.InternalValue };
                    }
                }
            } else {
                if (triple.P is Atom) {
                    if (triple.O is Atom) {
                        //
                        // (?, p, o)

                        return new long[] { triple.S.InternalValue };
                    } else {
                        //
                        // (?, p, ?)

                        return new long[] { triple.S.InternalValue, triple.O.InternalValue };
                    }
                } else {
                    if (triple.O is Atom) {
                        //
                        // (?, ?, o)

                        return new long[] { triple.S.InternalValue, triple.P.InternalValue };
                    } else {
                        //
                        // (?, ?, ?)

                        return new long[] { triple.S.InternalValue, triple.O.InternalValue, triple.P.InternalValue };
                    }
                }
            }
        }
    }
}
