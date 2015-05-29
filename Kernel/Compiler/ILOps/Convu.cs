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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.ILOps
{
    /// <summary>
    /// Handles the 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U1"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U2"/>, 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U4"/> and 
    /// <see cref="System.Reflection.Emit.OpCodes.Conv_U8"/> IL ops.
    /// </summary>
    /// <remarks>
    /// See MSDN for details of these ops.
    /// </remarks>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U1"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U2"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U4"/>
    /// <seealso cref="System.Reflection.Emit.OpCodes.Conv_U8"/>
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U1)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U2)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U4)]
    [ILOpTarget(Target = ILOp.OpCodes.Conv_U8)]
    public abstract class Convu : ILOp
    {
    }
}
