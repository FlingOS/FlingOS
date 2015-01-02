#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
