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
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_I" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_I1" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_I2" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_I4" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_I8" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_R4" />,
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_R8" /> and
    ///     <see cref="System.Reflection.Emit.OpCodes.Stind_Ref" /> IL ops.
    /// </summary>
    /// <remarks>
    ///     See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_I" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_I1" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_I2" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_I4" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_I8" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_R4" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_R8" />
    /// <seealso cref="System.Reflection.Emit.OpCodes.Stind_Ref" />
    [ILOpTarget(Target = OpCodes.Stind_I)]
    [ILOpTarget(Target = OpCodes.Stind_I1)]
    [ILOpTarget(Target = OpCodes.Stind_I2)]
    [ILOpTarget(Target = OpCodes.Stind_I4)]
    [ILOpTarget(Target = OpCodes.Stind_I8)]
    [ILOpTarget(Target = OpCodes.Stind_R4)]
    [ILOpTarget(Target = OpCodes.Stind_R8)]
    [ILOpTarget(Target = OpCodes.Stind_Ref)]
    public abstract class Stind : ILOp
    {
    }
}