using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Ldstr"/> IL op.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of this op.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldstr"/>
    [ILOpTarget(Target=ILOp.OpCodes.Ldstr)]
    public abstract class Ldstr : ILOp
    {
    }
}
