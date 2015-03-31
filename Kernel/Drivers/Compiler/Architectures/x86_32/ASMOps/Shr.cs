using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Shr : ASM.ASMOp
    {
        public string Src;
        public string Dest;
        /// <summary>
        /// Optional. Cannot be used with Signed.
        /// Set to use Extended Shift (see http://en.wikibooks.org/wiki/X86_Assembly/Shift_and_Rotate#Extended_Shift_Instructions)
        /// </summary>
        public string Count;
        /// <summary>
        /// Optional. Cannot be used with Count.
        /// </summary>
        public bool Signed = false;
        
        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (!string.IsNullOrWhiteSpace(Count))
            {
                return "shrd " + Dest + ", " + Src + ", " + Count;
            }
            else if (Signed)
            {
                return "sar " + Dest + ", " + Src;
            }
            else
            {
                return "shr " + Dest + ", " + Src;
            }
        }
    }
}
