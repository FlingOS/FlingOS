using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_I"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_I1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_I2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_I4"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_I8"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_I"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_I1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_I2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_I4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_I8"/>
    [ILOpTarget(Target = ILOp.OpCodes.Conv_I)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_I1)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_I2)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_I4)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_I8)]
    public abstract class Convi : ILOp
    {
    }
}
