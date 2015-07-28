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
using Drivers.Compiler.ASM;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Label : ASM.ASMLabel
    {
        public Label() : base()
        {
        }

        public Label(bool methodLabel) : base(methodLabel)
        {
        }

        /// <summary>
        /// Generates the label itself (using the ASM block) and the line of assembly for the label.
        /// </summary>
        /// <param name="theBlock">The block for which the comment is to be generated.</param>
        /// <returns>The complete line of assembly code.</returns>
        public override string Convert(ASMBlock theBlock)
        {
            if (MethodLabel)
            {
                return theBlock.GenerateMethodLabel() + ":";
            }
            else
            {
                return theBlock.GenerateILOpLabel(ILPosition, Extension) + ":";
            }
        }
    }
}
