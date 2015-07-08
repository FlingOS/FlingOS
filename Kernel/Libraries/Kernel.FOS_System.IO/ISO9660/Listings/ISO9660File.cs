using System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.ISO9660
{
    public class ISO9660File : File
    {
        /// <summary>
        /// Indicates whether the ISO9660File instance is being used to read/write ISO9660Directory data. 
        /// This is subtly different from IsDirectory.
        /// </summary>
        internal bool IsDirectoryFile = false;

        internal Disk.ISO9660.DirectoryRecord TheDirectoryRecord;

        public ISO9660File(ISO9660FileSystem fileSystem, ISO9660Directory parent, Disk.ISO9660.DirectoryRecord record)
            : base(fileSystem, parent, record.FileIdentifier.length > 0 ? (FOS_System.String)record.FileIdentifier.Split(';')[0] : "", record.DataLength)
        {
            TheDirectoryRecord = record;
        }

        public override bool Delete()
        {
            ExceptionMethods.Throw(new Exceptions.NotSupportedException("Cannot modify contents of ISO9660 disc (yet)! (File)"));
            return false;
        }
    }
}
