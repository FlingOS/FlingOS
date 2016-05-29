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
using System.IO;

namespace Drivers.Compiler
{
    /// <summary>
    ///     Static class for providing options to the compiler.
    /// </summary>
    public static class Options
    {
        /// <summary>
        ///     The available build modes.
        /// </summary>
        public enum BuildModes
        {
            /// <summary>
            ///     Specifies the compiler should include debug information and perform no optimisation.
            /// </summary>
            Debug,

            /// <summary>
            ///     Specifies the compiler should include no debug information and perform all optimisations.
            /// </summary>
            Release
        }

        /// <summary>
        ///     The available link modes.
        /// </summary>
        public enum LinkModes
        {
            /// <summary>
            ///     Specifies the compiler should compile an operating system to a .ISO file, including all dependencies such as
            ///     libraries.
            /// </summary>
            ISO,

            /// <summary>
            ///     Specifies the compiler should compile an ELF executable, and compile dependencies to library files (.a files).
            /// </summary>
            ELF
        }

        /// <summary>
        ///     List of all valid target architectures that are supported by the compiler.
        /// </summary>
        public static List<string> ValidTargetArchitectures = new List<string>
        {
            "x86",
            "mips"
        };

        /// <summary>
        ///     List of assembly names to ignore when compiling.
        /// </summary>
        private static List<string> ignoreAssemblies = new List<string>
        {
            "mscorlib",
            "Kernel.Compiler",
            "Drivers.Compiler"
        };

        /// <summary>
        ///     Path to the IL library to compile (the .dll or .exe file).
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static string LibraryPath { get; set; }

        /// <summary>
        ///     Path to the output folder.
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static string OutputPath { get; set; }

        /// <summary>
        ///     Path to the Drivers Compiler's Tools folder.
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static string ToolsPath { get; set; }

        /// <summary>
        ///     The build mode.
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static BuildModes BuildMode { get; set; }

        /// <summary>
        ///     The link mode.
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static LinkModes LinkMode { get; set; }

        /// <summary>
        ///     The target architecture. Should be one of the ones listed in <see cref="ValidTargetArchitectures" />.
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static string TargetArchitecture { get; set; }

        /// <summary>
        ///     The size of an address in bytes. 4 for 32-bit architecture, 8 for 64-bit, etc.
        /// </summary>
        /// <value>
        ///     Gets/sets an implicitly defined field.
        /// </value>
        public static int AddressSizeInBytes { get; set; }

        public static ulong BaseAddress { get; set; }

        public static long LoadOffset { get; set; }

        public static bool ShortenDependencyNames { get; set; }

        /// <summary>
        ///     List of assembly names to ignore when compiling.
        /// </summary>
        /// <value>
        ///     Gets/sets <see cref="ignoreAssemblies" />.
        /// </value>
        public static List<string> IgnoreAssemblies
        {
            get { return ignoreAssemblies; }
            set { ignoreAssemblies = value; }
        }

        /// <summary>
        ///     Initialises default options.
        /// </summary>
        static Options()
        {
            // Assume 32-bit architecture
            AddressSizeInBytes = 4;
            BaseAddress = 0;
            LoadOffset = 0;
            ShortenDependencyNames = true;
        }

        /// <summary>
        ///     Formats the input values to be in the format the compiler requires.
        /// </summary>
        public static void Format()
        {
            TargetArchitecture = TargetArchitecture.ToLower();
        }

        /// <summary>
        ///     Validates the options.
        /// </summary>
        /// <returns>True if all options are valid. Otherwise, false with a message describing what is wrong.</returns>
        public static Tuple<bool, string> Validate()
        {
            if (!File.Exists(LibraryPath))
            {
                return new Tuple<bool, string>(false, "Could not find library file! Library Path argument is invalid.");
            }

            if (!Directory.Exists(OutputPath))
            {
                return new Tuple<bool, string>(false,
                    "Could not find output directory! Output Path argument is invalid.");
            }

            if (!Directory.Exists(ToolsPath))
            {
                return new Tuple<bool, string>(false, "Could not find tools directory! Tools Path argument is invalid.");
            }

            if (!ValidTargetArchitectures.Contains(TargetArchitecture))
            {
                return new Tuple<bool, string>(false,
                    "Invalid Target Architecture specified! Valid architectures: " +
                    string.Join(", ", ValidTargetArchitectures));
            }

            return new Tuple<bool, string>(true, "");
        }
    }
}