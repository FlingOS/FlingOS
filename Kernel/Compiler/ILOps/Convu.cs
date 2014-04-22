using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U4"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U8"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U8"/>
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U1)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U2)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U4)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U8)]
    public abstract class Convu : ILOp
    {
    }
}
