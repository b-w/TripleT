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
    /// <summary>
    /// Represents a descriptive query plan, which is intended for communicating query plans but
    /// does not perform any physical query doperations.
    /// </summary>
    public class QueryPlan
    {
        private readonly Operator m_root;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryPlan"/> class.
        /// </summary>
        /// <param name="root">The root operator for the plan.</param>
        public QueryPlan(Operator root)
        {
            m_root = root;
        }

        /// <summary>
        /// Gets the root operator for the query plan.
        /// </summary>
        public Operator Root
        {
            get { return m_root; }
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value.</param>
        /// <param name="p">The p value.</param>
        /// <param name="o">The o value.</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, object s, object p, object o, string startAtom = "")
        {
            if (s is string) {
                if (p is string) {
                    if (o is string) {
                        return GetScan(bucket, (string)s, (string)p, (string)o, startAtom);
                    } else {
                        return GetScan(bucket, (string)s, (string)p, (long)o, startAtom);
                    }
                } else {
                    if (o is string) {
                        return GetScan(bucket, (string)s, (long)p, (string)o, startAtom);
                    } else {
                        return GetScan(bucket, (string)s, (long)p, (long)o, startAtom);
                    }
                }
            } else {
                if (p is string) {
                    if (o is string) {
                        return GetScan(bucket, (long)s, (string)p, (string)o, startAtom);
                    } else {
                        return GetScan(bucket, (long)s, (string)p, (long)o, startAtom);
                    }
                } else {
                    if (o is string) {
                        return GetScan(bucket, (long)s, (long)p, (string)o, startAtom);
                    } else {
                        return GetScan(bucket, (long)s, (long)p, (long)o, startAtom);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (atom).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, string s, string p, string o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (atom).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, long s, string p, string o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (atom).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, string s, long p, string o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (variable).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, string s, string p, long o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (atom).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, long s, long p, string o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (atom).</param>
        /// <param name="o">The o value (variable).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, long s, string p, long o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (atom).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (variable).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, string s, long p, long o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive scan operator for the given bucket, SAP, and optional starting atom.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <param name="s">The s value (variable).</param>
        /// <param name="p">The p value (variable).</param>
        /// <param name="o">The o value (variable).</param>
        /// <param name="startAtom">The starting atom.</param>
        /// <returns>
        /// A descriptive scan operator for the given bucket, SAP, and starting atom.
        /// </returns>
        public static Scan GetScan(TriplePosition bucket, long s, long p, long o, string startAtom = "")
        {
            var pattern = new Pattern(s, p, o);
            return new Scan(bucket, pattern, startAtom);
        }

        /// <summary>
        /// Gets a descriptive sort operator for the given input operator and sorting order.
        /// </summary>
        /// <param name="input">The input operator.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns>
        /// A descriptive sort operator for the given input operator and sorting order.
        /// </returns>
        public static Sort GetSort(Operator input, params long[] sortOrder)
        {
            return new Sort(input, sortOrder);
        }

        /// <summary>
        /// Gets a descriptive merge join operator for the given input operators and sorting order.
        /// </summary>
        /// <param name="left">The left input operator.</param>
        /// <param name="right">The right input operator.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns>
        /// A descriptive merge join operator for the given input operator and sorting order.
        /// </returns>
        public static MergeJoin GetMergeJoin(Operator left, Operator right, params long[] sortOrder)
        {
            return new MergeJoin(left, right, sortOrder);
        }

        /// <summary>
        /// Gets a descriptive hash join operator for the given input operators.
        /// </summary>
        /// <param name="left">The left input operator.</param>
        /// <param name="right">The right input operator.</param>
        /// <returns>
        /// A descriptive hash join operator for the given input operators.
        /// </returns>
        public static HashJoin GetHashJoin(Operator left, Operator right)
        {
            return new HashJoin(left, right);
        }
    }
}
