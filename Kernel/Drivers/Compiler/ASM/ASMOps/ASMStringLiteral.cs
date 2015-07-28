using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    [ASMOpTarget(Target=OpCodes.StringLiteral)]
    public abstract class ASMStringLiteral : ASMOp
    {
        public string Id;
        public string StringTypeId;
        public byte[] LengthBytes;
        public char[] Characters;

        public ASMStringLiteral(string id, string stringTypeId, byte[] lengthBytes, char[] characters)
        {
            Id = id;
            StringTypeId = stringTypeId;
            LengthBytes = lengthBytes;
            Characters = characters;
        }
    }
}
