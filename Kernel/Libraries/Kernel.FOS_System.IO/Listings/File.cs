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
        /// <summary>
        /// Deletes the specified file within the file system.
        /// </summary>
        /// <param name="name">The name of the file to delete.</param>
        /// <returns>True if the file was found and deleted. Otherwise, false.</returns>
        public static bool Delete(FOS_System.String name)
        {
            File theFile = Open(name);

            if (theFile == null)
            {
                return false;
            }

            return theFile.Delete();
        }
    }
}
