using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// Indicates the class is the kernel's String class. 
    /// Note: There should only ever be one of these used!
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class StringClassAttribute : Attribute
    {
    }
}
