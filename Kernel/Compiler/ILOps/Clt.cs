using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Clt"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Clt_Un"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Clt"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Clt_Un"/>
    [ILOpTarget(Target = ILOp.OpCodes.Clt)]
    [ILOpTarget(Target = ILOp.OpCodes.Clt_Un)]
    public abstract class Clt : ILOp
    {
    }
}
