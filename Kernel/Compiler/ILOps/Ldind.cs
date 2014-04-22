using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldind_U1"/>,  
    /// <see cref="System.Reflection.Emit.OpCodes.Ldind_U2"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldind_U4"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldind_U1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldind_U2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldind_U4"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldind_U1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldind_U2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldind_U4)]
    public abstract class Ldind : ILOp
    {
    }
}
