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

namespace TripleT.Util
{
    using System;
    using System.Text;

    /// <summary>
    /// Represents a centralized instance for generating random values using a persistent instance
    /// of a <see cref="System.Random"/> class.
    /// </summary>
    public class Generator
    {
        private static Generator m_instance;

        /// <summary>
        /// Gets the generator instance.
        /// </summary>
        private static Generator Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new Generator();

                return m_instance;
            }
        }

        private readonly Random m_rnd;
        private readonly char[] m_fileNameChars;

        /// <summary>
        /// Prevents a default instance of the <see cref="Generator"/> class from being created.
        /// </summary>
        private Generator()
        {
            m_rnd = new Random();
            m_fileNameChars = new char[26 + 26 + 10];
            var i = 0;
            var c = 'a';
            for (int j = 0; j < 26; j++) {
                m_fileNameChars[i] = c;
                c++;
                i++;
            }
            c = 'A';
            for (int j = 0; j < 26; j++) {
                m_fileNameChars[i] = c;
                c++;
                i++;
            }
            c = '0';
            for (int j = 0; j < 10; j++) {
                m_fileNameChars[i] = c;
                c++;
                i++;
            }
        }

        /// <summary>
        /// Gets a random number between 0 (inclusive) and a given upper bound (exclusive).
        /// </summary>
        /// <param name="upperBound">The exclusive upper bound for the number generated.</param>
        /// <returns>
        /// A randomly selected integer.
        /// </returns>
        public static int GetRandomNumber(int upperBound)
        {
            if (upperBound <= 0) {
                throw new ArgumentOutOfRangeException("upperBound", "Upper bound must be larger than 0!");
            }

            var gen = Generator.Instance;
            return gen.m_rnd.Next(0, upperBound);
        }

        /// <summary>
        /// Gets a random number between 0 (inclusive) and a given upper bound (exclusive).
        /// </summary>
        /// <param name="lowerBound">The inclusive lower bound for the number generated.</param>
        /// <param name="upperBound">The exclusive upper bound for the number generated.</param>
        /// <returns>
        /// A randomly selected integer.
        /// </returns>
        public static int GetRandomNumber(int lowerBound, int upperBound)
        {
            if (upperBound <= lowerBound) {
                throw new ArgumentOutOfRangeException("upperBound", "Upper bound must be larger than the lower bound!");
            }

            var gen = Generator.Instance;
            return gen.m_rnd.Next(lowerBound, upperBound);
        }

        /// <summary>
        /// Gets a random number between 0 (inclusive) and a given upper bound (exclusive).
        /// </summary>
        /// <param name="upperBound">The exclusive upper bound for the number generated.</param>
        /// <returns>
        /// A randomly selected integer.
        /// </returns>
        public static long GetRandomNumber(long upperBound)
        {
            if (upperBound <= 0) {
                throw new ArgumentOutOfRangeException("upperBound", "Upper bound must be larger than 0!");
            }

            var gen = Generator.Instance;
            return (long)(gen.m_rnd.NextDouble() * upperBound);
        }

        /// <summary>
        /// Gets a randomly generated filename of a given length. The file name returned will be
        /// a valid file name, and will not have a file extension.
        /// </summary>
        /// <param name="length">The length of the file name to be generated.</param>
        /// <returns>
        /// A randomly generated string which is a valid file name.
        /// </returns>
        public static string GetRandomFilename(int length)
        {
            var gen = Generator.Instance;
            var strBuild = new StringBuilder(length);
            for (int i = 0; i < length; i++) {
                strBuild.Append(gen.m_fileNameChars[gen.m_rnd.Next(0, gen.m_fileNameChars.Length)]);
            }
            return strBuild.ToString();
        }
    }
}
