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

#define ISO9660DIR_TRACE
#undef ISO9660DIR_TRACE

using Kernel.FileSystems.Streams.ISO9660;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.ISO9660
{
    public class ISO9660Directory : Directory
    {
        private readonly ISO9660File _theFile;
        private List _cachedlistings;
        private ISO9660FileStream _fileStream;
        internal Disk.ISO9660.DirectoryRecord TheDirectoryRecord;

        public ISO9660Directory(ISO9660FileSystem fileSystem, ISO9660Directory parent,
            Disk.ISO9660.DirectoryRecord record)
            : base(
                fileSystem, parent, record.FileIdentifier.Length > 0 ? (String)record.FileIdentifier.Split(';')[0] : ""
                )
        {
            TheDirectoryRecord = record;

            _theFile = new ISO9660File(fileSystem, parent, record) {IsDirectoryFile = true};
        }

        public override void AddListing(Base aListing)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (Directory)"));
        }

        public override bool Delete()
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (Directory)"));
            return false;
        }

        public override String GetFullPath()
        {
            if (TheDirectoryRecord.IsRootDirectory)
            {
                return TheFileSystem.TheMapping.Prefix;
            }
            return base.GetFullPath();
        }

        public override Base GetListing(List nameParts)
        {
            return TheFileSystem.GetListingFromListings(nameParts, Parent, GetListings());
        }

        public override List GetListings()
        {
            if (_cachedlistings == null)
            {
                Get_FileStream();
                byte[] data = new byte[(uint)_theFile.Size];
                _fileStream.Position = 0;
                int actuallyRead = _fileStream.Read(data, 0, data.Length);
                _cachedlistings = new List(10);

                uint position = 0;
                Disk.ISO9660.DirectoryRecord newRecord;
                do
                {
                    newRecord = new Disk.ISO9660.DirectoryRecord(data, position);
#if ISO9660DIR_TRACE
                    BasicConsole.WriteLine(newRecord.ConvertToString());
#endif
                    if (newRecord.RecordLength > 0)
                    {
                        if ((newRecord.TheFileFlags & Disk.ISO9660.DirectoryRecord.FileFlags.Directory) != 0)
                        {
                            // Directory
                            _cachedlistings.Add(new ISO9660Directory((ISO9660FileSystem)TheFileSystem, this, newRecord));
                        }
                        else
                        {
                            // File
                            _cachedlistings.Add(new ISO9660File((ISO9660FileSystem)TheFileSystem, this, newRecord));
                        }

                        position += newRecord.RecordLength;
                    }
                } while (position < data.Length && newRecord.RecordLength > 0);
            }
            return _cachedlistings;
        }

        private void Get_FileStream()
        {
            if (_fileStream == null)
            {
                _fileStream = (ISO9660FileStream)_theFile.GetStream();
            }
        }

        public override void RemoveListing(Base aListing)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (Directory)"));
        }

        public override void WriteListings()
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (Directory)"));
        }
    }
}