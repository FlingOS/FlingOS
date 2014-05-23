using System;

using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO
{
    public abstract class Directory : Base
    {
        public Directory(FileSystem aFileSystem, Directory parent, FOS_System.String aName)
            : base(aFileSystem, parent, aName, true)
        {
        }

        public static Directory Find(FOS_System.String directoryName)
        {
            FileSystemMapping theMapping = FileSystemManager.GetMapping(directoryName);
            if (theMapping == null)
            {
                return null;
            }

            directoryName = theMapping.RemoveMappingPrefix(directoryName);
            
            directoryName = directoryName.ToUpper();

            Base baseListing = theMapping.TheFileSystem.GetListing(directoryName);
            if (baseListing == null)
            {
                return null;
            }
            else
            {
                return (Directory)baseListing;
            }
        }

        public abstract List GetListings();
        public abstract Base GetListing(FOS_System.Collections.List nameParts);
        public abstract void WriteListings();

        public abstract void AddListing(Base aListing);

        public bool ListingExists(FOS_System.String name)
        {
            return ListingExists(name, GetListings());
        }
        public static bool ListingExists(FOS_System.String name, List listings)
        {
            for (int i = 0; i < listings.Count; i++)
            {
                if (((Base)listings[i]).Name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
