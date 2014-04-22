using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Cgt"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Cgt_Un"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Cgt"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Cgt_Un"/>
    [ILOpTarget(Target = ILOp.OpCodes.Cgt)]
    [ILOpTarget(Target = ILOp.OpCodes.Cgt_Un)]
    public abstract class Cgt : ILOp
    {
    }
}
