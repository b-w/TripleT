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
    using TripleT.Datastructures.Queries;

    /// <summary>
    /// Contains extension methods for the <see cref="TripleT.Datastructures.Queries.Pattern"/>
    /// class.
    /// </summary>
    public static class PatternExtensions
    {
        /// <summary>
        /// Computes the selectivity ranking for this pattern, where a lower rank value indicates a
        /// lower degree of selectivity.
        /// </summary>
        /// <param name="pattern">The pattern instance.</param>
        /// <returns>
        /// An integer indicating the selectivity ranking for this pattern.
        /// </returns>
        public static int GetSelectivityRanking(this Pattern pattern)
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

            if (pattern.SType == Pattern.ItemType.Atom) {
                if (pattern.OType == Pattern.ItemType.Atom) {
                    if (pattern.PType == Pattern.ItemType.Atom) {
                        //
                        // (s, p, o)

                        return 7;
                    } else {
                        //
                        // (s, ?, o)

                        return 6;
                    }
                } else {
                    if (pattern.PType == Pattern.ItemType.Atom) {
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
                if (pattern.OType == Pattern.ItemType.Atom) {
                    if (pattern.PType == Pattern.ItemType.Atom) {
                        //
                        // (?, p, o)

                        return 5;
                    } else {
                        //
                        // (?, ?, o)

                        return 3;
                    }
                } else {
                    if (pattern.PType == Pattern.ItemType.Atom) {
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
        /// Gets all the individual variables present in this SAP.
        /// </summary>
        /// <param name="pattern">The pattern instance.</param>
        /// <returns>
        /// An array of variables.
        /// </returns>
        public static long[] GetVariables(this Pattern pattern)
        {
            var vars = new List<long>();
            if (pattern.SType == Pattern.ItemType.Variable) {
                vars.Add((long)pattern.S);
            }
            if (pattern.OType == Pattern.ItemType.Variable) {
                vars.Add((long)pattern.O);
            }
            if (pattern.PType == Pattern.ItemType.Variable) {
                vars.Add((long)pattern.P);
            }
            return vars.ToArray();
        }

        /// <summary>
        /// Gets all the individual atoms present in this SAP.
        /// </summary>
        /// <param name="pattern">The pattern instance.</param>
        /// <returns>
        /// An array of atoms.
        /// </returns>
        public static string[] GetAtoms(this Pattern pattern)
        {
            var atoms = new List<string>();
            if (pattern.SType == Pattern.ItemType.Atom) {
                atoms.Add((string)pattern.S);
            }
            if (pattern.OType == Pattern.ItemType.Atom) {
                atoms.Add((string)pattern.O);
            }
            if (pattern.PType == Pattern.ItemType.Atom) {
                atoms.Add((string)pattern.P);
            }
            return atoms.ToArray();
        }
    }
}
