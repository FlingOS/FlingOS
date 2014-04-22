using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Ceq"/> 
    /// IL op.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ceq"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ceq)]
    public abstract class Ceq : ILOp
    {
    }
}
