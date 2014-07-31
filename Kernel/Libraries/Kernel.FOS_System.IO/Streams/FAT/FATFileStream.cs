#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
#define FATFileStream_TRACE
#undef FATFileStream_TRACE
    
using System;

using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO.FAT;

namespace Kernel.FOS_System.IO.Streams.FAT
{
    /// <summary>
    /// Represents a file stream to a FAT file or FAT directory file.
    /// </summary>
    public class FATFileStream : FileStream
    {
        /// <summary>
        /// The cluster numbers that are part of the file.
        /// </summary>
        protected UInt32List ClusterNums;

        /// <summary>
        /// The FAT file system to which the file the stream is for belongs.
        /// </summary>
        public FATFileSystem TheFATFileSystem
        {
            get
            {
                return (FATFileSystem)TheFile.TheFileSystem;
            }
        }
        /// <summary>
        /// The FAT file the stream is for.
        /// </summary>
        public FATFile TheFATFile
        {
            get
            {
                return (FATFile)TheFile;
            }
        }

        /// <summary>
        /// The position (as an offset from the start of the file) of the stream in the file.
        /// </summary>
        protected UInt64 mPosition = 0;
        /// <summary>
        /// Gets or sets the position (as an offset from the start of the file) of the stream in the file.
        /// </summary>
        public override long Position
        {
            get
            {
                return (long)mPosition;
            }
            set
            {
                if (value < 0L)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Position value must be > 0!"));
                }
                mPosition = (ulong)value;
            }
        }

        /// <summary>
        /// Whether to ignore the file size and use cluster sizes or not.
        /// This is always true for directories as directories report file
        /// size 0.
        /// </summary>
        /// <remarks>
        /// Directories having file size 0 makes perfect sense when you study
        /// the structure of a directory file.
        /// </remarks>
        public bool IgnoreFileSize = false;
        /// <summary>
        /// Gets the actual length (size) of the stream.
        /// For files, this is FileSize. For directories, it is calculated
        /// from the number of clusters times cluster size.
        /// </summary>
        /// <returns>The actual size of the file.</returns>
        public UInt64 GetActualSize()
        {
            if (IgnoreFileSize)
            {
                if (ClusterNums == null)
                {
                    GetClusterNums();
                }
                return (uint)ClusterNums.Count * TheFATFileSystem.BytesPerCluster;
            }
            return theFile.Size;
        }

        /// <summary>
        /// Initializes a new FAT file stream for the specified file.
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
        /// Gets the list of cluster numbers that are part of the file being read/written from/to.
        /// </summary>
        private void GetClusterNums()
        {
            if (TheFATFile.FirstClusterNum > 0 || IgnoreFileSize)
            {
                //BasicConsole.WriteLine("Reading cluster chain...");
                ClusterNums = TheFATFileSystem.ReadClusterChain(TheFile.Size, TheFATFile.FirstClusterNum);
                //BasicConsole.WriteLine("Read cluster chain.");
            }
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream from the current position into the buffer at the 
        /// specified offset or as many bytes as are available before the end of the stream is met.
        /// </summary>
        /// <param name="buffer">The byte array to read into.</param>
        /// <param name="offset">The offset within the buffer to start storing read data at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (TheFile.Size > 0 || IgnoreFileSize)
            {
                if (ClusterNums == null)
                {
                    GetClusterNums();
                    if(ClusterNums == null)
                    {
                        return 0;
                    }
                }

                FATFileSystem mFS = (FATFileSystem)TheFile.TheFileSystem;
                FATFile mFile = TheFATFile;

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Checking params...");
#endif

                if (count < 0)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Read: aCount must be > 0"));
                }
                else if (offset < 0)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Read: anOffset must be > 0"));
                }
                else if (buffer == null)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Read: aBuffer must not be null!"));
                }
                else if (buffer.Length - offset < count)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Read: Invalid offset / length values!"));
                }
                else if (mFile.FirstClusterNum == 0)
                {
                    // FirstSector can be 0 for 0 length files
                    return 0;
                }
                else if (!IgnoreFileSize && mPosition == mFile.Size)
                {
                    // EOF
                    return 0;
                }

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Params OK.");
#endif
                                
                // Reduce count, so that no out of bounds exceptions occur
                ulong fileSize = 0;
                if (IgnoreFileSize)
                {
                    fileSize = (ulong)ClusterNums.Count * TheFATFileSystem.BytesPerCluster;
                }
                else
                {
                    fileSize = mFile.Size;
                }
                ulong xMaxReadableBytes = fileSize - mPosition;
                ulong xCount = (ulong)count;
                if (xCount > xMaxReadableBytes)
                {
                    xCount = xMaxReadableBytes;
                }

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Creating new cluster array...");
#endif

                byte[] xCluster = mFS.NewClusterArray();
                UInt32 xClusterSize = mFS.BytesPerCluster;

                int read = 0;

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Reading data...");
#endif

                while (xCount > 0)
                {
                    UInt32 xClusterIdx = (UInt32)mPosition / xClusterSize;
                    UInt32 xPosInCluster = (UInt32)mPosition % xClusterSize;
#if FATFileStream_TRACE
                    BasicConsole.WriteLine(((FOS_System.String)"Reading cluster ") + ClusterNums[(int)xClusterIdx]);
#endif
                    mFS.ReadCluster(ClusterNums[(int)xClusterIdx], xCluster);
#if FATFileStream_TRACE
                    BasicConsole.WriteLine("Read cluster.");
#endif
                    uint xReadSize;
                    if (xPosInCluster + xCount > xClusterSize)
                    {
                        xReadSize = (xClusterSize - xPosInCluster - 1);
                    }
                    else
                    {
                        xReadSize = (uint)xCount;
                    }

                    // TODO: Should we do an argument check here just in case?
                    FOS_System.Array.Copy(xCluster, (int)xPosInCluster, buffer, offset, (int)xReadSize);
                    offset += (int)xReadSize;
                    xCount -= (ulong)xReadSize;
                    read += (int)xReadSize;
                }

