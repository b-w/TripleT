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
    using TripleT.Datastructures.QueryExpressions;

    /// <summary>
    /// Represents a filter for bindings.
    /// </summary>
    public class BindingFilter
    {
        private readonly Binding m_binding;
        private readonly Expression m_filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingFilter"/> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="filter">The filter expression to use.</param>
        public BindingFilter(Binding binding, Expression filter)
        {
            m_binding = binding;
            m_filter = filter;
        }

        /// <summary>
        /// Gets the binding used.
        /// </summary>
        public Binding Binding
        {
            get { return m_binding; }
        }

        /// <summary>
        /// Gets the filter expression used.
        /// </summary>
        public Expression Filter
        {
            get { return m_filter; }
        }
    }
}
