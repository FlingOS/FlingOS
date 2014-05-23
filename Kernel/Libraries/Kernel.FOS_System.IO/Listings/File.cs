using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO
{
    public abstract class File : Base
    {
        public File(FileSystem aFileSystem, Directory parent, FOS_System.String aName, UInt64 aSize)
            : base(aFileSystem, parent, aName, false)
        {
            mSize = aSize;
        }

        public Streams.FileStream GetStream()
        {
            return Streams.FileStream.Create(this);
        }

        public static File Open(FOS_System.String fileName)
        {
            FileSystemMapping theMapping = FileSystemManager.GetMapping(fileName);
            if(theMapping == null)
            {
                return null;
            }

            fileName = theMapping.RemoveMappingPrefix(fileName);

            fileName = fileName.ToUpper();

            Base baseListing = theMapping.TheFileSystem.GetListing(fileName);
            if (baseListing == null)
            {
                return null;
            }
            else
            {
                return (File)baseListing;
            }
        }
    }
}
