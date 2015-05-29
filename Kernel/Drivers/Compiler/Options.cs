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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Drivers.Compiler
{
    public static class Options
    {
        public static List<string> ValidTargetArchitectures = new List<string> {
            "x86"
        };

        public enum BuildModes
        {
            Debug,
            Release
        }

        public enum LinkModes
        {
            ISO,
            ELF
        }

        static Options()
        {
            AddressSizeInBytes = 4;
        }

        public static string LibraryPath
        {
            get;
            set;
        }
        public static string OutputPath
        {
            get;
            set;
        }
        public static string ToolsPath
        {
            get;
            set;
        }

        public static BuildModes BuildMode
        {
            get;
            set;
        }
        public static LinkModes LinkMode
        {
            get;
            set;
        }
        public static string TargetArchitecture
        {
            get;
            set;
        }

        public static int AddressSizeInBytes
        {
            get;
            set;
        }

        private static List<string> ignoreAssemblies = new List<string>
        {
            "mscorlib",
            "Kernel.Compiler",
            "Drivers.Compiler"
        };
        public static List<string> IgnoreAssemblies
        {
            get
            {
                return ignoreAssemblies;
            }
            set
            {
                ignoreAssemblies = value;
            }
        }

        public static void Format()
        {
            TargetArchitecture = TargetArchitecture.ToLower();
        }
        public static Tuple<bool, string> Validate()
        {
            if (!File.Exists(LibraryPath))
            {
                return new Tuple<bool,string>(false, "Could not find library file! Library Path argument is invalid.");
            }

            if (!Directory.Exists(OutputPath))
            {
                return new Tuple<bool,string>(false, "Could not find output directory! Output Path argument is invalid.");
            }

            if (!Directory.Exists(ToolsPath))
            {
                return new Tuple<bool,string>(false, "Could not find tools directory! Tools Path argument is invalid.");
            }

            if (!Options.ValidTargetArchitectures.Contains(TargetArchitecture))
            {
                return new Tuple<bool,string>(false, 
                    "Invalid Target Architecture specified! Valid architectures: " +
                    string.Join(", ", Options.ValidTargetArchitectures));
            }

            return new Tuple<bool,string>(true, "");
        }
    }
}
