using System;

namespace Kernel.FOS_System.IO.Streams
{
    public abstract class FileStream : Stream
    {
        protected File theFile;
        public File TheFile
        {
            get
            {
                return theFile;
            }
        }

        public FileStream(File aFile)
        {
            theFile = aFile;
        }

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
