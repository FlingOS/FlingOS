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
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_0" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_1" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_2" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_3" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_4" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_5" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_6" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_7" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_8" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_M1" /> and
    ///     <see cref="System.Reflection.Emit.OpCodes.Ldc_I4_S" /> IL ops.
    /// </summary>
    /// <remarks>
    ///     See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_0" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_1" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_2" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_3" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_4" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_5" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_6" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_7" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_8" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_M1" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ldc_I4_S" />
    [ILOpTarget(Target = OpCodes.Ldc_I4)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_0)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_1)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_2)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_3)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_4)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_5)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_6)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_7)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_8)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_M1)]
    [ILOpTarget(Target = OpCodes.Ldc_I4_S)]
    [ILOpTarget(Target = OpCodes.Ldc_I8)]
    [ILOpTarget(Target = OpCodes.Ldc_R4)]
    [ILOpTarget(Target = OpCodes.Ldc_R8)]
    public abstract class Ldc : ILOp
    {
    }
}