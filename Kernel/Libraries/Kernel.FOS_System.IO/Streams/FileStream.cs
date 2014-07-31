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
    
using System;

namespace Kernel.FOS_System.IO.Streams
{
    /// <summary>
    /// Represents a file stream which can read and write to files in a file system.
    /// </summary>
    public abstract class FileStream : Stream
    {
        /// <summary>
        /// The file which the stream reads/writes from/to.
        /// </summary>
        protected File theFile;
        /// <summary>
        /// The file which the stream reads/writes from/to.
        /// </summary>
        public File TheFile
        {
            get
            {
                return theFile;
            }
        }

        /// <summary>
        /// Initializes a new file stream for the specified file.
        /// </summary>
        /// <param name="aFile">The file which the file stream is reading from.</param>
        public FileStream(File aFile)
        {
            theFile = aFile;
        }

        /// <summary>
        /// Creates a file stream for the specified file. Handles creating
        /// the correct type of file stream for the file system to which 
        /// the file belongs.
        /// </summary>
        /// <param name="aFile">The file to get a stream to.</param>
        /// <returns>The new file stream.</returns>
        public static FileStream Create(File aFile)
        {
            if (aFile._Type == (FOS_System.Type)(typeof(IO.FAT.FATFile)))
            {
                return new FAT.FATFileStream((IO.FAT.FATFile)aFile, false);
            }
            else
            {
                return null;
            }
        }
    }
}
