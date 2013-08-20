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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using TripleT.Datastructures.Queries;
    using TripleT.Reporting;
    using TripleT.Util;

    public static class BatchTester
    {
        public static void Run(IEnumerable<string> datasets, IEnumerable<Tuple<string, Pattern[]>> namedQueries)
        {
            foreach (var dataset in datasets) {
#if DEBUG
                Console.WriteLine("[{0}] Starting dataset: {1}...", DateTime.Now, dataset);
#endif
                Run(dataset, namedQueries);
#if DEBUG
                Console.WriteLine("[{0}] Finished dataset: {1}...", DateTime.Now, dataset);
#endif
            }
        }

        public static void Run(string dataset, IEnumerable<Tuple<string, Pattern[]>> namedQueries)
        {
            foreach (var query in namedQueries) {
#if DEBUG
                Console.Write("[{0}] Query: {1}... ", DateTime.Now, query.Item1);
#endif
                Run(dataset, query);
#if DEBUG
                Console.WriteLine("DONE!");
#endif
            }

            using (var sw = new StreamWriter(String.Format("{0}.res.txt", dataset))) {
                foreach (var query in namedQueries) {
                    var fNamePrefix = String.Format("{0}.{1}", dataset, query.Item1);
                    using (var sr = new StreamReader(String.Format("{0}.res.txt", fNamePrefix))) {
                        var res = sr.ReadLine();
                        var time = sr.ReadLine();
                        sw.WriteLine(time);
                    }
                }
            }
        }

        public static void Run(IEnumerable<string> datasets, Tuple<string, Pattern[]> namedQuery)
        {
            foreach (var dataset in datasets) {
#if DEBUG
                Console.Write("[{0}] Dataset: {1}...", DateTime.Now, dataset);
#endif
                Run(dataset, namedQuery);
#if DEBUG
                Console.WriteLine("DONE!");
#endif
            }
        }

        public static void Run(string dataset, Tuple<string, Pattern[]> namedQuery)
        {
            using (var db = new Database(dataset)) {
                // open database
                db.Open();

                // get query plan
                var plan = db.GetQueryPlan(namedQuery.Item2);

#if DEBUG
                // start process monitoring...
                ProcessMonitor.Start(100, 4000, 3);
#endif

                // execute query
                var watch = new Stopwatch();
                var c = 0L;
                watch.Start();
                foreach (var item in db.Query(plan, false)) {
                    c++;
                }
                watch.Stop();

#if DEBUG
                // ...stop process monitoring
                ProcessMonitor.Stop();
#endif

                // produce reports
                var fNamePrefix = String.Format("{0}.{1}", dataset, namedQuery.Item1);
#if DEBUG
                CSVWriter.WriteProcessMonitorHistory(String.Format("{0}.proc.csv", fNamePrefix));
                TikzWriter.Write(plan, String.Format("{0}.tikz.txt", fNamePrefix), true);
#else
                TikzWriter.Write(plan, String.Format("{0}.tikz.txt", fNamePrefix), false);
#endif
                using (var sw = new StreamWriter(String.Format("{0}.res.txt", fNamePrefix))) {
                    sw.WriteLine(c);
                    sw.Write(watch.ElapsedMilliseconds.ToString());
                }

                // close database
                db.Close();
            }

            // clean up after yourself
            GC.Collect();
        }

        public static void Run(string dataset, IEnumerable<Tuple<string, Pattern[]>> namedQueries, int numRuns)
        {
            foreach (var item in namedQueries) {
                Run(dataset, item, numRuns);
            }
        }

        public static void Run(string dataset, Tuple<string, Pattern[]> namedQuery, int numRuns)
        {
            var runTimes = new List<string>();
            for (int i = 0; i < numRuns; i++) {
#if DEBUG
                Logger.WriteLine("Starting {0}, query {1}, run {2:00}...", dataset, namedQuery.Item1, i + 1);
#endif
                using (var db = new Database(dataset)) {
                    // open database
                    db.Open();

                    // get query plan
                    var plan = db.GetQueryPlan(namedQuery.Item2);

#if DEBUG
                    // start process monitoring...
                    ProcessMonitor.Start(100, 4000, 3);
#endif

                    // execute query
                    var watch = new Stopwatch();
                    var c = 0L;
                    watch.Start();
                    foreach (var item in db.Query(plan, false)) {
                        c++;
                    }
                    watch.Stop();

#if DEBUG
                    // ...stop process monitoring
                    ProcessMonitor.Stop();
#endif

                    var dir = String.Format("{0}.{1}", dataset, namedQuery.Item1);
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }

                    // produce reports
                    var fNamePrefix = String.Format("{0}.{1}\\run-{2:00}", dataset, namedQuery.Item1, i);
#if DEBUG
                    CSVWriter.WriteProcessMonitorHistory(String.Format("{0}.proc.csv", fNamePrefix));
                    TikzWriter.Write(plan, String.Format("{0}.tikz.txt", fNamePrefix), true);
#else
                TikzWriter.Write(plan, String.Format("{0}.tikz.txt", fNamePrefix), false);
#endif
                    using (var sw = new StreamWriter(String.Format("{0}.res.txt", fNamePrefix))) {
                        sw.WriteLine(c);
                        var rt = watch.ElapsedMilliseconds.ToString();
                        runTimes.Add(rt);
                        sw.Write(rt);
                    }

                    // close database
                    db.Close();
                }

                // clean up after yourself
                GC.Collect();
            }

            // write times for all runs
            using (var sw = new StreamWriter(String.Format("{0}.{1}\\times.txt", dataset, namedQuery.Item1))) {
                foreach (var item in runTimes) {
                    sw.WriteLine(item);
                }
            }
        }
    }
}
