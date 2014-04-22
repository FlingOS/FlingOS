using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the <see cref="System.Reflection.Emit.OpCodes.Div"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Div_Un"/> 
    /// IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Div"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Div_Un"/>
    [ILOpTarget(Target = ILOp.OpCodes.Div)]
    [ILOpTarget(Target = ILOp.OpCodes.Div_Un)]
    public abstract class Div : ILOp
    {
    }
}
