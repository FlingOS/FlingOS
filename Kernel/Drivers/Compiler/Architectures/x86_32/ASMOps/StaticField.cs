using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class StaticField : ASM.ASMStaticField
    {
        public StaticField(string fieldID, string size)
            : base(fieldID, size)
        {
        }

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return string.Format("GLOBAL {0}:data\r\n{0}: times {1} db 0", FieldID, Size);
        }
    }
}
