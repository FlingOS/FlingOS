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
    ///     Immutable class representing an 4-bit colour (/character attribute) 
    ///     consisting of a single 4-bit component.
    /// </summary>
    /// <remarks>
    ///     Used by external software when getting/setting characters.
    /// </remarks>
    public class Colour4Bit : Object
    {
        /// <summary>
        ///     The 4-bit component of the colour.
        /// </summary>
        public readonly byte Value;

        /// <summary>
        ///     Initialises a new 4-bit colour (/character attribute) instance with the 
        ///     specified value.
        /// </summary>
        /// <param name="Value">The 4-bit component value (auto-clipped).</param>
        public Colour4Bit(byte Value)
        {
            this.Value = (byte)(Value & 0xF);
        }
    }
}
