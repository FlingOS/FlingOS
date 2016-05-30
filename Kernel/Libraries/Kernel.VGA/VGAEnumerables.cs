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

using FlagsAttribute = System.FlagsAttribute;

namespace Kernel.VGA
{
    /// <summary>
    ///     The identifier values for the different frame buffers.
    /// </summary>
    /// <remarks>
    ///     Reading bits 2 and 3 of Miscellaneous Graphics will tell you
    ///     which frame buffer is currently in use.
    /// </remarks>
    /// <example>
    ///     (FrameBufferSegments)((MyVGARegisters.MiscellaneousGraphics >> 2) &amp; 3)
    /// </example>
    internal enum FrameBufferSegments
    {
        /// <summary>
        ///     VGA buffer
        /// </summary>
        VGA = 1,
        /// <summary>
        ///     Monchrome text buffer
        /// </summary>
        Monochrome = 2,
        /// <summary>
        ///     CGA text buffer
        /// </summary>
        CGA = 3
    }

    /// <summary>
    ///     VGA screen modes : Either text mode or graphical (pixel-based) mode
    /// </summary>
    public enum ScreenMode
    {
        /// <summary>
        ///     Text mode (Monochrome or CGA - see also <see cref="FrameBufferSegments"/>)
        /// </summary>
        Text,
        /// <summary>
        ///     Graphical mode (VGA - see also <see cref="FrameBufferSegments"/>)
        /// </summary>
        Graphical
    }

    /// <summary>
    ///     Flag bits within the Sequencer Memory Mode register
    /// </summary>
    [Flags]
    public enum SequencerMemoryModeFlags : byte
    {
        /// <summary>
        ///     Odd/Even Host Memory Write Adressing Disable
        /// </summary>
        OddEvenDisable = 0x4,
        /// <summary>
        ///     Low 2 bits of memory access determine which VGA map is selected. Odd/Even selector bit is ignored when this is 1.
        /// </summary>
        Chain4 = 0x8
    }

    /// <summary>
    ///     Flags bits within the Graphics Mode register
    /// </summary>
    [Flags]
    public enum GraphicsModeFlags : byte
    {
        /// <summary>
        ///     Low bit of the Write Mode
        /// </summary>
        WriteModeL = 0x1,
        /// <summary>
        ///     High bit of the Write Mode
        /// </summary>
        WriteModeH = 0x2,
        /// <summary>
        ///     Read mode
        /// </summary>
        ReadMode = 0x8,
        /// <summary>
        ///     Host Odd/Even Memory Read Addressing Enable - normally follows <see cref="SequencerMemoryModeFlags.OddEvenDisable"/>
        /// </summary>
        HostOddEvenMemoryReadAddressingEnable = 0x10,
        /// <summary>
        ///     Interleaved shift mode: 2 bits popped for planes in order Plane 0, Plane 2, Plane 1, Plane 3
        /// </summary>
        InterleavedShift = 0x20,
        /// <summary>
        ///     256-Colour Shift : Used for linear 16-bit colour mode. 4 bits popped each time (usually merged to give 8 bit pixels). 2 pixels for Plane 0 then 2 for Plane 1 and so on.
        /// </summary>
        ColorShift256 = 0x40
    }
}
