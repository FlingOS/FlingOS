using System;

using Kernel.FOS_System.Collections;
using Kernel.Hardware;

namespace Kernel.FOS_System.IO
{
    public abstract class FileSystem : FOS_System.Object
    {
        protected Partition thePartition;
        public Partition ThePartition
        {
            get
            {
                return thePartition;
            }
        }

        public FileSystem(Partition aPartition)
        {
            thePartition = aPartition;
        }

        public abstract Base GetListing(FOS_System.String aName);
        public Base GetListingFromListings(List nameParts, List listings)
        {
            for (int i = 0; i < listings.Count; i++)
            {
                Base aListing = (Base)listings[i];
                if (aListing.Name == (FOS_System.String)nameParts[0])
                {
                    nameParts.RemoveAt(0);
                    if (nameParts.Count == 0)
                    {
                        return aListing;
                    }
                    else if (aListing.IsDirectory)
                    {
                        return ((Directory)aListing).GetListing(nameParts);
                    }
                }
            }

            return null;
        }

        public abstract File NewFile(FOS_System.String name, Directory parent);
        public abstract Directory NewDirectory(FOS_System.String name, Directory parent);
    }
}
