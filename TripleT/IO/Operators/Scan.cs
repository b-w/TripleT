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
    using System.Collections.Generic;
    using TripleT.Datastructures;
    using qp = TripleT.Datastructures.Queries;

    /// <summary>
    /// Represents a physical operator for retrieving and matching triples from a triple bucket.
    /// </summary>
    public class Scan : Operator
    {
        private readonly TripleCursor m_cursor;
        private readonly SortOrder m_inputOrder;
        private readonly Triple<TripleItem, TripleItem, TripleItem> m_pattern;
        private readonly TriplePosition m_patternAtoms;
        private readonly bool m_isMiniBucket;
        private long m_count;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scan"/> class.
        /// </summary>
        /// <param name="planOperator">The descriptive query plan operator that is the counterpart of this physical operator.</param>
        /// <param name="cursor">The triple cursor used for reading in triples.</param>
        /// <param name="inputSortOrder">The sort order the triples arrive in.</param>
        /// <param name="pattern">The Simple Access Pattern to match.</param>
        /// <param name="count">The number of triples to read.</param>
        public Scan(qp::Operator planOperator, TripleCursor cursor, SortOrder inputSortOrder, Triple<TripleItem, TripleItem, TripleItem> pattern, long count = -1)
        {
            m_planOperator = planOperator;
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            m_cursor = cursor;
            m_inputOrder = inputSortOrder;
            m_pattern = pattern;

            if (count > -1) {
                m_isMiniBucket = true;
                m_count = count;
            } else {
                m_isMiniBucket = false;
            }

            //
            // identify the atoms in the given SAP to match

            m_patternAtoms = TriplePosition.None;
            if (m_pattern.S is Atom) {
                m_patternAtoms |= TriplePosition.S;
            }
            if (m_pattern.P is Atom) {
                m_patternAtoms |= TriplePosition.P;
            }
            if (m_pattern.O is Atom) {
                m_patternAtoms |= TriplePosition.O;
            }
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            //
            // prepare the next set of output bindings

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_cursor.Dispose();
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
            while (m_cursor.HasNext && (!m_isMiniBucket || m_count > 0)) {
#if DEBUG
                m_planOperator.StartIOWork();
#endif
                var next = m_cursor.Next();
#if DEBUG
                m_planOperator.StopIOWork();
                m_planOperator.StartCPUWork();
#endif

                if (m_isMiniBucket) {
                    m_count--;
                }

                if (IsMatch(next)) {
                    //
                    // if a triple has been found which matches the given SAP, prepare a binding
                    // set and add the atoms in positions where the SAP has a variable to the set

                    m_next = new BindingSet();

                    if (!m_patternAtoms.HasFlag(TriplePosition.S)) {
                        var b = new Binding(m_pattern.S as Variable, next.S);
                        m_next.Add(b);
                    }

                    if (!m_patternAtoms.HasFlag(TriplePosition.P)) {
                        var b = new Binding(m_pattern.P as Variable, next.P);
                        m_next.Add(b);
                    }

                    if (!m_patternAtoms.HasFlag(TriplePosition.O)) {
                        var b = new Binding(m_pattern.O as Variable, next.O);
                        m_next.Add(b);
                    }

                    m_hasNext = true;
#if DEBUG
                    m_planOperator.StopCPUWork();
                    m_planOperator.AddResult();
#endif
                    return true;
                } else if (!PossibleMatchesAfter(next)) {
                    //
                    // if the current triple does not match the SAP, see if it is possible that
                    // there are still matches some time after this triple. if there are no more
                    // matches possible, we are done here.
#if DEBUG
                    m_planOperator.StopCPUWork();
#endif
                    break;
                }
#if DEBUG
                m_planOperator.StopCPUWork();
#endif
            }

            m_hasNext = false;
            return false;
        }

        /// <summary>
        /// Determines whether the specified triple matches the operator's SAP.
        /// </summary>
        /// <param name="triple">The triple.</param>
        /// <returns>
        ///   <c>true</c> if the specified triple is matches the operator's SAP; otherwise, <c>false</c>.
        /// </returns>
        private bool IsMatch(Triple<Atom, Atom, Atom> triple)
        {
            if (triple == null) {
                return false;
            } else {
                var bindings = new Dictionary<long, long>();

                //
                // for a triple to match the SAP, it needs to have the same values in every
                // position where the SAP has an atom value

                if (m_patternAtoms.HasFlag(TriplePosition.S)) {
                    if (m_pattern.S.InternalValue != triple.S.InternalValue) {
                        return false;
                    }
                } else {
                    bindings.Add(m_pattern.S.InternalValue, triple.S.InternalValue);
                }

                if (m_patternAtoms.HasFlag(TriplePosition.P)) {
                    if (m_pattern.P.InternalValue != triple.P.InternalValue) {
                        return false;
                    }
                } else {
                    if (bindings.ContainsKey(m_pattern.P.InternalValue)) {
                        if (bindings[m_pattern.P.InternalValue] != triple.P.InternalValue) {
                            return false;
                        }
                    } else {
                        bindings.Add(m_pattern.P.InternalValue, triple.P.InternalValue);
                    }
                }

                if (m_patternAtoms.HasFlag(TriplePosition.O)) {
                    if (m_pattern.O.InternalValue != triple.O.InternalValue) {
                        return false;
                    }
                } else {
                    if (bindings.ContainsKey(m_pattern.O.InternalValue)) {
                        if (bindings[m_pattern.O.InternalValue] != triple.O.InternalValue) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Computes wether it is possible for there to be any more triples matching the current
        /// SAP after the given triple, considering the input sort order of the triples.
        /// </summary>
        /// <param name="triple">The triple.</param>
        /// <returns>
        /// <c>true</c> if given the current sort order there can still follow triples matching the current SAP; otherwise, <c>false</c>.
        /// </returns>
        private bool PossibleMatchesAfter(Triple<Atom, Atom, Atom> triple)
        {
            if (m_patternAtoms.HasFlag(m_inputOrder.Primary)) {
                if (triple[m_inputOrder.Primary].InternalValue > m_pattern[m_inputOrder.Primary].InternalValue) {
                    return false;
                } else {
                    if (m_patternAtoms.HasFlag(m_inputOrder.Secondary)) {
                        if (triple[m_inputOrder.Secondary].InternalValue > m_pattern[m_inputOrder.Secondary].InternalValue) {
                            return false;
                        } else {
                            if (m_patternAtoms.HasFlag(m_inputOrder.Tertiary)) {
                                if (triple[m_inputOrder.Tertiary].InternalValue > m_pattern[m_inputOrder.Tertiary].InternalValue) {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
