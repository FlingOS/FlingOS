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

using Drivers.Compiler.Attributes;

namespace Drivers.Framework
{
    /// <summary>
    ///     Provides constants and static methods for common mathematical functions and some operations not supported by
    ///     IL code.
    /// </summary>
    public static class Math
    {
        /// <summary>
        ///     Divides a UInt64 by a UInt32.
        /// </summary>
        /// <param name="dividend">The UInt64 to be divided.</param>
        /// <param name="divisor">The UInt32 to divide by.</param>
        /// <returns>The quotient of the division.</returns>
        [PluggedMethod(ASMFilePath = @"ASM\Math\Divide")]
        public static ulong Divide(ulong dividend, uint divisor)
        {
            return 0;
        }

        /// <summary>
        ///     Returns the lower of the two inputs.
        /// </summary>
        /// <param name="x">Input 1.</param>
        /// <param name="y">Input 2.</param>
        /// <returns>The lower of the two inputs.</returns>
        public static ushort Min(ushort x, ushort y)
        {
            return x < y ? x : y;
        }

        /// <summary>
        ///     Returns the lower of the two inputs.
        /// </summary>
        /// <param name="x">Input 1.</param>
        /// <param name="y">Input 2.</param>
        /// <returns>The lower of the two inputs.</returns>
        public static uint Min(uint x, uint y)
        {
            return x < y ? x : y;
        }

        /// <summary>
        ///     Returns the lower of the two inputs.
        /// </summary>
        /// <param name="x">Input 1.</param>
        /// <param name="y">Input 2.</param>
        /// <returns>The lower of the two inputs.</returns>
        public static int Min(int x, int y)
        {
            return x < y ? x : y;
        }

        /// <summary>
        ///     Returns the higher of the two inputs.
        /// </summary>
        /// <param name="x">Input 1.</param>
        /// <param name="y">Input 2.</param>
        /// <returns>The higher of the two inputs.</returns>
        public static int Max(int x, int y)
        {
            return x > y ? x : y;
        }
    }
}