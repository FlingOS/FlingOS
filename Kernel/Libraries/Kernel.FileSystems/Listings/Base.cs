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

using Kernel.Framework;
using Kernel.Framework.Processes.Synchronisation;

namespace Kernel.FileSystems
{
    /// <summary>
    ///     Represents any listing - a directory or a file.
    /// </summary>
    public abstract class Base : Lockable
    {
        /// <summary>
        ///     Whether this listing is a directory or not.
        /// </summary>
        public readonly bool IsDirectory;

        /// <summary>
        ///     The name of this listing.
        /// </summary>
        public readonly String Name;

        /// <summary>
        ///     The file system the listing belongs to.
        /// </summary>
        public readonly FileSystem TheFileSystem;

        /// <summary>
        ///     The size of the listing. 0 for directories.
        /// </summary>
        protected ulong mSize;

        /// <summary>
        ///     The parent directory of this listing. Null indicates this listing is the root directory.
        /// </summary>
        public Directory Parent;

        /// <summary>
        ///     The size of the listing. 0 for directories.
        /// </summary>
        public virtual ulong Size
        {
            get { return mSize; }
            internal set { mSize = value; }
        }

        /// <summary>
        ///     Initializes a new base listing.
        /// </summary>
        /// <param name="aFileSystem">The file system to which the listing belongs.</param>
        /// <param name="parent">The parent directory of the listing.</param>
        /// <param name="aName">The name of the listing.</param>
        /// <param name="isDirectory">Whether the listing is a directory or not.</param>
        protected Base(FileSystem aFileSystem, Directory parent, String aName, bool isDirectory)
            : base(1)
        {
            TheFileSystem = aFileSystem;
            Name = aName;
            IsDirectory = isDirectory;
            Parent = parent;
        }

        /// <summary>
        ///     Gets the full, simplified, path for the listing.
        /// </summary>
        /// <returns>The full path.</returns>
        public virtual String GetFullPath()
        {
            if (IsDirectory)
            {
                if (Parent != null)
                {
                    return Parent.GetFullPath() + Name + FileSystemManager.PathDelimiter;
                }
                return TheFileSystem.TheMapping.Prefix + Name + FileSystemManager.PathDelimiter;
            }
            if (Parent != null)
            {
                return Parent.GetFullPath() + Name;
            }
            return TheFileSystem.TheMapping.Prefix + Name;
        }

        /// <summary>
        ///     Deletes the listing from the file system.
        /// </summary>
        /// <returns>True if the listing was deleted. Otherwise, false.</returns>
        public abstract bool Delete();
    }
}