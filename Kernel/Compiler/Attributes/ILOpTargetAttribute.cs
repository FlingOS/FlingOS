using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Kernel.Compiler
{
    /// <summary>
    /// Indicates to the compiler which IL op an ILOp implementation targets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public class ILOpTargetAttribute : Attribute
    {
        /// <summary>
        /// The IL op code to target.
        /// </summary>
        public ILOps.ILOp.OpCodes Target;
    }
}
