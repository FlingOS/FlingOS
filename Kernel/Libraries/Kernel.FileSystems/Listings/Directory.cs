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

using Kernel.Framework;
using Kernel.Framework.Collections;

namespace Kernel.FileSystems
{
    /// <summary>
    ///     Represents a directory (folder) listing.
    /// </summary>
    public abstract class Directory : Base
    {
        /// <summary>
        ///     Initializes a new directory listing.
        /// </summary>
        /// <param name="aFileSystem">The file system to which the directory belongs.</param>
        /// <param name="parent">The parent directory of the directory.</param>
        /// <param name="aName">The name of the directory.</param>
        public Directory(FileSystem aFileSystem, Directory parent, String aName)
            : base(aFileSystem, parent, aName, true)
        {
        }

        /// <summary>
        ///     Attempts to find the specified directory within any file system.
        /// </summary>
        /// <param name="directoryName">The full path and name of the directory to find.</param>
        /// <returns>The directory or null if it isn't found.</returns>
        public static Directory Find(String directoryName)
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
            if (baseListing is Directory)
            {
                return (Directory)baseListing;
            }

            return ((File)baseListing).Parent;
        }

        /// <summary>
        ///     Gets all the listings within the directory.
        /// </summary>
        /// <returns>All the listings within the directory.</returns>
        public abstract List GetListings();

        /// <summary>
        ///     Gets the listing with specified name parts from within the current directory or its sub-directories.
        /// </summary>
        /// <param name="nameParts">
        ///     The full path and name of the listing to get, split into their separate parts. One part represents one
        ///     sub-directory or the final file/directory name.
        /// </param>
        /// <returns>Returns the listing or null if it is not found.</returns>
        public abstract Base GetListing(List nameParts);

        /// <summary>
        ///     Writes the cached listings to back to disc. This can either do a full re-write or an update-only approach.
        /// </summary>
        public abstract void WriteListings();

        /// <summary>
        ///     Adds the specified listing to the cached listings. Call WriteListings to save the new listing to disc.
        /// </summary>
        /// <param name="aListing">The listing to add.</param>
        public abstract void AddListing(Base aListing);

        /// <summary>
        ///     Removes the specified listing from the cached listings. Call WriteListings to save the change to disc.
        /// </summary>
        /// <param name="aListing">The listing to remove.</param>
        public abstract void RemoveListing(Base aListing);

        /// <summary>
        ///     Determines whether the specified listing exists or not within this directory or its sub-directories.
        /// </summary>
        /// <param name="name">The full path and name of the listing to check for.</param>
        /// <returns>Whether the listing exists or not.</returns>
        public bool ListingExists(String name)
        {
            return ListingExists(name, GetListings());
        }

        /// <summary>
        ///     Determines whether the specified listing exists or not within this directory or its sub-directories.
        /// </summary>
        /// <param name="name">The full path and name of the listing to check for.</param>
        /// <param name="listings">The list of listings to search through.</param>
        /// <returns>Whether the listing exists or not.</returns>
        public static bool ListingExists(String name, List listings)
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

        /// <summary>
        ///     Deletes the specified directory within the file system.
        /// </summary>
        /// <param name="name">The name of the directory to delete.</param>
        /// <returns>True if the directory was found and deleted. Otherwise, false.</returns>
        public static bool Delete(String name)
        {
            Directory theDir = Find(name);

            if (theDir == null)
            {
                return false;
            }

            return theDir.Delete();
        }
    }
}