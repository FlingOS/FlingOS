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
    ///     Custom IL op that rotates (upwards) the specified number of top-most stack items with eachother.
    ///     Does not alter compiler record of stack - caller must do that.
    ///     Acts totally ignorant of item types (e.g. int or float). Only moves dwords. Number of dwords to move should be in
    ///     value
    ///     bytes. Null value bytes indicates rotate (switch) top two items.
    /// </summary>
    /// <remarks>
    ///     This must at least have an empty stub implementation or the compiler
    ///     will fail to execute. It was added so the ILScanner could optimise
    ///     some code injections that it has to make.
    /// </remarks>
    public class StackSwitch : ILOp
    {
    }
}