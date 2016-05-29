#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

#define FATFileStream_TRACE
#undef FATFileStream_TRACE

using Kernel.FileSystems.FAT;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.Streams.FAT
{
    /// <summary>
    ///     Represents a file stream to a FAT file or FAT directory file.
    /// </summary>
    public class FATFileStream : FileStream
    {
        private const uint NumReadClusters = 64;
        //TODO: This implementation has no way of shrinking files - only growing them!

        /// <summary>
        ///     The cluster numbers that are part of the file.
        /// </summary>
        protected UInt32List ClusterNums;

        /// <summary>
        ///     Whether to ignore the file size and use cluster sizes or not.
        ///     This is always true for directories as directories report file
        ///     size 0.
        /// </summary>
        /// <remarks>
        ///     Directories having file size 0 makes perfect sense when you study
        ///     the structure of a directory file.
        /// </remarks>
        public bool IgnoreFileSize;

        /// <summary>
        ///     The position (as an offset from the start of the file) of the stream in the file.
        /// </summary>
        protected ulong position;

        private byte[] ReadClusterBuffer;
        private uint ReadClusterSize;

        /// <summary>
        ///     The FAT file system to which the file the stream is for belongs.
        /// </summary>
        public FATFileSystem TheFATFileSystem
        {
            get { return (FATFileSystem)TheFile.TheFileSystem; }
        }

        /// <summary>
        ///     The FAT file the stream is for.
        /// </summary>
        public FATFile TheFATFile
        {
            get { return (FATFile)TheFile; }
        }

        /// <summary>
        ///     Gets or sets the position (as an offset from the start of the file) of the stream in the file.
        /// </summary>
        public override long Position
        {
            get { return (long)position; }
            set
            {
                if (value < 0L)
                {
                    ExceptionMethods.Throw(new ArgumentException("FATFileStream.Position value must be > 0!"));
                }
                position = (ulong)value;
            }
        }

        /// <summary>
        ///     Initializes a new FAT file stream for the specified file.
        /// </summary>
        /// <param name="aFile">The file to create a stream to.</param>
        /// <param name="ignoreFileSize">Whether to ignore the file size or not. True for directories.</param>
        public FATFileStream(FATFile aFile, bool ignoreFileSize)
            : base(aFile)
        {
            IgnoreFileSize = ignoreFileSize;

            if (TheFATFile == null)
            {
                ExceptionMethods.Throw(new Exception("Could not create FATFileStream. Specified file object was null!"));
            }

            GetClusterNums();
        }

        /// <summary>
        ///     Gets the actual length (size) of the stream.
        ///     For files, this is FileSize. For directories, it is calculated
        ///     from the number of clusters times cluster size.
        /// </summary>
        /// <returns>The actual size of the file.</returns>
        public ulong GetActualSize()
        {
            //This is set for streams which access a directory 
            //  since directories look and work like files except for
            //  the fact that directory sizes are determined solely 
            //  by the number of clusters they use.
            if (IgnoreFileSize)
            {
                if (ClusterNums == null)
                {
                    GetClusterNums();
                }

                if (ClusterNums == null)
                {
                    ExceptionMethods.Throw(
                        new NullReferenceException("ClusterNums still null! GetClusterNums must have failed."));
                }
                else
                {
                    //We have assumed at this point that GetClusterNums worked. If it didn't,
                    //  then ClusterNums will be null and we will have a serious problem! :)
                    return (uint)ClusterNums.Count*TheFATFileSystem.BytesPerCluster;
                }
            }
            return TheFile.Size;
        }

        /// <summary>
        ///     Gets the list of cluster numbers that are part of the file being read/written from/to.
        /// </summary>
        private void GetClusterNums()
        {
            //Cluster number of 0 is invalid! Minimum is 2. Therefore, we can use
            //  the cluster number to determine whether this stream is to a valid
            //  / non-empty file or not.
            if (TheFATFile.FirstClusterNum > 0 || IgnoreFileSize)
            {
                //BasicConsole.WriteLine("Reading cluster chain...");
                ClusterNums = TheFATFileSystem.ReadClusterChain(TheFile.Size, TheFATFile.FirstClusterNum);
                //BasicConsole.WriteLine("Read cluster chain.");
            }
        }

        /// <summary>
        ///     Reads the specified number of bytes from the stream from the current position into the buffer at the
        ///     specified offset or as many bytes as are available before the end of the stream is met.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The offset within the buffer to start storing read data at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            //Don't attempt to read a file of 0 size.
            if (TheFile.Size > 0 || IgnoreFileSize)
            {
                //Load cluster chain if it hasn't already been loaded.
                if (ClusterNums == null)
                {
                    GetClusterNums();
                    //If loading cluster nums failed, don't throw an exception, 
                    //  just return nothing read.
                    if (ClusterNums == null)
                    {
                        return 0;
                    }
                }

                FATFileSystem TheFS = (FATFileSystem)TheFile.TheFileSystem;

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Checking params...");
#endif

                //Conditions for being able to read from the stream.
                if (count < 0)
                {
                    ExceptionMethods.Throw(new ArgumentException("FATFileStream.Read: aCount must be > 0"));
                }
                else if (offset < 0)
                {
                    ExceptionMethods.Throw(new ArgumentException("FATFileStream.Read: anOffset must be > 0"));
                }
                else if (buffer == null)
                {
                    ExceptionMethods.Throw(new ArgumentException("FATFileStream.Read: aBuffer must not be null!"));
                }
                else if (buffer.Length - offset < count)
                {
                    ExceptionMethods.Throw(new ArgumentException("FATFileStream.Read: Invalid offset / length values!"));
                }
                else if (TheFATFile.FirstClusterNum == 0)
                {
                    // First cluster number can be 0 for 0 length files
                    return 0;
                }
                else if (!IgnoreFileSize && position == TheFile.Size)
                {
                    // EOF
                    return 0;
                }

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Params OK.");
#endif

                // Clamp the count value so that no out of bounds exceptions occur
                ulong FileSize = 0;
                if (IgnoreFileSize)
                {
                    FileSize = (ulong)ClusterNums.Count*TheFATFileSystem.BytesPerCluster;
                }
                else
                {
                    FileSize = TheFile.Size;
                }
                ulong MaxReadableBytes = FileSize - position;
                ulong ActualCount = (ulong)count;
                if (ActualCount > MaxReadableBytes)
                {
                    ActualCount = MaxReadableBytes;
                }

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Creating new cluster array...");
#endif

                //Temporary store of cluster data since we can only
                //  read entire clusters at a time.
                if (ReadClusterBuffer == null)
                {
                    ReadClusterBuffer = new byte[TheFS.BytesPerCluster*NumReadClusters];
                    ReadClusterSize = TheFS.BytesPerCluster;
                }

                int read = 0;

                while (ActualCount > 0)
                {
                    uint NumClustersToRead = (uint)ActualCount/ReadClusterSize;
                    if ((uint)ActualCount%ReadClusterSize != 0)
                    {
                        NumClustersToRead++;
                    }

                    uint StartPosition = (uint)position;
                    int StartClusterIdx = (int)(StartPosition/ReadClusterSize);
                    uint StartClusterNum = ClusterNums[StartClusterIdx];

                    uint ContiguousClusters = 1;
                    int CurrentClusterIdx = StartClusterIdx;
                    uint CurrentClusterNum = ClusterNums[CurrentClusterIdx];

                    while (ContiguousClusters < NumClustersToRead && ContiguousClusters < NumReadClusters)
                    {
                        if (CurrentClusterNum + 1 != ClusterNums[CurrentClusterIdx + 1])
                        {
                            break;
                        }
                        CurrentClusterIdx++;
                        CurrentClusterNum++;
                        ContiguousClusters++;
                    }

                    TheFS.ReadClusters(StartClusterNum, ContiguousClusters, ReadClusterBuffer);

                    uint StartClusterOffset = StartPosition%ReadClusterSize;
                    uint ContiguousClusterSize = ContiguousClusters*ReadClusterSize;
                    uint ReadSize = ContiguousClusterSize;
                    if (StartClusterOffset > 0)
                    {
                        ReadSize -= StartClusterOffset;
                    }
                    if (ReadSize > ActualCount)
                    {
                        ReadSize = (uint)ActualCount;
                    }
                    Array.Copy(ReadClusterBuffer, (int)StartClusterOffset, buffer, offset, (int)ReadSize);

                    position += ReadSize;
                    offset += (int)ReadSize;
                    ActualCount -= ReadSize;
                    read += (int)ReadSize;
                }

                return read;
            }
            return 0;
        }

        /// <summary>
        ///     Writes the specified number of the bytes from the buffer starting at offset in the buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The offset within the buffer to start writing from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < 0)
            {
                ExceptionMethods.Throw(new ArgumentException("FATFileStream.Write: aCount must be > 0"));
            }
            else if (offset < 0)
            {
                ExceptionMethods.Throw(new ArgumentException("FATFileStream.Write: anOffset must be > 0"));
            }
            else if (buffer == null)
            {
                ExceptionMethods.Throw(new ArgumentException("FATFileStream.Write: aBuffer must not be null!"));
            }
            else if (buffer.Length - offset < count)
            {
                ExceptionMethods.Throw(new ArgumentException("FATFileStream.Write: Invalid offset / length values!"));
            }

            //BasicConsole.WriteLine("Checks passed.");

            FATFileSystem mFS = (FATFileSystem)TheFile.TheFileSystem;
            FATFile mFile = TheFATFile;

            if (ClusterNums == null)
            {
                //BasicConsole.WriteLine("Getting cluster nums...");

                GetClusterNums();
                if (ClusterNums == null)
                {
                    //BasicConsole.WriteLine("Failed to get cluster nums.");
                    return;
                }

                //BasicConsole.WriteLine("Got cluster nums.");
            }

            //BasicConsole.WriteLine("Creating write buffer...");

            uint xClusterSize = mFS.BytesPerCluster;
            byte[] writeBuffer = mFS.NewClusterArray();

            //BasicConsole.WriteLine("Writing data...");

            while (count > 0)
            {
                uint clusterIdx = (uint)position/xClusterSize;
                uint posInCluster = (uint)position%xClusterSize;

                bool newCluster = false;
                while (clusterIdx >= ClusterNums.Count)
                {
                    //BasicConsole.WriteLine("Expanding clusters...");

                    uint lastClusterNum = ClusterNums[ClusterNums.Count - 1];
                    uint nextClusterNum = mFS.GetNextFreeCluster(lastClusterNum);

                    //Clear cluster
                    mFS.WriteCluster(nextClusterNum, null);

                    //Set last FAT entry to point to next cluster
                    mFS.SetFATEntryAndSave(lastClusterNum, nextClusterNum);

                    //Set next cluster as EOF
                    mFS.SetFATEntryAndSave(nextClusterNum, FATFileSystem.GetFATEntryEOFValue(mFS.FATType));

                    //Add next cluster num to our list
                    ClusterNums.Add(nextClusterNum);
                    newCluster = true;
                }

                if ((posInCluster != 0 || count < xClusterSize) && !newCluster)
                {
                    //BasicConsole.WriteLine("Reading existing data...");

                    mFS.ReadClusters(ClusterNums[(int)clusterIdx], 1, writeBuffer);

                    //BasicConsole.WriteLine("Read existing data.");
                }

                //BasicConsole.WriteLine("Calculating write size...");
                int writeSize = count < xClusterSize - posInCluster
                    ? count
                    : (int)(xClusterSize - posInCluster);
                //BasicConsole.WriteLine("Calculated write size. Copying data to write...");
                Array.Copy(buffer, offset, writeBuffer, (int)posInCluster, writeSize);
                //BasicConsole.WriteLine("Data copied. Writing data to disk...");

                mFS.WriteCluster(ClusterNums[(int)clusterIdx], writeBuffer);

                //BasicConsole.WriteLine("Written data.");

                count -= writeSize;
                offset += writeSize;
                position += (uint)writeSize;
            }

            mFS.CleanDiskCaches();

            //BasicConsole.WriteLine("Write completed.");

            if (!IgnoreFileSize)
            {
                if (position > mFile.Size)
                {
                    //Update file info
                    mFile.Size = position;
                    //Update directory entry
                    mFile.Parent.WriteListings();
                }
            }
        }
    }
}