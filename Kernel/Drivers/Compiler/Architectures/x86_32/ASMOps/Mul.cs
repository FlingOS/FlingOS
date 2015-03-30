using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Mul : ASM.ASMOp
    {
        /// <summary>
        /// Arg (which is Dest for signed mul with src/aux options).
        /// </summary>
        public string Arg;
        /// <summary>
        /// Optional (only available for signed mul)
        /// </summary>
        public string Src = null;
        /// <summary>
        /// Optional (only available for signed mul, requires Src)
        /// </summary>
        public string Aux = null;
        public bool Signed = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (Signed)
            {
                if (!string.IsNullOrWhiteSpace(Aux))
                {
                    return "imul " + Arg + ", " + Src + ", " + Aux;
                }
                else if (!string.IsNullOrWhiteSpace(Src))
                {
                    return "imul " + Arg + ", " + Src;
                }
                else
                {
                    return "imul " + Arg;
                }
            }
            else
            {
                return "mul " + Arg;
            }
        }
    }
}
