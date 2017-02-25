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
using Kernel.VGA.Fonts;

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
        private static VGA _Instance;
        private static VGA Instance => _Instance ?? (_Instance = new VGA());

        private VGA()
        {
        }

        public static VGA GetInstance() => Instance;

        public static VGA GetConfiguredInstance(IVGAConfiguration Configuration, IFont Font)
        {
            SetConfiguration(Configuration, Font);
            return Instance;
        }

        public static void SetConfiguration(IVGAConfiguration Configuration, IFont Font)
        {
            Instance.LoadConfiguration(Configuration);
            if (Configuration.Mode == ScreenMode.Text)
            {
                Instance.LoadFont(Font);
            }
        }


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
        ///     The currently loaded font for the screen.
        /// </summary>
        public IFont Font { get; private set; }

        /// <summary>
        ///     Loads the specified screen configuration.
        /// </summary>
        /// <param name="TheConfiguration">The configuration to load.</param>
        public void LoadConfiguration(IVGAConfiguration TheConfiguration)
        {
            if (Configuration == TheConfiguration)
                return;

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

            if (TheConfiguration.Mode == ScreenMode.Text)
            {
                if (Font != null)
                {
                    LoadFont(Font, true);
                }
            }
        }

        /// <summary>
        ///     Loads the specified font to the VGA font buffer.
        /// </summary>
        /// <param name="TheFont">The font to load.</param>
        public void LoadFont(IFont TheFont)
        {
            LoadFont(TheFont, false);
        }
        private void LoadFont(IFont TheFont, bool SkipCheck)
        {
            if (!SkipCheck && Font == TheFont)
                return;

            Font = TheFont;

            // Save registers
            byte MapMask = Registers.MapMask;
            SequencerMemoryModeFlags SequencerMemoryMode = Registers.SequencerMemoryMode;

            // Switch to flat addressing (assuming chain-4 addressing already off)
            Registers.SequencerMemoryMode = SequencerMemoryMode | SequencerMemoryModeFlags.OddEvenDisable;

            // Save registers
            byte ReadMapSelect = Registers.ReadMapSelect;

            // Save register and turn off even-odd addressing
            GraphicsModeFlags GraphicsMode = Registers.GraphicsMode;
            Registers.GraphicsMode = GraphicsMode & ~GraphicsModeFlags.HostOddEvenMemoryReadAddressingEnable;

            // Save register and turn off even-odd addressing
            byte MiscellaneousGraphics = Registers.MiscellaneousGraphics;
            Registers.MiscellaneousGraphics = (byte)(MiscellaneousGraphics & ~0x02);

            // Write font to plane P4
            SelectPlane(2);

            // Write to font 0
            byte* Buffer = FrameBuffer;
            int FontHeight = TheFont.FontHeight;
            byte[] FontData = TheFont.FontData;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < FontHeight; j++)
                {
                    Buffer[(i * 32) + j] = FontData[(i * FontHeight) + j];
                }
            }

            // Restore registers
            Registers.MapMask = MapMask;
            Registers.SequencerMemoryMode = SequencerMemoryMode;
            Registers.ReadMapSelect = ReadMapSelect;
            Registers.GraphicsMode = GraphicsMode;
            Registers.MiscellaneousGraphics = MiscellaneousGraphics;
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
        /// <param name="ForeColour">The foreground colour (/attribute) for the cell.</param>
        /// <param name="BackColour">The background colour (/attribute) for the cell.</param>
        public void SetCell(int X, int Y, char Character, Colour4Bit ForeColour, Colour4Bit BackColour)
            => Configuration.SetCellMethod(this, X, Y, Character, ForeColour, BackColour);

        /// <summary>
        ///     Gets the character and colour (/attribute) of a cell on the screen.
        /// </summary>
        /// <remarks>
        ///     Assumes text mode is selected.
        /// </remarks>
        /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the cell to get.</param>
        /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the cell to get.</param>
        /// <param name="ForeColour">The foreground colour (/attribute) of the cell.</param>
        /// <param name="BackColour">The background colour (/attribute) of the cell.</param>
        /// <returns>The character of the cell.</returns>
        public char GetCell(int X, int Y, out Colour4Bit ForeColour, out Colour4Bit BackColour)
            => Configuration.GetCellMethod(this, X, Y, out ForeColour, out BackColour);

        /// <summary>
        ///     Sets the entire screen to the specified colour.
        /// </summary>
        /// <param name="Colour"></param>
        public void Clear(Colour24Bit Colour) => Configuration.ClearMethod(this, 0, 0, Colour);

        /// <summary>
        ///     The current colour palette.
        /// </summary>
        private Colour18Bit[] ColourPalette = new Colour18Bit[256];
        /// <summary>
        ///     Looks up the specified index in the (cached) colour palette.
        /// </summary>
        /// <param name="Index">The index of the entry to look up.</param>
        /// <returns>The entry value as an 18-bit colour.</returns>
        public Colour18Bit GetPaletteEntry(int Index) => ColourPalette[Index];

        /// <summary>
        ///     Sets the colour palette to the specified colours.
        /// </summary>
        /// <param name="NewPallete">The colours to set the palette to.</param>
        public void SetPalette(Colour18Bit[] NewPallete) => SetPalette(0, NewPallete);

        /// <summary>
        ///     Sets the colour palette starting from the specified offset to the specified colours.
        /// </summary>
        /// <param name="Offset">The offset to start setting at.</param>
        /// <param name="NewPallete">The colours to set the palette to.</param>
        public void SetPalette(int Offset, Colour18Bit[] NewPallete)
        {
            Registers.DACWriteAddress = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < NewPallete.Length; i++)
            {
                Registers.DACData = ColourPalette[i + Offset] = NewPallete[i];
            }
        }
        /// <summary>
        ///     Sets a specific entry in the colour palette.
        /// </summary>
        /// <seealso cref="SetPalette(int, Kernel.VGA.Colour18Bit[])"/>
        /// <param name="Index">The idnex of the entry to set.</param>
        /// <param name="Colour">The colour to set the entry to.</param>
        public void SetPaletteEntry(int Index, Colour18Bit Colour)
        {
            ColourPalette[Index] = Colour;

            Registers.DACWriteAddress = (byte)Index;
            Registers.DACData = Colour;
        }

        /// <summary>
        ///     Sets the first 16 palettes to the standard CGA colours.
        /// </summary>
        /// <remarks>
        ///     See <a href="https://en.wikipedia.org/wiki/Video_Graphics_Array#Color_palette">the Wikipedia article on VGA</a>./
        /// </remarks>
        public void SetCGAPalette()
        {
            // Force palette indexes

            BlankAndUnlock();

            Registers.Palette0 = 0;
            Registers.Palette1 = 1;
            Registers.Palette2 = 2;
            Registers.Palette3 = 3;
            Registers.Palette4 = 4;
            Registers.Palette5 = 5;
            Registers.Palette6 = 6;
            Registers.Palette7 = 7;
            Registers.Palette8 = 8;
            Registers.Palette9 = 9;
            Registers.Palette10 = 10;
            Registers.Palette11 = 11;
            Registers.Palette12 = 12;
            Registers.Palette13 = 13;
            Registers.Palette14 = 14;
            Registers.Palette15 = 15;

            UnblankAndLock();


            // Set palette colours

            SetPaletteEntry(0x0, new Colour18Bit(0, 0, 0));
            SetPaletteEntry(0x1, new Colour18Bit(0, 0, 42));
            SetPaletteEntry(0x2, new Colour18Bit(0, 42, 0));
            SetPaletteEntry(0x3, new Colour18Bit(0, 42, 42));
            SetPaletteEntry(0x4, new Colour18Bit(42, 0, 0));
            SetPaletteEntry(0x5, new Colour18Bit(42, 0, 42));
            SetPaletteEntry(0x6, new Colour18Bit(42, 21, 0));
            SetPaletteEntry(0x7, new Colour18Bit(42, 42, 42));
            SetPaletteEntry(0x8, new Colour18Bit(21, 21, 21));
            SetPaletteEntry(0x9, new Colour18Bit(21, 21, 63));
            SetPaletteEntry(0xA, new Colour18Bit(21, 63, 21));
            SetPaletteEntry(0xB, new Colour18Bit(21, 63, 63));
            SetPaletteEntry(0xC, new Colour18Bit(63, 21, 21));
            SetPaletteEntry(0xD, new Colour18Bit(63, 21, 63));
            SetPaletteEntry(0xE, new Colour18Bit(63, 63, 21));
            SetPaletteEntry(0xF, new Colour18Bit(63, 63, 63));
        }

        /// <summary>
        ///     Tests the 640x480, 4-bit graphical configuration
        /// </summary>
        public void TestMode_G_640x480x4()
        {
            LoadConfiguration(G_640x480x4.Instance);

            // Configure custom colours in the colour palette
            Registers.DACMask = 0xFF;
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
            //  Gray coding eliminates flicker between colours as the planes
            //  change separately.
            Colour24Bit ClearColour = new Colour24Bit(0x0, 0, 0);
            Clear(ClearColour);
            ClearColour.Red = 0x1;
            Clear(ClearColour);
            ClearColour.Red = 0x3;
            Clear(ClearColour);
            ClearColour.Red = 0x2;
            Clear(ClearColour);
            ClearColour.Red = 0x6;
            Clear(ClearColour);
            ClearColour.Red = 0x7;
            Clear(ClearColour);
            ClearColour.Red = 0x5;
            Clear(ClearColour);
            ClearColour.Red = 0x4;
            Clear(ClearColour);
            ClearColour.Red = 0xC;
            Clear(ClearColour);
            ClearColour.Red = 0xD;
            Clear(ClearColour);
            ClearColour.Red = 0xF;
            Clear(ClearColour);
            ClearColour.Red = 0xE;
            Clear(ClearColour);
            ClearColour.Red = 0xA;
            Clear(ClearColour);
            ClearColour.Red = 0xB;
            Clear(ClearColour);
            ClearColour.Red = 0x9;
            Clear(ClearColour);
            ClearColour.Red = 0x8;
            Clear(ClearColour);
        }
        /// <summary>
        ///     Tests the 640x480, 4-bit graphical configuration
        /// </summary>
        public void TestMode_G_720x480x4()
        {
            LoadConfiguration(G_720x480x4.Instance);

            // Configure custom colours in the colour palette
            Registers.DACMask = 0xFF;
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
            //  Gray coding eliminates flicker between colours as the planes
            //  change separately.
            Colour24Bit ClearColour = new Colour24Bit(0x0, 0, 0);
            Clear(ClearColour);
            ClearColour.Red = 0x1;
            Clear(ClearColour);
            ClearColour.Red = 0x3;
            Clear(ClearColour);
            ClearColour.Red = 0x2;
            Clear(ClearColour);
            ClearColour.Red = 0x6;
            Clear(ClearColour);
            ClearColour.Red = 0x7;
            Clear(ClearColour);
            ClearColour.Red = 0x5;
            Clear(ClearColour);
            ClearColour.Red = 0x4;
            Clear(ClearColour);
            ClearColour.Red = 0xC;
            Clear(ClearColour);
            ClearColour.Red = 0xD;
            Clear(ClearColour);
            ClearColour.Red = 0xF;
            Clear(ClearColour);
            ClearColour.Red = 0xE;
            Clear(ClearColour);
            ClearColour.Red = 0xA;
            Clear(ClearColour);
            ClearColour.Red = 0xB;
            Clear(ClearColour);
            ClearColour.Red = 0x9;
            Clear(ClearColour);
            ClearColour.Red = 0x8;
            Clear(ClearColour);
        }

        /// <summary>
        ///     Tests the 80x25, 8x16-font text configuration
        /// </summary>
        public void TestMode_T_80x25()
        {
            LoadConfiguration(T_80x25.Instance);
            LoadFont(LucidaConsole.Instance);

            //// Configure custom colours in the colour palette
            //Registers.DACMask = 0xFF;
            //int i = 0;
            //byte col = 0;
            //ColourPalette = new Colour18Bit[16];
            //while (i < 16)
            //{
            //    ColourPalette[i++] = new Colour18Bit(col, col, col);
            //    col += 4;
            //}
            //SetPalette(ColourPalette);

            Colour4Bit ForeColour = new Colour4Bit(0xF);
            Colour4Bit BackColour = new Colour4Bit(0x2);
            SetCell(0, 0, 'H', ForeColour, BackColour);
            SetCell(0, 1, 'e', ForeColour, BackColour);
            SetCell(0, 2, 'l', ForeColour, BackColour);
            SetCell(0, 3, 'l', ForeColour, BackColour);
            SetCell(0, 4, 'o', ForeColour, BackColour);
            SetCell(0, 5, ' ', ForeColour, BackColour);
            SetCell(0, 6, 'w', ForeColour, BackColour);
            SetCell(0, 7, 'o', ForeColour, BackColour);
            SetCell(0, 8, 'r', ForeColour, BackColour);
            SetCell(0, 9, 'l', ForeColour, BackColour);
            SetCell(0, 10, 'd', ForeColour, BackColour);
            SetCell(0, 11, '!', ForeColour, BackColour);
        }

        /// <summary>
        ///     Sets the cursor position to the specified offset.
        /// </summary>
        /// <param name="offset">The new offset value.</param>
        public void UpdateCursorOffset(ushort offset)
        {
            Registers.CursorLocationHigh = (byte)(offset >> 8);
            Registers.CursorLocationLow = (byte)(offset);
        }
    }
}
