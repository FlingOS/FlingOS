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

using Kernel.Devices;
using Kernel.VGA.Configurations.Graphical;

namespace Kernel.VGA
{
    /**
     * Useful websites:
     *  http://www.osdever.net/FreeVGA/vga/seqreg.htm
     *  ftp://ftp.apple.asimov.net/pub/apple_II/documentation/hardware/video/Second%20Sight%20VGA%20Registers.pdf
     */

    /// <summary>
    ///     Basic VGA driver
    /// </summary>
    public sealed unsafe class VGA : Device
    {
        private readonly VGARegisters Registers = new VGARegisters();

        private FrameBufferSegments FrameBufferSegment => (FrameBufferSegments)((Registers.MiscellaneousGraphics >> 2) & 3);

        /// <summary>
        ///     The current frame buffer.
        /// </summary>
        internal byte* FrameBuffer
        {
            get
            {
                switch (FrameBufferSegment)
                {
                    case FrameBufferSegments.VGA:
                        return VGAMemory.VGABlock;
                    case FrameBufferSegments.Monochrome:
                        return VGAMemory.MonochromeTextBlock;
                    case FrameBufferSegments.CGA:
                        return VGAMemory.CGATextBlock;
                    default:
                        return null;
                }
            }
        }

        private bool BlankedAndUnlocked { get; set; }

        private void BlankAndUnlock()
        {
            Registers.EndHorizontalBlanking = (byte)(Registers.EndHorizontalBlanking | 0x80);
            Registers.VerticalRetraceEnd = (byte)(Registers.VerticalRetraceEnd & 0x7F);
            BlankedAndUnlocked = true;
        }
        private void UnblankAndLock()
        {
            //Registers.VerticalRetraceEnd = (byte)(Registers.VerticalRetraceEnd | 0x80);
            //Registers.EndHorizontalBlanking = (byte)(Registers.EndHorizontalBlanking & 0x7F);

            Registers.PaletteAddressSource = true;

            BlankedAndUnlocked = false;
        }

        private void ConfigureForCurrentLockState(ref byte EndHorizontalBlankingValue,
            ref byte VerticalRetraceEndValue)
        {
            if (BlankedAndUnlocked)
            {
                EndHorizontalBlankingValue |= 0x80;
                VerticalRetraceEndValue &= 0x7F;
            }
            else
            {
                EndHorizontalBlankingValue &= 0x7F;
                VerticalRetraceEndValue |= 0x80;
            }
        }

        /// <summary>
        ///     The currently loaded configuration for the screen.
        /// </summary>
        public IVGAConfiguration Configuration { get; private set; }
        
        /// <summary>
        ///     Loads the specified screen configuration.
        /// </summary>
        /// <param name="TheConfiguration">The configuration to load.</param>
        public void LoadConfiguration(IVGAConfiguration TheConfiguration)
        {
            Configuration = TheConfiguration;
            
            Registers.MiscellaneousOutput = TheConfiguration.MiscellaneousOutput;
            
            WriteSequencerRegisters(TheConfiguration);

            // Unlock CRT registers and blank the screen
            BlankAndUnlock();

            // Write CRTC registers
            //  Also, take into account blank/unlock state for:
            //          End Horizontal Blanking Register
            //      and Vertical Retrace End Register

            byte EndHorizontalBlankingValue = TheConfiguration.EndHorizontalBlanking;
            byte VerticalRetraceEndValue = TheConfiguration.VerticalRetraceEnd;
            ConfigureForCurrentLockState(ref EndHorizontalBlankingValue, ref VerticalRetraceEndValue);

            WriteCRTCRegisters(TheConfiguration, EndHorizontalBlankingValue, VerticalRetraceEndValue);
            
            WriteGraphicsRegisters(TheConfiguration);

            WriteAttributeRegisters(TheConfiguration);

            // Lock CRT registers and unblank the screen
            UnblankAndLock();
        }

        private void WriteSequencerRegisters(IVGAConfiguration TheConfiguration)
        {
            Registers.Reset = TheConfiguration.Reset;
            Registers.ClockingMode = TheConfiguration.ClockingMode;
            Registers.MapMask = TheConfiguration.MapMask;
            Registers.CharacterMapSelect = TheConfiguration.CharacterMapSelect;
            Registers.SequencerMemoryMode = TheConfiguration.SequencerMemoryMode;
        }

