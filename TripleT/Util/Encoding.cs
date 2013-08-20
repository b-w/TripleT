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

namespace TripleT.Util
{
    using System;
    using System.Collections.Generic;
    using TripleT.Datastructures;
    using txt = System.Text;

    /// <summary>
    /// Static class containing shared functions for encoding and decoding values to byte arrays,
    /// for storage in binary files or in BerkeleyDB.
    /// </summary>
    public static class Encoding
    {
        /// <summary>
        /// Byte-encodes the given string, using UTF8 encoding.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>
        /// A byte-array encoding of the given string value.
        /// </returns>
        public static byte[] DbEncode(string value)
        {
            return txt::Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// Byte-encodes the given 16-bit integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>
        /// A byte-array encoding of the given integer value.
        /// </returns>
        public static byte[] DbEncode(short value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Byte-encodes the given 32-bit integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>
        /// A byte-array encoding of the given integer value.
        /// </returns>
        public static byte[] DbEncode(int value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Byte-encodes the given 64-bit integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>
        /// A byte-array encoding of the given integer value.
        /// </returns>
        public static byte[] DbEncode(long value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Decodes the given byte array as a UTF8-encoded string.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded string value.
        /// </returns>
        public static string DbDecodeString(byte[] value)
        {
            return txt::Encoding.UTF8.GetString(value);
        }

        /// <summary>
        /// Decodes the given byte array as a 16-bit integer.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded integer value.
        /// </returns>
        public static short DbDecodeInt16(byte[] value)
        {
            return BitConverter.ToInt16(value, 0);
        }

        /// <summary>
        /// Decodes the given byte array as a 32-bit integer.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded integer value.
        /// </returns>
        public static int DbDecodeInt32(byte[] value)
        {
            return BitConverter.ToInt32(value, 0);
        }

        /// <summary>
        /// Decodes the given byte array as a 64-bit integer.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded integer value.
        /// </returns>
        public static long DbDecodeInt64(byte[] value)
        {
            return BitConverter.ToInt64(value, 0);
        }

        /// <summary>
        /// Byte-encodes the given binding array.
        /// </summary>
        /// <param name="value">The binding array.</param>
        /// <returns>
        /// A byte-array encoding of the given binding array.
        /// </returns>
        public static byte[] DbEncode(Binding[] value)
        {
            var enc = new byte[8 * 2 * value.Length];
            for (int i = 0; i < value.Length; i++) {
                var encVar = TripleT.Util.Encoding.DbEncode(value[i].Variable.InternalValue);
                var encVal = TripleT.Util.Encoding.DbEncode(value[i].Value.InternalValue);
                Array.Copy(encVar, 0, enc, i * 16, 8);
                Array.Copy(encVal, 0, enc, (i * 16) + 8, 8);
            }
            return enc;
        }

        /// <summary>
        /// Decodes the given byte array as a binding array.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded binding array.
        /// </returns>
        public static Binding[] DbDecodeBinding(byte[] value)
        {
            var dec = new List<Binding>();
            var tmp = new byte[8];
            for (int i = 0; i < value.Length / 16; i++) {
                Array.Copy(value, i * 16, tmp, 0, 8);
                var decVar = TripleT.Util.Encoding.DbDecodeInt64(tmp);
                Array.Copy(value, (i * 16) + 8, tmp, 0, 8);
                var decVal = TripleT.Util.Encoding.DbDecodeInt64(tmp);
                dec.Add(new Binding(new Variable(decVar), new Atom(decVal)));
            }
            return dec.ToArray();
        }

        /// <summary>
        /// Byte-encodes the given binding st.
        /// </summary>
        /// <param name="value">The binding set.</param>
        /// <returns>
        /// A byte-array encoding of the given binding set.
        /// </returns>
        public static byte[] DbEncode(BindingSet value)
        {
            var b = new Binding[value.Count];
            var i = 0;
            foreach (var item in value.Bindings) {
                b[i++] = item;
            }
            return DbEncode(b);
        }

        /// <summary>
        /// Decodes the given byte array as a binding set.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded binding set.
        /// </returns>
        public static BindingSet DbDecodeBindingSet(byte[] value)
        {
            var b = DbDecodeBinding(value);
            return new BindingSet(b);
        }

        /// <summary>
        /// Byte-encodes the given 64-bit integer array.
        /// </summary>
        /// <param name="value">The integer array.</param>
        /// <returns>
        /// A byte-array encoding of the given integer array.
        /// </returns>
        public static byte[] DbEncode(long[] value)
        {
            var enc = new byte[8 * value.Length];
            for (int i = 0; i < value.Length; i++) {
                var encVal = TripleT.Util.Encoding.DbEncode(value[i]);
                Array.Copy(encVal, 0, enc, i * 8, 8);
            }
            return enc;
        }

        /// <summary>
        /// Decodes the given byte array as a 64-bit integer array.
        /// </summary>
        /// <param name="value">The byte-array value.</param>
        /// <returns>
        /// The decoded integer array.
        /// </returns>
        public static long[] DbDecodeInt64Array(byte[] value)
        {
            var dec = new long[value.Length / 8];
            var tmp = new byte[8];
            for (int i = 0; i < dec.Length; i++) {
                Array.Copy(value, i * 8, tmp, 0, 8);
                var decVal = TripleT.Util.Encoding.DbDecodeInt64(tmp);
                dec[i] = decVal;
            }
            return dec;
        }
    }
}
