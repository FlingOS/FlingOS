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
        public const string ASMCompiler_UnexpectedException_ErrorCode = "ASMC0003";

        public const string Linker_LinkFailed_ErrorCode = "LL0001";
        public const string Linker_UnexpectedException_ErrorCode = "LL0003";

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
            { "ASMC0002", "" },
            { "ASMC0003", "The ASM compiler failed to compile due to an unexpected error. {0}\r\n{1}." },
            
            { "LL0001", "The linker failed to link." },
            { "LL0002", "" },
            { "LL0003", "The linker failed to link due to an unexpected error. {0}\r\n{1}." },
            
        };
    }
}
