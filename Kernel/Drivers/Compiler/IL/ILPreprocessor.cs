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

namespace Drivers.Compiler.IL
{
    public static class ILPreprocessor
    {
        /* Tasks of the IL Preprocessor:
         *      - Pre-processing for special classes / methods:
         *              - Static constructors
         *      - Pre-scan IL ops to:
         *              - Type Scan any local variable which are of an unscanned types
         *      - Inject GC IL ops
         *      - Inject wrapping try-finally for GC
         *      - Inject IL ops for try-catch-finally
         */

        public static void Preprocess(ILLibrary TheLibrary)
        {
            if (TheLibrary == null)
            {
                return;
            }

            foreach (IL.ILLibrary aDependency in TheLibrary.Dependencies)
            {
                Preprocess(aDependency);
            }

            PreprocessSpecialClasses(TheLibrary);
            PreprocessSpecialMethods(TheLibrary);

            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                PreprocessILOps(TheLibrary, aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
            }

            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                InjectGC(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                InjectTryCatchFinally(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
            }
        }

        private static void PreprocessILOps(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            foreach (Types.VariableInfo aVarInfo in theMethodInfo.LocalInfos)
            {
                //Causes processing of the type - in case it hasn't already been processed
                TheLibrary.GetTypeInfo(aVarInfo.UnderlyingType);
            }
        }
        private static void PreprocessSpecialClasses(ILLibrary TheLibrary)
        {
            //TODO
        }
        private static void PreprocessSpecialMethods(ILLibrary TheLibrary)
        {
            //TODO
        }
        private static void InjectGC(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            //TODO
        }
        private static void InjectTryCatchFinally(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            //TODO
        }
    }
}
