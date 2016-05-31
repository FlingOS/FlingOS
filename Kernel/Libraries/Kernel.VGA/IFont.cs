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
    ///     Interface for a font for text-mode VGA configurations.
    /// </summary>
    /// <remarks>
    ///     A font implementation can be hard-coded by the developer
    ///     or loaded at runtime. However, it must remain constant once loaded.
    /// </remarks>
    public interface IFont : IObject
    {
        /// <summary>
        ///     The data for the font. Must contain exactly 256 characters.
        /// </summary>
        byte[] FontData { get; }
        /// <summary>
        ///     The height of the font, in pixels.
        /// </summary>
        int FontHeight { get; }
    }
}
