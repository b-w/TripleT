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

namespace TripleT.Reporting
{
    using System.IO;
    using TripleT.Test;

    public static class CSVWriter
    {
        public static void WriteProcessMonitorHistory(string filename)
        {
            using (var tw = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))) {
                tw.WriteLine("Process Monitor History");
                tw.Write("CPU");
                foreach (var item in ProcessMonitor.CPUHistory) {
                    tw.Write(";");
                    tw.Write(item);
                }
                tw.WriteLine();
                tw.Write("Memory");
                foreach (var item in ProcessMonitor.MemoryHistory) {
                    tw.Write(";");
                    tw.Write(item);
                }
                tw.WriteLine();
                tw.Write("I/O Ops/sec");
                foreach (var item in ProcessMonitor.IOOpsHistory) {
                    tw.Write(";");
                    tw.Write(item);
                }
                tw.WriteLine();
                tw.Write("I/O Bytes/sec");
                foreach (var item in ProcessMonitor.IOBytesHistory) {
                    tw.Write(";");
                    tw.Write(item);
                }
                tw.WriteLine();
                tw.Flush();
            }
        }
    }
}
