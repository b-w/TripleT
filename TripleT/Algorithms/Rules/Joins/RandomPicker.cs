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
    /// This rule simply selects a random option from the given set of choices.
    /// </summary>
    public class RandomPicker : Rule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomPicker"/> class.
        /// </summary>
        public RandomPicker()
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
            //
            // we use the generator from TripleT.Util for random selection, as it keeps a Random()
            // instance alive as long as the program runs, rather than creating a new one on every
            // execution of this rule

            var eList = new List<Edge>(edges);
            var i = Generator.GetRandomNumber(0, eList.Count);
            return eList[i];
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
            // the random picker does not support filtering

            foreach (var edge in edges) {
                yield return edge;
            }
        }
    }
}
