using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Kernel.Compiler
{
    /// <summary>
    /// Indicates to the compiler that a method should not have debug ops
    /// emitted for it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=false, Inherited=false)]
    public class NoDebugAttribute : Attribute
    {
    }
}
