using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Custom IL op that switches the top two stack items with eacother.
    /// </summary>
    /// <remarks>
    /// This must at least have an empty stub implementation or the compiler
    /// will fail to execute. It was added so the ILScanner could optimise 
    /// some code injections that it has to make.
    /// </remarks>
    public abstract class StackSwitch : ILOp
    {
    }
}
