using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO
{
    public class FileSystemMapping : FOS_System.Object
    {
        protected FOS_System.String prefix;
        public FOS_System.String Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;
            }
        }

        protected FileSystem theFileSystem;
        public FileSystem TheFileSystem
        {
            get
            {
                return theFileSystem;
            }
        }

        public FileSystemMapping(FOS_System.String aPrefix, FileSystem aFileSystem)
        {
            prefix = aPrefix;
            theFileSystem = aFileSystem;
        }

        public bool PathMatchesMapping(FOS_System.String aPath)
        {
            return aPath.StartsWith(prefix);
        }
        public FOS_System.String RemoveMappingPrefix(FOS_System.String aPath)
        {
            return aPath.Substring(prefix.length, aPath.length - prefix.length);
        }
    }
}
