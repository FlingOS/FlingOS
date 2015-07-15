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

namespace Drivers.Compiler
{
    public static class Errors
    {
        public const string PreReqs_OptionsInvalid_ErrorCode = "PR0001";
        public const string PreReqs_UnexpectedError_ErrorCode = "PR0003";

        public const string Loader_LibraryPathNullOrEmpty_ErrorCode = "LDR0001";
        public const string Loader_LibraryFileDoesntExist_ErrorCode = "LDR0002";
        public const string Loader_UnexpectedError_ErrorCode = "LDR0003";

        public const string ILCompiler_CompileFailed_ErrorCode = "ILC0001";
        public const string ILCompiler_NullRefException_ErrorCode = "ILC0002";
        public const string ILCompiler_UnexpectedException_ErrorCode = "ILC0003";
        public const string ILCompiler_LoadTargetArchError_ErrorCode = "ILC0004";
        public const string ILCompiler_ScanILOpFailure_ErrorCode = "ILC0005";
        public const string ILCompiler_ScanILOpCustomWarning_ErrorCode = "ILC0006";

        public const string ASMCompiler_CompileFailed_ErrorCode = "ASMC0001";
        public const string ASMCompiler_NASMException_ErrorCode = "ASMC0002";
        public const string ASMCompiler_UnexpectedException_ErrorCode = "ASMC0003";

        public const string Linker_LinkFailed_ErrorCode = "LL0001";
        public const string Linker_UnexpectedException_ErrorCode = "LL0003";

        public const string Utilities_ExternalError_ErrorCode = "UT0001";

        public static Dictionary<string, string> ErrorMessages = new Dictionary<string, string>()
        {
            { "PR0001", "Invalid options supplied. {0}" },
            { "PR0002", "" },
            { "PR0003", "Pre-requisites invalid due to an unexpected error. {0}\r\n{1}." },
            
            { "LDR0001", "Library Loader cannot load file from null or empty file path." },
            { "LDR0002", "Library Loader cannot find file to load. File does not exist." },
            { "LDR0003", "Library Loader encountered an unexpected error. {0}\r\n{1}" },

            { "ILC0001", "The IL compiler failed to compile." },
            { "ILC0002", "The IL compiler failed to compile due to null reference. {0}" },
            { "ILC0003", "The IL compiler failed to compile due to an unexpected error. {0}\r\n{1}" },
            { "ILC0004", "The IL scanner failed to load the target architecture. {0}" },
            { "ILC0005", "The IL scanner failed to scan an IL op. {0}" },
            { "ILC0006", "Scanned an IL op which warns the following: {0}" },
            
            { "ASMC0001", "The ASM compiler failed to compile." },
            { "ASMC0002", "NASM failed to execute for file {0}. Message: {1}" },
            { "ASMC0003", "The ASM compiler failed to compile due to an unexpected error. {0}\r\n{1}." },
            
            { "LL0001", "The linker failed to link." },
            { "LL0002", "" },
            { "LL0003", "The linker failed to link due to an unexpected error. {0}\r\n{1}." },
            
            { "UT0001", "Utilities: External process reported an error: {0}" }
        };
    }
}
