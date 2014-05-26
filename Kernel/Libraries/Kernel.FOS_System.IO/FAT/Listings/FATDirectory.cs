using System;

using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.FAT
{
    public sealed class FATDirectory : Directory
    {
        public UInt32 FirstClusterNum
        {
            get
            {
                return _theFile.FirstClusterNum;
            }
        }
        private FATFile _theFile;
        Streams.FAT.FATFileStream _fileStream;
        public FATDirectory(FATFileSystem aFileSystem, FATDirectory parent, FOS_System.String aName, UInt32 aFirstCluster)
            : base(aFileSystem, parent, aName)
        {
            _theFile = new FATFile(aFileSystem, parent, Name, 0, aFirstCluster);
        }

        private List _cachedlistings;
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
        public override Base GetListing(List nameParts)
        {
            return TheFileSystem.GetListingFromListings(nameParts, GetListings());
        }
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
        private void Get_FileStream()
        {
            if (_fileStream == null)
            {
                _fileStream = (Streams.FAT.FATFileStream)_theFile.GetStream();
                _fileStream.IgnoreFileSize = true;
            }
        }

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
