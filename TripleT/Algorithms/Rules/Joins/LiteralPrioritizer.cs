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

namespace TripleT.Algorithms.Rules.Joins
{
    using System.Collections.Generic;
    using System.Linq;
    using TripleT.Datastructures;
    using TripleT.Datastructures.JoinGraph;

    /// <summary>
    /// Rule aimed at prioritizing joins between two SAPs which feature the highest number of
    /// literal atom values.
    /// </summary>
    public class LiteralPrioritizer : Rule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralPrioritizer"/> class.
        /// </summary>
        public LiteralPrioritizer()
        {
        }

        /// <summary>
        /// Forces the rule to make a choice out of the given options.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="edges">The set of join edges to choose from.</param>
        /// <param name="joinGraph">The current join graph.</param>
        /// <returns>
        /// The chosen edge.
        /// </returns>
        public override Edge Choose(Database context, IEnumerable<Edge> edges, Graph joinGraph)
        {
            var eList = new List<Edge>(Filter(context, edges, joinGraph));
            return eList[0];
        }

        /// <summary>
        /// Filters the best choices out of the given options.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="edges">The set of join edges to choose from.</param>
        /// <param name="joinGraph">The current join graph.</param>
        /// <returns>
        /// The filtered set of edges.
        /// </returns>
        public override IEnumerable<Edge> Filter(Database context, IEnumerable<Edge> edges, Graph joinGraph)
        {
            //
            // first, filter any edges that do not contain an SAP on both the right and the left
            // side.

            var maxSAPs = new List<Triple<TripleItem, TripleItem, TripleItem>>();
            foreach (var edge in edges) {
                if (edge.Left.SAP != null && edge.Right.SAP != null) {
                    if (!maxSAPs.Contains(edge.Left.SAP)) {
                        maxSAPs.Add(edge.Left.SAP);
                    }
                    if (!maxSAPs.Contains(edge.Right.SAP)) {
                        maxSAPs.Add(edge.Right.SAP);
                    }
                }
            }

            //
            // if we have at least one, continue. if not, just yield the input edges as we cannot
            // perform our filtering action.

            if (maxSAPs.Count > 0) {
                //
                // find the SAP with the most literal atom values, and filter everything else

                var maxRank = maxSAPs.Max(t => LiteralCount(t));
                maxSAPs.RemoveAll(t => LiteralCount(t) < maxRank);

                //
                // for every SAP x remaining, find the edge connecting it to another SAP y, such
                // that the literal count of y is maximized.

                var maxJoins = new Dictionary<Triple<TripleItem, TripleItem, TripleItem>, Edge>();
                foreach (var edge in edges) {
                    if (maxSAPs.Contains(edge.Left.SAP)) {
                        var sap = edge.Left.SAP;
                        if (!maxJoins.ContainsKey(sap)) {
                            maxJoins.Add(sap, edge);
                        } else {
                            var prev = maxJoins[sap];
                            if (LiteralCount(sap) > LiteralCount(prev.Right.SAP)) {
                                maxJoins[sap] = edge;
                            }
                        }
                    }

                    if (maxSAPs.Contains(edge.Right.SAP)) {
                        var sap = edge.Right.SAP;
                        if (!maxJoins.ContainsKey(sap)) {
                            maxJoins.Add(sap, edge);
                        } else {
                            var prev = maxJoins[sap];
                            if (LiteralCount(sap) > LiteralCount(prev.Left.SAP)) {
                                maxJoins[sap] = edge;
                            }
                        }
                    }
                }

                //
                // now get the edges connecting SAPs x and y, such that LiteralCount(x) +
                // LiteralCount(y) is maximized

                var candidates = new List<Edge>();
                foreach (var kvPair in maxJoins) {
                    var edge = kvPair.Value;
                    if (!candidates.Contains(edge)) {
                        candidates.Add(edge);
                    }
                }

                //
                // if we still have some remaining at this point, yield them as output. if not,
                // just yield the original edge set.

                if (candidates.Count > 0) {
                    foreach (var edge in candidates) {
                        yield return edge;
                    }
                } else {
                    foreach (var edge in edges) {
                        yield return edge;
                    }
                }
            } else {
                foreach (var edge in edges) {
                    yield return edge;
                }
            }
        }

        /// <summary>
        /// Counts the number of literals appearing in a given SAP. Literals are atomic values that
        /// are not URIs, i.e. do not start with 'http://'.
        /// </summary>
        /// <param name="sap">The SAP.</param>
        /// <returns>
        /// The number of literals in the given SAP.
        /// </returns>
        private static int LiteralCount(Triple<TripleItem, TripleItem, TripleItem> sap)
        {
            if (sap == null) {
                return 0;
            }

            var c = 0;
            if (sap.S is Atom) {
                var sAtom = sap.S as Atom;
                if (!sAtom.TextValue.StartsWith("http://")) {
                    c++;
                }
            }
            if (sap.P is Atom) {
                var pAtom = sap.P as Atom;
                if (!pAtom.TextValue.StartsWith("http://")) {
                    c++;
                }
            }
            if (sap.O is Atom) {
                var oAtom = sap.O as Atom;
                if (!oAtom.TextValue.StartsWith("http://")) {
                    c++;
                }
            }
            return c;
        }
    }
}
