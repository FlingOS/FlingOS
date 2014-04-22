using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Custom IL op that is inserted as the last IL op before the 
    /// <see cref="Ret"/> op in a method.
    /// </summary>
    /// <remarks>
    /// This must at least have an empty stub implementation or the compiler
    /// will fail to execute. It was added so x86_32 architecture could
    /// do some stack management at the end of the method (e.g. restoring 
    /// the base pointer).
    /// </remarks>
    public abstract class MethodEnd : ILOp
    {
    }
}
