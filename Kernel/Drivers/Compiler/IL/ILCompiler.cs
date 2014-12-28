using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.IL
{
    public static class ILCompiler
    {
        public static CompileResult Compile(ILLibrary TheLibrary)
        {
            CompileResult Result = ExecuteILReader(TheLibrary);

            if (Result == CompileResult.OK)
            {
                Result = ExecuteILPreprocessor(TheLibrary);

                if (Result == CompileResult.OK)
                {
                    Result = ExecuteILScanner(TheLibrary);
                }
            }

            return Result;
        }

        private static CompileResult ExecuteILReader(ILLibrary TheLibrary)
        {
            return CompileResult.OK;
        }

        private static CompileResult ExecuteILPreprocessor(ILLibrary TheLibrary)
        {
            return CompileResult.OK;
        }

        private static CompileResult ExecuteILScanner(ILLibrary TheLibrary)
        {
            return CompileResult.OK;
        }
    }
}
