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
    using TripleT.Datastructures.JoinGraph;
    using TripleT.Util;

    /// <summary>
    /// Rule aimed at prioritizing joins where a merge join is possible.
    /// </summary>
    public class MergeJoinPrioritizer : Rule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeJoinPrioritizer"/> class.
        /// </summary>
        public MergeJoinPrioritizer()
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
            // in the filtering step, we simple get rid of all join edges where a merge join is
            // not a possibility.

            var joinEdges = new List<Edge>(edges);
            joinEdges.RemoveAll(e => !CanDoMergeJoin(e));

            //
            // if we have any options left, yield those. if not, just yield the original edge set.

            if (joinEdges.Count > 0) {
                foreach (var edge in joinEdges) {
                    yield return edge;
                }
            } else {
                foreach (var edge in edges) {
                    yield return edge;
                }
            }
        }

        /// <summary>
        /// Determines whether a merge join is possible for the specified edge.
        /// </summary>
        /// <param name="edge">The join edge.</param>
        /// <returns>
        ///   <c>true</c> if a merge join is possible for the specified edge; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDoMergeJoin(Edge edge)
        {
            //
            // first, determine the output sort order for the left and right inputs of the join
            // edge, if any exists to begin with.

            long[] orderLeft;
            long[] orderRight;

            if (edge.Left.HasOperator) {
                orderLeft = edge.Left.Operator.GetOutputSortOrder();
            } else {
                orderLeft = edge.Left.SAP.GetScanOrdering();
            }

            if (edge.Right.HasOperator) {
                orderRight = edge.Right.Operator.GetOutputSortOrder();
            } else {
                orderRight = edge.Right.SAP.GetScanOrdering();
            }

            //
            // if this ordering allows us to do a merge join, return true. otherwise, return false.

            return DecisionEngine.CanDoMergeJoin(orderLeft, orderRight);
        }
    }
}
