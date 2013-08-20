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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TripleT.Datastructures;
    using TripleT.Datastructures.JoinGraph;

    /// <summary>
    /// This rule aims to prioritize joins between SAPs which have the most selective positioning
    /// of join variables.
    /// </summary>
    public class PositionalPrioritizer : Rule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionalPrioritizer"/> class.
        /// </summary>
        public PositionalPrioritizer()
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
            // find the highest join rank amongst the input set, and filter everything else

            var maxEdges = new List<Edge>(edges);
            var maxJoinRank = maxEdges.Max(e => JoinRank(e));
            maxEdges.RemoveAll(e => JoinRank(e) < maxJoinRank);

            //
            // if we have any options left, yield those. if not, just yield the original edge set.

            if (maxEdges.Count > 0) {
                foreach (var edge in maxEdges) {
                    yield return edge;
                }
            } else {
                foreach (var edge in edges) {
                    yield return edge;
                }
            }
        }

        /// <summary>
        /// Computes the join rank of the given join edge. The join rank is based on the positioning
        /// of the join variables in the two SAPs belonging to the join edge. A higher join rank
        /// denotes higher selectivity.
        /// </summary>
        /// <param name="joinEdge">The join edge.</param>
        /// <returns>
        /// The join rank of the given edge.
        /// </returns>
        private static int JoinRank(Edge joinEdge)
        {
            var sapLeft = joinEdge.Left.SAP;
            var sapRight = joinEdge.Right.SAP;

            //
            // we can only compute the join rank if both the left and right inputs of this edge
            // are SAPs

            if (sapLeft == null || sapRight == null) {
                return -1;
            }

            //
            // the following ranking is used:
            //     s ^ p = 6 (= p ^ s)
            //     o ^ p = 5 (= o ^ p)
            //     s ^ o = 4 (= o ^ s)
            //     s ^ s = 3
            //     o ^ o = 2
            //     p ^ p = 1
            //
            // what follows is a simple branching case distinction to figure out the rank

            var maxRank = -1;
            if (sapLeft.S is Variable) {
                var sLeft = sapLeft.S as Variable;
                if (sapRight.S is Variable) {
                    var sRight = sapRight.S as Variable;
                    if (sLeft.InternalValue == sRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 3);
                    }
                }
                if (sapRight.P is Variable) {
                    var pRight = sapRight.P as Variable;
                    if (sLeft.InternalValue == pRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 6);
                    }
                }
                if (sapRight.O is Variable) {
                    var oRight = sapRight.O as Variable;
                    if (sLeft.InternalValue == oRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 4);
                    }
                }
            }
            if (sapLeft.P is Variable) {
                var pLeft = sapLeft.P as Variable;
                if (sapRight.S is Variable) {
                    var sRight = sapRight.S as Variable;
                    if (pLeft.InternalValue == sRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 6);
                    }
                }
                if (sapRight.P is Variable) {
                    var pRight = sapRight.P as Variable;
                    if (pLeft.InternalValue == pRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 1);
                    }
                }
                if (sapRight.O is Variable) {
                    var oRight = sapRight.O as Variable;
                    if (pLeft.InternalValue == oRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 5);
                    }
                }
            }
            if (sapLeft.O is Variable) {
                var oLeft = sapLeft.O as Variable;
                if (sapRight.S is Variable) {
                    var sRight = sapRight.S as Variable;
                    if (oLeft.InternalValue == sRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 4);
                    }
                }
                if (sapRight.P is Variable) {
                    var pRight = sapRight.P as Variable;
                    if (oLeft.InternalValue == pRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 5);
                    }
                }
                if (sapRight.O is Variable) {
                    var oRight = sapRight.O as Variable;
                    if (oLeft.InternalValue == oRight.InternalValue) {
                        maxRank = Math.Max(maxRank, 2);
                    }
                }
            }

            return maxRank;
        }
    }
}
