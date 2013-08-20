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
    using System.IO;
    using BerkeleyDB;

    /// <summary>
    /// Represents the TripleT atom dictionary, used for translating between friendly
    /// representations (strings) of atoms and their internal representations (integers) used by
    /// the database engine.
    /// </summary>
    public class AtomDictionary : IDisposable
    {
        private readonly HashDatabase m_dbStr2Long;
        private readonly HashDatabase m_dbLong2Str;
        private readonly string m_fileNextValue;
        private long m_next;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomDictionary"/> class.
        /// </summary>
        /// <param name="databaseName">Name of the TripleT database this dictionary belongs to.</param>
        public AtomDictionary(string databaseName)
        {
            //
            // names for each of the files involved

            var nameStr2Long = String.Format("{0}.dict.str.dat", databaseName);
            var nameLong2Str = String.Format("{0}.dict.int.dat", databaseName);
            m_fileNextValue = String.Format("{0}.dict.dat", databaseName);

            //
            // we manually keep a file containing the value for our auto-incrementing index integer
            // assigned to new string values inserted into the dictionary. BerkeleyDB does not
            // support such a feature...

            if (File.Exists(m_fileNextValue)) {
                using (var sr = new BinaryReader(File.Open(m_fileNextValue, FileMode.Open, FileAccess.Read, FileShare.Read))) {
                    m_next = sr.ReadInt64();
                }
            } else {
                m_next = 1;
            }

            //
            // configuration for the dictionary databases. memory allocated for the caches is
            // hardcoded here.

            var config = new HashDatabaseConfig();
            config.Duplicates = DuplicatesPolicy.NONE;
            config.CacheSize = new CacheInfo(0, 256 * 1024 * 1024, 4);
            config.PageSize = 512;
            config.Creation = CreatePolicy.IF_NEEDED;

            //
            // opening the databases...

            m_dbStr2Long = HashDatabase.Open(nameStr2Long, config);
            m_dbLong2Str = HashDatabase.Open(nameLong2Str, config);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //
            // write the current value for the auto-incrementing index integer to file

            using (var sw = new BinaryWriter(File.Open(m_fileNextValue, FileMode.Create, FileAccess.Write, FileShare.None))) {
                sw.Write(m_next);
            }

            //
            // close the BerkeleyDB databases

            m_dbStr2Long.Close(true);
            m_dbLong2Str.Close(true);
        }

        /// <summary>
        /// Gets the internal representation used by the database for a given string value.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>
        /// An integer which is the internal representation for the given string.
        /// </returns>
        public long GetInternalRepresentation(string value)
        {
            var key = new DatabaseEntry(Util.Encoding.DbEncode(value));

            //
            // if this string already exists in the dictionary we simply retrieve the internal
            // representation that belongs to it. if not, we assign an internal representation to
            // it on the fly and insert it into the dictionary. whatever case we are in is
            // indistinguishable to the caller.

            if (m_dbStr2Long.Exists(key)) {
                var kvPair = m_dbStr2Long.Get(key);
                return Util.Encoding.DbDecodeInt64(kvPair.Value.Data);
            } else {
                var next = m_next++;
                Insert(next, value);
                return next;
            }
        }

        /// <summary>
        /// Gets the external (or friendly) string representation belonging to a given integer
        /// used as internal representation by the database.
        /// </summary>
        /// <param name="value">The internal representation.</param>
        /// <returns>
        /// A string belonging to the given internal representation.
        /// </returns>
        public string GetExternalRepresentation(long value)
        {
            var key = new DatabaseEntry(Util.Encoding.DbEncode(value));

            //
            // if this particular internal representation does not already exist in the dictionary
            // we throw an argument exception.

            if (m_dbLong2Str.Exists(key)) {
                var kvPair = m_dbLong2Str.Get(key);
                return Util.Encoding.DbDecodeString(kvPair.Value.Data);
            } else {
                throw new ArgumentOutOfRangeException("value", "No such internal representation!");
            }
        }

        /// <summary>
        /// Inserts the specified (integer, string) pair of internal- and external representations
        /// into the dictionary.
        /// </summary>
        /// <param name="internalValue">The internal representation.</param>
        /// <param name="externalValue">The external representation.</param>
        private void Insert(long internalValue, string externalValue)
        {
            //
            // the (external, internal) key-value pair is put in the string2integer dictionary

            var str2LongKey = new DatabaseEntry(Util.Encoding.DbEncode(externalValue));
            var str2LongValue = new DatabaseEntry(Util.Encoding.DbEncode(internalValue));
            m_dbStr2Long.Put(str2LongKey, str2LongValue);

            //
            // similarly, the (internal, external) key-value pair goes into the integer2string
            // dictionary

            var long2StrKey = new DatabaseEntry(Util.Encoding.DbEncode(internalValue));
            var long2StrValue = new DatabaseEntry(Util.Encoding.DbEncode(externalValue));
            m_dbLong2Str.Put(long2StrKey, long2StrValue);
        }
    }
}
