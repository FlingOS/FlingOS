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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents a file system mapping. A file system 
    /// mapping maps a path prefix (e.g. A:/) to a particular
    /// file system.
    /// 
    /// </summary>
    public class FileSystemMapping : FOS_System.Object
    {
        /// <summary>
        /// The prefix to map. This must be unique.
        /// </summary>
        protected FOS_System.String prefix;
        /// <summary>
        /// The prefix to map. This must be unique.
        /// </summary>
        public FOS_System.String Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value.ToUpper();
            }
        }

        /// <summary>
        /// The file system to map.
        /// </summary>
        protected FileSystem theFileSystem;
        /// <summary>
        /// The file system to map.
        /// </summary>
        public FileSystem TheFileSystem
        {
            get
            {
                return theFileSystem;
            }
        }

        /// <summary>
        /// Initializes a new file system mapping.
        /// </summary>
        /// <param name="aPrefix">The prefix to map.</param>
        /// <param name="aFileSystem">The file system to map.</param>
        public FileSystemMapping(FOS_System.String aPrefix, FileSystem aFileSystem)
        {
            prefix = aPrefix;
            theFileSystem = aFileSystem;
        }

        /// <summary>
        /// Determines whether the specified path starts with this
        /// mapping's prefix.
        /// </summary>
        /// <param name="aPath">The path to check.</param>
        /// <returns>
        /// Whether the specified path starts with this
        /// mapping's prefix.
        /// </returns>
        public bool PathMatchesMapping(FOS_System.String aPath)
        {
            return aPath.ToUpper().StartsWith(prefix);
        }
        /// <summary>
        /// Removes the mapping's prefix from the specified path.
        /// </summary>
        /// <param name="aPath">The path to remove the prefix from.</param>
        /// <returns>The path without the prefix.</returns>
        public FOS_System.String RemoveMappingPrefix(FOS_System.String aPath)
        {
            return aPath.Substring(prefix.length, aPath.length - prefix.length);
        }
    }
}
