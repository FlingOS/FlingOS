using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Shr_Un"/> 
    /// IL op.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Shr_Un"/>
    [ILOpTarget(Target = ILOp.OpCodes.Shr_Un)]
    public abstract class Shr : ILOp
    {
    }
}
