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
    ///     Represents a label for an external dependency of an ASM block.
    /// </summary>
    [ASMOpTarget(Target = OpCodes.ExternalLabel)]
    public abstract class ASMExternalLabel : ASMOp
    {
        /// <summary>
        ///     The external label.
        /// </summary>
        public string Label;

        public ASMExternalLabel(string label)
        {
            Label = label;
        }

        /// <summary>
        ///     Gets a hash code for the external label which can be used for comparison to prevent
        ///     duplicate external labels being added.
        /// </summary>
        /// <remarks>
        ///     Uses the hash code of the Label field.
        /// </remarks>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }

        /// <summary>
        ///     Compares the external label to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the object is an external label and has the same value for Label. Otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ASMExternalLabel)
            {
                return Label.Equals(((ASMExternalLabel)obj).Label);
            }
            return base.Equals(obj);
        }
    }
}