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
    using System.Collections.Generic;

    /// <summary>
    /// Contains extension methods for the <see cref="System.Collections.Generic.List&lt;T&gt;"/>
    /// class.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Adds the given value to the list only if it is not already present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="item">The item to be added to the list.</param>
        public static void AddIfNotPresent<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) {
                list.Add(item);
            }
        }

        /// <summary>
        /// Gets a randomly selected item from the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list instance.</param>
        /// <returns>
        /// A randomly selected item from the list.
        /// </returns>
        public static T GetRandomItem<T>(this List<T> list)
        {
            var i = Generator.GetRandomNumber(list.Count);
            return list[i];
        }
    }
}
