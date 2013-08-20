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

namespace TripleT.Datastructures
{
    using System;
    using TripleT.Util;

    /// <summary>
    /// Represents an index payload for some atom value, indicating where triples with this value
    /// are located in each of the bucket files.
    /// </summary>
    public class IndexPayload
    {
        private readonly long m_sStart;
        private readonly long m_sCount;
        private readonly long m_pStart;
        private readonly long m_pCount;
        private readonly long m_oStart;
        private readonly long m_oCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexPayload"/> class.
        /// </summary>
        /// <param name="dbPayload">The byte array representation of the payload.</param>
        public IndexPayload(byte[] dbPayload)
        {
            var tmp = new byte[8];

            //
            // the binary layout for the index payload is:
            //
            // [00..07] -> start position for the S-bucket
            // [08..15] -> triple count for the S-bucket
            // [16..23] -> start position for the P-bucket
            // [24..31] -> triple count for the P-bucket
            // [32..39] -> start position for the O-bucket
            // [40..47] -> triple count for the O-bucket

            Array.Copy(dbPayload, 0, tmp, 0, 8);
            m_sStart = Encoding.DbDecodeInt64(tmp);
            Array.Copy(dbPayload, 8, tmp, 0, 8);
            m_sCount = Encoding.DbDecodeInt64(tmp);

            Array.Copy(dbPayload, 16, tmp, 0, 8);
            m_pStart = Encoding.DbDecodeInt64(tmp);
            Array.Copy(dbPayload, 24, tmp, 0, 8);
            m_pCount = Encoding.DbDecodeInt64(tmp);

            Array.Copy(dbPayload, 32, tmp, 0, 8);
            m_oStart = Encoding.DbDecodeInt64(tmp);
            Array.Copy(dbPayload, 40, tmp, 0, 8);
            m_oCount = Encoding.DbDecodeInt64(tmp);
        }

        /// <summary>
        /// Gets the offset (in numbers of triples) for the first triple in the S-bucket containing
        /// the atom value for this payload in its S-position.
        /// </summary>
        public long SStart
        {
            get { return m_sStart; }
        }

        /// <summary>
        /// Gets the number of triples in the S-bucket containing the atom value for this payload
        /// in their S-position.
        /// </summary>
        public long SCount
        {
            get { return m_sCount; }
        }

        /// <summary>
        /// Gets the offset (in numbers of triples) for the first triple in the P-bucket containing
        /// the atom value for this payload in its P-position.
        /// </summary>
        public long PStart
        {
            get { return m_pStart; }
        }

        /// <summary>
        /// Gets the number of triples in the P-bucket containing the atom value for this payload
        /// in their P-position.
        /// </summary>
        public long PCount
        {
            get { return m_pCount; }
        }

        /// <summary>
        /// Gets the offset (in numbers of triples) for the first triple in the O-bucket containing
        /// the atom value for this payload in its O-position.
        /// </summary>
        public long OStart
        {
            get { return m_oStart; }
        }

        /// <summary>
        /// Gets the number of triples in the O-bucket containing the atom value for this payload
        /// in their O-position.
        /// </summary>
        public long OCount
        {
            get { return m_oCount; }
        }
    }
}
