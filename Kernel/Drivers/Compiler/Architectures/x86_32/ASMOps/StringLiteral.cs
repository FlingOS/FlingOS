using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class StringLiteral : ASM.ASMStringLiteral
    {
        public StringLiteral(string id, string stringTypeId, byte[] lengthBytes, char[] characters)
            : base(id, stringTypeId, lengthBytes, characters)
        {
        }

        public override string Convert(ASM.ASMBlock theBlock)
        {
            StringBuilder LiteralASM = new StringBuilder();
            //This is UTF-16 (Unicode)/ASCII text
            LiteralASM.AppendLine(string.Format("GLOBAL {0}:data", Id));
            LiteralASM.AppendLine(string.Format("{0}:", Id));
            //Put in type info as FOS_System.String type
            LiteralASM.AppendLine(string.Format("dd {0}", StringTypeId));
            //Put in string length bytes
            LiteralASM.Append("db ");
            for (int i = 0; i < 3; i++)
            {
                LiteralASM.Append(LengthBytes[i]);
                LiteralASM.Append(", ");
            }
            LiteralASM.Append(LengthBytes[3]);
            //Put in string characters (as words)
            LiteralASM.Append("\ndw ");
            for (int i = 4; i < (Characters.Length - 1); i++)
            {
                LiteralASM.Append((uint)Characters[i]);
                LiteralASM.Append(", ");
            }
            LiteralASM.Append((uint)Characters.Last());
            LiteralASM.AppendLine();

            return LiteralASM.ToString();
        }
    }
}