        private void WriteCRTCRegisters(IVGAConfiguration TheConfiguration, byte EndHorizontalBlankingValue,
            byte VerticalRetraceEndValue)
        {
            Registers.HorizontalTotal = TheConfiguration.HorizontalTotal;
            Registers.EndHorizontalDisplay = TheConfiguration.EndHorizontalDisplay;
            Registers.StartHorizontalBlanking = TheConfiguration.StartHorizontalBlanking;
            Registers.EndHorizontalBlanking = EndHorizontalBlankingValue; // Special value
            Registers.StartHorizontalRetrace = TheConfiguration.StartHorizontalRetrace;
            Registers.EndHorizontalRetrace = TheConfiguration.EndHorizontalRetrace;
            Registers.VerticalTotal = TheConfiguration.VerticalTotal;
            Registers.Overflow = TheConfiguration.Overflow;
            Registers.PresetRowScan = TheConfiguration.PresetRowScan;
            Registers.MaximumScanLine = TheConfiguration.MaximumScanLine;
            Registers.CursorStart = TheConfiguration.CursorStart;
            Registers.CursorEnd = TheConfiguration.CursorEnd;
            Registers.StartAddressHigh = TheConfiguration.StartAddressHigh;
            Registers.StartAddressLow = TheConfiguration.StartAddressLow;
            Registers.CursorLocationHigh = TheConfiguration.CursorLocationHigh;
            Registers.CursorLocationLow = TheConfiguration.CursorLocationLow;
            Registers.VerticalRetraceStart = TheConfiguration.VerticalRetraceStart;
            Registers.VerticalRetraceEnd = VerticalRetraceEndValue; // Special value
            Registers.VerticalDisplayEnd = TheConfiguration.VerticalDisplayEnd;
            Registers.Offset = TheConfiguration.Offset;
            Registers.UnderlineLocation = TheConfiguration.UnderlineLocation;
            Registers.StartVerticalBlanking = TheConfiguration.StartVerticalBlanking;
            Registers.EndVerticalBlanking = TheConfiguration.EndVerticalBlanking;
            Registers.CRTCModeControl = TheConfiguration.CRTCModeControl;
            Registers.LineCompare = TheConfiguration.LineCompare;
        }

        private void WriteGraphicsRegisters(IVGAConfiguration TheConfiguration)
        {
            Registers.SetReset = TheConfiguration.SetReset;
            Registers.EnableSetReset = TheConfiguration.EnableSetReset;
            Registers.ColourCompare = TheConfiguration.ColourCompare;
            Registers.DataRotate = TheConfiguration.DataRotate;
            Registers.ReadMapSelect = TheConfiguration.ReadMapSelect;
            Registers.GraphicsMode = TheConfiguration.GraphicsMode;
            Registers.MiscellaneousGraphics = TheConfiguration.MiscellaneousGraphics;
            Registers.ColourDontCare = TheConfiguration.ColourDontCare;
            Registers.BitMask = TheConfiguration.BitMask;
        }

        private void WriteAttributeRegisters(IVGAConfiguration TheConfiguration)
        {
            Registers.Palette0 = TheConfiguration.Palette0;
            Registers.Palette1 = TheConfiguration.Palette1;
            Registers.Palette2 = TheConfiguration.Palette2;
            Registers.Palette3 = TheConfiguration.Palette3;
            Registers.Palette4 = TheConfiguration.Palette4;
            Registers.Palette5 = TheConfiguration.Palette5;
            Registers.Palette6 = TheConfiguration.Palette6;
            Registers.Palette7 = TheConfiguration.Palette7;
            Registers.Palette8 = TheConfiguration.Palette8;
            Registers.Palette9 = TheConfiguration.Palette9;
            Registers.Palette10 = TheConfiguration.Palette10;
            Registers.Palette11 = TheConfiguration.Palette11;
            Registers.Palette12 = TheConfiguration.Palette12;
            Registers.Palette13 = TheConfiguration.Palette13;
            Registers.Palette14 = TheConfiguration.Palette14;
            Registers.Palette15 = TheConfiguration.Palette15;
            Registers.AttributeModeControl = TheConfiguration.AttributeModeControl;
            Registers.OverscanColour = TheConfiguration.OverscanColour;
            Registers.ColourPlaneEnable = TheConfiguration.ColourPlaneEnable;
            Registers.HorizontalPixelPanning = TheConfiguration.HorizontalPixelPanning;
            Registers.ColourSelect = TheConfiguration.ColourSelect;
        }

        internal void SelectPlane(byte Plane)
        {
            Plane &= 3;
            
            // Set plane to read from
            Registers.ReadMapSelect = Plane;

            // Set plane to write to
            Registers.MapMask = (byte)(1 << Plane);
        }

