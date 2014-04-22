using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Stloc"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Stloc_0"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Stloc_1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Stloc_2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Stloc_3"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Stloc_S"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stloc"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stloc_0"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stloc_1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stloc_2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stloc_3"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stloc_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Stloc)]
    [ILOpTarget(Target = ILOp.OpCodes.Stloc_0)]
    [ILOpTarget(Target = ILOp.OpCodes.Stloc_1)]
    [ILOpTarget(Target = ILOp.OpCodes.Stloc_2)]
    [ILOpTarget(Target = ILOp.OpCodes.Stloc_3)]
    [ILOpTarget(Target = ILOp.OpCodes.Stloc_S)]
    public abstract class Stloc : ILOp
    {
    }
}
