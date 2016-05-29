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

namespace Drivers.Compiler.ASM.ASMOps
{
    /// <summary>
    ///     Represents a label which external ASM blocks can depend upon i.e. a global label.
    /// </summary>
    [ASMOpTarget(Target = OpCodes.GlobalLabel)]
    public abstract class ASMGlobalLabel : ASMOp
    {
        /// <summary>
        ///     The global label.
        /// </summary>
        public string Label;

        /// <summary>
        ///     The type specifier for the label. e.g. "function" or "data".
        /// </summary>
        public string LabelType = "function";

        public ASMGlobalLabel(string label)
        {
            Label = label;
        }

        public ASMGlobalLabel(string label, string labelType)
        {
            Label = label;
            LabelType = labelType;
        }

        /// <summary>
        ///     Gets a hash code for the global label which can be used for comparison to prevent
        ///     duplicate global labels being added.
        /// </summary>
        /// <remarks>
        ///     Uses the hash code of (Label + ":" + LabelType).
        /// </remarks>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            return (Label + ":" + LabelType).GetHashCode();
        }

        /// <summary>
        ///     Compares the global label to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the object is a global label and has the same value for Label and LabelType. Otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ASMGlobalLabel)
            {
                return Label.Equals(((ASMGlobalLabel)obj).Label) &&
                       LabelType.Equals(((ASMGlobalLabel)obj).LabelType);
            }
            return base.Equals(obj);
        }
    }
}