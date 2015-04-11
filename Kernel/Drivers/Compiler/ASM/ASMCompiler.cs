#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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

namespace Drivers.Compiler.ASM
{
    public static class ASMCompiler
    {
        public static CompileResult Compile(IL.ILLibrary TheILLibrary)
        {
            CompileResult Result = CompileResult.OK;

            foreach(IL.ILLibrary depLib in TheILLibrary.Dependencies)
            {
                Compile(depLib);
            }

            Result = Result == CompileResult.OK ? ExecuteASMPreprocessor(TheILLibrary.TheASMLibrary) : Result;

            if (Result == CompileResult.OK)
            {
                Result = ExecuteASMProcessor(TheILLibrary.TheASMLibrary);
            }

            return Result;
        }

        private static CompileResult ExecuteASMPreprocessor(ASMLibrary TheLibrary)
        {
            return ASMPreprocessor.Preprocess(TheLibrary);
        }

        private static CompileResult ExecuteASMProcessor(ASMLibrary TheLibrary)
        {
            return ASMProcessor.Process(TheLibrary);
        }
    }
}
