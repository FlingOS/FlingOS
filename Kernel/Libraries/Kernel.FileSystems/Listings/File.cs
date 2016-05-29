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

using Kernel.FileSystems.Streams;
using Kernel.Framework;

namespace Kernel.FileSystems
{
    /// <summary>
    ///     Represents a file listing in a file system.
    /// </summary>
    public abstract class File : Base
    {
        /// <summary>
        ///     Initializes a new file.
        /// </summary>
        /// <param name="aFileSystem">The file system to which the file belongs.</param>
        /// <param name="parent">The parent directory of the file.</param>
        /// <param name="aName">The name of the file.</param>
        /// <param name="aSize">The exact size of the file in bytes.</param>
        public File(FileSystem aFileSystem, Directory parent, String aName, ulong aSize)
            : base(aFileSystem, parent, aName, false)
        {
            mSize = aSize;
        }

        /// <summary>
        ///     Gets a stream to read/write from the file.
        /// </summary>
        /// <returns>A readable, writeable stream.</returns>
        public FileStream GetStream()
        {
            return FileStream.Create(this);
        }

        /// <summary>
        ///     Opens the specified file.
        /// </summary>
        /// <param name="fileName">The full path to the file to open.</param>
        /// <returns>The file listing or null if not found.</returns>
        public static File Open(String fileName)
        {
            FileSystemMapping theMapping = FileSystemManager.GetMapping(fileName);
            if (theMapping == null)
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
            if (baseListing is File)
            {
                return (File)baseListing;
            }
            return null;
        }

        /// <summary>
        ///     Deletes the specified file within the file system.
        /// </summary>
        /// <param name="name">The name of the file to delete.</param>
        /// <returns>True if the file was found and deleted. Otherwise, false.</returns>
        public static bool Delete(String name)
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