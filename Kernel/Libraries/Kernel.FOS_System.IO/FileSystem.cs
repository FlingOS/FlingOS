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
using Kernel.Hardware;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents a file system which must exist within a partition.
    /// </summary>
    public abstract class FileSystem : FOS_System.Object
    {
        /// <summary>
        /// The partition in which the file system resides.
        /// </summary>
        protected Partition thePartition;
        /// <summary>
        /// The partition in which the file system resides.
        /// </summary>
        public Partition ThePartition
        {
            get
            {
                return thePartition;
            }
        }

        /// <summary>
        /// The file system mapping for the file system.
        /// </summary>
        public FileSystemMapping TheMapping
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new file system for the specified partition.
        /// </summary>
        /// <param name="aPartition">The partition in which the partition resides.</param>
        public FileSystem(Partition aPartition)
        {
            thePartition = aPartition;
        }

        /// <summary>
        /// Gets the listing for the specified path.
        /// </summary>
        /// <param name="aName">The full path to the listing to get.</param>
        /// <returns>The listing or null if not found.</returns>
        public abstract Base GetListing(FOS_System.String aName);
        /// <summary>
        /// Gets a specific listing from the specified list of listings. Performs a recursive
        /// search down the file system tree.
        /// </summary>
        /// <param name="nameParts">The parts of the full path of the listing to get.</param>
        /// <param name="parent">The parent directory of the directory from which the listings were taken.</param>
        /// <param name="listings">The listings to search through.</param>
        /// <returns>The listing or null if not found.</returns>
        public Base GetListingFromListings(List nameParts, Directory parent, List listings)
        {
            //  ".." means "parent directory"
            if (((FOS_System.String)nameParts[0]) == "..")
            {
                nameParts.RemoveAt(0);
                if (nameParts.Count == 0)
                {
                    return parent;
                }

                return parent.GetListing(nameParts);
            }

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

        /// <summary>
        /// Creates a new file within the file system.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="parent">The parent directory of the new file.</param>
        /// <returns>The new file listing.</returns>
        public abstract File NewFile(FOS_System.String name, Directory parent);
        /// <summary>
        /// Creates a new directory within the file system.
        /// </summary>
        /// <param name="name">The name of the directory to create.</param>
        /// <param name="parent">The parent directory of the new directory.</param>
        /// <returns>The new directory listing.</returns>
        public abstract Directory NewDirectory(FOS_System.String name, Directory parent);
    }
}
