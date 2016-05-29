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
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.ISO9660
{
    public class ISO9660FileSystem : FileSystem
    {
        protected ISO9660Directory RootDirectory;

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
            ExceptionMethods.Throw(
                new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (FileSystem)"));
            return null;
        }

        public override File NewFile(String name, Directory parent)
        {
            ExceptionMethods.Throw(
                new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (FileSystem)"));
            return null;
        }

        public override Base GetListing(String aName)
        {
            if (aName == "")
            {
                return RootDirectory;
            }
            List nameParts = aName.Split(FileSystemManager.PathDelimiter);
            List listings = RootDirectory.GetListings();
            return GetListingFromListings(nameParts, null, listings);
        }
    }
}