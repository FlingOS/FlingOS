using System;

using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.FAT
{
    /// <summary>
    /// Represents a file in a FAT file system.
    /// </summary>
    public sealed class FATFile : File
    {
        /// <summary>
        /// The FAT file system to which the file belongs.
        /// </summary>
        public readonly FATFileSystem TheFATFileSystem;
        /// <summary>
        /// The first cluster number of the file.
        /// </summary>
        public readonly UInt32 FirstClusterNum;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aFileSystem"></param>
        /// <param name="parent"></param>
        /// <param name="aName"></param>
        /// <param name="aSize"></param>
        /// <param name="aFirstCluster"></param>
        /// <remarks>
        /// Size is UInt32 because FAT doesn't support bigger. Dont change to UInt64.
        /// </remarks>
        public FATFile(FATFileSystem aFileSystem, FATDirectory parent, FOS_System.String aName, UInt32 aSize, UInt32 aFirstCluster)
            : base(aFileSystem, parent, aName, aSize)
        {
            TheFATFileSystem = aFileSystem;
            FirstClusterNum = aFirstCluster;
        }
    }
}
