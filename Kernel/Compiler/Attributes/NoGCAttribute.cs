using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// Indicates to the compiler that a method should not have GC
    /// applied to it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NoGCAttribute : Attribute
    {
    }
}
