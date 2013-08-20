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

    /// <summary>
    /// Represents a rule for making a decision or filtering choices when a seed for a SAP needs to
    /// be selected from an atom collapse.
    /// </summary>
    public abstract class Rule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        protected Rule()
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
        public abstract Node Choose(Database context, IEnumerable<Node> seeds, Graph fullCollapse, IEnumerable<Node> currentCollapse);

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
        public abstract IEnumerable<Node> Filter(Database context, IEnumerable<Node> seeds, Graph fullCollapse, IEnumerable<Node> currentCollapse);

        /// <summary>
        /// Forces the rule to choose a preferred seed position (s, p, or o) for a given SAP.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="sap">The SAP.</param>
        /// <returns>
        /// The preferred seed position.
        /// </returns>
        public abstract TriplePosition ChooseSeedPosition(Database context, Triple<TripleItem, TripleItem, TripleItem> sap);
    }
}
