#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_I"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_I1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_I2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_I4"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_I8"/>,
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_R4"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_R8"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_U1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_U2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelem_U4"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldelema"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_I"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_I1"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_I2"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_I4"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_I8"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_R4"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_R8"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_U1"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_U2"/> 
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelem_U4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldelema"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_I)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_I1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_I2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_I4)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_I8)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_R4)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_R8)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_Ref)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_U1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_U2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelem_U4)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldelema)]
    public abstract class Ldelem : ILOp
    {
    }
}
