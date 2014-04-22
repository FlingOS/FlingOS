using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the
    /// <see cref="System.Reflection.Emit.OpCodes.Starg"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Starg_S"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Starg"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Starg_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Starg)]
    [ILOpTarget(Target = ILOp.OpCodes.Starg_S)]
    public abstract class Starg : ILOp
    {
    }
}
