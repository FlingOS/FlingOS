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
    ///     Delegate for a method for setting a pixel in a particular VGA configuration.
    /// </summary>
    /// <param name="TheVGA">The VGA device making the call.</param>
    /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the pixel to set.</param>
    /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the pixel to set.</param>
    /// <param name="Colour">The 24-bit colour to set the pixel to.</param>
    public delegate void SetPixelDelegate(VGA TheVGA, int X, int Y, Colour24Bit Colour);
    /// <summary>
    ///     Delegate for a method for getting a pixel in a particular VGA configuration.
    /// </summary>
    /// <param name="TheVGA">The VGA device making the call.</param>
    /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the pixel to get.</param>
    /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the pixel to get.</param>
    /// <returns>The 24-bit colour of the pixel.</returns>
    public delegate Colour24Bit GetPixelDelegate(VGA TheVGA, int X, int Y);

    /// <summary>
    ///     Delegate for a method for setting a cell in a particular VGA configuration.
    /// </summary>
    /// <param name="TheVGA">The VGA device making the call.</param>
    /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the cell to set.</param>
    /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the cell to set.</param>
    /// <param name="Character">The character to set the cell to.</param>
    /// <param name="ForeColour">The foreground colour (/attribute) for the cell.</param>
    /// <param name="BackColour">The background colour (/attribute) for the cell.</param>
    public delegate void SetCellDelegate(VGA TheVGA, int X, int Y, char Character, Colour4Bit ForeColour, Colour4Bit BackColour);
    /// <summary>
    ///     Delegate for a method for getting a cell in a particular VGA configuration.
    /// </summary>
    /// <param name="TheVGA">The VGA device making the call.</param>
    /// <param name="X">The X-coordinate (from left to right) as a 0-based index of the cell to get.</param>
    /// <param name="Y">The Y-coordinate (from top to bottom) as a 0-based index of the cell to get.</param>
    /// <param name="ForeColour">The foreground colour (/attribute) of the cell.</param>
    /// <param name="BackColour">The background colour (/attribute) of the cell.</param>
    /// <returns>The character of the cell.</returns>
    public delegate char GetCellDelegate(VGA TheVGA, int X, int Y, out Colour4Bit ForeColour, out Colour4Bit BackColour);

    /// <summary>
    ///     Interface for a configuration of the VGA registers.
    /// </summary>
    /// <remarks>
    ///     A configuration implementation can be hard-coded by the developer
    ///     or loaded at runtime. However, it must remain constant once loaded.
    /// </remarks>
    public interface IVGAConfiguration : IObject
    {
        /// <summary>
        ///     Width of the screen in pixels.
        /// </summary>
        uint Width { get; }
        
        /// <summary>
        ///     Height of the screen in pixels.
        /// </summary>
        uint Height { get; }

        /// <summary>
        ///     Colour depth of the palette in bits.
        /// </summary>
        uint BitDepth { get; }

        /// <summary>
        ///     Whether the configuration is a text or graphical mode configuration.
        /// </summary>
        ScreenMode Mode { get; }

        /// <summary>
        ///     Method for setting a cell on the screen.
        /// </summary>
        /// <remarks>
        ///     This can still be valid for a graphical mode configuration if it supports
        ///     graphical text output.
        /// </remarks>
        SetCellDelegate SetCellMethod { get; }
        /// <summary>
        ///     Method for getting a cell from the screen.
        /// </summary>
        /// <remarks>
        ///     This can still be valid for a graphical mode configuration if it supports
        ///     graphical text output.
        /// </remarks>
        GetCellDelegate GetCellMethod { get; }

        /// <summary>
        ///     Method for setting a pixel on the screen.
        /// </summary>
        /// <remarks>
        ///     Not valid for text mode configurations. Should return null if the configuration
        ///     is a text mode configuration.
        /// </remarks>
        SetPixelDelegate SetPixelMethod { get; }
        /// <summary>
        ///     Method for getting a pixel from the screen.
        /// </summary>
        /// <remarks>
        ///     Not valid for text mode configurations. Should return null if the configuration
        ///     is a text mode configuration.
        /// </remarks>
        GetPixelDelegate GetPixelMethod { get; }

        /// <summary>
        ///     Method for setting all the pixels on the screen.
        /// </summary>
        /// <remarks>
        ///     Not valid for text mode configurations. Should return null if the configuration
        ///     is a text mode configuration.
        /// </remarks>
        SetPixelDelegate ClearMethod { get; }

        #region Registers

        #region Graphics registers

        /// <summary>
        ///    Value for the Set Reset register
        /// </summary>
        byte SetReset { get; }

        /// <summary>
        ///    Value for the Enable Set Reset register
        /// </summary>
        byte EnableSetReset { get; }

        /// <summary>
        ///    Value for the Colour Compare register
        /// </summary>
        byte ColourCompare { get; }

        /// <summary>
        ///    Value for the Data Rotate register
        /// </summary>
        byte DataRotate { get; }

        /// <summary>
        ///    Value for the Read Map Select register
        /// </summary>
        byte ReadMapSelect { get; }

        /// <summary>
        ///    Value for the Graphics Mode register
        /// </summary>
        GraphicsModeFlags GraphicsMode { get; }

        /// <summary>
        ///    Value for the Miscellaneous Graphics register
        /// </summary>
        byte MiscellaneousGraphics { get; }

        /// <summary>
        ///    Value for the Colour Dont Care register
        /// </summary>
        byte ColourDontCare { get; }

        /// <summary>
        ///    Value for the Bit Mask register
        /// </summary>
        byte BitMask { get; }

        #endregion

        #region Sequencer registers

        /// <summary>
        ///    Value for the Reset register
        /// </summary>
        byte Reset { get; }

        /// <summary>
        ///    Value for the Clocking Mode register
        /// </summary>
        byte ClockingMode { get; }

        /// <summary>
        ///    Value for the Map Mask register
        /// </summary>
        byte MapMask { get; }

        /// <summary>
        ///    Value for the Character Map Select register
        /// </summary>
        byte CharacterMapSelect { get; }

        /// <summary>
        ///    Value for the Sequencer Memory Mode register
        /// </summary>
        SequencerMemoryModeFlags SequencerMemoryMode { get; }

        #endregion

        #region Attribute registers

        /// <summary>
        ///    Value for the Palette 0 register
        /// </summary>
        byte Palette0 { get; }

        /// <summary>
        ///    Value for the Palette 1 register
        /// </summary>
        byte Palette1 { get; }

        /// <summary>
        ///    Value for the Palette 2 register
        /// </summary>
        byte Palette2 { get; }

        /// <summary>
        ///    Value for the Palette 3 register
        /// </summary>
        byte Palette3 { get; }

        /// <summary>
        ///    Value for the Palette 4 register
        /// </summary>
        byte Palette4 { get; }

        /// <summary>
        ///    Value for the Palette 5 register
        /// </summary>
        byte Palette5 { get; }

        /// <summary>
        ///    Value for the Palette 6 register
        /// </summary>
        byte Palette6 { get; }

        /// <summary>
        ///    Value for the Palette 7 register
        /// </summary>
        byte Palette7 { get; }

        /// <summary>
        ///    Value for the Palette 8 register
        /// </summary>
        byte Palette8 { get; }

        /// <summary>
        ///    Value for the Palette 9 register
        /// </summary>
        byte Palette9 { get; }

        /// <summary>
        ///    Value for the Palette 10 register
        /// </summary>
        byte Palette10 { get; }

        /// <summary>
        ///    Value for the Palette 11 register
        /// </summary>
        byte Palette11 { get; }

        /// <summary>
        ///    Value for the Palette 12 register
        /// </summary>
        byte Palette12 { get; }

        /// <summary>
        ///    Value for the Palette 13 register
        /// </summary>
        byte Palette13 { get; }

        /// <summary>
        ///    Value for the Palette 14 register
        /// </summary>
        byte Palette14 { get; }

        /// <summary>
        ///    Value for the Palette 15 register
        /// </summary>
        byte Palette15 { get; }

        /// <summary>
        ///    Value for the Attribute Mode Control register
        /// </summary>
        byte AttributeModeControl { get; }

        /// <summary>
        ///    Value for the Overscan Colour register
        /// </summary>
        byte OverscanColour { get; }

        /// <summary>
        ///    Value for the Colour Plane Enable register
        /// </summary>
        byte ColourPlaneEnable { get; }

        /// <summary>
        ///    Value for the Horizontal Pixel Panning register
        /// </summary>
        byte HorizontalPixelPanning { get; }

        /// <summary>
        ///    Value for the Colour Select register
        /// </summary>
        byte ColourSelect { get; }

        #endregion

        #region CRTC registers

        /// <summary>
        ///    Value for the Horizontal Total register
        /// </summary>
        byte HorizontalTotal { get; }

        /// <summary>
        ///    Value for the End Horizontal Display register
        /// </summary>
        byte EndHorizontalDisplay { get; }

        /// <summary>
        ///    Value for the Start Horizontal Blanking register
        /// </summary>
        byte StartHorizontalBlanking { get; }

        /// <summary>
        ///    Value for the End Horizontal Blanking register
        /// </summary>
        byte EndHorizontalBlanking { get; }

        /// <summary>
        ///    Value for the Start Horizontal Retrace register
        /// </summary>
        byte StartHorizontalRetrace { get; }

        /// <summary>
        ///    Value for the End Horizontal Retrace register
        /// </summary>
        byte EndHorizontalRetrace { get; }

        /// <summary>
        ///    Value for the Vertical Total register
        /// </summary>
        byte VerticalTotal { get; }

        /// <summary>
        ///    Value for the Overflow register
        /// </summary>
        byte Overflow { get; }

        /// <summary>
        ///    Value for the Preset Row Scan register
        /// </summary>
        byte PresetRowScan { get; }

        /// <summary>
        ///    Value for the Maximum Scan Line register
        /// </summary>
        byte MaximumScanLine { get; }

        /// <summary>
        ///    Value for the Cursor Start register
        /// </summary>
        byte CursorStart { get; }

        /// <summary>
        ///    Value for the Cursor End register
        /// </summary>
        byte CursorEnd { get; }

        /// <summary>
        ///    Value for the Start Address High register
        /// </summary>
        byte StartAddressHigh { get; }

        /// <summary>
        ///    Value for the Start Address Low register
        /// </summary>
        byte StartAddressLow { get; }

        /// <summary>
        ///    Value for the Cursor Location High register
        /// </summary>
        byte CursorLocationHigh { get; }

        /// <summary>
        ///    Value for the Cursor Location Low register
        /// </summary>
        byte CursorLocationLow { get; }

        /// <summary>
        ///    Value for the Vertical Retrace Start register
        /// </summary>
        byte VerticalRetraceStart { get; }

        /// <summary>
        ///    Value for the Vertical Retrace End register
        /// </summary>
        byte VerticalRetraceEnd { get; }

        /// <summary>
        ///    Value for the Vertical Display End register
        /// </summary>
        byte VerticalDisplayEnd { get; }

        /// <summary>
        ///    Value for the Offset register
        /// </summary>
        byte Offset { get; }

        /// <summary>
        ///    Value for the Underline Location register
        /// </summary>
        byte UnderlineLocation { get; }

        /// <summary>
        ///    Value for the Start Vertical Blanking register
        /// </summary>
        byte StartVerticalBlanking { get; }

        /// <summary>
        ///    Value for the End Vertical Blanking register
        /// </summary>
        byte EndVerticalBlanking { get; }

        /// <summary>
        ///    Value for the CRTC Mode Control register
        /// </summary>
        byte CRTCModeControl { get; }

        /// <summary>
        ///    Value for the Line Compare register
        /// </summary>
        byte LineCompare { get; }

        #endregion

        #region Extended registers

        /// <summary>
        ///    Value for the Miscellaneous Output register
        /// </summary>
        byte MiscellaneousOutput { get; }

        #endregion

        #endregion
    }
}