#if FATFileStream_TRACE
                BasicConsole.WriteLine("Read data.");
#endif

                mPosition += (ulong)offset;
                return read;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Writes the specified number of the bytes from the buffer starting at offset in the buffer.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The offset within the buffer to start writing from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Write: aCount must be > 0"));
            }
            else if (offset < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Write: anOffset must be > 0"));
            }
            else if (buffer == null)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Write: aBuffer must not be null!"));
            }
            else if (buffer.Length - offset < count)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("FATFileStream.Write: Invalid offset / length values!"));
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

            UInt32 xClusterSize = mFS.BytesPerCluster;
            byte[] writeBuffer = mFS.NewClusterArray();

            //BasicConsole.WriteLine("Writing data...");

            while(count > 0)
            {
                UInt32 clusterIdx = (UInt32)mPosition / xClusterSize;
                UInt32 posInCluster = (UInt32)mPosition % xClusterSize;

                bool newCluster = false;
                while (clusterIdx >= ClusterNums.Count)
                {
                    //BasicConsole.WriteLine("Expanding clusters...");

                    UInt32 lastClusterNum = ClusterNums[ClusterNums.Count];
                    UInt32 nextClusterNum = mFS.GetNextFreeCluster(lastClusterNum);

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

                if((posInCluster != 0 || count < xClusterSize) && !newCluster)
                {
                    //BasicConsole.WriteLine("Reading existing data...");

                    mFS.ReadCluster(ClusterNums[(int)clusterIdx], writeBuffer);

                    //BasicConsole.WriteLine("Read existing data.");
                }

                //BasicConsole.WriteLine("Calculating write size...");
                int writeSize = count < (xClusterSize - posInCluster) ? count : 
                                            (int)(xClusterSize - posInCluster);
                //BasicConsole.WriteLine("Calculated write size. Copying data to write...");
                Array.Copy(buffer, offset, writeBuffer, (int)posInCluster, writeSize);
                //BasicConsole.WriteLine("Data copied. Writing data to disk...");

                mFS.WriteCluster(ClusterNums[(int)clusterIdx], writeBuffer);

                //BasicConsole.WriteLine("Written data.");

                count -= writeSize;
                offset += writeSize;
                mPosition += (uint)writeSize;
            }

            //BasicConsole.WriteLine("Write completed.");

            if (!IgnoreFileSize)
            {
                if (mPosition > mFile.Size)
                {
                    //Update file info
                    mFile.Size = mPosition;
                    //Update directory entry
                    mFile.Parent.WriteListings();
                }
            }
        }
    }
}
