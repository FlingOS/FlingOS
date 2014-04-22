using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldsfld"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Ldsflda"/>
    /// IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldsfld"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldsflda"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldsfld)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldsflda)]
    public abstract class Ldsfld : ILOp
    {
    }
}
