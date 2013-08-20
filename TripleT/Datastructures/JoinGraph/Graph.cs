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

namespace TripleT.Datastructures.JoinGraph
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a join graph.
    /// </summary>
    public class Graph
    {
        private readonly List<Node> m_nodes;
        private readonly List<Edge> m_edges;

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class.
        /// </summary>
        /// <param name="nodes">The nodes belonging to the graph.</param>
        /// <param name="edges">The edges belonging to the graph.</param>
        public Graph(IEnumerable<Node> nodes, IEnumerable<Edge> edges)
        {
            m_edges = new List<Edge>(edges);
            m_nodes = new List<Node>(nodes);
        }

        /// <summary>
        /// Gets the nodes belonging to the graph.
        /// </summary>
        public List<Node> Nodes
        {
            get { return m_nodes; }
        }

        /// <summary>
        /// Gets the edges belonging to the graph.
        /// </summary>
        public List<Edge> Edges
        {
            get { return m_edges; }
        }
    }
}
