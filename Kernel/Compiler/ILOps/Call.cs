using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Call"/> and 
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Callvirt"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of this op.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Call"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Callvirt"/>
    [ILOpTarget(Target = ILOp.OpCodes.Call)]
    [ILOpTarget(Target = ILOp.OpCodes.Callvirt)]
    public abstract class Call : ILOp
    {
    }
}
