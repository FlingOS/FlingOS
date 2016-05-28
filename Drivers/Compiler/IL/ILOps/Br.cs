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
    ///     <see cref="System.Reflection.Emit.OpCodes.Br" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Br_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Brtrue" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Brtrue_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Brfalse" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Brfalse_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Beq" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Beq_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bne_Un" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bne_Un_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bge" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bge_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bge_Un" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bge_Un_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bgt" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bgt_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bgt_Un" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Bgt_Un_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ble" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ble_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ble_Un" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Ble_Un_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Blt" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Blt_S" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Blt_Un" /> and
    ///     <see cref="System.Reflection.Emit.OpCodes.Blt_Un_S" />
    ///     IL ops.
    /// </summary>
    /// <remarks>
    ///     See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Br" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Br_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brtrue" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brtrue_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brfalse" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Brfalse_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Beq" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Beq_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bne_Un" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bne_Un_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge_Un" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bge_Un_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bgt" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bgt_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bgt_Un" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Bgt_Un_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble_Un" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Ble_Un_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt_S" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt_Un" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Blt_Un_S" />
    [ILOpTarget(Target = OpCodes.Br)]
    [ILOpTarget(Target = OpCodes.Br_S)]
    [ILOpTarget(Target = OpCodes.Brtrue)]
    [ILOpTarget(Target = OpCodes.Brtrue_S)]
    [ILOpTarget(Target = OpCodes.Brfalse)]
    [ILOpTarget(Target = OpCodes.Brfalse_S)]
    [ILOpTarget(Target = OpCodes.Beq)]
    [ILOpTarget(Target = OpCodes.Beq_S)]
    [ILOpTarget(Target = OpCodes.Bne_Un)]
    [ILOpTarget(Target = OpCodes.Bne_Un_S)]
    [ILOpTarget(Target = OpCodes.Bge)]
    [ILOpTarget(Target = OpCodes.Bge_S)]
    [ILOpTarget(Target = OpCodes.Bge_Un)]
    [ILOpTarget(Target = OpCodes.Bge_Un_S)]
    [ILOpTarget(Target = OpCodes.Bgt)]
    [ILOpTarget(Target = OpCodes.Bgt_S)]
    [ILOpTarget(Target = OpCodes.Bgt_Un)]
    [ILOpTarget(Target = OpCodes.Bgt_Un_S)]
    [ILOpTarget(Target = OpCodes.Ble)]
    [ILOpTarget(Target = OpCodes.Ble_S)]
    [ILOpTarget(Target = OpCodes.Ble_Un)]
    [ILOpTarget(Target = OpCodes.Ble_Un_S)]
    [ILOpTarget(Target = OpCodes.Blt)]
    [ILOpTarget(Target = OpCodes.Blt_S)]
    [ILOpTarget(Target = OpCodes.Blt_Un)]
    [ILOpTarget(Target = OpCodes.Blt_Un_S)]
    public abstract class Br : ILOp
    {
    }
}