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

namespace TripleT.Algorithms.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TripleT.Datastructures;
    using TripleT.Datastructures.Queries;
    using TripleT.Util;
    using ac = TripleT.Datastructures.AtomCollapse;
    using jg = TripleT.Datastructures.JoinGraph;
    using rj = TripleT.Algorithms.Rules.Joins;
    using rs = TripleT.Algorithms.Rules.Seeds;

    /// <summary>
    /// Represents a central point for resolving decision points using configurable, ordered sets
    /// of rules.
    /// </summary>
    public class DecisionEngine
    {
        private readonly Database m_context;
        private readonly List<rs::Rule> m_rulesSeeds;
        private readonly List<rj::Rule> m_rulesJoins;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionEngine"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public DecisionEngine(Database context)
        {
            m_context = context;
            m_rulesSeeds = new List<rs::Rule>();
            m_rulesJoins = new List<rj::Rule>();
        }

        /// <summary>
        /// Gets the (ordered) list of seed rules.
        /// </summary>
        public List<rs::Rule> SeedRules
        {
            get { return m_rulesSeeds; }
        }

        /// <summary>
        /// Gets the (ordered) list of join rules.
        /// </summary>
        public List<rj::Rule> JoinRules
        {
            get { return m_rulesJoins; }
        }

        /// <summary>
        /// Applies the current rule set for selecting a seed node from a given set of options.
        /// </summary>
        /// <param name="seeds">The set of options (seed nodes).</param>
        /// <param name="fullCollapse">The full atom collapse.</param>
        /// <param name="currentCollapse">The current atom collapse.</param>
        /// <returns>
        /// The selected seed node.
        /// </returns>
        public ac::Node ChooseSeed(IEnumerable<ac::Node> seeds, ac::Graph fullCollapse, IEnumerable<ac::Node> currentCollapse)
        {
            //
            // we apply each rule in sequence to filter the set of available candidates until we
            // either 1) have only one option left or 2) arrive at the last rule, which is then
            // forced to make a choice

            var candidates = new List<ac::Node>(seeds);
            for (int i = 0; i < m_rulesSeeds.Count; i++) {

                //
                // if there's only one option left we can return this immediately

                if (candidates.Count == 1) {
                    return candidates[0];
                }

                if (i == m_rulesSeeds.Count - 1) {

                    //
                    // last rule, so force it to choose

                    return m_rulesSeeds[i].Choose(m_context, candidates, fullCollapse, currentCollapse);
                } else {

                    //
                    // perform a filtering step

                    var filtered = new List<ac::Node>(m_rulesSeeds[i].Filter(m_context, candidates, fullCollapse, currentCollapse));
                    candidates = filtered;
                }
            }

            //
            // if, after application of all rules, we still have not arrived at a conclusive choice,
            // we simply select the first option from the candidates we have left

            if (candidates.Count > 0) {
                return candidates[0];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Applies the current rule set for selecting a join edge from a given set of options.
        /// </summary>
        /// <param name="joinGraph">The join graph.</param>
        /// <returns>
        /// The selected join edge.
        /// </returns>
        public jg::Edge ChooseJoinEdge(jg::Graph joinGraph)
        {
            //
            // we apply each rule in sequence to filter the set of available candidates until we
            // either 1) have only one option left or 2) arrive at the last rule, which is then
            // forced to make a choice

            var candidates = new List<jg::Edge>(joinGraph.Edges);
            for (int i = 0; i < m_rulesJoins.Count; i++) {

                //
                // if there's only one option left we can return this immediately

                if (candidates.Count == 1) {
                    return candidates[0];
                }

                if (i == m_rulesJoins.Count - 1) {

                    //
                    // last rule, so force it to choose

                    return m_rulesJoins[i].Choose(m_context, candidates, joinGraph);
                } else {

                    //
                    // perform a filtering step

                    var filtered = new List<jg::Edge>(m_rulesJoins[i].Filter(m_context, candidates, joinGraph));
                    candidates = filtered;
                }
            }

            //
            // if, after application of all rules, we still have not arrived at a conclusive choice,
            // we simply select the first option from the candidates we have left

            if (candidates.Count > 0) {
                return candidates[0];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Applies the current rule set for selecting Scan operator for the given join node.
        /// </summary>
        /// <param name="joinNode">The join node.</param>
        /// <returns>
        /// The selected Scan operator.
        /// </returns>
        public Scan ChooseScan(jg::Node joinNode)
        {
            object s, p, o;

            //
            // the primary seed selection rule gets to make the choice of which seed position to use

            var bucket = m_rulesSeeds[0].ChooseSeedPosition(m_context, joinNode.SAP);

            //
            // variables s, p, and o can be either text values if they are atoms (or literal
            // values), or integers if they are variables

            if (joinNode.SAP.S is Atom) {
                s = (joinNode.SAP.S as Atom).TextValue;
            } else {
                s = joinNode.SAP.S.InternalValue;
            }

            if (joinNode.SAP.P is Atom) {
                p = (joinNode.SAP.P as Atom).TextValue;
            } else {
                p = joinNode.SAP.P.InternalValue;
            }

            if (joinNode.SAP.O is Atom) {
                o = (joinNode.SAP.O as Atom).TextValue;
            } else {
                o = joinNode.SAP.O.InternalValue;
            }

            //
            // the start atom is equal to the literal value of the atom in the seed position

            var startAtom = String.Empty;
            switch (bucket) {
                case TriplePosition.None:
                    startAtom = String.Empty;
                    break;
                case TriplePosition.S:
                    if (joinNode.SAP.S is Atom) {
                        startAtom = (joinNode.SAP.S as Atom).TextValue;
                    }
                    break;
                case TriplePosition.P:
                    if (joinNode.SAP.P is Atom) {
                        startAtom = (joinNode.SAP.P as Atom).TextValue;
                    }
                    break;
                case TriplePosition.O:
                    if (joinNode.SAP.O is Atom) {
                        startAtom = (joinNode.SAP.O as Atom).TextValue;
                    }
                    break;
                default:
                    startAtom = String.Empty;
                    break;
            }

            //
            // the scan operator is then constructed in the Datastructures.Queries.QueryPlan class

            return QueryPlan.GetScan(bucket, s, p, o, startAtom);
        }

        /// <summary>
        /// Selects a join type (merge or hash) for the given join edge, and returns a join node
        /// which merges the left and right nodes of the given join edge.
        /// </summary>
        /// <param name="joinEdge">The join edge.</param>
        /// <returns>
        /// The join node merging the left and right nodes from the given join edge.
        /// </returns>
        public jg::Node ChooseJoinType(jg::Edge joinEdge)
        {
            //
            // get the operators for the left and right nodes. if a node does not have an operator,
            // then it must have an SAP and we select a scan operator for it here.

            Operator leftOp, rightOp;

            if (joinEdge.Left.HasOperator) {
                leftOp = joinEdge.Left.Operator;
            } else {
                leftOp = ChooseScan(joinEdge.Left);
            }

            if (joinEdge.Right.HasOperator) {
                rightOp = joinEdge.Right.Operator;
            } else {
                rightOp = ChooseScan(joinEdge.Right);
            }

            //
            // create a new, empty join node

            var joinNode = new jg::Node(null, null);

            if (CanDoMergeJoin(leftOp, rightOp)) {

                //
                // heuristic: if we can do a merge join, do so

                var leftSortOrder = leftOp.GetOutputSortOrder();
                var rightSortOrder = rightOp.GetOutputSortOrder();
                var joinVars = new HashSet<long>(leftSortOrder);
                joinVars.IntersectWith(rightSortOrder);

                //
                // the merge joinoperator is then constructed in the Datastructures.Queries.QueryPlan
                // class.

                joinNode.Operator = QueryPlan.GetMergeJoin(leftOp, rightOp, joinVars.ToArray());
            } else {

                //
                // if we cannot do a merge join, do a hash join instead. the hash join operator is
                // then constructed in the Datastructures.Queries.QueryPlan class.

                joinNode.Operator = QueryPlan.GetHashJoin(leftOp, rightOp);
            }

            return joinNode;
        }

        /// <summary>
        /// Determines whether it is possible to do a merge join between the outputs of two given
        /// operators.
        /// </summary>
        /// <param name="left">The left operator.</param>
        /// <param name="right">The right operator.</param>
        /// <returns>
        ///   <c>true</c> if it is possible to do a merge join between the two operators; otherwise, <c>false</c>.
        /// </returns>
        internal static bool CanDoMergeJoin(Operator left, Operator right)
        {
            //
            // we get the sort order in which each operator produces its output, and then see if
            // this order allows for a merge join

            var leftSortOrder = left.GetOutputSortOrder();
            var rightSortOrder = right.GetOutputSortOrder();
            return CanDoMergeJoin(leftSortOrder, rightSortOrder);
        }

        /// <summary>
        /// Determines whether it is possible to do a merge join between two streams of bindings
        /// sorted in a particular way.
        /// </summary>
        /// <param name="leftSortOrder">The left sort order.</param>
        /// <param name="rightSortOrder">The right sort order.</param>
        /// <returns>
        ///   <c>true</c> if it is possible to do a merge join given the provided sort orders; otherwise, <c>false</c>.
        /// </returns>
        internal static bool CanDoMergeJoin(long[] leftSortOrder, long[] rightSortOrder)
        {
            //
            // we do a set intersection to determine the shared join variables

            var joinVars = new HashSet<long>(leftSortOrder);
            joinVars.IntersectWith(rightSortOrder);

            //
            // we can do a merge join if the following conditions are met:
            //     1. there is at least one join variable
            //     2. given that there are n join variables, we have for all 0 < i <= n, the left
            //        i-th variable is equal to the right i-th variable

            var canMerge = (joinVars.Count > 0);
            for (int i = 0; i < joinVars.Count; i++) {
                if (!(leftSortOrder[i] == rightSortOrder[i] && joinVars.Contains(leftSortOrder[i]))) {
                    canMerge = false;
                }
            }

            return canMerge;
        }
    }
}
