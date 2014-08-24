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
