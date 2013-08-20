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
    /// Static class containing centralized functionality for logging of application events.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Writes the given message to the console, and prefixes it with the current date and time.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="arg">The array of objects to write using the given format string.</param>
        public static void WriteLine(string value, params object[] arg)
        {
            Console.WriteLine("[{0}] {1}", DateTime.Now, String.Format(value, arg));
        }
    }
}
