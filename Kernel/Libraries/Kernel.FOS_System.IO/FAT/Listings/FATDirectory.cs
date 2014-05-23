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
                Get_FileStream();
                byte[] xData = new byte[(uint)_fileStream.GetActualSize()];
                _fileStream.Position = 0;
                int actuallyRead = _fileStream.Read(xData, 0, (int)xData.Length);
                _cachedlistings = ((FATFileSystem)TheFileSystem).ParseDirectoryTable(xData, actuallyRead, this);
            }
            return _cachedlistings;
        }
        public override Base GetListing(List nameParts)
        {
            return TheFileSystem.GetListingFromListings(nameParts, GetListings());
        }
        public override void WriteListings()
        {
            byte[] listingsBytes = ((FATFileSystem)TheFileSystem).EncodeDirectoryTable(_cachedlistings, Name == "ROOT" && Parent == null);
            Get_FileStream();
            _fileStream.Position = 0;
            _fileStream.Write(listingsBytes, 0, (int)listingsBytes.Length);
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
            GetListings();
            _cachedlistings.Add(aListing);
        }
    }
}
