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

namespace TripleT.Test.QuerySets
{
    using System;
    using TripleT.Datastructures.Queries;

    public static class Uniprot
    {
        private static readonly Pattern[] m_qChainSS = new Pattern[]
            {
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 2),
                new Pattern(2, "http://purl.uniprot.org/core/locusName", 3),
                new Pattern("http://purl.uniprot.org/uniprot/P0C9G1", "http://purl.uniprot.org/core/classifiedWith", 1)
            };

        private static readonly Pattern[] m_qChainSL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 2),
                new Pattern(2, "http://purl.uniprot.org/core/locusName", 3),
                new Pattern("http://purl.uniprot.org/uniprot/P0C9G1", "http://purl.uniprot.org/core/classifiedWith", 1)
            };

        private static readonly Pattern[] m_qChainNS = new Pattern[]
            {
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 2),
                new Pattern(2, "http://purl.uniprot.org/core/locusName", 3),
                new Pattern(4, "http://purl.uniprot.org/core/classifiedWith", 1)
            };

        private static readonly Pattern[] m_qChainNL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 2),
                new Pattern(2, "http://purl.uniprot.org/core/locusName", 3),
                new Pattern(4, "http://purl.uniprot.org/core/classifiedWith", 1)
            };

        private static readonly Pattern[] m_qStarSS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/citation", "http://purl.uniprot.org/SHA-384/A2F91AA898203168B21443E43F32FF4E7565680547B3DB5B3FFCDB1D1A4021D8282ADA80FD7F0B743C9E5CC57C3CD5A5"),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 4)
            };

        private static readonly Pattern[] m_qStarSL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 4),
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 5),
                new Pattern(1, "http://purl.uniprot.org/core/isolatedFrom", "http://purl.uniprot.org/tissues/614"),
                new Pattern(1, "http://purl.uniprot.org/core/interaction", 7)
            };

        private static readonly Pattern[] m_qStarNS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 4)
            };

        private static readonly Pattern[] m_qStarNL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 4),
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 5),
                new Pattern(1, "http://purl.uniprot.org/core/isolatedFrom", 6),
                new Pattern(1, "http://purl.uniprot.org/core/interaction", 7)
            };

        private static readonly Pattern[] m_qStarChainSS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 4),
                new Pattern(4, "http://purl.uniprot.org/core/author", "Hill J."),
                new Pattern(4, "http://purl.uniprot.org/core/group", 6),
                new Pattern(4, "http://purl.uniprot.org/core/title", 7)
            };

        private static readonly Pattern[] m_qStarChainSL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 8),
                new Pattern(1, "http://purl.uniprot.org/core/isolatedFrom", 9),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 4),
                new Pattern(4, "http://purl.uniprot.org/core/author", "Hill J."),
                new Pattern(4, "http://purl.uniprot.org/core/group", 6),
                new Pattern(4, "http://purl.uniprot.org/core/title", 7)
            };

        private static readonly Pattern[] m_qStarChainNS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 4),
                new Pattern(4, "http://purl.uniprot.org/core/locator", 5),
                new Pattern(4, "http://purl.uniprot.org/core/group", 6),
                new Pattern(4, "http://purl.uniprot.org/core/title", 7)
            };

        private static readonly Pattern[] m_qStarChainNL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://purl.uniprot.org/core/Protein"),
                new Pattern(1, "http://purl.uniprot.org/core/modified", 2),
                new Pattern(1, "http://purl.uniprot.org/core/annotation", 3),
                new Pattern(1, "http://purl.uniprot.org/core/encodedBy", 8),
                new Pattern(1, "http://purl.uniprot.org/core/isolatedFrom", 9),
                new Pattern(1, "http://purl.uniprot.org/core/citation", 4),
                new Pattern(4, "http://purl.uniprot.org/core/locator", 5),
                new Pattern(4, "http://purl.uniprot.org/core/group", 6),
                new Pattern(4, "http://purl.uniprot.org/core/title", 7)
            };

        private static readonly Pattern[][] m_querySet = new Pattern[][]
            {
                m_qChainSS, m_qChainSL, m_qChainNS, m_qChainNL,
                m_qStarSS, m_qStarSL, m_qStarNS, m_qStarNL,
                m_qStarChainSS, m_qStarChainSL, m_qStarChainNS, m_qStarChainNL
            };

        private static readonly Tuple<string, Pattern[]>[] m_namedQuerySet = new Tuple<string, Pattern[]>[]
            {
                Tuple.Create("Q-Chain-SS", m_qChainSS),
                Tuple.Create("Q-Chain-SL", m_qChainSL),
                Tuple.Create("Q-Chain-NS", m_qChainNS),
                Tuple.Create("Q-Chain-NL", m_qChainNL),

                Tuple.Create("Q-Star-SS", m_qStarSS),
                Tuple.Create("Q-Star-SL", m_qStarSL),
                Tuple.Create("Q-Star-NS", m_qStarNS),
                Tuple.Create("Q-Star-NL", m_qStarNL),

                Tuple.Create("Q-StarChain-SS", m_qStarChainSS),
                Tuple.Create("Q-StarChain-SL", m_qStarChainSL),
                Tuple.Create("Q-StarChain-NS", m_qStarChainNS),
                Tuple.Create("Q-StarChain-NL", m_qStarChainNL)
            };

        public static Pattern[][] QuerySet
        {
            get { return m_querySet; }
        }

        public static Tuple<string, Pattern[]>[] NamedQuerySet
        {
            get { return m_namedQuerySet; }
        }
    }
}
