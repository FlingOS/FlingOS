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

using Kernel.Framework;

namespace Kernel.VGA
{
    /// <summary>
    ///     Immutable class representing an 18-bit colour consisting of
    ///     three 6-bit components: red, green and blue.
    /// </summary>
    /// <remarks>
    ///     Used when accessing the DAC data.
    /// </remarks>
    public class Colour18Bit : Object
    {
        /// <summary>
        ///     The 6-bit red component of the colour.
        /// </summary>
        public readonly byte Red;
        /// <summary>
        ///     The 6-bit green component of the colour.
        /// </summary>
        public readonly byte Green;
        /// <summary>
        ///     The 6-bit blue component of the colour.
        /// </summary>
        public readonly byte Blue;

        /// <summary>
        ///     Initialises a new 18-bit RGB colour instance with the specified
        ///     values. Values are clipped to low 6-bits.
        /// </summary>
        /// <param name="Red">The 6-bit red component value (auto-clipped).</param>
        /// <param name="Green">The 6-bit green component value (auto-clipped).</param>
        /// <param name="Blue">The 6-bit blue component value (auto-clipped).</param>
        public Colour18Bit(byte Red, byte Green, byte Blue)
        {
            this.Red = (byte)(Red & 0x3F);
            this.Green = (byte)(Green & 0x3F);
            this.Blue = (byte)(Blue & 0x3F);
        }
    }
}
