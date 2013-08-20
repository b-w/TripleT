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
    using TripleT.Datastructures.JoinGraph;

    /// <summary>
    /// This rule aims to prioritize joins which have the smallest expected output size, as
    /// computed using available dataset statistics.
    /// </summary>
    public class StatisticalPrioritizer : Rule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticalPrioritizer"/> class.
        /// </summary>
        public StatisticalPrioritizer()
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
            if (edges.Count() > 0) {
                //
                // find the smallest estimate, as computed by the database statistics set, and
                // discard edges with higher estimates.

                var minEst = edges.Min(e => context.Statistics.GetEstimate(e));
                var bestEdges = edges.Where(e => context.Statistics.GetEstimate(e) <= minEst);

                //
                // if we have any options left, yield those. if not, just yield the original edge set.

                if (bestEdges.Count() > 0) {
                    return bestEdges;
                } else {
                    return edges;
                }
            } else {
                return edges;
            }
        }
    }
}
