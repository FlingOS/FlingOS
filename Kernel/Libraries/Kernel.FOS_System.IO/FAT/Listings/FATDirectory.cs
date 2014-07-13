#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
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
            _theFile = new FATFile(aFileSystem, parent, Name, 0, aFirstCluster);
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
                //BasicConsole.WriteLine("Getting stream...");
                Get_FileStream();
                //BasicConsole.WriteLine("Got stream. Calculating actual size...");
                ulong actualSize = _fileStream.GetActualSize();
                //BasicConsole.WriteLine(((FOS_System.String)"actualSize: ") + actualSize);
                //BasicConsole.WriteLine("Creating data array...");
                byte[] xData = new byte[(uint)actualSize];
                //BasicConsole.WriteLine("Created data array.");
                _fileStream.Position = 0;
                //BasicConsole.WriteLine("Reading data...");
                int actuallyRead = _fileStream.Read(xData, 0, (int)xData.Length);
                //BasicConsole.WriteLine("Read data. Parsing table...");
                _cachedlistings = ((FATFileSystem)TheFileSystem).ParseDirectoryTable(xData, actuallyRead, this);
                //BasicConsole.WriteLine("Parsed table.");
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
            return TheFileSystem.GetListingFromListings(nameParts, GetListings());
        }
        /// <summary>
        /// Writes the cached listings back to disk.
        /// </summary>
        public override void WriteListings()
        {
            //BasicConsole.WriteLine("Encoding listings...");
            byte[] listingsBytes = ((FATFileSystem)TheFileSystem).EncodeDirectoryTable(_cachedlistings, Name == "ROOT" && Parent == null);
            //BasicConsole.WriteLine("Encoded listings. Getting file stream...");
            Get_FileStream();
            //BasicConsole.WriteLine("Got file stream. Writing listings to disk...");
            _fileStream.Position = 0;
            _fileStream.Write(listingsBytes, 0, (int)listingsBytes.Length);
            //BasicConsole.WriteLine("Written to disk.");
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
        /// Adds the specified listing to the directory's listings.
        /// </summary>
        /// <param name="aListing">The listing to add.</param>
        public override void AddListing(Base aListing)
        {
            //BasicConsole.WriteLine("Add listing: Getting existing listings...");
            GetListings();
            //BasicConsole.WriteLine("Got existing listings. Adding listing to cache...");
            _cachedlistings.Add(aListing);
            //BasicConsole.WriteLine("Added listing.");
        }
    }
}
