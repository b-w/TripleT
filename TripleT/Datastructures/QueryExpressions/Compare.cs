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

namespace TripleT.Datastructures.QueryExpressions
{
    /// <summary>
    /// Represents a comparison expression, indicating any given atoms must compare in a certain
    /// way to some fixed comparison value.
    /// </summary>
    public class Compare : Expression
    {
        private readonly Atom m_value;
        private readonly CompareOption m_option;

        /// <summary>
        /// Initializes a new instance of the <see cref="Compare"/> class.
        /// </summary>
        /// <param name="option">The comparison option to use.</param>
        /// <param name="value">The base value against which to compare all other values.</param>
        public Compare(CompareOption option, Atom value)
        {
            m_value = value;
            m_option = option;
        }

        /// <summary>
        /// Determines whether the specified atom matches the expression.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <returns>
        ///   <c>true</c> if the specified atom matches the expression; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsMatch(Atom atom)
        {
            switch (m_option) {
                case CompareOption.None:
                    return false;
                case CompareOption.LessThan:
                    return (m_value.InternalValue < atom.InternalValue);
                case CompareOption.LessOrEquals:
                    return (m_value.InternalValue <= atom.InternalValue);
                case CompareOption.Equals:
                    return (m_value.InternalValue == atom.InternalValue);
                case CompareOption.GreaterOrEquals:
                    return (m_value.InternalValue >= atom.InternalValue);
                case CompareOption.GreaterThan:
                    return (m_value.InternalValue > atom.InternalValue);
                default:
                    return false;
            }
        }
    }
}
