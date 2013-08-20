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

namespace TripleT.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TripleT.Algorithms.Rules;
    using TripleT.Datastructures;
    using TripleT.Datastructures.Queries;
    using TripleT.Util;
    using ac = TripleT.Datastructures.AtomCollapse;
    using jg = TripleT.Datastructures.JoinGraph;
    using rj = TripleT.Algorithms.Rules.Joins;
    using rs = TripleT.Algorithms.Rules.Seeds;

    /// <summary>
    /// Static class containing the algorithms used for query plan generation.
    /// </summary>
    public static class PlanGenerator
    {
        /// <summary>
        /// Computes a descriptive query plan for a given Basic Graph Pattern within a given
        /// database context.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="pattern">The Basic Graph Pattern.</param>
        /// <returns>
        /// A descriptive query plan for the given BGP.
        /// </returns>
        public static QueryPlan ComputePlan(Database context, params Triple<TripleItem, TripleItem, TripleItem>[] pattern)
        {
            //
            // initialize the decision engine
            var decisionEngine = GetDecisionEngine(context);

            //
            // compute the atom collapse
            var collapse = AtomCollapse.Compute(pattern);

            //
            // step 1: compute join graph

            //
            // working copy of collapse graph nodes
            var currentCollapse = new List<ac::Node>();
            foreach (var node in collapse.Nodes) {
                currentCollapse.Add(node.Copy());
            }

            List<ac::Node> seeds;
            var todo = new List<Triple<TripleItem, TripleItem, TripleItem>>();
            var eval = new List<ac::Node>();

            do {
                //
                // compute the seeds
                seeds = ComputeSeeds(currentCollapse);

                //
                // pick one
                var seed = decisionEngine.ChooseSeed(seeds, collapse, currentCollapse).Copy();

                //
                // compute new todo's
                if (todo.Contains(seed.FirstSAP)) {
                    todo.Remove(seed.FirstSAP);
                }
                foreach (var sap in ComputeNewTodos(seed, currentCollapse)) {
                    todo.AddIfNotPresent(sap);
                }

                //
                // update the current collapse
                foreach (var node in currentCollapse) {
                    if (node.SAPs.Contains(seed.FirstSAP)) {
                        node.SAPs.Remove(seed.FirstSAP);
                    }
                }
                currentCollapse.RemoveAll(n => n.SAPs.Count == 0);

                //
                // add the seed to the evaluation list
                eval.Add(seed);
            } while (todo.Count > 0);

            //
            // compute edges
            var collapseEdges = new List<ac::Edge>();
            foreach (var edge in collapse.Edges) {
                foreach (var x in eval) {
                    foreach (var y in eval) {
                        if (edge.Left.Equals(x) && edge.Right.Equals(y)) {
                            collapseEdges.Add(edge);
                        }
                    }
                }
            }

            //
            // create join graph nodes
            var joinNodes = new List<jg::Node>();
            foreach (var cNode in eval) {
                var jNode = new jg::Node(cNode.Item, cNode.FirstSAP);
                joinNodes.Add(jNode);
            }

            //
            // create join graph edges
            var joinEdges = new List<jg::Edge>();
            foreach (var cEdge in collapseEdges) {
                foreach (var x in joinNodes) {
                    foreach (var y in joinNodes) {
                        if (cEdge.Left.Item.Equals(x.Item) && cEdge.Left.FirstSAP.Equals(x.SAP)) {
                            if (cEdge.Right.Item.Equals(y.Item) && cEdge.Right.FirstSAP.Equals(y.SAP)) {
                                var jEdge = new jg::Edge(x, y);
                                foreach (var lbl in cEdge.Labels) {
                                    jEdge.Labels.Add(lbl);
                                }
                                joinEdges.Add(jEdge);
                            }
                        }
                    }
                }
            }

            //
            // make the join graph
            var joinGraph = new jg::Graph(joinNodes, joinEdges);

            //
            // step 2: compute query plan

            if (joinGraph.Edges.Count == 0) {
                var n = joinGraph.Nodes.ElementAt(0);
                n.Operator = decisionEngine.ChooseScan(n);
            } else {
                while (joinGraph.Nodes.Count > 1) {
                    //
                    // choose a join to perform
                    var joinEdge = decisionEngine.ChooseJoinEdge(joinGraph);

                    //
                    // compute the join node for this edge
                    var newNode = decisionEngine.ChooseJoinType(joinEdge);

                    //
                    // merge the nodes of the join edge
                    MergeJoinNodes(joinGraph, joinEdge, newNode);
                }
            }

            //
            // create plan object
            var plan = new QueryPlan(joinGraph.Nodes.ElementAt(0).Operator);

            //
            // assign memory to operators
            AssignMemory(context, plan.Root);

            //
            // return the query plan
            return plan;
        }

        /// <summary>
        /// Computes a list of possible seed nodes for the given atom collapse graph.
        /// </summary>
        /// <param name="currentCollapse">The current atom collapse graph.</param>
        /// <returns>
        /// A list of atom collapse nodes that are seed nodes in this graph.
        /// </returns>
        private static List<ac::Node> ComputeSeeds(IEnumerable<ac::Node> currentCollapse)
        {
            //
            // seed nodes are simply nodes which have an atom (as opposed to a variable) as their
            // representing item

            var seeds = new List<ac::Node>();
            foreach (var node in currentCollapse) {
                if (node.Item is Atom) {
                    seeds.Add(node);
                }
            }
            return seeds;
        }

        /// <summary>
        /// Computes a new set of SAPs which can be reached from a given seed node.
        /// </summary>
        /// <param name="seed">The seed node.</param>
        /// <param name="currentCollapse">The nodes of current collapse graph.</param>
        /// <returns>
        /// A list of SAPs which can be reach from the given seed node. 
        /// </returns>
        private static List<Triple<TripleItem, TripleItem, TripleItem>> ComputeNewTodos(ac::Node seed, IEnumerable<ac::Node> currentCollapse)
        {
            //
            // new TODOs are simply other nodes from the collapse graph which share a variable with
            // the given seed node

            var todo = new List<Triple<TripleItem, TripleItem, TripleItem>>();
            foreach (var node in currentCollapse) {
                if (node.Item is Variable && node.SAPs.Contains(seed.FirstSAP)) {
                    foreach (var sap in node.SAPs) {
                        if (!sap.Equals(seed.FirstSAP)) {
                            todo.Add(sap);
                        }
                    }
                }
            }
            return todo;
        }

        /// <summary>
        /// Merges a pair of given join nodes, causing all edges connecting to either of the two
        /// join nodes to connect to the newly formed node which merges the two.
        /// </summary>
        /// <param name="joinGraph">The join graph.</param>
        /// <param name="oldEdge">The old edge which is to be replaced.</param>
        /// <param name="newNode">The new node which takes merges the two nodes belonging to the given edge.</param>
        private static void MergeJoinNodes(jg::Graph joinGraph, jg::Edge oldEdge, jg::Node newNode)
        {
            //
            // remove the old edge and its nodes from the join graph

            joinGraph.Edges.Remove(oldEdge);
            joinGraph.Nodes.Remove(oldEdge.Left);
            joinGraph.Nodes.Remove(oldEdge.Right);

            //
            // any edges connecting to the removed nodes will be redirected to the new node that
            // merges the two

            foreach (var edge in joinGraph.Edges) {
                if (edge.Left.Equals(oldEdge.Left) || edge.Left.Equals(oldEdge.Right)) {
                    edge.Left = newNode;
                }

                if (edge.Right.Equals(oldEdge.Left) || edge.Right.Equals(oldEdge.Right)) {
                    edge.Right = newNode;
                }
            }

            //
            // we might end up with duplicate edges pointing to the new node, so make sure to
            // filter those out

            var uniqueEdges = joinGraph.Edges.Distinct().ToList();
            joinGraph.Edges.Clear();
            joinGraph.Edges.AddRange(uniqueEdges);

            //
            // insert the new node into the join graph

            joinGraph.Nodes.Add(newNode);
        }

        /// <summary>
        /// Distributes the available database memory over the operators in a query plan, starting
        /// from the given operator as the plan root.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="oper">The root operator.</param>
        private static void AssignMemory(Database context, Operator oper)
        {
            //
            // the heuristic we follow here is simply that operators get assigned memory based on
            // their average distance to a leaf (scan operator). operators which are closer to the
            // leaves of the query plan get a greater share of memory because they are more likely
            // to need it.

            var dList = new List<Tuple<float, Operator>>();
            GetAVGDistanceFromLeaf(oper, dList);

            var s = dList.Sum(t => 1F / t.Item1);
            foreach (var t in dList) {
                var d = t.Item1;
                var o = t.Item2;
                var f = (1F / d) / s;
                var mLong = Convert.ToInt64(Database.MEMORY_SIZE_TOTAL * f);
                if (mLong > UInt32.MaxValue) {
                    mLong = UInt32.MaxValue;
                }
                var m = Convert.ToUInt32(mLong);
                o.MemorySize = m;
            }
        }

        /// <summary>
        /// Computes the average distance from a given operator to a leaf in the query plan the
        /// operator is a part of.
        /// </summary>
        /// <param name="oper">The operator.</param>
        /// <param name="distList">The list containing distances which gets recursively populated.</param>
        /// <returns>
        /// The average distance from the given operator to a leaf in the query plan.
        /// </returns>
        private static float GetAVGDistanceFromLeaf(Operator oper, List<Tuple<float, Operator>> distList)
        {
            if (oper is Scan) {
                //
                // no distance, and not required for memory assignment

                return 0F;
            } else if (oper is Sort) {
                //
                // distance simply increases by one

                var qSort = (Sort)oper;
                var d = 1F + GetAVGDistanceFromLeaf(qSort.Input, distList);
                distList.Add(Tuple.Create(d, oper));
                return d;
            } else if (oper is MergeJoin) {
                //
                // distance is the average of the distance for the left input node and the distance
                // for the right input node

                var qMerge = (MergeJoin)oper;
                var dLeft = 1F + GetAVGDistanceFromLeaf(qMerge.Left, distList);
                var dRight = 1F + GetAVGDistanceFromLeaf(qMerge.Right, distList);
                var d = (dLeft + dRight) / 2F;
                distList.Add(Tuple.Create(d, oper));
                return d;
            } else if (oper is HashJoin) {
                //
                // distance is the average of the distance for the left input node and the distance
                // for the right input node

                var qHash = (HashJoin)oper;
                var dLeft = 1F + GetAVGDistanceFromLeaf(qHash.Left, distList);
                var dRight = 1F + GetAVGDistanceFromLeaf(qHash.Right, distList);
                var d = (dLeft + dRight) / 2F;
                distList.Add(Tuple.Create(d, oper));
                return d;
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Instantiates the decision engine used during query plan generation for decision point
        /// resolving.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>
        /// An instantiated decision engine.
        /// </returns>
        private static DecisionEngine GetDecisionEngine(Database context)
        {
            //
            // any rules that are used are hardcoded here

            var de = new DecisionEngine(context);

            //
            // load seed rules

            de.SeedRules.Add(new rs::PositionalPrioritizer());      // Rule 1
            //de.SeedRules.Add(new rs::StatisticalPrioritizer());     // Rule 2
            de.SeedRules.Add(new rs::RandomPicker());

            //
            // load join rules

            de.JoinRules.Add(new rj::PatternPrioritizer());         // Rule 2
            de.JoinRules.Add(new rj::MergeJoinPrioritizer());       // Rule 1
            de.JoinRules.Add(new rj::LiteralPrioritizer());         // Rule 4
            de.JoinRules.Add(new rj::PositionalPrioritizer());      // Rule 3
            //de.JoinRules.Add(new rj::StatisticalPrioritizer());     // Rule 5
            de.JoinRules.Add(new rj::RandomPicker());

            return de;
        }
    }
}
