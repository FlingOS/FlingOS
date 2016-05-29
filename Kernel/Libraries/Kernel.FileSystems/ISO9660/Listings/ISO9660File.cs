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
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.ISO9660
{
    public class ISO9660File : File
    {
        /// <summary>
        ///     Indicates whether the ISO9660File instance is being used to read/write ISO9660Directory data.
        ///     This is subtly different from IsDirectory.
        /// </summary>
        internal bool IsDirectoryFile = false;

        internal Disk.ISO9660.DirectoryRecord TheDirectoryRecord;

        public ISO9660File(ISO9660FileSystem fileSystem, ISO9660Directory parent, Disk.ISO9660.DirectoryRecord record)
            : base(
                fileSystem, parent, record.FileIdentifier.Length > 0 ? (String)record.FileIdentifier.Split(';')[0] : "",
                record.DataLength)
        {
            TheDirectoryRecord = record;
        }

        public override bool Delete()
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (File)"));
            return false;
        }
    }
}