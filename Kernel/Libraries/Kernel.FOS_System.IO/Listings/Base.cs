using System;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents any listing - a directory or a file.
    /// </summary>
    public abstract class Base : FOS_System.Object
    {
        /// <summary>
        /// The file system the listing belongs to.
        /// </summary>
        public readonly FileSystem TheFileSystem;
        /// <summary>
        /// The parent directory of this listing. Null indicates this listing is the root directory.
        /// </summary>
        public Directory Parent;
        /// <summary>
        /// The name of this listing.
        /// </summary>
        public readonly FOS_System.String Name;

        /// <summary>
        /// Whether this listing is a directory or not.
        /// </summary>
        public readonly bool IsDirectory;

        /// <summary>
        /// Initializes a new base listing.
        /// </summary>
        /// <param name="aFileSystem">The file system to which the listing belongs.</param>
        /// <param name="parent">The parent directory of the listing.</param>
        /// <param name="aName">The name of the listing.</param>
        /// <param name="isDirectory">Whether the listing is a directory or not.</param>
        protected Base(FileSystem aFileSystem, Directory parent, FOS_System.String aName, bool isDirectory)
        {
            TheFileSystem = aFileSystem;
            Name = aName;
            IsDirectory = isDirectory;
            Parent = parent;
        }

        /// <summary>
        /// The size of the listing. 0 for directories.
        /// </summary>
        protected UInt64 mSize;
        /// <summary>
        /// The size of the listing. 0 for directories.
        /// </summary>
        public virtual UInt64 Size
        {
            get { return mSize; }
            internal set { mSize = value; }
        }
    }
}
