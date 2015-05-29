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
            CompileResult result = CompileResult.OK;

            if (TheLibrary.ILRead)
            {
                return result;
            }
            TheLibrary.ILRead = true;

            foreach (ILLibrary depLib in TheLibrary.Dependencies)
            {
                result = result == CompileResult.OK ? ExecuteILReader(depLib) : result;
            }

            foreach (Types.TypeInfo aTypeInfo in TheLibrary.TypeInfos)
            {
                foreach (Types.MethodInfo aMethodInfo in aTypeInfo.MethodInfos)
                {
                    TheLibrary.ILBlocks.Add(aMethodInfo, ILReader.Read(aMethodInfo));
                }
            }

            return result;
        }

        private static CompileResult ExecuteILPreprocessor(ILLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            if (ILScanner.Init())
            {
                ILPreprocessor.Preprocess(TheLibrary);

                ILPreprocessor.PreprocessSpecialClasses(TheLibrary);
                ILPreprocessor.PreprocessSpecialMethods(TheLibrary);
            }
            else
            {
                result = CompileResult.Fail;
            }

            return result;
        }

        private static CompileResult ExecuteILScanner(ILLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            result = ILScanner.Scan(TheLibrary);

            return result;
        }
    }
}
