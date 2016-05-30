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

namespace Kernel.VGA
{
    /// <summary>
    ///     Pointers to and sizes of the various VGA memory regions.
    /// </summary>
    internal static unsafe class VGAMemory
    {
        /// <summary>
        ///     0xA0000
        /// </summary>
        public static readonly byte* VGABlock = (byte*)0xA0000;

        /// <summary>
        ///     64KiB
        /// </summary>
        public const uint VGABlockSize = 1024*64;

        /// <summary>
        ///     0xB0000
        /// </summary>
        public static readonly byte* MonochromeTextBlock = (byte*)0xB0000;

        /// <summary>
        ///     32KiB
        /// </summary>
        public const uint MonochromeTextBlockSize = 1024*32;
        
        /// <summary>
        ///     0xB8000
        /// </summary>
        public static readonly byte* CGATextBlock = (byte*)0xB8000;

        /// <summary>
        ///     32KiB
        /// </summary>
        public const uint CGATextBlockSize = 1024 * 32;

        
    }
}
