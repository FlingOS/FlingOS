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

using Drivers.Compiler.Types;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     The IL Compiler is the high-level class which manages the IL compilation step
    ///     including executing the IL Reader, the IL Preprocessor and the IL Scanner.
    /// </summary>
    /// <remarks>
    ///     The IL compiler itself does very little work. It is mostly a wrapper to ensure
    ///     that the processing steps happen in the correct order and are hidden from other
    ///     parts of the compiler. It also allows additional IL processing steps to be added
    ///     more easily later.
    /// </remarks>
    public static class ILCompiler
    {
        /// <summary>
        ///     Compiles the specified IL library and any dependencies using the IL Reader, Il Preprocessor and IL Scanner.
        /// </summary>
        /// <remarks>
        ///     The IL Compiler's steps read in the IL bytes for each block into IL ops and then preprocess and compile them
        ///     into ASM ops.
        /// </remarks>
        /// <param name="TheLibrary">The library to compile.</param>
        /// <returns>CompileResult.OK if all steps complete successfully.</returns>
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

        /// <summary>
        ///     Executes the IL Reader for the specified library.
        /// </summary>
        /// <param name="TheLibrary">The library to read.</param>
        /// <returns>The return value from the IL Reader.</returns>
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

            foreach (TypeInfo aTypeInfo in TheLibrary.TypeInfos)
            {
                foreach (MethodInfo aMethodInfo in aTypeInfo.MethodInfos)
                {
                    TheLibrary.ILBlocks.Add(aMethodInfo, ILReader.Read(aMethodInfo));
                }
            }

            return result;
        }

        /// <summary>
        ///     Executes the IL Preprocessor for the specified library.
        /// </summary>
        /// <param name="TheLibrary">The library to preprocess.</param>
        /// <returns>The return value from the IL Preprocessor.</returns>
        private static CompileResult ExecuteILPreprocessor(ILLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            ILPreprocessor.Preprocess(TheLibrary);

            ILPreprocessor.PreprocessSpecialClasses(TheLibrary);
            ILPreprocessor.PreprocessSpecialMethods(TheLibrary);

            return result;
        }

        /// <summary>
        ///     Executes the IL Scanner for the specified library.
        /// </summary>
        /// <param name="TheLibrary">The library to scan.</param>
        /// <returns>The return value from the IL Scanner.</returns>
        private static CompileResult ExecuteILScanner(ILLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            result = ILScanner.Scan(TheLibrary);

            return result;
        }
    }
}