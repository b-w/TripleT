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

    public static class Sp2Bench
    {
        private static readonly Pattern[] m_qChainSS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Journal"),
                new Pattern(2, "http://swrc.ontoware.org/ontology#journal", 1),
                new Pattern(2, "http://purl.org/dc/elements/1.1/creator", "http://localhost/persons/Paul_Erdoes")
            };

        private static readonly Pattern[] m_qChainSL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Journal"),
                new Pattern(2, "http://swrc.ontoware.org/ontology#journal", 1),
                new Pattern(2, "http://purl.org/dc/elements/1.1/creator", 3),
                new Pattern(3, "http://xmlns.com/foaf/0.1/name", 4),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", "Journal 1 (1940)")
            };

        private static readonly Pattern[] m_qChainNS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Journal"),
                new Pattern(2, "http://swrc.ontoware.org/ontology#journal", 1),
                new Pattern(2, "http://purl.org/dc/elements/1.1/creator", 3)
            };

        private static readonly Pattern[] m_qChainNL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Journal"),
                new Pattern(2, "http://swrc.ontoware.org/ontology#journal", 1),
                new Pattern(2, "http://purl.org/dc/elements/1.1/creator", 3),
                new Pattern(3, "http://xmlns.com/foaf/0.1/name", 4),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 5)
            };

        private static readonly Pattern[] m_qStarSS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Inproceedings"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", "Bhoomika_Secord"),
                new Pattern(1, "http://localhost/vocabulary/bench/booktitle", 3),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 4)
            };

        private static readonly Pattern[] m_qStarSL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Inproceedings"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", "Bhoomika_Secord"),
                new Pattern(1, "http://localhost/vocabulary/bench/booktitle", 3),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 4),
                new Pattern(1, "http://purl.org/dc/terms/partOf", 5),
                new Pattern(1, "http://www.w3.org/2000/01/rdf-schema#seeAlso", 6),
                new Pattern(1, "http://swrc.ontoware.org/ontology#pages", 7),
                new Pattern(1, "http://xmlns.com/foaf/0.1/homepage", 8),
                new Pattern(1, "http://purl.org/dc/terms/issued", 9)
            };

        private static readonly Pattern[] m_qStarNS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Inproceedings"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", 2),
                new Pattern(1, "http://localhost/vocabulary/bench/booktitle", 3),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 4)
            };

        private static readonly Pattern[] m_qStarNL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Inproceedings"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", 2),
                new Pattern(1, "http://localhost/vocabulary/bench/booktitle", 3),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 4),
                new Pattern(1, "http://purl.org/dc/terms/partOf", 5),
                new Pattern(1, "http://www.w3.org/2000/01/rdf-schema#seeAlso", 6),
                new Pattern(1, "http://swrc.ontoware.org/ontology#pages", 7),
                new Pattern(1, "http://xmlns.com/foaf/0.1/homepage", 8),
                new Pattern(1, "http://purl.org/dc/terms/issued", 9)
            };

        private static readonly Pattern[] m_qStarChainSS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", "http://localhost/persons/Paul_Erdoes"),
                new Pattern(1, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(1, "http://purl.org/dc/terms/references", 4),
                new Pattern(5, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(5, "http://purl.org/dc/elements/1.1/creator", "Dell_Kosel"),
                new Pattern(5, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(5, "http://purl.org/dc/terms/references", 7)
            };

        private static readonly Pattern[] m_qStarChainSL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", "http://localhost/persons/Paul_Erdoes"),
                new Pattern(1, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(1, "http://purl.org/dc/terms/references", 4),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 8),
                new Pattern(1, "http://localhost/vocabulary/bench/abstract", 9),
                new Pattern(5, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(5, "http://purl.org/dc/elements/1.1/creator", "Dell_Kosel"),
                new Pattern(5, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(5, "http://purl.org/dc/terms/references", 7),
                new Pattern(5, "http://localhost/vocabulary/bench/cdrom", 10),
                new Pattern(5, "http://www.w3.org/2000/01/rdf-schema#seeAlso", 11)
            };

        private static readonly Pattern[] m_qStarChainNS = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", 2),
                new Pattern(1, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(1, "http://purl.org/dc/terms/references", 4),
                new Pattern(5, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(5, "http://purl.org/dc/elements/1.1/creator", 6),
                new Pattern(5, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(5, "http://purl.org/dc/terms/references", 7)
            };

        private static readonly Pattern[] m_qStarChainNL = new Pattern[]
            {
                new Pattern(1, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(1, "http://purl.org/dc/elements/1.1/creator", 2),
                new Pattern(1, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(1, "http://purl.org/dc/terms/references", 4),
                new Pattern(1, "http://purl.org/dc/elements/1.1/title", 8),
                new Pattern(1, "http://localhost/vocabulary/bench/abstract", 9),
                new Pattern(5, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://localhost/vocabulary/bench/Article"),
                new Pattern(5, "http://purl.org/dc/elements/1.1/creator", 6),
                new Pattern(5, "http://swrc.ontoware.org/ontology#journal", 3),
                new Pattern(5, "http://purl.org/dc/terms/references", 7),
                new Pattern(5, "http://localhost/vocabulary/bench/cdrom", 10),
                new Pattern(5, "http://www.w3.org/2000/01/rdf-schema#seeAlso", 11)
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
