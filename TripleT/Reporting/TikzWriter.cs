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

namespace TripleT.Reporting
{
    using System;
    using System.IO;
    using System.Text;
    using TripleT.Datastructures.Queries;

    public static class TikzWriter
    {
        public static void Write(QueryPlan plan, string filename, bool includeMetrics = false)
        {
            using (var sw = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))) {
                sw.WriteLine(@"\begin{tikzpicture}[");
                sw.WriteLine(@"every node/.style={rectangle,draw},");
                sw.WriteLine(@"every label/.style={draw=none,font=\scriptsize},");
                sw.WriteLine(@"level distance=25mm,");
                sw.WriteLine(@"align=center");
                sw.WriteLine(@"]");
                sw.WriteLine(@"\Tree [. \node[draw=none]{};");
                WriteNode(sw, plan.Root, includeMetrics, true);
                sw.WriteLine(@"]");
                sw.WriteLine(@"\end{tikzpicture}");
            }
        }

        private static void WriteNode(StreamWriter sw, Operator oper, bool includeMetrics, bool edgeLabelRight)
        {
            if (includeMetrics) {
                sw.Write(@"\edge node[auto=");
                if (edgeLabelRight) {
                    sw.Write(@"right");
                } else {
                    sw.Write(@"left");
                }
                sw.Write(@",draw=none,font=\scriptsize] {");
#if DEBUG
                sw.Write(oper.ResultCount);
#endif
                sw.Write(@"};");
            }
            sw.Write(@"[.\node[drop shadow,top color=white");
            if (includeMetrics) {
                sw.Write(@",");
                sw.Write(GetLabelText(oper));
            }
            sw.Write(@"]");
            sw.Write(GetNodeText(oper));
            sw.Write(@";");
            sw.WriteLine();
            if (oper is Scan) {
                // no child nodes to write
            } else if (oper is MergeJoin) {
                var oMerge = oper as MergeJoin;

                WriteNode(sw, oMerge.Left, includeMetrics, true);
                WriteNode(sw, oMerge.Right, includeMetrics, false);
            } else if (oper is HashJoin) {
                var oHash = oper as HashJoin;

                WriteNode(sw, oHash.Left, includeMetrics, true);
                WriteNode(sw, oHash.Right, includeMetrics, false);
            } else if (oper is Sort) {
                var oSort = oper as Sort;

                WriteNode(sw, oSort.Input, includeMetrics, true);
            }
            sw.Write(@"]");
        }

        private static string GetLabelText(Operator oper)
        {
#if DEBUG
            if (oper is Scan) {
                // place label below
                return String.Format("label=below:{{CPU {0}ms\\\\IO {1}ms}}", oper.CPUTime, oper.IOTime);
            } else {
                // place label to the right
                return String.Format("label=right:{{CPU {0}ms\\\\IO {1}ms}}", oper.CPUTime, oper.IOTime);
            }
#else
            return String.Empty;
#endif
        }

        private static string GetNodeText(Operator oper)
        {
            var nodeTxt = new StringBuilder();
            nodeTxt.Append(@"{");
            if (oper is Scan) {
                var oScan = oper as Scan;

                switch (oScan.Bucket) {
                    case TripleT.Datastructures.TriplePosition.None:
                        break;
                    case TripleT.Datastructures.TriplePosition.S:
                        nodeTxt.Append("S-");
                        break;
                    case TripleT.Datastructures.TriplePosition.P:
                        nodeTxt.Append("P-");
                        break;
                    case TripleT.Datastructures.TriplePosition.O:
                        nodeTxt.Append("O-");
                        break;
                    default:
                        break;
                }
                nodeTxt.Append("Scan\\\\");

                nodeTxt.Append("$ (");
                if (oScan.SelectPattern.SType == Pattern.ItemType.Variable) {
                    nodeTxt.Append("?");
                    nodeTxt.Append((long)oScan.SelectPattern.S);
                } else {
                    nodeTxt.Append("s");
                }
                nodeTxt.Append(", ");
                if (oScan.SelectPattern.PType == Pattern.ItemType.Variable) {
                    nodeTxt.Append("?");
                    nodeTxt.Append((long)oScan.SelectPattern.P);
                } else {
                    nodeTxt.Append("p");
                }
                nodeTxt.Append(", ");
                if (oScan.SelectPattern.OType == Pattern.ItemType.Variable) {
                    nodeTxt.Append("?");
                    nodeTxt.Append((long)oScan.SelectPattern.O);
                } else {
                    nodeTxt.Append("o");
                }
                nodeTxt.Append(") $");
            } else if (oper is MergeJoin) {
                var oMerge = oper as MergeJoin;

                nodeTxt.Append("Merge Join\\\\");
                nodeTxt.Append("$ (");
                for (int i = 0; i < oMerge.SortOrder.Length; i++) {
                    nodeTxt.Append("?");
                    nodeTxt.Append(oMerge.SortOrder[i]);
                    if (i < oMerge.SortOrder.Length - 1) {
                        nodeTxt.Append(", ");
                    }
                }
                nodeTxt.Append(") $");
            } else if (oper is HashJoin) {
                var oHash = oper as HashJoin;

                nodeTxt.Append("Hash Join\\\\");
            } else if (oper is Sort) {
                var oSort = oper as Sort;

                nodeTxt.Append("Sort\\\\");
                nodeTxt.Append("$ (");
                for (int i = 0; i < oSort.SortOrder.Length; i++) {
                    nodeTxt.Append("?");
                    nodeTxt.Append(oSort.SortOrder[i]);
                    if (i < oSort.SortOrder.Length) {
                        nodeTxt.Append(", ");
                    }
                }
                nodeTxt.Append(") $");
            }
            nodeTxt.Append(@"}");
            return nodeTxt.ToString();
        }
    }
}
