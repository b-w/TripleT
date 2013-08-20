﻿/* TripleT: an RDF database engine.
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

namespace TripleT.Util
{
    using System.Collections.Generic;
    using System.Linq;
    using TripleT.Datastructures;
    using TripleT.Datastructures.Queries;
    using io = TripleT.IO.Operators;
    using qp = TripleT.Datastructures.Queries;

    /// <summary>
    /// Contains extension methods for TripleT operator classes.
    /// </summary>
    public static class OperatorExtensions
    {
        /// <summary>
        /// Computes the sort order for the output generated by this operator.
        /// </summary>
        /// <param name="op">The operator instance.</param>
        /// <returns>
        /// An integer array representing the output sort order.
        /// </returns>
        public static long[] GetOutputSortOrder(this qp::Operator op)
        {
            if (op is qp::Sort) {
                return (op as qp::Sort).SortOrder;
            } else if (op is qp::Scan) {
                var opScan = op as qp::Scan;
                var order = new List<long>();
                switch (opScan.Bucket) {
                    case TriplePosition.None:
                        break;
                    case TriplePosition.S:
                        if (opScan.SelectPattern.SType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.S);
                        }
                        if (opScan.SelectPattern.OType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.O);
                        }
                        if (opScan.SelectPattern.PType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.P);
                        }
                        break;
                    case TriplePosition.P:
                        if (opScan.SelectPattern.PType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.P);
                        }
                        if (opScan.SelectPattern.SType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.S);
                        }
                        if (opScan.SelectPattern.OType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.O);
                        }
                        break;
                    case TriplePosition.O:
                        if (opScan.SelectPattern.OType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.O);
                        }
                        if (opScan.SelectPattern.SType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.S);
                        }
                        if (opScan.SelectPattern.PType == Pattern.ItemType.Variable) {
                            order.Add((long)opScan.SelectPattern.P);
                        }
                        break;
                    default:
                        break;
                }
                return order.ToArray();
            } else if (op is qp::HashJoin) {
                return new long[] { };
            } else if (op is qp::MergeJoin) {
                return (op as qp::MergeJoin).SortOrder;
            } else {
                return new long[] { };
            }
        }

        /// <summary>
        /// Computes the variables present in the output stream (binding sets) generated by this
        /// operator.
        /// </summary>
        /// <param name="op">The operator instance.</param>
        /// <returns>
        /// The variables present in the binding sets outputted by this operator.
        /// </returns>
        public static long[] GetOutputVariables(this qp::Operator op)
        {
            if (op is qp::Sort) {
                return (op as qp::Sort).Input.GetOutputVariables();
            } else if (op is qp::Scan) {
                return (op as qp::Scan).SelectPattern.GetVariables();
            } else if (op is qp::HashJoin) {
                var opHash = op as qp::HashJoin;
                var vars = new HashSet<long>();
                foreach (var v in opHash.Left.GetOutputVariables()) {
                    vars.Add(v);
                }
                foreach (var v in opHash.Right.GetOutputVariables()) {
                    vars.Add(v);
                }
                return vars.ToArray();
            } else if (op is qp::MergeJoin) {
                var opMerge = op as qp::MergeJoin;
                var vars = new HashSet<long>();
                foreach (var v in opMerge.Left.GetOutputVariables()) {
                    vars.Add(v);
                }
                foreach (var v in opMerge.Right.GetOutputVariables()) {
                    vars.Add(v);
                }
                return vars.ToArray();
            } else {
                return new long[] { };
            }
        }

        /// <summary>
        /// Computes the shared join variables of the two input streams for this operator.
        /// </summary>
        /// <param name="opMerge">The operator instance.</param>
        /// <returns>
        /// A set of variables shared by the inputs for this operator.
        /// </returns>
        public static long[] GetJoinVariables(this qp::MergeJoin opMerge)
        {
            var vars = new HashSet<long>(opMerge.Left.GetOutputVariables());
            vars.IntersectWith(opMerge.Right.GetOutputVariables());
            return vars.ToArray();
        }

        /// <summary>
        /// Computes the shared join variables of the two input streams for this operator.
        /// </summary>
        /// <param name="opHash">The operator instance.</param>
        /// <returns>
        /// A set of variables shared by the inputs for this operator.
        /// </returns>
        public static long[] GetJoinVariables(this qp::HashJoin opHash)
        {
            var vars = new HashSet<long>(opHash.Left.GetOutputVariables());
            vars.IntersectWith(opHash.Right.GetOutputVariables());
            return vars.ToArray();
        }
    }
}