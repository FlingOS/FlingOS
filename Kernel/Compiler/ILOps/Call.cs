using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Call"/> IL op.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of this op.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Call"/>
    [ILOpTarget(Target=ILOp.OpCodes.Call)]
    public abstract class Call : ILOp
    {
    }
}
