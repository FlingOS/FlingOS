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

using System.Collections.Generic;

namespace Drivers.Compiler
{
    /// <summary>
    ///     Defines error codes and messages used throughout the compiler.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        ///     Error code for invalid options specified on the command line or in MSBuild.
        /// </summary>
        public const string PreReqs_OptionsInvalid_ErrorCode = "PR0001";

        /// <summary>
        ///     Error code for any unexpected exception that occurs during compiler initialisation.
        /// </summary>
        public const string PreReqs_UnexpectedError_ErrorCode = "PR0003";

        /// <summary>
        ///     Error code for an invalid library path meaning the loader couldn't load the IL library to compile.
        /// </summary>
        public const string Loader_LibraryPathNullOrEmpty_ErrorCode = "LDR0001";

        /// <summary>
        ///     Error code for the user specifying a path to a library file which doesn't exist meaning the loader couldn't load
        ///     the IL library to compile.
        /// </summary>
        public const string Loader_LibraryFileDoesntExist_ErrorCode = "LDR0002";

        /// <summary>
        ///     Error code for any unexpected / unhandled exception that occurs during the compiler load process.
        /// </summary>
        public const string Loader_UnexpectedError_ErrorCode = "LDR0003";

        /// <summary>
        ///     Error code for the IL compiler failing by returning CompileResult.Fail.
        /// </summary>
        public const string ILCompiler_CompileFailed_ErrorCode = "ILC0001";

        /// <summary>
        ///     Error code for the IL compiler failing due to a null reference exception.
        /// </summary>
        /// <remarks>
        ///     Usually caused by a special class or special method missing / not specified in the
        ///     library being compiled. Alternatively, an IL op being used which is not supported
        ///     by the target architecture library.
        /// </remarks>
        public const string ILCompiler_NullRefException_ErrorCode = "ILC0002";

        /// <summary>
        ///     Error code for the IL compiler failing due to an unexpected exception.
        /// </summary>
        public const string ILCompiler_UnexpectedException_ErrorCode = "ILC0003";

        /// <summary>
        ///     Error code for the IL compiler failing because it couldn't load the target architecture library.
        /// </summary>
        public const string ILCompiler_LoadTargetArchError_ErrorCode = "ILC0004";

        /// <summary>
        ///     Error code for the IL compiler failing because the scanner couldn't scan a particular IL op.
        /// </summary>
        public const string ILCompiler_ScanILOpFailure_ErrorCode = "ILC0005";

        /// <summary>
        ///     Error code for the IL compiler failing due to a custom warning exception.
        /// </summary>
        public const string ILCompiler_ScanILOpCustomWarning_ErrorCode = "ILC0006";

        /// <summary>
        ///     Error code for the ASM compiler failing by returning CompileResult.Fail.
        /// </summary>
        public const string ASMCompiler_CompileFailed_ErrorCode = "ASMC0001";

        /// <summary>
        ///     Error code for the ASM compiler failing due to the external assembly code compiler (e.g. NASM) reporting an error
        ///     or crashing.
        /// </summary>
        public const string ASMCompiler_ASMCodeCompilerException_ErrorCode = "ASMC0002";

        /// <summary>
        ///     Error code for the ASM compiler failing due to an unexpected / unhandled exception.
        /// </summary>
        public const string ASMCompiler_UnexpectedException_ErrorCode = "ASMC0003";

        /// <summary>
        ///     Error code for the linker failing by returning CompileResult.Fail.
        /// </summary>
        public const string Linker_LinkFailed_ErrorCode = "LL0001";

        /// <summary>
        ///     Error code for the likner failing due to an unexpected / unhandled exception.
        /// </summary>
        public const string Linker_UnexpectedException_ErrorCode = "LL0003";

        /// <summary>
        ///     Error code for an external process reporting an error to the Utilities (which are used to start/manage external
        ///     processes).
        /// </summary>
        public const string Utilities_ExternalError_ErrorCode = "UT0001";

        /// <summary>
        ///     Error messages mapped from their respective error codes. Some require use with String.Format.
        /// </summary>
        public static Dictionary<string, string> ErrorMessages = new Dictionary<string, string>
        {
            {"PR0001", "Invalid options supplied. {0}"},
            {"PR0002", ""},
            {"PR0003", "Pre-requisites invalid due to an unexpected error. {0}\r\n{1}."},
            {"LDR0001", "Library Loader cannot load file from null or empty file path."},
            {"LDR0002", "Library Loader cannot find file to load. File does not exist."},
            {"LDR0003", "Library Loader encountered an unexpected error. {0}\r\n{1}"},
            {"ILC0001", "The IL compiler failed to compile."},
            {"ILC0002", "The IL compiler failed to compile due to null reference. {0}\r\n{1}"},
            {"ILC0003", "The IL compiler failed to compile due to an unexpected error. {0}\r\n{1}"},
            {"ILC0004", "The IL scanner failed to load the target architecture. {0}"},
            {"ILC0005", "The IL scanner failed to scan an IL op. {0} - {1}"},
            {"ILC0006", "Scanned an IL op which warns the following: {0}"},
            {"ASMC0001", "The ASM compiler failed to compile."},
            {"ASMC0002", "Assembly code compiler (e.g. NASM) failed to execute for file {0}. Message: {1}"},
            {"ASMC0003", "The ASM compiler failed to compile due to an unexpected error. {0}\r\n{1}."},
            {"LL0001", "The linker failed to link."},
            {"LL0002", ""},
            {"LL0003", "The linker failed to link due to an unexpected error. {0}\r\n{1}."},
            {"UT0001", "Utilities: External process reported an error: {0}"}
        };
    }
}