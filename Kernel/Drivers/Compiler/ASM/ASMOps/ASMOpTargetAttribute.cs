using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    /// <summary>
    /// Indicates to the compiler which ASM op an ASMOp implementation targets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ASMOpTargetAttribute : Attribute
    {
        /// <summary>
        /// The ASM op code to target.
        /// </summary>
        public OpCodes Target;
    }
}
