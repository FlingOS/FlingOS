using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    public static class ASMCompiler
    {
        public static CompileResult Compile(ASMLibrary TheLibrary)
        {
            CompileResult Result = ExecuteASMPreprocessor(TheLibrary);

            if (Result == CompileResult.OK)
            {
                Result = ExecuteASMProcessor(TheLibrary);
            }

            return Result;
        }

        private static CompileResult ExecuteASMPreprocessor(ASMLibrary TheLibrary)
        {
            return CompileResult.OK;
        }

        private static CompileResult ExecuteASMProcessor(ASMLibrary TheLibrary)
        {
            return CompileResult.OK;
        }
    }
}
