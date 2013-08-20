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
    /// Represents a logical AND expression, indicating all child expressions must match a given
    /// atom.
    /// </summary>
    public class And : Expression
    {
        private readonly Expression[] m_expressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="And"/> class.
        /// </summary>
        /// <param name="expressions">The expressions that all need to match.</param>
        public And(params Expression[] expressions)
        {
            m_expressions = expressions;
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
            //
            // this is a logical AND expression, hence we simply check if all child expressions
            // match the given atom. if any one of them doesn't, we return false.

            for (int i = 0; i < m_expressions.Length; i++) {
                if (!m_expressions[i].IsMatch(atom)) {
                    return false;
                }
            }

            return true;
        }
    }
}
