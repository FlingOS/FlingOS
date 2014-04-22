using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Br"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Br_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Brtrue"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Brtrue_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Brfalse"/>,  
    /// <see cref="System.Reflection.Emit.OpCodes.Brfalse_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Beq"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Beq_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Bne_Un"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Bne_Un_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Bge"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Bge_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Bge_Un"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Bge_Un_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ble"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ble_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ble_Un"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ble_Un_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Blt"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Blt_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Blt_Un"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Blt_Un_S"/> 
    /// IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Br"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Br_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brtrue"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brtrue_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brfalse"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brfalse_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Beq"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Beq_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bne_Un"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bne_Un_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge_Un"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge_Un_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble_Un"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble_Un_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt_S"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt_Un"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt_Un_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Br)]
    [ILOpTarget(Target = ILOp.OpCodes.Br_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Brtrue)]
    [ILOpTarget(Target = ILOp.OpCodes.Brtrue_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Brfalse)]
    [ILOpTarget(Target = ILOp.OpCodes.Brfalse_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Beq)]
    [ILOpTarget(Target = ILOp.OpCodes.Beq_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Bne_Un)]
    [ILOpTarget(Target = ILOp.OpCodes.Bne_Un_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Bge)]
    [ILOpTarget(Target = ILOp.OpCodes.Bge_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Bge_Un)]
    [ILOpTarget(Target = ILOp.OpCodes.Bge_Un_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Ble)]
    [ILOpTarget(Target = ILOp.OpCodes.Ble_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Ble_Un)]
    [ILOpTarget(Target = ILOp.OpCodes.Ble_Un_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Blt)]
    [ILOpTarget(Target = ILOp.OpCodes.Blt_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Blt_Un)]
    [ILOpTarget(Target = ILOp.OpCodes.Blt_Un_S)]
    public abstract class Br : ILOp
    {
    }
}
