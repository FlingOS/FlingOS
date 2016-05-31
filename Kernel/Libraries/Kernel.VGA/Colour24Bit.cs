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
    ///     Class representing an 24-bit colour consisting of
    ///     three 8-bit components: red, green and blue.
    /// </summary>
    /// <remarks>
    ///     Used by external software when getting/setting pixels.
    /// </remarks>
    public class Colour24Bit : Object
    {
        /// <summary>
        ///     The 8-bit red component of the colour.
        /// </summary>
        public byte Red;
        /// <summary>
        ///     The 8-bit green component of the colour.
        /// </summary>
        public byte Green;
        /// <summary>
        ///     The 8-bit blue component of the colour.
        /// </summary>
        public byte Blue;

        /// <summary>
        ///     Initialises a new 24-bit RGB colour instance with the specified
        ///     values.
        /// </summary>
        /// <param name="Red">The 8-bit red component value.</param>
        /// <param name="Green">The 8-bit green component value.</param>
        /// <param name="Blue">The 8-bit blue component value.</param>
        public Colour24Bit(byte Red, byte Green, byte Blue)
        {
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
        }
    }
}
