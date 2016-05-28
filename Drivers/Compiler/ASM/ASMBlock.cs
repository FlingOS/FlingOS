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

using System.Collections.Generic;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.ASM
{
    /// <summary>
    ///     Represents a block of assembly code.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An ASM Block is a section of assembly code which can be considered as a single unit.
    ///         It may have external dependencies (such as variables and other methods) but it should
    ///         itself not contain, for example, multiple methods.
    ///     </para>
    ///     <para>
    ///         The ASM Block has a PlugPath field which should be set to the path to the plug assembly
    ///         file, if there is one. The path should be relative to the root of the build directory
    ///         (e.g. relative to bin\Debug meaning the path would be @"ASM\Example". The extension
    ///         should not be included in the plug path as it is added by the compiler to target the
    ///         specific architecture.
    ///     </para>
    ///     <para>
    ///         This class provides generic methods for adding assembler ops and external and global
    ///         labels. The labels can be for other methods, variables or similar or they can be for
    ///         specific IL/ASM ops within the block.
    ///     </para>
    ///     <para>
    ///         Lastly, assembler blocks have a Priority (which is usually copied across from the
    ///         originating IL block). However, the Priority can be set by the ASM Preprocessor to
    ///         override the default output position.
    ///     </para>
    /// </remarks>
    /// <seealso cref="Drivers.Compiler.ASM.ASMBlock.Priority" />
    public class ASMBlock
    {
        /// <summary>
        ///     The assembly ops contained within this ASM block.
        /// </summary>
        /// <remarks>
        ///     These should be ignored if the block is plugged. This list might not be empty
        ///     even if the block is plugged.
        /// </remarks>
        public List<ASMOp> ASMOps = new List<ASMOp>();

        /// <summary>
        ///     The path to which the ASM block is outputted as ASM text.
        /// </summary>
        public string ASMOutputFilePath;

        /// <summary>
        ///     The external labels required by this block.
        /// </summary>
        /// <remarks>
        ///     These should be ignored if the block is plugged.
        /// </remarks>
        public List<string> ExternalLabels = new List<string>();

        /// <summary>
        ///     The global labels required by this block.
        /// </summary>
        /// <remarks>
        ///     These should be ignored if the block is plugged.
        /// </remarks>
        public List<string> GlobalLabels = new List<string>();

        /// <summary>
        ///     The path to which the ASM block is outputted as an object file.
        /// </summary>
        public string ObjectOutputFilePath;

        /// <summary>
        ///     The method info from which the assembler block originated.
        /// </summary>
        /// <remarks>
        ///     Not all assembly blocks will originate from a method. Some assembler blocks
        ///     are generated and injected by the compiler itself. For example, type tables
        ///     do not have an origin method.
        /// </remarks>
        public MethodInfo OriginMethodInfo;

        public bool PageAlign = false;
        public string PageAlignLabel = null;

        /// <summary>
        ///     The relative path to the plug file excluding the extension. The path should
        ///     be relative to the build directory e.g. @"ASM\Example". Ensure that plug files
        ///     have the "Copy to output: If newer" property set.
        /// </summary>
        public string PlugPath = null;

        /// <summary>
        ///     The priority of this block. Lower (or more negative) results is higher priority i.e.
        ///     appears first in the file.
        /// </summary>
        /// <remarks>
        ///     ASM blocks are ordered by ascending priority value before being outputted to the file.
        ///     It is assumed that blocks with the same priority can go in any order. The default
        ///     priority is 0.
        /// </remarks>
        public long Priority = 0;

        /// <summary>
        ///     Whether the ASM block is plugged or not.
        /// </summary>
        /// <value>Gets whether the PlugPath is not null.</value>
        public bool Plugged
        {
            get { return PlugPath != null; }
        }

        /// <summary>
        ///     Appends the specified op to the list of ASM ops.
        /// </summary>
        /// <remarks>
        ///     This should be used in preference to ASMOps.Add as it allows injection / manipulation
        ///     on an op-by-op basis, as they are added.
        /// </remarks>
        /// <param name="anOp">The op to append.</param>
        public void Append(ASMOp anOp)
        {
            ASMOps.Add(anOp);
        }

        /// <summary>
        ///     Adds the specified label as a required external label i.e. an external dependency.
        /// </summary>
        /// <param name="label">The label to add.</param>
        public void AddExternalLabel(string label)
        {
            ExternalLabels.Add(label);
        }

        /// <summary>
        ///     Adds the specified label as a global label i.e. a label which other files might depend upon.
        /// </summary>
        /// <param name="label">The label to add.</param>
        public void AddGlobalLabel(string label)
        {
            GlobalLabels.Add(label);
        }

        /// <summary>
        ///     Generates the unique label for the method that this block originated from.
        /// </summary>
        /// <returns>The label or null if the block has no origin.</returns>
        public string GenerateMethodLabel()
        {
            if (OriginMethodInfo != null)
            {
                return GenerateLabel(OriginMethodInfo.ID);
            }
            return null;
        }

        /// <summary>
        ///     Generates a local label with specified extension for the requested IL op position.
        /// </summary>
        /// <param name="ILPosition">The offset of the IL op from the start of the method. Default: No offset (int.MinValue).</param>
        /// <param name="Extension">The extension to append to the label. Default: none (null).</param>
        /// <returns>The label.</returns>
        public string GenerateILOpLabel(int ILPosition, string Extension = null)
        {
            return GenerateLabel(null, ILPosition, Extension);
        }

        /// <summary>
        ///     Generates a label for the specified method with specified IL position and extension.
        /// </summary>
        /// <remarks>
        ///     If only the method ID is supplied, a global label is produced. Otherwise, the method ID is
        ///     omitted and a local label is produced.
        /// </remarks>
        /// <param name="MethodID">Required. The ID of the method the label is for.</param>
        /// <param name="ILPosition">Optional. The offset of the IL op from the start of the method.</param>
        /// <param name="Extension">Optional. The extension text to append to the label.</param>
        /// <returns>The label.</returns>
        public static string GenerateLabel(string MethodID, int ILPosition = int.MinValue, string Extension = null)
        {
            if (ILPosition != int.MinValue)
            {
                if (!string.IsNullOrWhiteSpace(MethodID))
                {
                    if (!string.IsNullOrWhiteSpace(Extension))
                    {
                        return string.Format("{0}.IL_{1}_{2}", MethodID, ILPosition.ToString("X2"), Extension);
                    }

                    return string.Format("{0}.IL_{1}", MethodID, ILPosition.ToString("X2"));
                }

                if (!string.IsNullOrWhiteSpace(Extension))
                {
                    return string.Format(".IL_{0}_{1}", ILPosition.ToString("X2"), Extension);
                }

                return string.Format(".IL_{0}", ILPosition.ToString("X2"));
            }

            return MethodID;
        }
    }
}