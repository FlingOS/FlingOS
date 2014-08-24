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
    
#define FATDIR_TRACE
#undef FATDIR_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.FAT
{
    /// <summary>
    /// Represents a directory in a FAT file system.
    /// </summary>
    public sealed class FATDirectory : Directory
    {
        /// <summary>
        /// The first cluster number of the directory.
        /// </summary>
        public UInt32 FirstClusterNum
        {
            get
            {
                return _theFile.FirstClusterNum;
            }
        }
        /// <summary>
        /// The underlying FAT file used to access the directory listings file.
        /// </summary>
        private FATFile _theFile;
        /// <summary>
        /// The underlying FAT file stream used to read data from the FAT file.
        /// </summary>
        Streams.FAT.FATFileStream _fileStream;
        /// <summary>
        /// Initializes a new FAT directory.
        /// </summary>
        /// <param name="aFileSystem">The FAT file system to which the directory belongs.</param>
        /// <param name="parent">The FAT directory which is the parent of the directory. Null for the root directory.</param>
        /// <param name="aName">The name of the directory.</param>
        /// <param name="aFirstCluster">The first cluster number of the directory.</param>
        public FATDirectory(FATFileSystem aFileSystem, FATDirectory parent, FOS_System.String aName, UInt32 aFirstCluster)
            : base(aFileSystem, parent, aName)
        {
            _theFile = new FATFile(aFileSystem, parent, Name, 0, aFirstCluster) { IsDirectoryFile = true };
        }

        /// <summary>
        /// The cached listings in the directory.
        /// </summary>
        private List _cachedlistings;
        /// <summary>
        /// Reads the directory's listings off disk unless they have already been
        /// cached.
        /// </summary>
        /// <returns>The listings.</returns>
        public override List GetListings()
        {
            if (_cachedlistings == null)
            {
#if FATDIR_TRACE
                BasicConsole.WriteLine("Getting stream...");
#endif
                Get_FileStream();
#if FATDIR_TRACE
                BasicConsole.WriteLine("Got stream. Calculating actual size...");
#endif
                ulong actualSize = _fileStream.GetActualSize();
#if FATDIR_TRACE
                BasicConsole.WriteLine(((FOS_System.String)"actualSize: ") + actualSize);
                BasicConsole.WriteLine("Creating data array...");
#endif
                byte[] xData = new byte[(uint)actualSize];
#if FATDIR_TRACE
                BasicConsole.WriteLine("Created data array.");
#endif
                _fileStream.Position = 0;
#if FATDIR_TRACE
                BasicConsole.WriteLine("Reading data...");
#endif
                int actuallyRead = _fileStream.Read(xData, 0, (int)xData.Length);
#if FATDIR_TRACE
                BasicConsole.WriteLine("Read data. Parsing table...");
#endif
                _cachedlistings = ((FATFileSystem)TheFileSystem).ParseDirectoryTable(xData, actuallyRead, this);
#if FATDIR_TRACE
                BasicConsole.WriteLine("Parsed table.");
#endif
            }
            return _cachedlistings;
        }
        /// <summary>
        /// Gets the listing for the specified path.
        /// </summary>
        /// <param name="nameParts">The path to the listing.</param>
        /// <returns>The listing or null if not found.</returns>
        public override Base GetListing(List nameParts)
        {
            return TheFileSystem.GetListingFromListings(nameParts, Parent, GetListings());
        }
        /// <summary>
        /// Writes the cached listings back to disk.
        /// </summary>
        public override void WriteListings()
        {
#if FATDIR_TRACE
            BasicConsole.WriteLine("Encoding listings...");
#endif
            byte[] listingsBytes = ((FATFileSystem)TheFileSystem).EncodeDirectoryTable(_cachedlistings, Name == "ROOT" && Parent == null);
#if FATDIR_TRACE
            BasicConsole.WriteLine("Encoded listings. Getting file stream...");
#endif
            Get_FileStream();
#if FATDIR_TRACE
            BasicConsole.WriteLine("Got file stream. Writing listings to disk...");
#endif
            _fileStream.Position = 0;
            _fileStream.Write(listingsBytes, 0, (int)listingsBytes.Length);
#if FATDIR_TRACE
            BasicConsole.WriteLine("Written to disk.");
#endif
        }
        /// <summary>
        /// Initializes the underlying file stream.
        /// </summary>
        private void Get_FileStream()
        {
            if (_fileStream == null)
            {
                _fileStream = (Streams.FAT.FATFileStream)_theFile.GetStream();
                _fileStream.IgnoreFileSize = true;
            }
        }

        /// <summary>
        /// Adds the specified listing to the cached listings. Call WriteListings to save the new listing to disc.
        /// </summary>
        /// <param name="aListing">The listing to add.</param>
        public override void AddListing(Base aListing)
        {
#if FATDIR_TRACE
            BasicConsole.WriteLine("Add listing: Getting existing listings...");
#endif
            GetListings();
#if FATDIR_TRACE
            BasicConsole.WriteLine("Got existing listings. Adding listing to cache...");
#endif
            _cachedlistings.Add(aListing);
#if FATDIR_TRACE
            BasicConsole.WriteLine("Added listing.");
#endif
        }
        /// <summary>
        /// Removes the specified listing from the cached listings. Call WriteListings to save the change to disc.
        /// </summary>
        /// <param name="aListing">The listing to remove.</param>
        public override void RemoveListing(Base aListing)
        {
#if FATDIR_TRACE
            BasicConsole.WriteLine("Remove listing: Getting existing listings...");
#endif
            GetListings();
#if FATDIR_TRACE
            BasicConsole.WriteLine("Got existing listings. Removing listing from cache...");
#endif
            _cachedlistings.Remove(aListing);
#if FATDIR_TRACE
            BasicConsole.WriteLine("Removed listing.");
#endif
        }

        /// <summary>
        /// Gets the full, simplified, path for the directory.
        /// </summary>
        /// <returns>The full path.</returns>
        public override String GetFullPath()
        {
            if (this == ((FATFileSystem)TheFileSystem).RootDirectory_FAT32)
            {
                return this.TheFileSystem.TheMapping.Prefix;
            }
            return base.GetFullPath();
        }

        /// <summary>
        /// Deletes the directory from the file system.
        /// </summary>
        /// <returns>True if the directory was deleted. Otherwise, false.</returns>
        public override bool Delete()
        {
            //Delete children
            bool OK = true;
            GetListings();
            //Backwards search as items will be removed from the list.
            for (int i = _cachedlistings.Count - 1; i > -1; i--)
            {
                OK = OK && ((Base)_cachedlistings[i]).Delete();
            }

            //Delete the directory file
            OK = OK && _theFile.Delete();
            
            //Remove listing
            Parent.RemoveListing(this);

            //Write listings
            Parent.WriteListings();
            
            return OK;
        }
    }
}
