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
    ///     Provides accessors for the VGA registers (indexed and non-indexed)
    /// </summary>
    internal sealed class VGARegisters : Object
    {
        /// <summary>
        ///     The VGA IO ports.
        /// </summary>
        private readonly VGAIOPorts IO = new VGAIOPorts();

        #region Graphics registers

        /// <summary>
        ///     The Set/Reset register
        /// </summary>
        public byte SetReset
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.SetResetRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.SetResetRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Enable Set/Reset register
        /// </summary>
        public byte EnableSetReset
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.EnableSetResetRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.EnableSetResetRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Colour Compare register
        /// </summary>
        public byte ColourCompare
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.ColourCompareRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.ColourCompareRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Data Rotate register
        /// </summary>
        public byte DataRotate
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.DataRotateRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.DataRotateRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Read Map Select register
        /// </summary>
        public byte ReadMapSelect
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.ReadMapSelectRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.ReadMapSelectRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Graphics Mode register
        /// </summary>
        public GraphicsModeFlags GraphicsMode
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.GraphicsModeRegisterIndex);
                return (GraphicsModeFlags)IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.GraphicsModeRegisterIndex);
                IO.GraphicsControllerData.Write_Byte((byte)value);
            }
        }

        /// <summary>
        ///     The Miscellaneous Graphics register
        /// </summary>
        public byte MiscellaneousGraphics
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.MiscellaneousGraphicsRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.MiscellaneousGraphicsRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Colour Don't Care register
        /// </summary>
        public byte ColourDontCare
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.ColourDontCareRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.ColourDontCareRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Bit Mask register
        /// </summary>
        public byte BitMask
        {
            get
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.BitMaskRegisterIndex);
                return IO.GraphicsControllerData.Read_Byte();
            }
            set
            {
                IO.GraphicsControllerIndex.Write_Byte(VGAConstants.BitMaskRegisterIndex);
                IO.GraphicsControllerData.Write_Byte(value);
            }
        }

        #endregion

        #region Sequencer registers

        /// <summary>
        ///     The Reset register
        /// </summary>
        public byte Reset
        {
            get
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.ResetRegisterIndex);
                return IO.SequencerData.Read_Byte();
            }
            set
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.ResetRegisterIndex);
                IO.SequencerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Clocking Mode register
        /// </summary>
        public byte ClockingMode
        {
            get
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.ClockingModeRegisterIndex);
                return IO.SequencerData.Read_Byte();
            }
            set
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.ClockingModeRegisterIndex);
                IO.SequencerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Map Mask register
        /// </summary>
        public byte MapMask
        {
            get
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.MapMaskRegisterIndex);
                return IO.SequencerData.Read_Byte();
            }
            set
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.MapMaskRegisterIndex);
                IO.SequencerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Character Map Select register
        /// </summary>
        public byte CharacterMapSelect
        {
            get
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.CharacterMapSelectRegisterIndex);
                return IO.SequencerData.Read_Byte();
            }
            set
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.CharacterMapSelectRegisterIndex);
                IO.SequencerData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Sequencer Memory Mode register
        /// </summary>
        public SequencerMemoryModeFlags SequencerMemoryMode
        {
            get
            {
                // Write index to SequencerIndex
                IO.SequencerIndex.Write_Byte(VGAConstants.SequencerMemoryModeRegisterIndex);
                // Read data from SequencerData
                return (SequencerMemoryModeFlags)IO.SequencerData.Read_Byte();
            }
            set
            {
                IO.SequencerIndex.Write_Byte(VGAConstants.SequencerMemoryModeRegisterIndex);
                IO.SequencerData.Write_Byte((byte)value);
            }
        }

        #endregion

        #region Attribute registers


        /// <summary>
        ///     Palette Register 0
        /// </summary>
        public byte Palette0
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette0RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette0RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 1
        /// </summary>
        public byte Palette1
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette1RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette1RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 2
        /// </summary>
        public byte Palette2
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette2RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette2RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 3
        /// </summary>
        public byte Palette3
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette3RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette3RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 4
        /// </summary>
        public byte Palette4
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette4RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette4RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 5
        /// </summary>
        public byte Palette5
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette5RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette5RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 6
        /// </summary>
        public byte Palette6
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette6RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette6RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 7
        /// </summary>
        public byte Palette7
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette7RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette7RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 8
        /// </summary>
        public byte Palette8
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette8RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette8RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 9
        /// </summary>
        public byte Palette9
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette9RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette9RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 10
        /// </summary>
        public byte Palette10
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette10RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette10RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 11
        /// </summary>
        public byte Palette11
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette11RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette11RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 12
        /// </summary>
        public byte Palette12
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette12RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette12RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 13
        /// </summary>
        public byte Palette13
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette13RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette13RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 14
        /// </summary>
        public byte Palette14
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette14RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette14RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     Palette Register 15
        /// </summary>
        public byte Palette15
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette15RegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.Palette15RegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Attribute Mode Control register
        /// </summary>
        public byte AttributeModeControl
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.AttributeModeControlRegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.AttributeModeControlRegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Overscan Colour register
        /// </summary>
        public byte OverscanColour
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.OverscanColourRegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.OverscanColourRegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Colour Plane Enable register
        /// </summary>
        public byte ColourPlaneEnable
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.ColourPlaneEnableRegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.ColourPlaneEnableRegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Horizontal Pixel Panning register
        /// </summary>
        public byte HorizontalPixelPanning
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.HorizontalPixelPanningRegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.HorizontalPixelPanningRegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Colour Select register
        /// </summary>
        public byte ColourSelect
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.ColourSelectRegisterIndex);
                return IO.AttributeControllerReadData.Read_Byte();
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(VGAConstants.ColourSelectRegisterIndex);
                IO.AttributeControllerWriteIndexData.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Palette Address Source bit in the Attribute Address register
        /// </summary>
        public bool PaletteAddressSource
        {
            get
            {
                IO.InputStatus1Colour.Read_Byte();
                return ((IO.AttributeControllerReadData.Read_Byte() >> 5) & 0x1) == 0;
            }
            set
            {
                IO.InputStatus1Colour.Read_Byte();
                IO.AttributeControllerWriteIndexData.Write_Byte(value ? (byte)0x20 : (byte)0x00);
            }
        }

        #endregion

        #region CRTC registers
        
        /// <summary>
        ///     The Horizontal Total register
        /// </summary>
        public byte HorizontalTotal
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.HorizontalTotalRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.HorizontalTotalRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The End Horizontal Display register
        /// </summary>
        public byte EndHorizontalDisplay
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndHorizontalDisplayRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndHorizontalDisplayRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Start Horizontal Blanking register
        /// </summary>
        public byte StartHorizontalBlanking
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartHorizontalBlankingRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartHorizontalBlankingRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The End Horizontal Blanking register
        /// </summary>
        public byte EndHorizontalBlanking
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndHorizontalBlankingRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndHorizontalBlankingRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Start Horizontal Retrace register
        /// </summary>
        public byte StartHorizontalRetrace
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartHorizontalRetraceRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartHorizontalRetraceRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The End Horizontal Retrace register
        /// </summary>
        public byte EndHorizontalRetrace
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndHorizontalRetraceRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndHorizontalRetraceRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Vertical Total register
        /// </summary>
        public byte VerticalTotal
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalTotalRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalTotalRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Overflow register
        /// </summary>
        public byte Overflow
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.OverflowRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.OverflowRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Preset Row Scan register
        /// </summary>
        public byte PresetRowScan
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.PresetRowScanRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.PresetRowScanRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Maximum Scan Line register
        /// </summary>
        public byte MaximumScanLine
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.MaximumScanLineRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.MaximumScanLineRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Cursor Start register
        /// </summary>
        public byte CursorStart
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorStartRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorStartRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Cursor End register
        /// </summary>
        public byte CursorEnd
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorEndRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorEndRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Start Address High register
        /// </summary>
        public byte StartAddressHigh
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartAddressHighRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartAddressHighRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Start Address Low register
        /// </summary>
        public byte StartAddressLow
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartAddressLowRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartAddressLowRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Cursor Location High register
        /// </summary>
        public byte CursorLocationHigh
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorLocationHighRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorLocationHighRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Cursor Location Low register
        /// </summary>
        public byte CursorLocationLow
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorLocationLowRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CursorLocationLowRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Vertical Retrace Start register
        /// </summary>
        public byte VerticalRetraceStart
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalRetraceStartRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalRetraceStartRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Vertical Retrace End register
        /// </summary>
        public byte VerticalRetraceEnd
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalRetraceEndRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalRetraceEndRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Vertical Display End register
        /// </summary>
        public byte VerticalDisplayEnd
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalDisplayEndRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.VerticalDisplayEndRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Offset register
        /// </summary>
        public byte Offset
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.OffsetRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.OffsetRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Underline Location register
        /// </summary>
        public byte UnderlineLocation
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.UnderlineLocationRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.UnderlineLocationRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Start Vertical Blanking register
        /// </summary>
        public byte StartVerticalBlanking
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartVerticalBlankingRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.StartVerticalBlankingRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The End Vertical Blanking
        /// </summary>
        public byte EndVerticalBlanking
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndVerticalBlankingRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.EndVerticalBlankingRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The CRTC Mode Control register
        /// </summary>
        public byte CRTCModeControl
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CRTCModeControlRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.CRTCModeControlRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        /// <summary>
        ///     The Line Compare register
        /// </summary>
        public byte LineCompare
        {
            get
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.LineCompareRegisterIndex);
                return IO.CRTControllerDataColour.Read_Byte();
            }
            set
            {
                IO.CRTControllerIndexColour.Write_Byte(VGAConstants.LineCompareRegisterIndex);
                IO.CRTControllerDataColour.Write_Byte(value);
            }
        }

        #endregion

        #region Colour registers

        public byte DACMask
        {
            get { return IO.DACMask.Read_Byte(); }
            set { IO.DACMask.Write_Byte(value); }
        }

        /// <summary>
        ///     The DAC Write-Access Address register
        /// </summary>
        public byte DACWriteAddress
        {
            get { return IO.DACIndexForWriteData.Read_Byte(); }
            set { IO.DACIndexForWriteData.Write_Byte(value); }
        }

        /// <summary>
        ///     The DAC Read-Access Address register
        /// </summary>
        /// <remarks>
        ///     This is a write-only register.
        /// </remarks>
        public byte DACReadAddress
        {
            set { IO.DACIndexForReadData.Write_Byte(value); }
        }
        
        /// <summary>
        ///     The DAC Data register
        /// </summary>
        public Colour18Bit DACData
        {
            get { return new Colour18Bit(IO.DACData.Read_Byte(), 
                                         IO.DACData.Read_Byte(), 
                                         IO.DACData.Read_Byte()); }
            set
            {
                IO.DACData.Write_Byte(value.Red);
                IO.DACData.Write_Byte(value.Green);
                IO.DACData.Write_Byte(value.Blue);
            }
        }

        /// <summary>
        ///     The DAC State register
        /// </summary>
        /// <remarks>
        ///     This is a read-only register.
        /// </remarks>
        public byte DACState => IO.DACState.Read_Byte();

        #endregion

        #region Extended registers

        /// <summary>
        ///     The Miscellaneous Output register
        /// </summary>
        public byte MiscellaneousOutput
        {
            get { return IO.MiscellaneousOutputRead.Read_Byte(); }
            set { IO.MiscellaneousOutputWrite.Write_Byte(value); }
        }

        /// <summary>
        ///     The colour-mode Feature Control register
        /// </summary>
        /// <remarks>
        ///     Read operations are identical for colour and mono modes.
        ///     Colour mode only makes a difference when writing to the 
        ///     feature control register.
        /// </remarks>
        public byte FeatureControlColour
        {
            get { return IO.FeatureControlRead.Read_Byte(); }
            set { IO.FeatureControlWriteColour.Write_Byte(value); }
        }

        /// <summary>
        ///     The mono-mode Feature Control register
        /// </summary>
        /// <remarks>
        ///     Read operations are identical for colour and mono modes.
        ///     Colour mode only makes a difference when writing to the 
        ///     feature control register.
        /// </remarks>
        public byte FeatureControlMono
        {
            get { return IO.FeatureControlRead.Read_Byte(); }
            set { IO.FeatureControlWriteMono.Write_Byte(value); }
        }

        /// <summary>
        ///     The Input Status 0 register
        /// </summary>
        /// <remarks>
        ///     This is a read-only register.
        /// </remarks>
        public byte InputStatus0 => IO.InputStatus0.Read_Byte();

        /// <summary>
        ///     The colour-mode Input Status 1 register
        /// </summary>
        /// <remarks>
        ///     This is a read-only register.
        /// </remarks>
        public byte InputStatus1Colour => IO.InputStatus1Colour.Read_Byte();

        /// <summary>
        ///     The mono-mode Input Status 1 register
        /// </summary>
        /// <remarks>
        ///     This is a read-only register.
        /// </remarks>
        public byte InputStatus1Mono => IO.InputStatus1Mono.Read_Byte();

        #endregion

    }
}
