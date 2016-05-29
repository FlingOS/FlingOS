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

#define FATFILE_TRACE
#undef FATFILE_TRACE

using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.FAT
{
    /// <summary>
    ///     Represents a file in a FAT file system.
    /// </summary>
    public sealed class FATFile : File
    {
        /// <summary>
        ///     The first cluster number of the file.
        /// </summary>
        public readonly uint FirstClusterNum;

        /// <summary>
        ///     The FAT file system to which the file belongs.
        /// </summary>
        public readonly FATFileSystem TheFATFileSystem;

        /// <summary>
        ///     Indicates whether the FATFile instance is being used to read/write FATDirectory data.
        ///     This is subtly different from IsDirectory.
        /// </summary>
        internal bool IsDirectoryFile = false;

        /// <summary>
        ///     Initializes a new FAT file.
        /// </summary>
        /// <param name="aFileSystem">The FAT file system to which the file belongs.</param>
        /// <param name="parent">The parent directory of the file.</param>
        /// <param name="aName">The name of the file.</param>
        /// <param name="aSize">The size of the file.</param>
        /// <param name="aFirstCluster">The first cluster number of the file.</param>
        /// <remarks>
        ///     Blocks is UInt32 because FAT doesn't support bigger. Don't change to UInt64.
        /// </remarks>
        public FATFile(FATFileSystem aFileSystem, FATDirectory parent, String aName, uint aSize, uint aFirstCluster)
            : base(aFileSystem, parent, aName, aSize)
        {
            TheFATFileSystem = aFileSystem;
            FirstClusterNum = aFirstCluster;
        }

        /// <summary>
        ///     Deletes the listing from the file system.
        /// </summary>
        /// <returns>True if the listing was deleted. Otherwise, false.</returns>
        public override bool Delete()
        {
            if (TheFATFileSystem.FATType != FATFileSystem.FATTypeEnum.FAT32)
            {
                ExceptionMethods.Throw(new NotSupportedException("FATFile.Delete for non-FAT32 not supported!"));
            }

#if FATFILE_TRACE
            BasicConsole.WriteLine("FATFile.Delete : Reading cluster chain...");
#endif
            UInt32List clusters = TheFATFileSystem.ReadClusterChain(Size, FirstClusterNum);
#if FATFILE_TRACE
            BasicConsole.WriteLine("FATFile.Delete : Processing cluster chain...");
#endif
            for (int i = 0; i < clusters.Count; i++)
            {
#if FATFILE_TRACE
                BasicConsole.WriteLine("FATFile.Delete : Writing cluster...");
#endif
                //Write 0s (null) to clusters
                TheFATFileSystem.WriteCluster(clusters[i], null);

#if FATFILE_TRACE
                BasicConsole.WriteLine("FATFile.Delete : Setting FAT entry...");
#endif
                //Write "empty" to FAT entries
                TheFATFileSystem.SetFATEntryAndSave(clusters[i], 0);
            }

            //If the file actually being used to read/write a FATDirectory, 
            //      it will not be in the parent listings, the FATDirectory instance will be.
            //      So we should not attempt to edit the parent listings from within the 
            //      FATFile instance.
            if (!IsDirectoryFile)
            {
#if FATFILE_TRACE
            BasicConsole.WriteLine("FATFile.Delete : Removing listing...");
#endif
                //Remove listing
                Parent.RemoveListing(this);

#if FATFILE_TRACE
            BasicConsole.WriteLine("FATFile.Delete : Writing listings...");
#endif
                //Write listings
                Parent.WriteListings();
            }

#if FATFILE_TRACE
            BasicConsole.WriteLine("FATFile.Delete : Complete.");
#endif

            return true;
        }
    }
}