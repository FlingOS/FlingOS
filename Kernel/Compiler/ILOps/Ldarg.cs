using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarg"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarg_0"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarg_1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarg_2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarg_3"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarg_S"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_0"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_3"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_0)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_3)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_S)]
    public abstract class Ldarg : ILOp
    {
    }
}
