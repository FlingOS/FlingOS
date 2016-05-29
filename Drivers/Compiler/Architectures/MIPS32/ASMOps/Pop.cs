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

using Drivers.Compiler.ASM;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Pop : ASMOp
    {
        public string Dest;
        public bool SignExtend = false;
        public OperandSize Size;

        public override string Convert(ASMBlock TheBlock)
        {
            int numBytes = (int)Size;
            string loadOp = "";
            switch (Size)
            {
                case OperandSize.Byte:
                    if (SignExtend)
                    {
                        loadOp = "lb";
                    }
                    else
                    {
                        loadOp = "lbu";
                    }
                    break;
                case OperandSize.Halfword:
                    if (SignExtend)
                    {
                        loadOp = "lh";
                    }
                    else
                    {
                        loadOp = "lhu";
                    }
                    break;
                case OperandSize.Word:
                    loadOp = "lw";
                    break;
            }
            return loadOp + " " + Dest + ", 0($sp)\r\n" +
                   "addi $sp, $sp, " + numBytes;
        }
    }
}