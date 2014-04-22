using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// Indicates a class contains a plugged methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute must be applied to a class containing plugged methods
    /// otherwise the plugged methods are not detected.
    /// </para>
    /// <para>
    /// This is a compiler optimisation so we don't have to scan every method
    /// to see if it is marked as plugged.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluggedClassAttribute : Attribute
    {
    }
}
