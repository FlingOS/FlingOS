using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Shl : ASM.ASMOp
    {
        public string Src;
        public string Dest;
        /// <summary>
        /// Optional. Set to use Extended Shift (see http://en.wikibooks.org/wiki/X86_Assembly/Shift_and_Rotate#Extended_Shift_Instructions)
        /// </summary>
        public string Count;
        
        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (!string.IsNullOrWhiteSpace(Count))
            {
                return "shld " + Dest + ", " + Src + ", " + Count;
            }
            else
            {
                return "shl " + Dest + ", " + Src;
            }
        }
    }
}
