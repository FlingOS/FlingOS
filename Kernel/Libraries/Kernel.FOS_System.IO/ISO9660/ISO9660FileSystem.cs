using System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.ISO9660
{
    public class ISO9660FileSystem : FileSystem
    {
        protected ISO9660.ISO9660Directory RootDirectory;

        public ISO9660FileSystem(Disk.ISO9660.PrimaryVolumeDescriptor primaryVolume)
            : base(primaryVolume)
        {
            isValid = true;

            RootDirectory = new ISO9660Directory(this, null, primaryVolume.RootDirectory);
        }

        public override void CleanDiskCaches()
        {
            thePartition.CleanCaches();
        }

        public override Directory NewDirectory(String name, Directory parent)
        {
            ExceptionMethods.Throw(new Exceptions.NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (FileSystem)"));
            return null;
        }
        public override File NewFile(String name, Directory parent)
        {
            ExceptionMethods.Throw(new Exceptions.NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (FileSystem)"));
            return null;
        }

        public override Base GetListing(String aName)
        {
            if (aName == "")
            {
                return RootDirectory;
            }
            else
            {
                List nameParts = aName.Split(FileSystemManager.PathDelimiter);
                List listings = RootDirectory.GetListings();
                return GetListingFromListings(nameParts, null, listings);
            }
        }
    }
}
