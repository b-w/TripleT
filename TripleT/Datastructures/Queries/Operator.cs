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

namespace TripleT.Datastructures.Queries
{
    using System.Diagnostics;

    /// <summary>
    /// Represents an operator in a descriptive query plan. This operator does not perform any
    /// physical operations.
    /// </summary>
    public abstract class Operator
    {
        private uint m_memorySize;
#if DEBUG
        private readonly Stopwatch m_cpuWatch;
        private readonly Stopwatch m_ioWatch;
        private int m_cpuCount;
        private int m_ioCount;
        private long m_resCount;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Operator"/> class.
        /// </summary>
        public Operator()
        {
#if DEBUG
            m_cpuWatch = new Stopwatch();
            m_ioWatch = new Stopwatch();
            m_cpuCount = 0;
            m_ioCount = 0;
            m_resCount = 0;
#endif
        }

        /// <summary>
        /// Gets the amount of memory in bytes allocated to this operator.
        /// </summary>
        /// <value>
        /// The amount of memory in bytes.
        /// </value>
        public uint MemorySize
        {
            get { return m_memorySize; }
            internal set { m_memorySize = value; }
        }

#if DEBUG
        /// <summary>
        /// Gets the time (in milliseconds) spent by this operator on CPU work.
        /// </summary>
        public long CPUTime
        {
            get { return m_cpuWatch.ElapsedMilliseconds; }
        }

        /// <summary>
        /// Gets the time (in milliseconds) spent by this operator on I/O work.
        /// </summary>
        public long IOTime
        {
            get { return m_ioWatch.ElapsedMilliseconds; }
        }

        /// <summary>
        /// Gets the number of triples returned by this operator.
        /// </summary>
        public long ResultCount
        {
            get { return m_resCount; }
        }

        /// <summary>
        /// Indicates that this operator should start the stopwatch used for measuring the amount
        /// of time spent on CPU work.
        /// </summary>
        internal void StartCPUWork()
        {
            if (m_cpuCount == 0) {
                m_cpuWatch.Start();
            }

            m_cpuCount++;
        }

        /// <summary>
        /// Indicates that this operator should stop the stopwatch used for measuring the amount
        /// of time spent on CPU work.
        /// </summary>
        internal void StopCPUWork()
        {
            m_cpuCount--;

            if (m_cpuCount == 0) {
                m_cpuWatch.Stop();
            }
        }

        /// <summary>
        /// Indicates that this operator should start the stopwatch used for measuring the amount
        /// of time spent on I/O work.
        /// </summary>
        internal void StartIOWork()
        {
            if (m_ioCount == 0) {
                m_ioWatch.Start();
            }

            m_ioCount++;
        }

        /// <summary>
        /// Indicates that this operator should stop the stopwatch used for measuring the amount
        /// of time spent on I/O work.
        /// </summary>
        internal void StopIOWork()
        {
            m_ioCount--;

            if (m_ioCount == 0) {
                m_ioWatch.Stop();
            }
        }

        /// <summary>
        /// Indicates that the counter measuring the amount of triples returned by this operator
        /// should be incremented by one.
        /// </summary>
        internal void AddResult()
        {
            m_resCount++;
        }
#endif
    }
}
