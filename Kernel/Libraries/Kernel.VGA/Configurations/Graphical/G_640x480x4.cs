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
    public sealed class G_640x480x4 : Object, IVGAConfiguration
    {
        private static G_640x480x4 _Instance;
        public static G_640x480x4 Instance => _Instance ?? (_Instance = new G_640x480x4());

        private G_640x480x4()
        {
        }

        public uint Width => 640;
        public uint Height => 480;
        public uint BitDepth => 4;
        public ScreenMode Mode => ScreenMode.Graphical;
        public SetCellDelegate SetCellMethod => SetCell;
        public GetCellDelegate GetCellMethod => GetCell;
        public SetPixelDelegate SetPixelMethod => SetPixel;
        public GetPixelDelegate GetPixelMethod => GetPixel;
        public SetPixelDelegate ClearMethod => Clear;

        private static void SetCell(VGA TheVGA, int X, int Y, char Character, Colour4Bit ForeColour, Colour4Bit BackColour)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot set cell in 640x480x4 graphical VGA mode."));
        }

        private static char GetCell(VGA TheVGA, int X, int Y, out Colour4Bit ForeColour, out Colour4Bit BackColour)
        {
            ExceptionMethods.Throw(new NotSupportedException("Cannot get cell in 640x480x4 graphical VGA mode."));
            ForeColour = null;
            BackColour = null;
            return '\0';
        }

        private static unsafe void SetPixel(VGA TheVGA, int X, int Y, Colour24Bit Colour)
        {
            uint Offset = (uint)(X / 8 + (640 / 8) * Y);

            X = X & 7;

            uint Mask = (byte)(0x80 >> X);
            uint PlaneMask = 1;

            byte* FrameBuffer = TheVGA.FrameBuffer;

            // TODO: Proper colour to palette index translation
            uint ColourUI32 = Colour.Red;

            for (byte PlaneIndex = 0; PlaneIndex < 4; PlaneIndex++)
            {
                TheVGA.SelectPlane(PlaneIndex);

                if ((PlaneMask & ColourUI32) != 0)
                {
                    FrameBuffer[Offset] = (byte)(FrameBuffer[Offset] | Mask);
                }

                else
                {
                    FrameBuffer[Offset] = (byte)(FrameBuffer[Offset] & ~Mask);
                }

                PlaneMask <<= 1;
            }
        }

        private static unsafe void Clear(VGA TheVGA, int X, int Y, Colour24Bit Colour)
        {
            uint PlaneMask = 1;

            uint* FrameBuffer = (uint*)TheVGA.FrameBuffer;

            // TODO: Proper colour to palette index translation
            uint ColourUI32 = Colour.Red;

            const uint Pixels = (640*480)/32;

            for (byte PlaneIndex = 0; PlaneIndex < 4; PlaneIndex++)
            {
                TheVGA.SelectPlane(PlaneIndex);

                if ((PlaneMask & ColourUI32) != 0)
                {
                    for (uint Offset = 0; Offset < Pixels; Offset++)
                    {
                        FrameBuffer[Offset] = 0xFFFFFFFF;
                    }
                }
                else
                {
                    for (uint Offset = 0; Offset < Pixels; Offset++)
                    {
                        FrameBuffer[Offset] = 0x00000000;
                    }
                }

                PlaneMask <<= 1;
            }
        }

        private static unsafe Colour24Bit GetPixel(VGA TheVGA, int X, int Y)
        {
            uint Offset = (uint)(X / 8 + (640 / 8) * Y);

            X = X & 7;

            uint Mask = (byte)(0x80 >> X);
            uint PlaneMask = 1;

            uint Color = 0;

            byte* FrameBuffer = TheVGA.FrameBuffer;

            for (byte PlaneIndex = 0; PlaneIndex < 4; PlaneIndex++)
            {
                TheVGA.SelectPlane(PlaneIndex);

                if ((FrameBuffer[Offset] & Mask) != 0)
                {
                    Color |= PlaneMask;
                }

                PlaneMask <<= 1;
            }

            // TODO: Proper palette index to colour translation
            return new Colour24Bit((byte)Color, 0, 0);
        }

        public byte SetReset => 0x00;
        public byte EnableSetReset => 0x00;
        public byte ColourCompare => 0x00;
        public byte DataRotate => 0x00;
        public byte ReadMapSelect => 0x03;
        public GraphicsModeFlags GraphicsMode => 0x00;
        public byte MiscellaneousGraphics => 0x05;
        public byte ColourDontCare => 0x0F;
        public byte BitMask => 0xFF;
        public byte Reset => 0x03;
        public byte ClockingMode => 0x01;
        public byte MapMask => 0x08;
        public byte CharacterMapSelect => 0x00;
        public SequencerMemoryModeFlags SequencerMemoryMode => (SequencerMemoryModeFlags)0x06;
        // Gray coding applied so that a smooth progression up the colours
        //  can be made
        public byte Palette0 => 0x00;
        public byte Palette1 => 0x01;
        public byte Palette2 => 0x03;
        public byte Palette3 => 0x02;
        public byte Palette4 => 0x07;
        public byte Palette5 => 0x06;
        public byte Palette6 => 0x04;
        public byte Palette7 => 0x05;
        public byte Palette8 => 0x0F;
        public byte Palette9 => 0x0E;
        public byte Palette10 => 0x0C;
        public byte Palette11 => 0x0D;
        public byte Palette12 => 0x08;
        public byte Palette13 => 0x09;
        public byte Palette14 => 0x0B;
        public byte Palette15 => 0x0A;
        public byte AttributeModeControl => 0x01;
        public byte OverscanColour => 0x00;
        public byte ColourPlaneEnable => 0x0F;
        public byte HorizontalPixelPanning => 0x00;
        public byte ColourSelect => 0x00;
        public byte HorizontalTotal => 0x5F;
        public byte EndHorizontalDisplay => 0x4F;
        public byte StartHorizontalBlanking => 0x50;
        public byte EndHorizontalBlanking => 0x82;
        public byte StartHorizontalRetrace => 0x54;
        public byte EndHorizontalRetrace => 0x80;
        public byte VerticalTotal => 0x0B;
        public byte Overflow => 0x3E;
        public byte PresetRowScan => 0x00;
        public byte MaximumScanLine => 0x40;
        public byte CursorStart => 0x00;
        public byte CursorEnd => 0x00;
        public byte StartAddressHigh => 0x00;
        public byte StartAddressLow => 0x00;
        public byte CursorLocationHigh => 0x00;
        public byte CursorLocationLow => 0x00;
        public byte VerticalRetraceStart => 0xEA;
        public byte VerticalRetraceEnd => 0x8C;
        public byte VerticalDisplayEnd => 0xDF;
        public byte Offset => 0x28;
        public byte UnderlineLocation => 0x00;
        public byte StartVerticalBlanking => 0xE7;
        public byte EndVerticalBlanking => 0x04;
        public byte CRTCModeControl => 0xE3;
        public byte LineCompare => 0xFF;
        public byte MiscellaneousOutput => 0xE3;
    }
}
