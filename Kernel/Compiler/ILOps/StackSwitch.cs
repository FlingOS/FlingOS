using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Custom IL op that rotates (upwards) the specified number of top-most stack items with eachother. 
    /// Does not alter compiler record of stack - caller must do that. 
    /// Acts totally ignorant of item types (e.g. int or float). Only moves dwords. Number of dwords to move should be in value 
    /// bytes. Null value bytes indicates rotate (switch) top two items.
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
