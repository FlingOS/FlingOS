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

        public const string ILCompiler_CompileFailed_ErrorCode = "ILC0001";

        public const string ASMCompiler_CompileFailed_ErrorCode = "ASMC0001";

        public const string Linker_LinkFailed_ErrorCode = "LL0001";

        public static Dictionary<string, string> ErrorMessages = new Dictionary<string, string>()
        {
            { "PR0001", "Invalid options supplied. {0}" },
            { "ILC0001", "The IL compiler failed to compile." },
            { "ASMC0001", "The ASM compiler failed to compile." },
            { "LL0001", "The linker failed to link." }
        };
    }
}
