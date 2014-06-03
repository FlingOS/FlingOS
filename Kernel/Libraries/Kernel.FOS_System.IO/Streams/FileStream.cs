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
