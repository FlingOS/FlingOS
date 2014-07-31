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
        /// Initializes a new FAT file.
        /// </summary>
        /// <param name="aFileSystem">The FAT file system to which the file belongs.</param>
        /// <param name="parent">The parent directory of the file.</param>
        /// <param name="aName">The name of the file.</param>
        /// <param name="aSize">The size of the file.</param>
        /// <param name="aFirstCluster">The first cluster number of the file.</param>
        /// <remarks>
        /// Size is UInt32 because FAT doesn't support bigger. Don't change to UInt64.
        /// </remarks>
        public FATFile(FATFileSystem aFileSystem, FATDirectory parent, FOS_System.String aName, UInt32 aSize, UInt32 aFirstCluster)
            : base(aFileSystem, parent, aName, aSize)
        {
            TheFATFileSystem = aFileSystem;
            FirstClusterNum = aFirstCluster;
        }
    }
}
