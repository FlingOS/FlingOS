using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_0"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_3"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_S"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_0"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_3"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_0)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_3)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_S)]
    public abstract class Ldloc : ILOp
    {
    }
}
