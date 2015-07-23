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

namespace Drivers.Compiler.ASM
{
    /// <summary>
    /// Represents a label for an external dependency of an ASM block.
    /// </summary>
    /// <remarks>
    /// The Convert method ought to be abstracted to the target architecture library
    /// since different assembly syntaxes use different syntaxes for denoting external 
    /// labels. The target architecture determines the syntax.
    /// </remarks>
    public class ASMExternalLabel : ASMOp
    {
        /// <summary>
        /// The external label.
        /// </summary>
        public string Label;

        /// <summary>
        /// Generates the line of assembly for the external label.
        /// </summary>
        /// <param name="theBlock">The block for which the comment is to be generated.</param>
        /// <returns>The complete line of assembly code.</returns>
        public override string Convert(ASMBlock theBlock)
        {
            return "extern " + Label;
        }

        /// <summary>
        /// Gets a hash code for the external label which can be used for comparison to prevent
        /// duplicate external labels being added.
        /// </summary>
        /// <remarks>
        /// Uses the hash code of the Label field.
        /// </remarks>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }
        /// <summary>
        /// Compares the external label to the specified object.
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
