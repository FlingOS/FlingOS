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
    ///     Constant values used by the VGA driver
    /// </summary>
    internal static class VGAConstants
    {
        #region Registers

        #region Graphics registers

        /// <summary>
        ///     Index of the Set/Reset register
        /// </summary>
        public const byte SetResetRegisterIndex = 0x00;

        /// <summary>
        ///     Index of the Enable Set/Reset register
        /// </summary>
        public const byte EnableSetResetRegisterIndex = 0x01;

        /// <summary>
        ///     Index of the Colour Compare register
        /// </summary>
        public const byte ColourCompareRegisterIndex = 0x02;

        /// <summary>
        ///     Index of the Data Rotate register
        /// </summary>
        public const byte DataRotateRegisterIndex = 0x03;

        /// <summary>
        ///     Index of the Read Map Select register
        /// </summary>
        public const byte ReadMapSelectRegisterIndex = 0x04;

        /// <summary>
        ///     Index of the Graphics Mode register
        /// </summary>
        public const byte GraphicsModeRegisterIndex = 0x05;

        /// <summary>
        ///     Index of the Miscellaneous Graphics register
        /// </summary>
        public const byte MiscellaneousGraphicsRegisterIndex = 0x06;

        /// <summary>
        ///     Index of the Colour Don't Care register
        /// </summary>
        public const byte ColourDontCareRegisterIndex = 0x07;

        /// <summary>
        ///     Index of the Bit Mask register
        /// </summary>
        public const byte BitMaskRegisterIndex = 0x08;

        #endregion

        #region Sequencer registers
        
        /// <summary>
        ///     Index of the Reset register
        /// </summary>
        public const byte ResetRegisterIndex = 0x00;
        
        /// <summary>
        ///     Index of the Clocking Mode register
        /// </summary>
        public const byte ClockingModeRegisterIndex = 0x01;

        /// <summary>
        ///     Index of the Map Mask register
        /// </summary>
        public const byte MapMaskRegisterIndex = 0x02;

        /// <summary>
        ///     Index of the Character Map Select register
        /// </summary>
        public const byte CharacterMapSelectRegisterIndex = 0x03;

        /// <summary>
        ///     Index of the Sequencer Memory Mode register
        /// </summary>
        public const byte SequencerMemoryModeRegisterIndex = 0x04;

        #endregion

        #region Attribute registers

        /// <summary>
        ///     Index of palette register 0
        /// </summary>
        public const byte Palette0RegisterIndex = 0x0;

        /// <summary>
        ///     Index of palette register 1
        /// </summary>
        public const byte Palette1RegisterIndex = 0x1;

        /// <summary>
        ///     Index of palette register 2
        /// </summary>
        public const byte Palette2RegisterIndex = 0x2;

        /// <summary>
        ///     Index of palette register 3
        /// </summary>
        public const byte Palette3RegisterIndex = 0x3;

        /// <summary>
        ///     Index of palette register 4
        /// </summary>
        public const byte Palette4RegisterIndex = 0x4;

        /// <summary>
        ///     Index of palette register 5
        /// </summary>
        public const byte Palette5RegisterIndex = 0x5;

        /// <summary>
        ///     Index of palette register 6
        /// </summary>
        public const byte Palette6RegisterIndex = 0x6;

        /// <summary>
        ///     Index of palette register 7
        /// </summary>
        public const byte Palette7RegisterIndex = 0x7;

        /// <summary>
        ///     Index of palette register 8
        /// </summary>
        public const byte Palette8RegisterIndex = 0x8;

        /// <summary>
        ///     Index of palette register 9
        /// </summary>
        public const byte Palette9RegisterIndex = 0x9;

        /// <summary>
        ///     Index of palette register 10
        /// </summary>
        public const byte Palette10RegisterIndex = 0xA;

        /// <summary>
        ///     Index of palette register 11
        /// </summary>
        public const byte Palette11RegisterIndex = 0xB;

        /// <summary>
        ///     Index of palette register 12
        /// </summary>
        public const byte Palette12RegisterIndex = 0xC;

        /// <summary>
        ///     Index of palette register 13
        /// </summary>
        public const byte Palette13RegisterIndex = 0xD;

        /// <summary>
        ///     Index of palette register 14
        /// </summary>
        public const byte Palette14RegisterIndex = 0xE;

        /// <summary>
        ///     Index of palette register 15
        /// </summary>
        public const byte Palette15RegisterIndex = 0xF;

        /// <summary>
        ///     Index of the Attribute Mode Control register
        /// </summary>
        public const byte AttributeModeControlRegisterIndex = 0x10;

        /// <summary>
        ///     Index of the Overscan Colour register
        /// </summary>
        public const byte OverscanColourRegisterIndex = 0x11;

        /// <summary>
        ///     Index of the Colour Plane Enable register
        /// </summary>
        public const byte ColourPlaneEnableRegisterIndex = 0x12;

        /// <summary>
        ///     Index of the Horizontal Pixel Panning register
        /// </summary>
        public const byte HorizontalPixelPanningRegisterIndex = 0x13;

        /// <summary>
        ///     Index of the Colour Select register
        /// </summary>
        public const byte ColourSelectRegisterIndex = 0x14;

        #endregion

        #region CRTC registers
        
        /// <summary>
        ///     Index of the Horizontal Total register
        /// </summary>
        public const byte HorizontalTotalRegisterIndex = 0x00;

        /// <summary>
        ///     Index of the End Horizontal Display register
        /// </summary>
        public const byte EndHorizontalDisplayRegisterIndex = 0x01;

        /// <summary>
        ///     Index of the Start Horizontal Blanking register
        /// </summary>
        public const byte StartHorizontalBlankingRegisterIndex = 0x02;

        /// <summary>
        ///     Index of the End Horizontal Blanking register
        /// </summary>
        public const byte EndHorizontalBlankingRegisterIndex = 0x03;

        /// <summary>
        ///     Index of the Start Horizontal Retrace register
        /// </summary>
        public const byte StartHorizontalRetraceRegisterIndex = 0x04;

        /// <summary>
        ///     Index of the End Horizontal Retrace register
        /// </summary>
        public const byte EndHorizontalRetraceRegisterIndex = 0x05;

        /// <summary>
        ///     Index of the Vertical Total register
        /// </summary>
        public const byte VerticalTotalRegisterIndex = 0x06;

        /// <summary>
        ///     Index of the Overflow register
        /// </summary>
        public const byte OverflowRegisterIndex = 0x07;

        /// <summary>
        ///     Index of the Preset Row Scan register
        /// </summary>
        public const byte PresetRowScanRegisterIndex = 0x08;

        /// <summary>
        ///     Index of the Maximum Scan Line register
        /// </summary>
        public const byte MaximumScanLineRegisterIndex = 0x09;

        /// <summary>
        ///     Index of the Cursor Start register
        /// </summary>
        public const byte CursorStartRegisterIndex = 0x0A;

        /// <summary>
        ///     Index of the Cursor End register
        /// </summary>
        public const byte CursorEndRegisterIndex = 0x0B;

        /// <summary>
        ///     Index of the Start Address High register
        /// </summary>
        public const byte StartAddressHighRegisterIndex = 0x0C;

        /// <summary>
        ///     Index of the Start Address Low register
        /// </summary>
        public const byte StartAddressLowRegisterIndex = 0x0D;

        /// <summary>
        ///     Index of the Cursor Location High register
        /// </summary>
        public const byte CursorLocationHighRegisterIndex = 0x0E;

        /// <summary>
        ///     Index of the Cursor Location Low register
        /// </summary>
        public const byte CursorLocationLowRegisterIndex = 0x0F;

        /// <summary>
        ///     Index of the Vertical Retrace Start register
        /// </summary>
        public const byte VerticalRetraceStartRegisterIndex = 0x10;

        /// <summary>
        ///     Index of the Vertical Retrace End register
        /// </summary>
        public const byte VerticalRetraceEndRegisterIndex = 0x11;

        /// <summary>
        ///     Index of the Vertical Display End register
        /// </summary>
        public const byte VerticalDisplayEndRegisterIndex = 0x12;

        /// <summary>
        ///     Index of the Offset register
        /// </summary>
        public const byte OffsetRegisterIndex = 0x13;

        /// <summary>
        ///     Index of the Underline Location register
        /// </summary>
        public const byte UnderlineLocationRegisterIndex = 0x14;

        /// <summary>
        ///     Index of the Start Vertical Blanking register
        /// </summary>
        public const byte StartVerticalBlankingRegisterIndex = 0x15;

        /// <summary>
        ///     Index of the End Vertical Blanking
        /// </summary>
        public const byte EndVerticalBlankingRegisterIndex = 0x16;

        /// <summary>
        ///     Index of the CRTC Mode Control register
        /// </summary>
        public const byte CRTCModeControlRegisterIndex = 0x17;

        /// <summary>
        ///     Index of the Line Compare register
        /// </summary>
        public const byte LineCompareRegisterIndex = 0x18;

        #endregion

        #endregion

        #region Masks

        /// <summary>
        ///     Mask for the Enable Set/Reset bits in the Enable Set/Reset register
        /// </summary>
        public const byte EnableSetResetMask = 0xF;

        /// <summary>
        ///     Mask for the Set/Reset Value bits in the Set/Reset register
        /// </summary>
        public const byte SetResetValueMask = 0xF;

        /// <summary>
        ///     Mask for the Rotate Count bits in the Data Rotate register
        /// </summary>
        public const byte RotateCountMask = 0x7;

        /// <summary>
        ///     Mask for the Logical Operation bits in the Data Rotate register
        /// </summary>
        public const byte LogicalOperationMask = 0x18;

        #endregion
    }
}
