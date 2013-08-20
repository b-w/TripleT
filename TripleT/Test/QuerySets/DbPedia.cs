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

    public static class DbPedia
    {
        private static readonly Pattern[] m_qChainSS = new Pattern[]
            {
                new Pattern("http://dbpedia.org/resource/George_Orwell", "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/birthDate", 3)
            };

        private static readonly Pattern[] m_qChainSL = new Pattern[]
            {
                new Pattern("http://dbpedia.org/resource/George_Orwell", "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/influencedBy", 3),
                new Pattern(3, "http://dbpedia.org/ontology/author", 4),
                new Pattern(4, "http://dbpedia.org/ontology/releaseDate", 5)
            };

        private static readonly Pattern[] m_qChainNS = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(2, "http://dbpedia.org/ontology/governingBody", 3),
                new Pattern(3, "http://dbpedia.org/ontology/formationDate", 4)
            };

        private static readonly Pattern[] m_qChainNL = new Pattern[]
            {
                new Pattern(1, "http://xmlns.com/foaf/0.1/name", 6),
                new Pattern(2, "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/influencedBy", 3),
                new Pattern(3, "http://dbpedia.org/ontology/author", 4),
                new Pattern(4, "http://dbpedia.org/ontology/releaseDate", 5)
            };

        private static readonly Pattern[] m_qStarSS = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/influencedBy", "http://dbpedia.org/resource/George_Orwell"),
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/birthName", 4)
            };

        private static readonly Pattern[] m_qStarSL = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/influencedBy", "http://dbpedia.org/resource/George_Orwell"),
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/birthName", 4),
                new Pattern(1, "http://dbpedia.org/ontology/pseudonym", 5),
                new Pattern(1, "http://dbpedia.org/ontology/spouse", 6),
                new Pattern(7, "http://dbpedia.org/ontology/influenced", 1),
                new Pattern(8, "http://dbpedia.org/ontology/author", 1)
            };

        private static readonly Pattern[] m_qStarNS = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/birthName", 4),
                new Pattern(1, "http://dbpedia.org/ontology/spouse", 5)
            };

        private static readonly Pattern[] m_qStarNL = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/birthName", 4),
                new Pattern(1, "http://dbpedia.org/ontology/pseudonym", 5),
                new Pattern(1, "http://dbpedia.org/ontology/spouse", 6),
                new Pattern(7, "http://dbpedia.org/ontology/influenced", 1),
                new Pattern(8, "http://dbpedia.org/ontology/author", 1)
            };

        private static readonly Pattern[] m_qStarChainSS = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", "http://dbpedia.org/resource/London"),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/deathPlace", "http://dbpedia.org/resource/London"),
                new Pattern(1, "http://dbpedia.org/ontology/deathDate", 5),
                new Pattern(1, "http://dbpedia.org/ontology/spouse", 6),
                new Pattern(6, "http://dbpedia.org/ontology/birthPlace", 7),
                new Pattern(6, "http://dbpedia.org/ontology/birthDate", 8),
                new Pattern(6, "http://dbpedia.org/ontology/deathPlace", 9),
                new Pattern(6, "http://dbpedia.org/ontology/deathDate", 10)
            };

        private static readonly Pattern[] m_qStarChainSL = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", "http://dbpedia.org/resource/London"),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/deathPlace", 4),
                new Pattern(1, "http://dbpedia.org/ontology/deathDate", 5),
                new Pattern(1, "http://dbpedia.org/ontology/nationality", 11),
                new Pattern(1, "http://dbpedia.org/ontology/parent", 12),
                new Pattern(1, "http://dbpedia.org/ontology/spouse", 6),
                new Pattern(6, "http://dbpedia.org/ontology/birthPlace", "http://dbpedia.org/resource/London"),
                new Pattern(6, "http://dbpedia.org/ontology/birthDate", 8),
                new Pattern(6, "http://dbpedia.org/ontology/deathPlace", 9),
                new Pattern(6, "http://dbpedia.org/ontology/deathDate", 10),
                new Pattern(6, "http://dbpedia.org/ontology/nationality", 13),
                new Pattern(1, "http://dbpedia.org/ontology/parent", 14)
            };

        private static readonly Pattern[] m_qStarChainNS = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/deathPlace", 4),
                new Pattern(1, "http://dbpedia.org/ontology/deathDate", 5),
                new Pattern(1, "http://dbpedia.org/ontology/spouse", 6),
                new Pattern(6, "http://dbpedia.org/ontology/birthPlace", 7),
                new Pattern(6, "http://dbpedia.org/ontology/birthDate", 8),
                new Pattern(6, "http://dbpedia.org/ontology/deathPlace", 9),
                new Pattern(6, "http://dbpedia.org/ontology/deathDate", 10)
            };

        private static readonly Pattern[] m_qStarChainNL = new Pattern[]
            {
                new Pattern(1, "http://dbpedia.org/ontology/birthPlace", 2),
                new Pattern(1, "http://dbpedia.org/ontology/birthDate", 3),
                new Pattern(1, "http://dbpedia.org/ontology/deathPlace", 4),
                new Pattern(1, "http://dbpedia.org/ontology/deathDate", 5),
                new Pattern(1, "http://dbpedia.org/ontology/child", 6),
                new Pattern(6, "http://dbpedia.org/ontology/spouse", 7),
                new Pattern(7, "http://dbpedia.org/ontology/child", 8),
                new Pattern(8, "http://dbpedia.org/ontology/birthPlace", 9),
                new Pattern(8, "http://dbpedia.org/ontology/birthDate", 10),
                new Pattern(8, "http://dbpedia.org/ontology/deathPlace", 11),
                new Pattern(8, "http://dbpedia.org/ontology/deathDate", 12)
            };

        private static readonly Pattern[] m_qLoopSS = new Pattern[]
            {
                new Pattern("http://dbpedia.org/resource/Aldous_Huxley", "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/influencedBy", 3),
                new Pattern(3, "http://dbpedia.org/ontology/deathPlace", 1)
            };

        private static readonly Pattern[] m_qLoopSL = new Pattern[]
            {
                new Pattern("http://dbpedia.org/resource/Aldous_Huxley", "http://dbpedia.org/ontology/birthDate", 1),
                new Pattern(2, "http://dbpedia.org/ontology/influencedBy", 3),
                new Pattern(3, "http://dbpedia.org/ontology/author", 4),
                new Pattern(4, "http://dbpedia.org/ontology/releaseDate", 1)
            };

        private static readonly Pattern[] m_qLoopNS = new Pattern[]
            {
                new Pattern(2, "http://dbpedia.org/ontology/birthPlace", 1),
                new Pattern(2, "http://dbpedia.org/ontology/influencedBy", 3),
                new Pattern(3, "http://dbpedia.org/ontology/deathPlace", 1)
            };

        private static readonly Pattern[] m_qLoopNL = new Pattern[]
            {
                new Pattern(2, "http://dbpedia.org/ontology/birthDate", 1),
                new Pattern(2, "http://dbpedia.org/ontology/influencedBy", 3),
                new Pattern(3, "http://dbpedia.org/ontology/author", 4),
                new Pattern(4, "http://dbpedia.org/ontology/releaseDate", 1)
            };

        private static readonly Pattern[][] m_querySet = new Pattern[][]
            {
                m_qChainSS, m_qChainSL, m_qChainNS, m_qChainNL,
                m_qStarSS, m_qStarSL, m_qStarNS, m_qStarNL,
                m_qStarChainSS, m_qStarChainSL, m_qStarChainNS, m_qStarChainNL,
                m_qLoopSS, m_qLoopSL, m_qLoopNS, m_qLoopNL
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
                Tuple.Create("Q-StarChain-NL", m_qStarChainNL),

                Tuple.Create("Q-Loop-SS", m_qLoopSS),
                Tuple.Create("Q-Loop-SL", m_qLoopSL),
                Tuple.Create("Q-Loop-NS", m_qLoopNS),
                Tuple.Create("Q-Loop-NL", m_qLoopNL)
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
