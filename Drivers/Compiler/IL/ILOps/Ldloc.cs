#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

namespace Drivers.Compiler.IL.ILOps
{
    /// <summary>
    ///     Handles the
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloc" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloc_0" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloc_1" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloc_2" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloc_3" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloc_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloca" /> and
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldloca_S" /> IL ops.
    /// </summary>
    /// <remarks>
    ///     See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_0" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_1" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_2" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_3" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloc_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloca" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldloca_S" />
    [ILOpTarget(Target = OpCodes.Ldloc)]
    [ILOpTarget(Target = OpCodes.Ldloc_0)]
    [ILOpTarget(Target = OpCodes.Ldloc_1)]
    [ILOpTarget(Target = OpCodes.Ldloc_2)]
    [ILOpTarget(Target = OpCodes.Ldloc_3)]
    [ILOpTarget(Target = OpCodes.Ldloc_S)]
    [ILOpTarget(Target = OpCodes.Ldloca)]
    [ILOpTarget(Target = OpCodes.Ldloca_S)]
    public abstract class Ldloc : ILOp
    {
    }
}