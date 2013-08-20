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

namespace TripleT.IO.Operators
{
    using System;
    using System.Collections.Generic;
    using TripleT.Datastructures;
    using TripleT.Datastructures.QueryExpressions;

    /// <summary>
    /// Represents a physical operator for performing filtering operations on sets of bindings.
    /// </summary>
    public class Filter : Operator
    {
        private readonly Operator m_input;
        private readonly Dictionary<Binding, Expression> m_filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="input">This operator's input operator.</param>
        /// <param name="bindingFilters">The binding filters to use.</param>
        public Filter(Operator input, params BindingFilter[] bindingFilters)
        {
            if (bindingFilters.Length == 0) {
                throw new ArgumentOutOfRangeException("bindingFilters", "Provide a non-empty set of binding filters!");
            }

            m_input = input;
            m_filters = new Dictionary<Binding, Expression>();

            //
            // add the set of filters, indexed by binding

            for (int i = 0; i < bindingFilters.Length; i++) {
                if (!m_filters.ContainsKey(bindingFilters[i].Binding)) {
                    m_filters.Add(bindingFilters[i].Binding, bindingFilters[i].Filter);
                }
            }

            //
            // prepare the next set of output bindings

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_input.Dispose();
        }

        /// <summary>
        /// Attempts to read from this operator's underlying input stream(s) until the next set of
        /// bindings is found which can be outputted by this operator.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if a next binding set is found available to output; otherwise, <c>false</c>.
        /// </returns>
        internal override bool TryReadNext()
        {
            while (m_input.HasNext) {
                var next = m_input.Next();

                if (IsMatch(next)) {
                    m_next = next;
                    m_hasNext = true;
                    return true;
                }
            }

            m_hasNext = false;
            return false;
        }

        /// <summary>
        /// Determines whether the specified binding set matches this operator's filters.
        /// </summary>
        /// <param name="bindings">The binding set.</param>
        /// <returns>
        ///   <c>true</c> if the specified binding set is a match; otherwise, <c>false</c>.
        /// </returns>
        private bool IsMatch(BindingSet bindings)
        {
            //
            // simply go through all filters for each binding in the set, and see if they match

            foreach (var item in bindings.Bindings) {
                if (m_filters.ContainsKey(item)) {
                    var filter = m_filters[item];
                    if (!filter.IsMatch(item.Value)) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
