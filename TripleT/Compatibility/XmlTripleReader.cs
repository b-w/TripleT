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

namespace TripleT.Compatibility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Represents a reader used for reading triples from an XML formatted data source.
    /// </summary>
    public class XmlTripleReader : TripleReader
    {
        private readonly XmlTextReader m_reader;
        private readonly Queue<XmlNode> m_queue;
        private string m_currentSubject;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTripleReader"/> class.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        public XmlTripleReader(Stream input)
        {
            m_reader = new XmlTextReader(input);
            m_queue = new Queue<XmlNode>();

            //
            // attempt to read the first triple

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (m_reader != null) {
                m_reader.Close();
            }
        }

        /// <summary>
        /// Attempts to reads the next triple from the underlying data source.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        protected override bool TryReadNext()
        {
            if (m_queue.Count > 0) {
                //
                // if the queue is non-empty, then read from the queue first before continuing on
                // the file

                NextFromQueue();

                m_hasNext = true;
                return true;
            } else {
                while (m_reader.Read()) {
                    //
                    // we are looking for element nodes called "Description". these contain
                    // information on triples.

                    if (m_reader.NodeType == XmlNodeType.Element && m_reader.LocalName.Equals("Description")) {
                        //
                        // the "subject" part of the triple is in the node's "rdf:about" attribute.
                        // this is the subject for all of its child nodes.

                        m_currentSubject = m_reader.GetAttribute("rdf:about");

                        //
                        // the inner XML contains the "predicate" and "object" parts (possible for
                        // multiple triples). we load this in its own little XML document for easy
                        // processing, which should not take too much memory even for large input
                        // documents.

                        var xml = "<tuples>" + m_reader.ReadInnerXml() + "</tuples>";
                        var doc = new XmlDocument();
                        doc.LoadXml(xml);

                        //
                        // each child node gets put into the queue to be parsed

                        foreach (XmlNode item in doc.ChildNodes[0].ChildNodes) {
                            m_queue.Enqueue(item);
                        }

                        //
                        // fetch the next triple from queue

                        NextFromQueue();

                        m_hasNext = true;
                        return true;
                    }
                }

                m_hasNext = false;
                return false;
            }
        }

        /// <summary>
        /// Fetches the next triple from the parse queue.
        /// </summary>
        private void NextFromQueue()
        {
            var node = m_queue.Dequeue();

            //
            // the "predicate" part of the triple is a combination of the node's namespace URI and
            // the node's local name

            var p = String.Format("{0}{1}", node.NamespaceURI, node.LocalName);

            //
            // the "object" part of the triple is either in an "rdf:resource" attribute of the node,
            // or is simply the node's inner text

            string o;
            if (node.Attributes["rdf:resource"] != null) {
                o = node.Attributes["rdf:resource"].Value;
            } else {
                o = node.InnerText;
            }

            //
            // the "subject" is the one we're currently processing

            m_next = Tuple.Create(m_currentSubject, p, o);
        }
    }
}