        //TODO: Load font

        /// <summary>
        ///     Sets the colour of a pixel on the screen.
        /// </summary>
        /// <remarks>
        ///     Assumes graphical mode is selected.
        /// </remarks>
        /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the pixel to set.</param>
        /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the pixel to set.</param>
        /// <param name="Colour">The 24-bit colour to set the pixel to.</param>
        public void SetPixel(int X, int Y, Colour24Bit Colour)
            => Configuration.SetPixelMethod(this, X, Y, Colour);

        /// <summary>
        ///     Gets the colour of a pixel on the screen.
        /// </summary>
        /// <remarks>
        ///     Assumes graphical mode is selected.
        /// </remarks>
        /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the pixel to get.</param>
        /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the pixel to get.</param>
        /// <returns>The 24-bit colour of the pixel.</returns>
        public Colour24Bit GetPixel(int X, int Y) => Configuration.GetPixelMethod(this, X, Y);

        /// <summary>
        ///     Sets a cell on the screen to the specified character and colour (/attribute).
        /// </summary>
        /// <remarks>
        ///     Assumes text mode is selected.
        /// </remarks>
        /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the cell to set.</param>
        /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the cell to set.</param>
        /// <param name="Character">The character to set the cell to.</param>
        /// <param name="Colour">The colour (/attribute) for the cell.</param>
        public void SetCell(int X, int Y, char Character, Colour8Bit Colour)
            => Configuration.SetCellMethod(this, X, Y, Character, Colour);

        /// <summary>
        ///     Gets the character and colour (/attribute) of a cell on the screen.
        /// </summary>
        /// <remarks>
        ///     Assumes text mode is selected.
        /// </remarks>
        /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the cell to get.</param>
        /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the cell to get.</param>
        /// <param name="Colour">The colour (/attribute) of the cell.</param>
        /// <returns>The character of the cell.</returns>
        public char GetCell(int X, int Y, out Colour8Bit Colour)
            => Configuration.GetCellMethod(this, X, Y, out Colour);

        /// <summary>
        ///     Tests the 640x480, 4-bit graphical configuration
        /// </summary>
        public void TestMode_G_640x480x4()
        {
            LoadConfiguration(new G_640x480x4());

            Registers.DACMask = 0xFF;
            
            // Some flickering is to be expected as each plane is updated in turn!
            //  Changes between colours that happen much faster than the clear calls
            //  below are just flicker and can be ignored.
            
            int i = 0;
            byte col = 0;

            ColourPalette = new Colour18Bit[16];
            while (i < 16)
            {
                ColourPalette[i++] = new Colour18Bit(col, col, col);
                col += 4;
            }
            SetPalette(ColourPalette);

            // Gray code progression through the colours
            Clear(new Colour24Bit(0x0, 0, 0));
            Clear(new Colour24Bit(0x1, 0, 0));
            Clear(new Colour24Bit(0x3, 0, 0));
            Clear(new Colour24Bit(0x2, 0, 0));
            Clear(new Colour24Bit(0x6, 0, 0));
            Clear(new Colour24Bit(0x7, 0, 0));
            Clear(new Colour24Bit(0x5, 0, 0));
            Clear(new Colour24Bit(0x4, 0, 0));
            Clear(new Colour24Bit(0xC, 0, 0));
            Clear(new Colour24Bit(0xD, 0, 0));
            Clear(new Colour24Bit(0xF, 0, 0));
            Clear(new Colour24Bit(0xE, 0, 0));
            Clear(new Colour24Bit(0xA, 0, 0));
            Clear(new Colour24Bit(0xB, 0, 0));
            Clear(new Colour24Bit(0x9, 0, 0));
            Clear(new Colour24Bit(0x8, 0, 0));
        }

        public void Clear(Colour24Bit Colour)
        {
            Configuration.ClearMethod(this, 0, 0, Colour);
        }

        private Colour18Bit[] ColourPalette = new Colour18Bit[256];
        public Colour18Bit GetPaletteEntry(int Index) => ColourPalette[Index];

        public void SetPalette(Colour18Bit[] NewPallete)
        {
            ColourPalette = NewPallete;

            Registers.DACWriteAddress = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < NewPallete.Length; i++)
            {
                Registers.DACData = NewPallete[i];
            }
        }
        public void SetPaletteEntry(int Index, Colour18Bit Colour)
        {
            ColourPalette[Index] = Colour;

            Registers.DACWriteAddress = (byte)Index;
            Registers.DACData = Colour;
        }
    }
}
