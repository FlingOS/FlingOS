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
using Kernel.Framework.Exceptions;

namespace Kernel.VGA.Configurations.Graphical
{
    public sealed class T_80x50 : Object, IVGAConfiguration
    {
        private static T_80x50 _Instance;
        public static T_80x50 Instance => _Instance ?? (_Instance = new T_80x50());

        private T_80x50()
        {
        }

        public uint Width => 80;
        public uint Height => 50;
        public uint BitDepth => 4;
        public ScreenMode Mode => ScreenMode.Text;
        public SetCellDelegate SetCellMethod => SetCell;
        public GetCellDelegate GetCellMethod => GetCell;
        public SetPixelDelegate SetPixelMethod => SetPixel;
        public GetPixelDelegate GetPixelMethod => GetPixel;
        public SetPixelDelegate ClearMethod => Clear;

        private static unsafe void SetCell(VGA TheVGA, int X, int Y, char Character, Colour4Bit ForeColour, Colour4Bit BackColour)
        {
            int Offset = X + Y * 80;
            char* FrameBuffer = (char*)TheVGA.FrameBuffer;
            FrameBuffer[Offset] = (char)(((BackColour.Value << 4 | ForeColour.Value) << 8) | (Character & 0x00FF));
        }

        private static char GetCell(VGA TheVGA, int X, int Y, out Colour4Bit ForeColour, out Colour4Bit BackColour)
        {
            ExceptionMethods.Throw(new NotImplementedException("Get cell in 80x50 text VGA mode not implemented yet."));
            ForeColour = null;
            BackColour = null;
            return '\0';
        }

        private static unsafe void SetPixel(VGA TheVGA, int X, int Y, Colour24Bit Colour)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot set pixel in 80x50 text VGA mode."));
        }

        private static unsafe void Clear(VGA TheVGA, int X, int Y, Colour24Bit Colour)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot clear pixels in 80x50 text VGA mode."));
        }

        private static unsafe Colour24Bit GetPixel(VGA TheVGA, int X, int Y)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot get pixel in 80x50 text VGA mode."));
            return null;
        }

        public byte SetReset => 0x00;
        public byte EnableSetReset => 0x00;
        public byte ColourCompare => 0x00;
        public byte DataRotate => 0x00;
        public byte ReadMapSelect => 0x00;
        public GraphicsModeFlags GraphicsMode => GraphicsModeFlags.HostOddEvenMemoryReadAddressingEnable;
        public byte MiscellaneousGraphics => 0x0E;
        public byte ColourDontCare => 0x00;
        public byte BitMask => 0xFF;
        public byte Reset => 0x03;
        public byte ClockingMode => 0x00;
        public byte MapMask => 0x03;
        public byte CharacterMapSelect => 0x00;
        public SequencerMemoryModeFlags SequencerMemoryMode => (SequencerMemoryModeFlags)0x02;
        // Gray coding applied so that a smooth progression up the colours
        //  can be made
        public byte Palette0 => 0x00;
        public byte Palette1 => 0x01;
        public byte Palette2 => 0x02;
        public byte Palette3 => 0x03;
        public byte Palette4 => 0x04;
        public byte Palette5 => 0x05;
        public byte Palette6 => 0x06;
        public byte Palette7 => 0x07;
        public byte Palette8 => 0x08;
        public byte Palette9 => 0x09;
        public byte Palette10 => 0x0A;
        public byte Palette11 => 0x0B;
        public byte Palette12 => 0x0C;
        public byte Palette13 => 0x0D;
        public byte Palette14 => 0x0E;
        public byte Palette15 => 0x0F;
        public byte AttributeModeControl => 0x0C;
        public byte OverscanColour => 0x00;
        public byte ColourPlaneEnable => 0x0F;
        public byte HorizontalPixelPanning => 0x08;
        public byte ColourSelect => 0x00;
        public byte HorizontalTotal => 0x5F;
        public byte EndHorizontalDisplay => 0x4F;
        public byte StartHorizontalBlanking => 0x50;
        public byte EndHorizontalBlanking => 0x82;
        public byte StartHorizontalRetrace => 0x55;
        public byte EndHorizontalRetrace => 0x81;
        public byte VerticalTotal => 0xBF;
        public byte Overflow => 0x1F;
        public byte PresetRowScan => 0x00;
        public byte MaximumScanLine => 0x47;
        public byte CursorStart => 0x06;
        public byte CursorEnd => 0x07;
        public byte StartAddressHigh => 0x00;
        public byte StartAddressLow => 0x00;
        public byte CursorLocationHigh => 0x01;
        public byte CursorLocationLow => 0x40;
        public byte VerticalRetraceStart => 0x9C;
        public byte VerticalRetraceEnd => 0x8E;
        public byte VerticalDisplayEnd => 0x8F;
        public byte Offset => 0x28;
        public byte UnderlineLocation => 0x1F;
        public byte StartVerticalBlanking => 0x96;
        public byte EndVerticalBlanking => 0xB9;
        public byte CRTCModeControl => 0xA3;
        public byte LineCompare => 0xFF;
        public byte MiscellaneousOutput => 0x67;
    }
}
