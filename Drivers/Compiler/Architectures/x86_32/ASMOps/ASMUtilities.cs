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

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public static class ASMUtilities
    {
        public static string GetOpSizeStr(OperandSize Size)
        {
            return Enum.GetName(typeof(OperandSize), Size).ToLower();
        }


        /// <summary>
        ///     Gets the allocation string for the specified number of bytes.
        /// </summary>
        /// <param name="numBytes">The number of bytes being allocated.</param>
        /// <returns>The allocation string.</returns>
        public static string GetAllocStringForSize(int numBytes)
        {
            switch (numBytes)
            {
                case 1:
                    return "db";
                case 2:
                    return "dw";
                case 4:
                    return "dd";
                default:
                    return "NOSIZEALLOC";
            }
        }
    }
}