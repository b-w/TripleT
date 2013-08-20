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

namespace TripleT.Test
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class ProcessMonitor
    {
        private static ProcessMonitor m_instance;
        private readonly Process m_process;
        private readonly PerformanceCounter m_cpuCounter;
        private readonly PerformanceCounter m_ramCounter;
        private readonly PerformanceCounter m_ioOpsCounter;
        private readonly PerformanceCounter m_ioBytesCounter;
        private Thread m_thread;
        private float[] m_cpuAvgs;
        private float[] m_ramAvgs;
        private float[] m_ioOpsAvgs;
        private float[] m_ioBytesAvgs;
        private long m_total;

        private ProcessMonitor()
        {
            m_process = Process.GetCurrentProcess();
            m_cpuCounter = new PerformanceCounter("Process", "% Processor Time", m_process.ProcessName);
            m_ramCounter = new PerformanceCounter("Process", "Working Set", m_process.ProcessName);
            m_ioOpsCounter = new PerformanceCounter("Process", "IO Data Operations/sec", m_process.ProcessName);
            m_ioBytesCounter = new PerformanceCounter("Process", "IO Data Bytes/sec", m_process.ProcessName);
        }

        private static ProcessMonitor Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new ProcessMonitor();

                return m_instance;
            }
        }

        public static void Start(int interval, int memory, int steps)
        {
            var pm = ProcessMonitor.Instance;

            pm.m_cpuAvgs = new float[memory];
            pm.m_ramAvgs = new float[memory];
            pm.m_ioOpsAvgs = new float[memory];
            pm.m_ioBytesAvgs = new float[memory];

            pm.m_thread = new Thread(() => ProcessMonitor.MonitorCycle(pm, interval, memory, steps));
            pm.m_thread.Start();
        }

        public static void Stop()
        {
            var pm = ProcessMonitor.Instance;
            pm.m_thread.Abort();
        }

        public static float CPUCurrent
        {
            get
            {
                var pm = ProcessMonitor.Instance;
                return pm.m_cpuCounter.NextValue();
            }
        }

        public static float MemoryCurrent
        {
            get
            {
                var pm = ProcessMonitor.Instance;
                return pm.m_ramCounter.NextValue();
            }
        }

        public static float IOOpsCurrent
        {
            get
            {
                var pm = ProcessMonitor.Instance;
                return pm.m_ioOpsCounter.NextValue();
            }
        }

        public static float IOBytesCurrent
        {
            get
            {
                var pm = ProcessMonitor.Instance;
                return pm.m_ioBytesCounter.NextValue();
            }
        }

        public static IEnumerable<float> CPUHistory
        {
            get
            {
                var pm = ProcessMonitor.Instance;

                lock (pm.m_cpuAvgs) {
                    var s = pm.m_cpuAvgs.Length;
                    var c = (pm.m_total > s) ? pm.m_total + 1 : 0;

                    for (int i = 0; i < s; i++) {
                        yield return pm.m_cpuAvgs[c % s];
                        c++;
                    }
                }
            }
        }

        public static IEnumerable<float> MemoryHistory
        {
            get
            {
                var pm = ProcessMonitor.Instance;

                lock (pm.m_ramAvgs) {
                    var s = pm.m_ramAvgs.Length;
                    var c = (pm.m_total > s) ? pm.m_total + 1 : 0;

                    for (int i = 0; i < s; i++) {
                        yield return pm.m_ramAvgs[c % s];
                        c++;
                    }
                }
            }
        }

        public static IEnumerable<float> IOOpsHistory
        {
            get
            {
                var pm = ProcessMonitor.Instance;

                lock (pm.m_ioOpsAvgs) {
                    var s = pm.m_ioOpsAvgs.Length;
                    var c = (pm.m_total > s) ? pm.m_total + 1 : 0;

                    for (int i = 0; i < s; i++) {
                        yield return pm.m_ioOpsAvgs[c % s];
                        c++;
                    }
                }
            }
        }

        public static IEnumerable<float> IOBytesHistory
        {
            get
            {
                var pm = ProcessMonitor.Instance;

                lock (pm.m_ioBytesAvgs) {
                    var s = pm.m_ioBytesAvgs.Length;
                    var c = (pm.m_total > s) ? pm.m_total + 1 : 0;

                    for (int i = 0; i < s; i++) {
                        yield return pm.m_ioBytesAvgs[c % s];
                        c++;
                    }
                }
            }
        }

        private static void MonitorCycle(ProcessMonitor instance, int interval, int memory, int steps)
        {
            var step = 0L;
            instance.m_total = 0L;

            var cpuSteps = new float[steps];
            var ramSteps = new float[steps];
            var ioOpsSteps = new float[steps];
            var ioBytesSteps = new float[steps];

            while (true) {
                cpuSteps[step] = instance.m_cpuCounter.NextValue();
                ramSteps[step] = instance.m_ramCounter.NextValue();
                ioOpsSteps[step] = instance.m_ioOpsCounter.NextValue();
                ioBytesSteps[step] = instance.m_ioBytesCounter.NextValue();

                step++;

                if (step >= steps) {
                    float avg;

                    avg = cpuSteps.Average();
                    lock (instance.m_cpuAvgs) {
                        instance.m_cpuAvgs[instance.m_total % memory] = avg;
                    }

                    avg = ramSteps.Average();
                    lock (instance.m_ramAvgs) {
                        instance.m_ramAvgs[instance.m_total % memory] = avg;
                    }

                    avg = ioOpsSteps.Average();
                    lock (instance.m_ioOpsAvgs) {
                        instance.m_ioOpsAvgs[instance.m_total % memory] = avg;
                    }

                    avg = ioBytesSteps.Average();
                    lock (instance.m_ioBytesAvgs) {
                        instance.m_ioBytesAvgs[instance.m_total % memory] = avg;
                    }

                    step = 0L;

                    instance.m_total++;
                }

                Thread.Sleep(interval);
            }
        }
    }
}
