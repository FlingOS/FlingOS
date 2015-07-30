using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    [ASMOpTarget(Target=OpCodes.StaticField)]
    public abstract class ASMStaticField : ASMOp
    {
        public string FieldID;
        public string Size;

        public ASMStaticField(string fieldID, string size)
        {
            FieldID = fieldID;
            Size = size;
        }
    }
}
