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
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarga" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarga_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarg" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarg_0" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarg_1" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarg_2" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarg_3" /> and
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldarg_S" /> IL ops.
    /// </summary>
    /// <remarks>
    ///     See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_0" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_1" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_2" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_3" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarg_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarga" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldarga_S" />
    [ILOpTarget(Target = OpCodes.Ldarg)]
    [ILOpTarget(Target = OpCodes.Ldarg_0)]
    [ILOpTarget(Target = OpCodes.Ldarg_1)]
    [ILOpTarget(Target = OpCodes.Ldarg_2)]
    [ILOpTarget(Target = OpCodes.Ldarg_3)]
    [ILOpTarget(Target = OpCodes.Ldarg_S)]
    [ILOpTarget(Target = OpCodes.Ldarga)]
    [ILOpTarget(Target = OpCodes.Ldarga_S)]
    public abstract class Ldarg : ILOp
    {
    }
}