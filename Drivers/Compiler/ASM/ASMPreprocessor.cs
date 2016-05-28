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

using System.Linq;
using Drivers.Compiler.ASM.ASMOps;

namespace Drivers.Compiler.ASM
{
    /// <summary>
    ///     The ASM Preprocessor manages evaluating ASM blocks and altering the ASM (by additions,
    ///     removals and even discarding entire blocks). It executes before the ASM Processor
    ///     performs the main compilation step.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Empty ASM blocks cause problems in later processing so are removed from the library
    ///         entirely. For this reason, the rest of the compiler should avoid caching copies of
    ///         ASM blocks when they aren't currently processing them.
    ///     </para>
    ///     <para>
    ///         The ASM Preprocessor also handles injecting IL ops for the method label, the global
    ///         labels and the external labels.
    ///     </para>
    /// </remarks>
    public static class ASMPreprocessor
    {
        /// <summary>
        ///     Preprocesses the specified ASM library.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Loops over all the ASM blocks, removing empty ones, checking plug paths and
        ///         preprocessing non-empty blocks.
        ///     </para>
        /// </remarks>
        /// <param name="TheLibrary">The library to preprocess.</param>
        /// <returns>Always CompileResult.OK. In all other cases, exceptions are thrown.</returns>
        public static CompileResult Preprocess(ASMLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            if (TheLibrary.ASMPreprocessed)
            {
                return result;
            }
            TheLibrary.ASMPreprocessed = true;

            for (int i = 0; i < TheLibrary.ASMBlocks.Count; i++)
            {
                ASMBlock aBlock = TheLibrary.ASMBlocks[i];
                if (aBlock.Plugged)
                {
                    if (string.IsNullOrWhiteSpace(aBlock.PlugPath))
                    {
                        TheLibrary.ASMBlocks.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    if (aBlock.ASMOps.Count == 0)
                    {
                        TheLibrary.ASMBlocks.RemoveAt(i);
                        i--;
                        continue;
                    }
                    Preprocess(aBlock);
                }
            }

            return result;
        }

        /// <summary>
        ///     Preprocesses the given ASM block.
        /// </summary>
        /// <param name="theBlock">The block to preprocess.</param>
        private static void Preprocess(ASMBlock theBlock)
        {
            // Due to "insert 0", asm ops are constructed in reverse order here

            int InsertIndex = 0;

            if (!typeof(ASMHeader).IsAssignableFrom(theBlock.ASMOps[InsertIndex].GetType()))
            {
                ASMOp newHeaderOp = TargetArchitecture.CreateASMOp(OpCodes.Header, "text");
                theBlock.ASMOps.Insert(InsertIndex, newHeaderOp);
            }

            InsertIndex = 1;

            string currMethodLabel = theBlock.GenerateMethodLabel();
            if (currMethodLabel != null)
            {
                ASMOp newLabelOp = TargetArchitecture.CreateASMOp(OpCodes.Label, true);
                theBlock.ASMOps.Insert(InsertIndex, newLabelOp);
            }
            if (currMethodLabel != null)
            {
                ASMOp newGlobalLabelOp = TargetArchitecture.CreateASMOp(OpCodes.GlobalLabel, currMethodLabel);
                theBlock.ASMOps.Insert(InsertIndex, newGlobalLabelOp);
            }

            foreach (string anExternalLabel in theBlock.ExternalLabels.Distinct())
            {
                if (anExternalLabel != currMethodLabel)
                {
                    ASMOp newExternalLabelOp = TargetArchitecture.CreateASMOp(OpCodes.ExternalLabel, anExternalLabel);
                    theBlock.ASMOps.Insert(InsertIndex, newExternalLabelOp);
                }
            }
        }
    }
}