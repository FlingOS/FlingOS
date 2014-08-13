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

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents a directory (folder) listing.
    /// </summary>
    public abstract class Directory : Base
    {
        /// <summary>
        /// Initializes a new directory listing.
        /// </summary>
        /// <param name="aFileSystem">The file system to which the directory belongs.</param>
        /// <param name="parent">The parent directory of the directory.</param>
        /// <param name="aName">The name of the directory.</param>
        public Directory(FileSystem aFileSystem, Directory parent, FOS_System.String aName)
            : base(aFileSystem, parent, aName, true)
        {
        }

        /// <summary>
        /// Attempts to find the specified directory within any file system.
        /// </summary>
        /// <param name="directoryName">The full path and name of the directory to find.</param>
        /// <returns>The directory or null if it isn't found.</returns>
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

        /// <summary>
        /// Gets all the listings within the directory.
        /// </summary>
        /// <returns>All the listings within the directory.</returns>
        public abstract List GetListings();
        /// <summary>
        /// Gets the listing with specified name parts from within the current directory or its sub-directories.
        /// </summary>
        /// <param name="nameParts">
        /// The full path and name of the listing to get, split into their separate parts. One part represents one 
        /// sub-directory or the final file/directory name.
        /// </param>
        /// <returns>Returns the listing or null if it is not found.</returns>
        public abstract Base GetListing(FOS_System.Collections.List nameParts);
        /// <summary>
        /// Writes the cached listings to back to disc. This can either do a full re-write or an update-only approach.
        /// </summary>
        public abstract void WriteListings();

        /// <summary>
        /// Adds the specified listing to the cached listings. Call WriteListings to save the new listing to disc.
        /// </summary>
        /// <param name="aListing">The listing to add.</param>
        public abstract void AddListing(Base aListing);

        /// <summary>
        /// Determines whether the specified listing exists or not within this directory or its sub-directories.
        /// </summary>
        /// <param name="name">The full path and name of the listing to check for.</param>
        /// <returns>Whether the listing exists or not.</returns>
        public bool ListingExists(FOS_System.String name)
        {
            return ListingExists(name, GetListings());
        }
        /// <summary>
        /// Determines whether the specified listing exists or not within this directory or its sub-directories.
        /// </summary>
        /// <param name="name">The full path and name of the listing to check for.</param>
        /// <param name="listings">The list of listings to search through.</param>
        /// <returns>Whether the listing exists or not.</returns>
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

        /// <summary>
        /// Gets the full, simplified, path for the directory.
        /// </summary>
        /// <returns>The full path.</returns>
        public virtual FOS_System.String GetFullPath()
        {
            if (Parent != null)
            {
                return Parent.GetFullPath() + this.Name + "/";
            }
            else
            {
                return this.TheFileSystem.TheMapping.Prefix + this.Name + "/";
            }
        }
    }
}
