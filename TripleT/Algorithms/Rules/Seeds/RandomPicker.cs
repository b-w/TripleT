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

namespace TripleT.Algorithms.Rules.Seeds
{
    using System.Collections.Generic;
    using TripleT.Datastructures;
    using TripleT.Datastructures.AtomCollapse;
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
        /// <param name="seeds">The set of seeds to choose from.</param>
        /// <param name="fullCollapse">The full atom collapse.</param>
        /// <param name="currentCollapse">The current (partial) atom collapse.</param>
        /// <returns>
        /// The chosen seed node.
        /// </returns>
        public override Node Choose(Database context, IEnumerable<Node> seeds, Graph fullCollapse, IEnumerable<Node> currentCollapse)
        {
            //
            // we use the generator from TripleT.Util for random selection, as it keeps a Random()
            // instance alive as long as the program runs, rather than creating a new one on every
            // execution of this rule

            var sList = new List<Node>(seeds);
            var i = Generator.GetRandomNumber(0, sList.Count);
            return sList[i];
        }

        /// <summary>
        /// Filters the best choices out of the given options.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="seeds">The set of seeds to choose from.</param>
        /// <param name="fullCollapse">The full atom collapse.</param>
        /// <param name="currentCollapse">The current (partial) atom collapse.</param>
        /// <returns>
        /// The filtered set of seed nodes.
        /// </returns>
        public override IEnumerable<Node> Filter(Database context, IEnumerable<Node> seeds, Graph fullCollapse, IEnumerable<Node> currentCollapse)
        {
            //
            // the random picker does not support filtering

            foreach (var seed in seeds) {
                yield return seed;
            }
        }

        /// <summary>
        /// Forces the rule to choose a preferred seed position (s, p, or o) for a given SAP.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="sap">The SAP.</param>
        /// <returns>
        /// The preferred seed position.
        /// </returns>
        public override TriplePosition ChooseSeedPosition(Database context, Datastructures.Triple<TripleItem, TripleItem, TripleItem> sap)
        {
            //
            // the random picker always prefers S. this might not look random from the outsize, but
            // the problem with random is that you can never be sure...

            return TriplePosition.S;
        }
    }
}
