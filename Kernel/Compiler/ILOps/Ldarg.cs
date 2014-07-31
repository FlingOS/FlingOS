#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
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
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarga"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Ldarga_S"/>, 
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
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarga"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarga_S"/>
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_0)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_1)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_2)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_3)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarg_S)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarga)]
    [ILOpTarget(Target = ILOp.OpCodes.Ldarga_S)]
    public abstract class Ldarg : ILOp
    {
    }
}
