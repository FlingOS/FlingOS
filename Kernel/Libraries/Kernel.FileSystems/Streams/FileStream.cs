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

using Kernel.FileSystems.FAT;
using Kernel.FileSystems.ISO9660;
using Kernel.FileSystems.Streams.FAT;
using Kernel.FileSystems.Streams.ISO9660;

namespace Kernel.FileSystems.Streams
{
    /// <summary>
    ///     Represents a file stream which can read and write to files in a file system.
    /// </summary>
    public abstract class FileStream : Stream
    {
        //TODO: This implementation has no way of shrinking files - only growing them!

        /// <summary>
        ///     The file which the stream reads/writes from/to.
        /// </summary>
        protected File theFile;

        /// <summary>
        ///     The file which the stream reads/writes from/to.
        /// </summary>
        public File TheFile
        {
            get { return theFile; }
        }

        /// <summary>
        ///     Initializes a new file stream for the specified file.
        /// </summary>
        /// <param name="aFile">The file which the file stream is reading from.</param>
        public FileStream(File aFile)
        {
            theFile = aFile;
        }

        /// <summary>
        ///     Creates a file stream for the specified file. Handles creating
        ///     the correct type of file stream for the file system to which
        ///     the file belongs.
        /// </summary>
        /// <param name="aFile">The file to get a stream to.</param>
        /// <returns>The new file stream.</returns>
        public static FileStream Create(File aFile)
        {
            //TODO: Change this to a factory pattern or something
            if (aFile is FATFile)
            {
                return new FATFileStream((FATFile)aFile, false);
            }
            if (aFile is ISO9660File)
            {
                return new ISO9660FileStream((ISO9660File)aFile);
            }
            return null;
        }
    }
}