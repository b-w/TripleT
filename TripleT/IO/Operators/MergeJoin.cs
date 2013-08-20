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
    using TripleT.Algorithms;
    using TripleT.Datastructures;
    using qp = TripleT.Datastructures.Queries;

    /// <summary>
    /// Represents a physical operator for performing a naturual join operation via merging on two
    /// sets of bindings.
    /// </summary>
    public class MergeJoin : Operator
    {
        private readonly Operator m_left;
        private readonly Operator m_right;
        private readonly Variable[] m_sortOrder;
        private BindingBuffer m_tmpBuffer;
        private BindingSet m_leftNext;
        private BindingSet m_rightNext;
        private readonly BindingSetComparer m_comparer;
        private ReadingMode m_mode;
        private long m_blockSize;
        private long m_blockPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeJoin"/> class.
        /// </summary>
        /// <param name="planOperator">The descriptive query plan operator that is the counterpart of this physical operator.</param>
        /// <param name="left">This operator's left input operator.</param>
        /// <param name="right">This operator's right input operator.</param>
        /// <param name="inputSortOrder">The input sort order for the join variables.</param>
        public MergeJoin(qp::Operator planOperator, Operator left, Operator right, Variable[] inputSortOrder)
        {
            if (inputSortOrder.Length == 0) {
                throw new ArgumentOutOfRangeException("inputSortOrder", "Provide a non-empty input sort ordering!");
            }

            m_planOperator = planOperator;
            m_left = left;
            m_right = right;
            m_sortOrder = inputSortOrder;
            m_comparer = new BindingSetComparer(m_sortOrder);
            m_mode = ReadingMode.Advance;
            m_blockPos = 0;
            m_blockSize = 0;

            //
            // prepare the next set of output bindings

            TryReadNext();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_left.Dispose();
            m_right.Dispose();
            m_tmpBuffer.Dispose();
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
            //
            // set up the buffer if it doesn't already exist

            if (m_tmpBuffer == null) {
                if (m_left.HasNext) {
                    //
                    // peek from the left input operator (the one that will get buffered) to find
                    // out its size, and compute how many of those we can fit in the memory we are
                    // provided with

                    var pk = m_left.Peek();
                    var pkSize = (uint)pk.Count * Database.MEMORY_SIZE_BINDING;
                    var buffSlots = m_planOperator.MemorySize / pkSize;
                    int m;
                    if (buffSlots > Int32.MaxValue) {
                        m = Int32.MaxValue;
                    } else {
                        m = Convert.ToInt32(buffSlots);
                    }
                    m_tmpBuffer = new BindingBuffer(m);
                } else {
                    m_tmpBuffer = new BindingBuffer(8 * 1024);
                }
#if DEBUG
                m_tmpBuffer.SetPlanOperator(m_planOperator);
#endif
            }

            while (true) {
                switch (m_mode) {
                    case ReadingMode.None:
                        break;
                    case ReadingMode.Advance:
                        //
                        // in this mode, we advance the input streams until a join match is found.
                        // if this happens, we set the buffer's jumpback position to the current
                        // position, and switch to block read mode.

                        if (Advance()) {
                            if (IsJoinMatch(m_leftNext, m_rightNext)) {
                                m_tmpBuffer.SetJumpback();
                                m_blockSize = 1;
                                m_blockPos = 0;
                                m_mode = ReadingMode.BlockRead;
#if DEBUG
                                m_planOperator.AddResult();
#endif
                                m_next = Merge(m_leftNext, m_rightNext);
                                m_hasNext = true;
                                return true;
                            }
                        } else {
                            m_hasNext = false;
                            return false;
                        }
                        break;
                    case ReadingMode.BlockRead:
                        //
                        // in this mode, we read from the left input stream until either we cannot
                        // advance left any further, or we do not find any more join matches
                        // between the next left value and the current value for right.
                        // 
                        // once we can no longer advance left we advance right by one. if after
                        // doing so the right value is unchanged (with regards to the join
                        // variables), we do a jumpback in the buffer and go into block repeat
                        // mode.
                        //
                        // if the right value is changed, we need to check if it's still a join
                        // match with the current left value. if it is, we need to set the buffer's
                        // jumpback to the current position and remain in block read mode. if not,
                        // we go back into advance mode.

                        if (AdvanceLeft() && IsJoinMatch(m_leftNext, m_rightNext)) {
                            m_blockSize++;
#if DEBUG
                            m_planOperator.AddResult();
#endif
                            m_next = Merge(m_leftNext, m_rightNext);
                            m_hasNext = true;
                            return true;
                        } else {
                            var prev = m_rightNext;
                            if (AdvanceRight()) {
#if DEBUG
                                m_planOperator.StartCPUWork();
#endif
                                var c = m_comparer.Compare(prev, m_rightNext);
#if DEBUG
                                m_planOperator.StopCPUWork();
#endif
                                if (c != 0) {
                                    if (IsJoinMatch(m_leftNext, m_rightNext)) {
                                        m_tmpBuffer.SetJumpback();
                                        m_blockSize = 1;
                                        m_mode = ReadingMode.BlockRead;
#if DEBUG
                                        m_planOperator.AddResult();
#endif
                                        m_next = Merge(m_leftNext, m_rightNext);
                                        m_hasNext = true;
                                        return true;
                                    } else {
                                        m_mode = ReadingMode.Advance;
                                    }
                                } else {
                                    m_tmpBuffer.Jumpback();
                                    m_blockPos = 0;
                                    m_mode = ReadingMode.BlockRepeat;
                                }
                            } else {
                                m_hasNext = false;
                                return false;
                            }
                        }
                        break;
                    case ReadingMode.BlockRepeat:
                        //
                        // in this mode we repeat the block currently in the buffer (from the
                        // current jumpback position to the head). all items in that block are
                        // known join matches.

                        // after finishing the block repeat we advance the right input stream by
                        // one. if after doing so the right value is unchanged (with regards to the
                        // join variables), we do a jumpback in the buffer and go into block repeat
                        // mode.
                        //
                        // if the right value is changed, we need to check if it's still a join
                        // match with the current left value. if it is, we need to set the buffer's
                        // jumpback to the current position and remain in block read mode. if not,
                        // we go back into advance mode.

                        if (m_blockPos < m_blockSize) {
                            m_blockPos++;
                            m_leftNext = m_tmpBuffer.Next();
#if DEBUG
                            m_planOperator.AddResult();
#endif
                            m_next = Merge(m_leftNext, m_rightNext);
                            m_hasNext = true;
                            return true;
                        } else {
                            var prev = m_rightNext;
                            if (AdvanceRight()) {
#if DEBUG
                                m_planOperator.StartCPUWork();
#endif
                                var c = m_comparer.Compare(prev, m_rightNext);
#if DEBUG
                                m_planOperator.StopCPUWork();
#endif
                                if (c != 0) {
                                    if (IsJoinMatch(m_leftNext, m_rightNext)) {
                                        m_tmpBuffer.SetJumpback();
                                        m_blockSize = 1;
                                        m_mode = ReadingMode.BlockRead;
#if DEBUG
                                        m_planOperator.AddResult();
#endif
                                        m_next = Merge(m_leftNext, m_rightNext);
                                        m_hasNext = true;
                                        return true;
                                    } else {
                                        m_mode = ReadingMode.Advance;
                                    }
                                } else {
                                    m_tmpBuffer.Jumpback();
                                    m_blockPos = 0;
                                    m_mode = ReadingMode.BlockRepeat;
                                }
                            } else {
                                m_hasNext = false;
                                return false;
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Determines whether the two binding sets are a join match.
        /// </summary>
        /// <param name="left">The left binding set.</param>
        /// <param name="right">The right binding set.</param>
        /// <returns>
        ///   <c>true</c> if the two sets are a join match; otherwise, <c>false</c>.
        /// </returns>
        private bool IsJoinMatch(BindingSet left, BindingSet right)
        {
            if (left == null || right == null) {
                return false;
            }
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            //
            // for the sake of speed (we need it), let's just assume both sets contain the
            // sort-order variables (which are the join variables)

            for (int i = 0; i < m_sortOrder.Length; i++) {
                if (left[m_sortOrder[i]].Value.InternalValue != right[m_sortOrder[i]].Value.InternalValue) {
#if DEBUG
                    m_planOperator.StopCPUWork();
#endif
                    return false;
                }
            }
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            return true;
        }

        /// <summary>
        /// Merges the two specified binding sets together, returning a new binding set which
        /// contains bindings from the two given sets.
        /// </summary>
        /// <param name="left">The left binding set.</param>
        /// <param name="right">The right binding set.</param>
        /// <returns>
        /// A new binding set merging the two given sets.
        /// </returns>
        private BindingSet Merge(BindingSet left, BindingSet right)
        {
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            var b = new BindingSet();

            //
            // binding sets internally disregard (v : a) if they already have some (v : a')

            foreach (var item in left.Bindings) {
                b.Add(item);
            }
            foreach (var item in right.Bindings) {
                b.Add(item);
            }
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            return b;
        }

        /// <summary>
        /// Advances either the left or the right input stream, depending on which one is ahead of
        /// the other with respect to the join variables.
        /// </summary>
        /// <returns>
        /// <c>true</c> if one of the two input streams has been advanced; otherwise, <c>false</c>.
        /// </returns>
        private bool Advance()
        {
            if (m_leftNext == null && m_rightNext == null) {
                return AdvanceLeft() && AdvanceRight();
            }
#if DEBUG
            m_planOperator.StartCPUWork();
#endif
            var c = m_comparer.Compare(m_leftNext, m_rightNext);
#if DEBUG
            m_planOperator.StopCPUWork();
#endif
            if (c > 0) {
                //
                // left is larger, so advance right

                return AdvanceRight();
            } else {
                //
                // right is larger or they are equal, so advance left

                return AdvanceLeft();
            }
        }

        /// <summary>
        /// Advances the left input stream.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the input stream has been advanced; otherwise, <c>false</c>.
        /// </returns>
        private bool AdvanceLeft()
        {
            if (m_tmpBuffer.HasNext) {
                m_leftNext = m_tmpBuffer.Next();
                return true;
            } else if (m_left.HasNext) {
                m_tmpBuffer.Add(m_left.Next());
                m_leftNext = m_tmpBuffer.Next();
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Advances the right input stream.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the input stream has been advanced; otherwise, <c>false</c>.
        /// </returns>
        private bool AdvanceRight()
        {
            if (m_right.HasNext) {
                m_rightNext = m_right.Next();
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Denotes the current mode the merge join operator is in.
        /// </summary>
        private enum ReadingMode
        {
            None = 0,
            Advance,
            BlockRead,
            BlockRepeat
        }
    }
}
