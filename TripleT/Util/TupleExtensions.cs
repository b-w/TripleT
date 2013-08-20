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

    /// <summary>
    /// Contains extension methods for the <see cref="System.Tuple"/> class.
    /// </summary>
    public static class TupleExtensions
    {
        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        public static void Decompose<T1>(this Tuple<T1> tuple, out T1 item1)
        {
            item1 = tuple.Item1;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        public static void Decompose<T1, T2>(this Tuple<T1, T2> tuple, out T1 item1, out T2 item2)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        /// <param name="item3">Will contain the third item in the tuple.</param>
        public static void Decompose<T1, T2, T3>(this Tuple<T1, T2, T3> tuple, out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        /// <param name="item3">Will contain the third item in the tuple.</param>
        /// <param name="item4">Will contain the fouth item in the tuple.</param>
        public static void Decompose<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> tuple, out T1 item1, out T2 item2, out T3 item3, out T4 item4)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth item in the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        /// <param name="item3">Will contain the third item in the tuple.</param>
        /// <param name="item4">Will contain the fouth item in the tuple.</param>
        /// <param name="item5">Will contain the fifth item in the tuple.</param>
        public static void Decompose<T1, T2, T3, T4, T5>(this Tuple<T1, T2, T3, T4, T5> tuple, out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
            item5 = tuple.Item5;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth item in the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth item in the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        /// <param name="item3">Will contain the third item in the tuple.</param>
        /// <param name="item4">Will contain the fouth item in the tuple.</param>
        /// <param name="item5">Will contain the fifth item in the tuple.</param>
        /// <param name="item6">Will contain the sixth item in the tuple.</param>
        public static void Decompose<T1, T2, T3, T4, T5, T6>(this Tuple<T1, T2, T3, T4, T5, T6> tuple, out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
            item5 = tuple.Item5;
            item6 = tuple.Item6;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth item in the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth item in the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth item in the tuple.</typeparam>
        /// <typeparam name="T7">The type of the seventh item in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        /// <param name="item3">Will contain the third item in the tuple.</param>
        /// <param name="item4">Will contain the fouth item in the tuple.</param>
        /// <param name="item5">Will contain the fifth item in the tuple.</param>
        /// <param name="item6">Will contain the sixth item in the tuple.</param>
        /// <param name="item7">Will contain the seventh item in the tuple.</param>
        public static void Decompose<T1, T2, T3, T4, T5, T6, T7>(this Tuple<T1, T2, T3, T4, T5, T6, T7> tuple, out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
            item5 = tuple.Item5;
            item6 = tuple.Item6;
            item7 = tuple.Item7;
        }

        /// <summary>
        /// Decomposes the specified tuple into the provided variables.
        /// </summary>
        /// <typeparam name="T1">The type of the first item in the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second item in the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third item in the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth item in the tuple.</typeparam>
        /// <typeparam name="T5">The type of the fifth item in the tuple.</typeparam>
        /// <typeparam name="T6">The type of the sixth item in the tuple.</typeparam>
        /// <typeparam name="T7">The type of the seventh item in the tuple.</typeparam>
        /// <typeparam name="TRest">The type of the remaining components in the tuple.</typeparam>
        /// <param name="tuple">The tuple instance.</param>
        /// <param name="item1">Will contain the first item in the tuple.</param>
        /// <param name="item2">Will contain the second item in the tuple.</param>
        /// <param name="item3">Will contain the third item in the tuple.</param>
        /// <param name="item4">Will contain the fouth item in the tuple.</param>
        /// <param name="item5">Will contain the fifth item in the tuple.</param>
        /// <param name="item6">Will contain the sixth item in the tuple.</param>
        /// <param name="item7">Will contain the seventh item in the tuple.</param>
        /// <param name="rest">Will contain the remaining components in the tuple.</param>
        public static void Decompose<T1, T2, T3, T4, T5, T6, T7, TRest>(this Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple, out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out TRest rest)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
            item3 = tuple.Item3;
            item4 = tuple.Item4;
            item5 = tuple.Item5;
            item6 = tuple.Item6;
            item7 = tuple.Item7;
            rest = tuple.Rest;
        }
    }
}