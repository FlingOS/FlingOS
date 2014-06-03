using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents a file listing in a file system.
    /// </summary>
    public abstract class File : Base
    {
        /// <summary>
        /// Initializes a new file.
        /// </summary>
        /// <param name="aFileSystem">The file system to which the file belongs.</param>
        /// <param name="parent">The parent directory of the file.</param>
        /// <param name="aName">The name of the file.</param>
        /// <param name="aSize">The exact size of the file in bytes.</param>
        public File(FileSystem aFileSystem, Directory parent, FOS_System.String aName, UInt64 aSize)
            : base(aFileSystem, parent, aName, false)
        {
            mSize = aSize;
        }

        /// <summary>
        /// Gets a stream to read/write from the file.
        /// </summary>
        /// <returns>A readable, writeable stream.</returns>
        public Streams.FileStream GetStream()
        {
            return Streams.FileStream.Create(this);
        }

        /// <summary>
        /// Opens the specified file.
        /// </summary>
        /// <param name="fileName">The full path to the file to open.</param>
        /// <returns>The file listing or null if not found.</returns>
        public static File Open(FOS_System.String fileName)
        {
            FileSystemMapping theMapping = FileSystemManager.GetMapping(fileName);
            if(theMapping == null)
            {
                return null;
            }

            fileName = theMapping.RemoveMappingPrefix(fileName);

            fileName = fileName.ToUpper();

            Base baseListing = theMapping.TheFileSystem.GetListing(fileName);
            if (baseListing == null)
            {
                return null;
            }
            else
            {
                return (File)baseListing;
            }
        }
    }
}
