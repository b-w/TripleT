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
    using TripleT.Datastructures;
    using TripleT.Datastructures.AtomCollapse;
    using TripleT.Util;

    /// <summary>
    /// Static class representing the algorithm for computing an atom collapse of a given BGP.
    /// </summary>
    public static class AtomCollapse
    {
        /// <summary>
        /// Computes the atom collapse for a given BGP.
        /// </summary>
        /// <param name="pattern">The Basic Graph Pattern (BGP).</param>
        /// <returns>
        /// The atom collapse graph.
        /// </returns>
        public static Graph Compute(params Triple<TripleItem, TripleItem, TripleItem>[] pattern)
        {
            //
            // Step 1:
            //     Let NODES be a new, empty set. For each SAP S in the BGP P, let A be the set
            //     of unique atoms and V be the set of unique variables in S. then for every atom
            //     a in A we add (a, {S}) to NODES. for every variable v in V we add S to S' of node
            //     (v, S') in NODES (if there is no (v, S'), we add (v, {S} to NODES)

            var nodes = new List<Node>();

            var tmpVarList = new Dictionary<TripleItem, Node>();
            TripleItem[] atoms;
            TripleItem[] vars;
            for (int i = 0; i < pattern.Length; i++) {
                GetUniqueItems(pattern[i]).Decompose(out atoms, out vars);
                if (atoms.Length > 0) {
                    for (int j = 0; j < atoms.Length; j++) {
                        var n = new Node(atoms[j]);
                        n.SAPs.Add(pattern[i]);
                        nodes.Add(n);
                    }
                } else {

                    //
                    // edge case for all-variable SAPs

                    var a = new Atom(-1);
                    var n = new Node(a);
                    n.SAPs.Add(pattern[i]);
                    nodes.Add(n);
                }
                for (int j = 0; j < vars.Length; j++) {
                    if (!tmpVarList.ContainsKey(vars[j])) {
                        var n = new Node(vars[j]);
                        tmpVarList.Add(vars[j], n);
                    }

                    tmpVarList[vars[j]].SAPs.Add(pattern[i]);
                }
            }
            foreach (var kvPair in tmpVarList) {
                nodes.Add(kvPair.Value);
            }

            //
            // Step 2:
            //     Let EDGESET be a new, empty set. For each pair of S1, S2 in BGP P, with S1 != S2,
            //     and for each pair of variables v1 in S1 and v2 in S2 such that v1 = v2, add label
            //     (S1, S2, v1, p1, p2) to EDGESET (where p1 and p2 are the positions s, p, or o of
            //     v1 and v2 in S1 and S2, respectively.

            var edgeSet = new HashSet<EdgeLabel>();
            for (int i = 0; i < pattern.Length; i++) {
                for (int j = 0; j < pattern.Length; j++) {
                    if (i != j) {
                        if (pattern[i].S is Variable) {
                            var iVar = pattern[i].S as Variable;
                            if (pattern[j].S is Variable) {
                                var jVar = pattern[j].S as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.S, TriplePosition.S));
                                }
                            }
                            if (pattern[j].P is Variable) {
                                var jVar = pattern[j].P as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.S, TriplePosition.P));
                                }
                            }
                            if (pattern[j].O is Variable) {
                                var jVar = pattern[j].O as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.S, TriplePosition.O));
                                }
                            }
                        }
                        if (pattern[i].P is Variable) {
                            var iVar = pattern[i].P as Variable;
                            if (pattern[j].S is Variable) {
                                var jVar = pattern[j].S as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.P, TriplePosition.S));
                                }
                            }
                            if (pattern[j].P is Variable) {
                                var jVar = pattern[j].P as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.P, TriplePosition.P));
                                }
                            }
                            if (pattern[j].O is Variable) {
                                var jVar = pattern[j].O as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.P, TriplePosition.O));
                                }
                            }
                        }
                        if (pattern[i].O is Variable) {
                            var iVar = pattern[i].O as Variable;
                            if (pattern[j].S is Variable) {
                                var jVar = pattern[j].S as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.O, TriplePosition.S));
                                }
                            }
                            if (pattern[j].P is Variable) {
                                var jVar = pattern[j].P as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.O, TriplePosition.P));
                                }
                            }
                            if (pattern[j].O is Variable) {
                                var jVar = pattern[j].O as Variable;
                                if (iVar == jVar) {
                                    edgeSet.Add(new EdgeLabel(pattern[i], pattern[j], iVar, TriplePosition.O, TriplePosition.O));
                                }
                            }
                        }
                    }
                }
            }

            //
            // Step 3:
            //     Let EDGES be a new, empty set. For each label l = (S1, S2, v1, p1, p2) from
            //     EDGESET, for each pair of nodes (x, X) and (y, Y) from NODES such that S1 is in
            //     X and S2 is in Y, if there already exists some edge ((x, X) -- (y, Y) : L) in
            //     EDGES for some set of labels L, then add l to L, and if not then add new edge
            //     ((x, X) -- (y, Y) : {l}) to EDGES (provided that there is no edge ((y, Y) --
            //     (x, X) : L') already in EDGES.

            var edges = new List<Edge>();
            foreach (var edgeSetItem in edgeSet) {
                for (int i = 0; i < nodes.Count; i++) {
                    var ti = nodes[i];
                    for (int j = i + 1; j < nodes.Count; j++) {
                        var tj = nodes[j];
                        foreach (var x in ti.SAPs) {
                            foreach (var y in tj.SAPs) {
                                if (x == edgeSetItem.Left && y == edgeSetItem.Right) {
                                    var edge = new Edge(ti, tj);
                                    if (!edges.Contains(edge)) {
                                        edges.Add(edge);
                                    }
                                    edges.Find(e => e == edge).Labels.Add(edgeSetItem);
                                }
                            }
                        }
                    }
                }
            }

            //
            // Step 4:
            //     The atom collapse now consists of nodes from NODES and edges from EDGES.

            return new Graph(nodes, edges);
        }

        /// <summary>
        /// Gets an (atom[], variable[]) tuple containing the unique atoms and variables from the
        /// given SAP.
        /// </summary>
        /// <param name="triple">The SAP.</param>
        /// <returns>
        /// A tuple containing the unique atoms and variables from the given SAP.
        /// </returns>
        private static Tuple<TripleItem[], TripleItem[]> GetUniqueItems(Triple<TripleItem, TripleItem, TripleItem> triple)
        {
            var atomSet = new HashSet<TripleItem>();
            var varSet = new HashSet<TripleItem>();

            //
            // if an item at some position is an atom, add it to the atom set, if not add it to
            // the variable set. the sets are hashsets, which automatically filter duplicates.

            if (triple.S is Atom) {
                atomSet.Add(triple.S);
            } else {
                varSet.Add(triple.S);
            }

            if (triple.P is Atom) {
                atomSet.Add(triple.P);
            } else {
                varSet.Add(triple.P);
            }

            if (triple.O is Atom) {
                atomSet.Add(triple.O);
            } else {
                varSet.Add(triple.O);
            }

            return Tuple.Create(atomSet.ToArray(), varSet.ToArray());
        }
    }
}
