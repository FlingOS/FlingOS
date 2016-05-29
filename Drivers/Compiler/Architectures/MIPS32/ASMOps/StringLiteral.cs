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
using System.Text;
using Drivers.Compiler.ASM;
using Drivers.Compiler.ASM.ASMOps;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class StringLiteral : ASMStringLiteral
    {
        public StringLiteral(string id, string stringTypeId, byte[] lengthBytes, char[] characters)
            : base(id, stringTypeId, lengthBytes, characters)
        {
        }

        public override string Convert(ASMBlock TheBlock)
        {
            StringBuilder LiteralASM = new StringBuilder();
            //This is UTF-16 (Unicode)/ASCII text
            LiteralASM.AppendLine(string.Format(".globl {0}", Id));
            LiteralASM.AppendLine(".align 2");
            LiteralASM.AppendLine(string.Format("{0}:", Id));
            //Put in type info as Framework.String type
            LiteralASM.AppendLine(string.Format(".word {0}", StringTypeId));
            //Put in string length bytes
            LiteralASM.Append(".align 0\n.byte ");
            for (int i = 0; i < 3; i++)
            {
                LiteralASM.Append(LengthBytes[i]);
                LiteralASM.Append(", ");
            }
            LiteralASM.Append(LengthBytes[3]);

            if (Characters.Length > 0)
            {
                //Put in string characters (as words)
                LiteralASM.Append("\n.align 0\n.hword ");
                for (int i = 0; i < Characters.Length - 1; i++)
                {
                    LiteralASM.Append((uint)Characters[i]);
                    LiteralASM.Append(", ");
                }
                LiteralASM.Append((uint)Characters.Last());
            }

            LiteralASM.AppendLine();

            return LiteralASM.ToString();
        }
    }
}