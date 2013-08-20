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
    using System;
    using System.Collections.Generic;
    using TripleT.Datastructures;
    using TripleT.Datastructures.AtomCollapse;

    /// <summary>
    /// This rule aims to select a single seed for each SAP in the input set, based on which
    /// position (s, p, or o) of the seeds within the SAPs has the smallest output size, as computed
    /// by the available dataset statistics.
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
        /// <param name="seeds">The set of seeds to choose from.</param>
        /// <param name="fullCollapse">The full atom collapse.</param>
        /// <param name="currentCollapse">The current (partial) atom collapse.</param>
        /// <returns>
        /// The chosen seed node.
        /// </returns>
        public override Node Choose(Database context, IEnumerable<Node> seeds, Graph fullCollapse, IEnumerable<Node> currentCollapse)
        {
            var sList = new List<Node>(Filter(context, seeds, fullCollapse, currentCollapse));
            return sList[0];
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
            // we'll compute a dictionary with an entry for each SAP containing the best seed
            // node for this SAP

            var dict = new Dictionary<Triple<TripleItem, TripleItem, TripleItem>, Node>();
            foreach (var seed in seeds) {
                //
                // if the SAP already exists in the dictionary, we need to compare the current
                // seed node to the one in the dictionary, and see if we need to replace it. if not,
                // we can simply put the current node in the dictionary right away.

                if (dict.ContainsKey(seed.FirstSAP)) {
                    //
                    // get the current node from the dictionary, find the position (s, p, or o) of
                    // the seed, and compute the output size estimation according to dataset
                    // statistics

                    var currentBest = dict[seed.FirstSAP];
                    var currentBestPos = TriplePosition.None;
                    if (currentBest.FirstSAP.S == currentBest.Item) {
                        currentBestPos = TriplePosition.S;
                    } else if (currentBest.FirstSAP.O == currentBest.Item) {
                        currentBestPos = TriplePosition.O;
                    } else if (currentBest.FirstSAP.P == currentBest.Item) {
                        currentBestPos = TriplePosition.P;
                    }
                    var currentBestScore = context.Statistics.GetRecordCount(currentBest.Item as Atom, currentBestPos);

                    //
                    // find the position (s, p, or o) for the candidate seed, and compute its
                    // output size estimation according to dataset statistics

                    var seedPos = TriplePosition.None;
                    if (seed.FirstSAP.S == seed.Item) {
                        seedPos = TriplePosition.S;
                    } else if (seed.FirstSAP.O == seed.Item) {
                        seedPos = TriplePosition.O;
                    } else if (seed.FirstSAP.P == seed.Item) {
                        seedPos = TriplePosition.P;
                    }
                    var seedScore = context.Statistics.GetRecordCount(seed.Item as Atom, seedPos);

                    //
                    // if the candidate has a smaller estimated output size, then replace the seed
                    // currently in the dictionary

                    if (seedScore < currentBestScore) {
                        dict[seed.FirstSAP] = seed;
                    }
                } else {
                    dict.Add(seed.FirstSAP, seed);
                }
            }

            //
            // yield the optimal seed nodes that we've found for each SAP

            foreach (var kvPair in dict) {
                yield return kvPair.Value;
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
        public override TriplePosition ChooseSeedPosition(Database context, Triple<TripleItem, TripleItem, TripleItem> sap)
        {
            //
            // we select the seed which produces the smallest expected output size, as computed by
            // available database statistics

            var best = Int64.MaxValue;
            var bestPos = TriplePosition.None;

            if (sap.S is Atom) {
                var score = context.Statistics.GetRecordCount(sap.S as Atom, TriplePosition.S);
                if (score < best) {
                    best = score;
                    bestPos = TriplePosition.S;
                }
            }

            if (sap.P is Atom) {
                var score = context.Statistics.GetRecordCount(sap.P as Atom, TriplePosition.P);
                if (score < best) {
                    best = score;
                    bestPos = TriplePosition.P;
                }
            }

            if (sap.O is Atom) {
                var score = context.Statistics.GetRecordCount(sap.O as Atom, TriplePosition.O);
                if (score < best) {
                    best = score;
                    bestPos = TriplePosition.O;
                }
            }

            if (bestPos == TriplePosition.None) {

                //
                // edge case for all-variable SAPs

                bestPos = TriplePosition.S;
            }

            return bestPos;
        }
    }
}
