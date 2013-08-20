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

namespace TripleT.Examples
{
    using System;
    using System.IO;
    using TripleT.Compatibility;
    using TripleT.Datastructures.Queries;

    class Program
    {
        static void Main(string[] args)
        {
            //
            // first build the database. this only needs to happen on the
            // first run. after that, comment out this line.

            BuildExampleDatabase();

            //
            // next, run some queries!

            QueryExampleDatabase();

            Console.WriteLine("Program completed by your command.");
            Console.ReadKey();
        }

        static void BuildExampleDatabase()
        {
            using (var db = new Database("Data\\dbp-persons-1K")) {
                db.Open();

                db.BeginBatchInsert();
                using (var reader = new SplitlineTripleReader(File.OpenRead("Data\\dbp-persons-1K.txt"), "|*|")) {
                    db.InsertAll(reader);
                }
                db.EndBatchInsert();

                db.BuildIndex();
            }
        }

        static void QueryExampleDatabase()
        {
            using (var db = new Database("Data\\dbp-persons-1K")) {
                db.Open();

                var pattern = new Pattern[]
                    {
                        new Pattern(
                            "http://dbpedia.org/resource/Animal_Farm",
                            "http://dbpedia.org/ontology/author",
                            1),
                        new Pattern(
                            1,
                            "http://dbpedia.org/ontology/influencedBy",
                            2),
                        new Pattern(
                            2,
                            "http://dbpedia.org/ontology/birthDate",
                            3)
                    };

                var plan = db.GetQueryPlan(pattern);

                foreach (var bindingSet in db.Query(plan)) {
                    Console.WriteLine("{");
                    foreach (var binding in bindingSet.Bindings) {
                        Console.WriteLine("\t{0} := {1}",
                            binding.Variable.InternalValue,
                            binding.Value.TextValue);
                    }
                    Console.WriteLine("}");
                }
            }
        }
    }
}
