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
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_0"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_3"/>,  
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloc_S"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloca"/> and
    /// <see cref="System.Reflection.Emit.OpCodes.Ldloca_S"/> IL ops.
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
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloca"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloca_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_0)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_3)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloc_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloca)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldloca_S)]
    public abstract class Ldloc : ILOp
    {
    }
}
