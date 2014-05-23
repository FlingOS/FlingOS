using System;

namespace Kernel.FOS_System.IO
{
    public abstract class Base : FOS_System.Object
    {
        public readonly FileSystem TheFileSystem;
        public Directory Parent;
        public readonly FOS_System.String Name;

        public readonly bool IsDirectory;

        protected Base(FileSystem aFileSystem, Directory parent, FOS_System.String aName, bool isDirectory)
        {
            TheFileSystem = aFileSystem;
            Name = aName;
            IsDirectory = isDirectory;
            Parent = parent;
        }

        protected UInt64 mSize;
        public virtual UInt64 Size
        {
            get { return mSize; }
            internal set { mSize = value; }
        }
    }
}
