using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Rem"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Rem_Un"/> 
    /// IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Rem"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Rem_Un"/>
    [ILOpTarget(Target = ILOp.OpCodes.Rem)]
    [ILOpTarget(Target = ILOp.OpCodes.Rem_Un)]
    public abstract class Rem : ILOp
    {
    }
}
