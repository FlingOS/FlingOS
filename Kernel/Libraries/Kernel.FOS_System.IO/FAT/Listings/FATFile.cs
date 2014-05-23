using System;

using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.FAT
{
    public sealed class FATFile : File
    {
        public readonly FATFileSystem TheFATFileSystem;
        public readonly UInt32 FirstClusterNum;

        // Size is UInt32 because FAT doesn't support bigger.
        // Dont change to UInt64
        public FATFile(FATFileSystem aFileSystem, FATDirectory parent, FOS_System.String aName, UInt32 aSize, UInt32 aFirstCluster)
            : base(aFileSystem, parent, aName, aSize)
        {
            TheFATFileSystem = aFileSystem;
            FirstClusterNum = aFirstCluster;
        }
    }
}
