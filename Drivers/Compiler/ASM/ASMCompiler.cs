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

using Drivers.Compiler.IL;

namespace Drivers.Compiler.ASM
{
    /// <summary>
    ///     The ASM Compiler is the high-level class which manages the ASM compilation step
    ///     including executing the ASM Preprocessor and the ASM Processor.
    /// </summary>
    /// <remarks>
    ///     The ASM compiler itself does very little work. It is mostly a wrapper to ensure
    ///     that the processing steps (i.e. preprocess followed by process) happen in the
    ///     correct order and are hidden from other parts of the compiler. It also allows
    ///     additional ASM processing steps to be added more easily later.
    /// </remarks>
    public static class ASMCompiler
    {
        /// <summary>
        ///     Compiles the specified IL Library and any dependencies using the ASM Preprocesor and ASM Processor.
        /// </summary>
        /// <remarks>
        ///     The ASM Compiler's steps convert the ASM into machine code.
        /// </remarks>
        /// <param name="TheILLibrary">The library to compile.</param>
        /// <returns>CompileResult.OK if all steps complete successfully.</returns>
        public static CompileResult Compile(ILLibrary TheILLibrary)
        {
            CompileResult Result = CompileResult.OK;

            foreach (ILLibrary depLib in TheILLibrary.Dependencies)
            {
                Result = Result == CompileResult.OK ? Compile(depLib) : Result;
            }

            Result = Result == CompileResult.OK ? ExecuteASMPreprocessor(TheILLibrary.TheASMLibrary) : Result;

            if (Result == CompileResult.OK)
            {
                Result = ExecuteASMProcessor(TheILLibrary.TheASMLibrary);
            }

            return Result;
        }

        /// <summary>
        ///     Executes the ASM Preprocessor for the specified library.
        /// </summary>
        /// <param name="TheLibrary">The library to execute the preprocessor on.</param>
        /// <returns>The return value from the ASMPreprocessor.Preprocess method.</returns>
        private static CompileResult ExecuteASMPreprocessor(ASMLibrary TheLibrary)
        {
            return ASMPreprocessor.Preprocess(TheLibrary);
        }

        /// <summary>
        ///     Executes the ASM Processor for the specified library.
        /// </summary>
        /// <param name="TheLibrary">The library to execute the processor on.</param>
        /// <returns>The return value from the ASMProcessor.Process method.</returns>
        private static CompileResult ExecuteASMProcessor(ASMLibrary TheLibrary)
        {
            return ASMProcessor.Process(TheLibrary);
        }
    }
}