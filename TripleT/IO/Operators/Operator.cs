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
    using TripleT.Datastructures;
    using qp = TripleT.Datastructures.Queries;

    /// <summary>
    /// Represents an physical operator, part of a physical query plan.
    /// </summary>
    public abstract class Operator : IDisposable
    {
        protected bool m_hasNext;
        protected BindingSet m_next;
        protected qp::Operator m_planOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Operator"/> class.
        /// </summary>
        public Operator()
        {
            m_hasNext = false;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a next set of bindings available for
        /// consumption.
        /// </summary>
        public bool HasNext
        {
            get { return m_hasNext; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Attempts to read from this operator's underlying input stream(s) until the next set of
        /// bindings is found which can be outputted by this operator.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a next binding set is found available to output; otherwise, <c>false</c>.
        /// </returns>
        internal abstract bool TryReadNext();

        /// <summary>
        /// Gets the next set of output bindings available for this operator, and advances the
        /// output stream.
        /// </summary>
        /// <returns>
        /// A set of bindings.
        /// </returns>
        public BindingSet Next()
        {
            if (!m_hasNext) {
                throw new InvalidOperationException("No new bindings are available!");
            } else {
                var b = m_next;
                TryReadNext();
                return b;
            }
        }

        /// <summary>
        /// Gets the next set of output bindings available for this operator, but does not advance
        /// the output stream.
        /// </summary>
        /// <returns>
        /// A set of bindings.
        /// </returns>
        public BindingSet Peek()
        {
            if (!m_hasNext) {
                throw new InvalidOperationException("No new bindings are available!");
            } else {
                return m_next;
            }
        }

        /// <summary>
        /// Gets the descriptive counterpart of this query plan operator.
        /// </summary>
        public qp::Operator PlanOperator
        {
            get { return m_planOperator; }
        }
    }
}
