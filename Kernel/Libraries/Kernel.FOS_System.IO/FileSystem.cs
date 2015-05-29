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

        public abstract void CleanDiskCaches();
    }
}
