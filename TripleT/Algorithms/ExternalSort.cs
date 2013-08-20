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

namespace TripleT.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TripleT.Datastructures;
    using TripleT.IO;
    using TripleT.Util;

    /// <summary>
    /// Static class containing functionality for external sorting of triple buckets as well as
    /// files containing n-tuples.
    /// </summary>
    public static class ExternalSort
    {
        /// <summary>
        /// Sort a TripleT bucket in external memory. The bucket file is overwritten.
        /// </summary>
        /// <param name="bucket">The bucket to sort.</param>
        /// <param name="maxTriplesInMemory">The maximum number of triples to keep in memory at any point.</param>
        public static void SortBucket(Bucket bucket, int maxTriplesInMemory)
        {
            //
            // step 1: sort in chuncks that fit in the memory we have. the size of each chunk is
            // equal to the amount of memory provided (given in numbers of triples)

            //
            // temp files used to store the sorted chunks
            var chunkFiles = new List<string>();

            //
            // IComparer used to compare two triples.
            // the priorities are determined by the type of bucket and in what order the bucket
            // should be sorted
            var sorter = new TripleComparer(bucket.SortOrder.Primary, bucket.SortOrder.Secondary, bucket.SortOrder.Tertiary);

            //
            // open the bucket for reading, using a regular triple cursor
            using (var cursor = bucket.OpenRead()) {
                //
                // counter to keep track of how much triples we've read
                var i = 0;

                //
                // current chunk, represented as an array of triples
                var chunk = new Triple<Atom, Atom, Atom>[maxTriplesInMemory];

                //
                // we read as long as we have not yet reached the end of the bucket
                while (cursor.HasNext) {
                    //
                    // if we still have space in the chunk, read the next triple from the bucket

                    if (i < maxTriplesInMemory) {
                        chunk[i] = cursor.Next();
                        i++;
                    }
                        //
                        // if not, we need to sort and output the current chunk.
                        // for this part sorting is done in-memory
                    else {
                        //
                        // use default array sorting using the IComparer we have defined earlier
                        Array.Sort(chunk, sorter);

                        //
                        // generate a file name for the chunk. it needs to be unique for obvious
                        // reasons
                        var fileName = String.Format("~{0}.chunk.tmp", Generator.GetRandomFilename(12));

                        //
                        // add the file to the list of chunk files. we'll need it later
                        chunkFiles.Add(fileName);

                        //
                        // open a binary writer and write the sorted triples from the current chunk
                        // to the chunk file
                        using (var sw = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))) {
                            for (int j = 0; j < chunk.Length; j++) {
                                if (chunk[j] != null) {
                                    TripleSerializer.Write(sw, chunk[j]);
                                }
                            }
                        }

                        //
                        // clear the chunk array as it might not get completely overwritten during
                        // the next pass
                        Array.Clear(chunk, 0, chunk.Length);

                        //
                        // reset the triples-read counter
                        i = 0;
                    }
                }

                //
                // here, we have read the entire bucket, but there still might be some triples left
                // in the last chunk that we need to handle.
                // the code to do this is duplicated from the else block above.
                if (i > 0) {
                    Array.Sort(chunk, sorter);
                    var fileName = String.Format("~{0}.chunk.tmp", Generator.GetRandomFilename(12));
                    chunkFiles.Add(fileName);
                    using (var sw = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))) {
                        for (int j = 0; j < chunk.Length; j++) {
                            if (chunk[j] != null) {
                                TripleSerializer.Write(sw, chunk[j]);
                            }
                        }
                    }
                }
            }

            //
            // force garbage collection
            GC.Collect();

            //
            // step 2: perform k-way merge sort on the chunks. now that we have the sorted chunks,
            // we need to merge them and write out the final result.

            //
            // the number of pages used for reading must be equal to the number of chunk files
            var numReadPages = chunkFiles.Count;

            //
            // the total number of pages has one additional page for writing the output to
            var numPages = numReadPages + 1;

            //
            // the size of each page is the number of triples that may be kept in memory
            // devided by the number of pages. WARNING: if the amount of memory available is
            // too small in relation to the size of the input bucket, then there may be too
            // many chunks, hence too many pages to fit in memory when each page should have
            // at least space for 1 triple.
            var pageSize = maxTriplesInMemory / numPages;

            //
            // the buffer simply contains the pages
            var buffer = new TripleBuffer(numPages, pageSize);

            //
            // the last page is the one used for writing the output to. this variable is a shortcut
            // that allows us to say buffer[pageOutputId] instead of buffer[numPages - 1].
            var pageOutputId = numPages - 1;

            //
            // the page cursors are simply integers denoting the position of the reading pointers
            // in each of the pages
            var pageCursors = new int[numPages];

            //
            // the chunk cursors are triple cursors pointing to each of the chunks from the
            // previous step
            var chunkCursors = new TripleCursor[numReadPages];

            //
            // these variables keep the currenly smallest triple found (e.g. the one that needs to
            // be written next to the output file to maintain the sort order), and the Id of the
            // chunk this triple resides in
            Triple<Atom, Atom, Atom> minTriple = null;
            var minChunkId = -1;

            //
            // we start by initializing the cursors and reading the initial parts of all the chunks
            // into their respective buffer pages
            for (int i = 0; i < numReadPages; i++) {
                pageCursors[i] = 0;
                chunkCursors[i] = new TripleCursor(File.Open(chunkFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read));
                ReadToPage(chunkCursors[i], buffer[i]);
            }

            //
            // we open a binary writer to write the output (the sorted bucket) to. note we just
            // overwrite the existing bucket. this is possible because all information from the
            // original bucket is now duplicated in the sorted chunks anyway.
            using (var sw = new BinaryWriter(File.Open(bucket.FileName, FileMode.Create, FileAccess.Write, FileShare.None))) {
                //
                // entering the main loop of the merge that we will remain in for as long as there
                // are triples in any of the pages used for reading
                do {
                    //
                    // reset the next triple in the sort order to null. this is used to determine
                    // if can leave the main loop.
                    minTriple = null;

                    //
                    // here we find the next triple in the sort order (the smallest triple) in any
                    // of the read pages
                    for (int i = 0; i < numReadPages; i++) {
                        //
                        // fetch the current smallest triple in this page. it will always be the
                        // one the respective cursor is pointing to.
                        var t = buffer[i][pageCursors[i]];

                        //
                        // it's possible the page does not contain (any more) triples. if so we
                        // can just ignore it. for this we do assume that if we read a null from
                        // position i at some page, then all positions j > i on that same page are
                        // also null
                        if (t != null) {
                            //
                            // if the current smallest triple doesn't exist yet, then the one we've
                            // just read is trivially the new smallest triple
                            if (minTriple == null) {
                                minTriple = t;
                                minChunkId = i;
                            }
                                //
                                // if a smallest triple does exist, we need to do a comparison first.
                                // we use the same IComparer that we defined earlier
                            else {
                                var c = sorter.Compare(t, minTriple);
                                if (c < 0) {
                                    minTriple = t;
                                    minChunkId = i;
                                }
                            }
                        }
                    }

                    //
                    // checking if a new smallest triple has been identified. if it is we need to
                    // write it to the output page. if not, we don't need to do anything and will
                    // exit the main loop.
                    if (minTriple != null) {
                        //
                        // we increment the cursor belonging to the page where the next smallest
                        // triple resides
                        pageCursors[minChunkId]++;

                        //
                        // check if we have read all the triples on this page. if so, read the next
                        // set of triples from the chunk file and reset the cursor
                        if (pageCursors[minChunkId] >= pageSize) {
                            ReadToPage(chunkCursors[minChunkId], buffer[minChunkId]);
                            pageCursors[minChunkId] = 0;
                        }

                        //
                        // put the next smallest triple in the output page
                        buffer[pageOutputId][pageCursors[pageOutputId]] = minTriple;

                        //
                        // increment the output page cursor
                        pageCursors[pageOutputId]++;

                        //
                        // check if the output page is full. if so, write its triples to the output
                        // file, clear the page, and reset the cursor
                        if (pageCursors[pageOutputId] >= pageSize) {
                            for (int i = 0; i < pageSize; i++) {
                                TripleSerializer.Write(sw, buffer[pageOutputId][i]);
                            }
                            buffer[pageOutputId].Clear();
                            pageCursors[pageOutputId] = 0;
                        }
                    }
                } while (minTriple != null);

                //
                // the triples from all the chunks have been read and sorted, but the last couple
                // of them may still reside on the output page. here we write these last ones to
                // the output file.
                for (int i = 0; i < pageSize; i++) {
                    var t = buffer[pageOutputId][i];
                    if (t != null) {
                        TripleSerializer.Write(sw, t);
                    } else {
                        break;
                    }
                }
            }

            //
            // close the chunk cursors and delete the chunk files
            for (int i = 0; i < numReadPages; i++) {
                chunkCursors[i].Dispose();
                File.Delete(chunkFiles[i]);
            }
        }

        /// <summary>
        /// Reads from a given triple cursor and stores the read triples in a triple buffer page.
        /// This will attempt to read a much triples as fit on the given page.
        /// </summary>
        /// <param name="cursor">The triple cursor.</param>
        /// <param name="page">The triple buffer page.</param>
        private static void ReadToPage(TripleCursor cursor, TriplePage page)
        {
            page.Clear();
            for (int i = 0; i < page.Size; i++) {
                if (!cursor.HasNext) {
                    break;
                } else {
                    page[i] = cursor.Next();
                }
            }
        }

        /// <summary>
        /// Sort an n-tuple file in external memory. The file is overwritten.
        /// </summary>
        /// <param name="file">The n-tuple file.</param>
        /// <param name="itemLength">The length of the tuples in the file.</param>
        /// <param name="maxItemsInMemory">The maximum amount of n-tuples to keep in memory.</param>
        /// <param name="sortOrder">The sort order to use.</param>
        public static void SortFile(string file, int itemLength, int maxItemsInMemory, params int[] sortOrder)
        {
            //
            // step 1: sort in chuncks that fit in the memory we have. the size of each chunk is
            // equal to the amount of memory provided (given in numbers of items)

            //
            // temp files used to store the sorted chunks
            var chunkFiles = new List<string>();

            //
            // IComparer used to compare two items. the priorities are given.
            var sorter = new NComparer(sortOrder);

            //
            // open the file for reading
            using (var br = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))) {
                //
                // counter to keep track of how much items we've read
                var i = 0;

                //
                // current chunk, represented as an array of items
                var chunk = new long[maxItemsInMemory][];

                //
                // we read as long as we have not yet reached the end of the file
                while (br.BaseStream.Position < br.BaseStream.Length) {
                    //
                    // if we still have space in the chunk, read the next triple from the file
                    if (i < maxItemsInMemory) {
                        chunk[i] = Read(br, itemLength);
                        i++;
                    }
                        //
                        // if not, we need to sort and output the current chunk.
                        // for this part sorting is done in-memory
                    else {
                        //
                        // use default array sorting using the IComparer we have defined earlier
                        Array.Sort(chunk, sorter);

                        //
                        // generate a file name for the chunk. it needs to be unique for obvious
                        // reasons
                        var fileName = String.Format("~{0}.chunk.tmp", Generator.GetRandomFilename(12));

                        //
                        // add the file to the list of chunk files. we'll need it later
                        chunkFiles.Add(fileName);

                        //
                        // open a binary writer and write the sorted items from the current chunk
                        // to the chunk file
                        using (var bw = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))) {
                            for (int j = 0; j < chunk.Length; j++) {
                                if (chunk[j] != null) {
                                    Write(bw, chunk[j]);
                                }
                            }
                        }

                        //
                        // clear the chunk array as it might not get completely overwritten during
                        // the next pass
                        Array.Clear(chunk, 0, chunk.Length);

                        //
                        // reset the triples-read counter
                        i = 0;
                    }
                }

                //
                // here, we have read the entire file, but there still might be some items left
                // in the last chunk that we need to handle.
                // the code to do this is duplicated from the else block above.
                if (i > 0) {
                    Array.Sort(chunk, sorter);

                    var fileName = String.Format("~{0}.chunk.tmp", Generator.GetRandomFilename(12));
                    chunkFiles.Add(fileName);

                    using (var bw = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))) {
                        for (int j = 0; j < chunk.Length; j++) {
                            if (chunk[j] != null) {
                                Write(bw, chunk[j]);
                            }
                        }
                    }
                }
            }

            //
            // force garbage collection
            GC.Collect();

            //
            // step 2: perform k-way merge sort on the chunks. now that we have the sorted chunks,
            // we need to merge them and write out the final result.

            //
            // the number of pages used for reading must be equal to the number of chunk files
            var numReadPages = chunkFiles.Count;

            //
            // the total number of pages has one additional page for writing the output to
            var numPages = numReadPages + 1;

            //
            // the size of each page is the number of items that may be kept in memory
            // devided by the number of pages. WARNING: if the amount of memory available is
            // too small in relation to the size of the input file, then there may be too
            // many chunks, hence too many pages to fit in memory when each page should have
            // at least space for 1 item.
            var pageSize = maxItemsInMemory / numPages;

            //
            // the buffer simply contains the pages
            var buffer = new NBuffer(numPages, pageSize);

            //
            // the last page is the one used for writing the output to. this variable is a shortcut
            // that allows us to say buffer[pageOutputId] instead of buffer[numPages - 1].
            var pageOutputId = numPages - 1;

            //
            // the page cursors are simply integers denoting the position of the reading pointers
            // in each of the pages
            var pageCursors = new int[numPages];

            //
            // the chunk cursors are binary readers pointing to each of the chunks from the
            // previous step
            var chunkCursors = new BinaryReader[numReadPages];

            //
            // these variables keep the currenly smallest item found (e.g. the one that needs to
            // be written next to the output file to maintain the sort order), and the Id of the
            // chunk this item resides in
            long[] minItem = null;
            var minChunkId = -1;

            //
            // we start by initializing the cursors and reading the initial parts of all the chunks
            // into their respective buffer pages
            for (int i = 0; i < numReadPages; i++) {
                pageCursors[i] = 0;
                chunkCursors[i] = new BinaryReader(File.Open(chunkFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read));
                ReadToPage(chunkCursors[i], itemLength, buffer[i]);
            }

            //
            // we open a binary writer to write the output (the sorted file) to. note we just
            // overwrite the existing file. this is possible because all information from the
            // original file is now duplicated in the sorted chunks anyway.

            using (var bw = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None))) {
                //
                // entering the main loop of the merge that we will remain in for as long as there
                // are items in any of the pages used for reading
                do {
                    //
                    // reset the next item in the sort order to null. this is used to determine
                    // if can leave the main loop.
                    minItem = null;

                    //
                    // here we find the next item in the sort order (the smallest item) in any
                    // of the read pages
                    for (int i = 0; i < numReadPages; i++) {
                        //
                        // fetch the current smallest item in this page. it will always be the
                        // one the respective cursor is pointing to.
                        var t = buffer[i][pageCursors[i]];

                        //
                        // it's possible the page does not contain (any more) items. if so we
                        // can just ignore it. for this we do assume that if we read a null from
                        // position i at some page, then all positions j > i on that same page are
                        // also null
                        if (t != null) {
                            //
                            // if the current smallest item doesn't exist yet, then the one we've
                            // just read is trivially the new smallest item
                            if (minItem == null) {
                                minItem = t;
                                minChunkId = i;
                            }
                                //
                                // if a smallest item does exist, we need to do a comparison first.
                                // we use the same IComparer that we defined earlier
                            else {
                                var c = sorter.Compare(t, minItem);
                                if (c < 0) {
                                    minItem = t;
                                    minChunkId = i;
                                }
                            }
                        }
                    }

                    //
                    // checking if a new smallest item has been identified. if it is we need to
                    // write it to the output page. if not, we don't need to do anything and will
                    // exit the main loop.
                    if (minItem != null) {
                        //
                        // we increment the cursor belonging to the page where the next smallest
                        // item resides
                        pageCursors[minChunkId]++;

                        //
                        // check if we have read all the items on this page. if so, read the next
                        // set of items from the chunk file and reset the cursor
                        if (pageCursors[minChunkId] >= pageSize) {
                            ReadToPage(chunkCursors[minChunkId], itemLength, buffer[minChunkId]);
                            pageCursors[minChunkId] = 0;
                        }

                        //
                        // put the next smallest item in the output page
                        buffer[pageOutputId][pageCursors[pageOutputId]] = minItem;

                        //
                        // increment the output page cursor
                        pageCursors[pageOutputId]++;

                        //
                        // check if the output page is full. if so, write its items to the output
                        // file, clear the page, and reset the cursor
                        if (pageCursors[pageOutputId] >= pageSize) {
                            for (int i = 0; i < pageSize; i++) {
                                Write(bw, buffer[pageOutputId][i]);
                            }
                            buffer[pageOutputId].Clear();
                            pageCursors[pageOutputId] = 0;
                        }
                    }
                } while (minItem != null);

                //
                // the items from all the chunks have been read and sorted, but the last couple
                // of them may still reside on the output page. here we write these last ones to
                // the output file.
                for (int i = 0; i < pageSize; i++) {
                    var t = buffer[pageOutputId][i];
                    if (t != null) {
                        Write(bw, t);
                    } else {
                        break;
                    }
                }
            }

            //
            // close the chunk cursors and delete the chunk files
            for (int i = 0; i < numReadPages; i++) {
                chunkCursors[i].Dispose();
                File.Delete(chunkFiles[i]);
            }
        }

        /// <summary>
        /// Reads a specified number of 64-bit integers from the given binary reader.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <param name="count">The number of 64-bit integers to read.</param>
        /// <returns>An array containing the read 64-bit integers (in order).</returns>
        private static long[] Read(BinaryReader reader, int count)
        {
            var r = new long[count];
            for (int i = 0; i < count; i++) {
                r[i] = reader.ReadInt64();
            }
            return r;
        }

        /// <summary>
        /// Writes an array of 64-bit integers to the given binary writer.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        /// <param name="data">The array of 64-bit integers to write.</param>
        private static void Write(BinaryWriter writer, long[] data)
        {
            for (int i = 0; i < data.Length; i++) {
                writer.Write(data[i]);
            }
        }

        /// <summary>
        /// Reads a number of n-tuples from a binary reader to an n-tuple buffer page. This will
        /// attempt to read a much n-tuples as fit on the given page.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <param name="count">The length of each n-tuple.</param>
        /// <param name="page">The n-tuple buffer page.</param>
        private static void ReadToPage(BinaryReader reader, int count, NPage page)
        {
            page.Clear();
            for (int i = 0; i < page.Size; i++) {
                if (reader.BaseStream.Position >= reader.BaseStream.Length) {
                    break;
                } else {
                    page[i] = Read(reader, count);
                }
            }
        }
    }
}
