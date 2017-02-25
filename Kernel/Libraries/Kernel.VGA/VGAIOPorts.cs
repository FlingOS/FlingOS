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
using Kernel.Devices;

namespace Kernel.VGA
{
    /// <summary>
    ///     The IO ports used by VGA
    /// </summary>
    /// <remarks>
    ///     Some addresses a used twice because two different registers 
    ///     (one read-only, one write-only) were put at the same address
    ///     in the original EGA standard.
    /// </remarks>
    internal sealed class VGAIOPorts : Object
    {
        public readonly IOPort AttributeControllerWriteIndexData = new IOPort(0x3C0);
        public readonly IOPort AttributeControllerReadData = new IOPort(0x3C1);

        public readonly IOPort MiscellaneousOutputWrite = new IOPort(0x3C2);
        public readonly IOPort MiscellaneousOutputRead = new IOPort(0x3CC);

        public readonly IOPort SequencerIndex = new IOPort(0x3C4);
        public readonly IOPort SequencerData = new IOPort(0x3C5);
        public readonly IOPort GraphicsControllerIndex = new IOPort(0x3CE);
        public readonly IOPort GraphicsControllerData = new IOPort(0x3CF);

        public readonly IOPort DACMask = new IOPort(0x3C6); // Should normally be set to 0xFF
        public readonly IOPort DACIndexForReadData = new IOPort(0x3C7);
        public readonly IOPort DACIndexForWriteData = new IOPort(0x3C8);
        public readonly IOPort DACData = new IOPort(0x3C9);
        public readonly IOPort DACState;

        public readonly IOPort FeatureControlRead = new IOPort(0x3CA);
        public readonly IOPort FeatureControlWriteColour = new IOPort(0x3DA);
        public readonly IOPort FeatureControlWriteMono = new IOPort(0x3BA);

        public readonly IOPort CRTControllerIndexColour = new IOPort(0x3D4); // If Bit 0 of MiscellaneousOutput set, this is 0x3B4, [Registers 0 to 7] protected by [Bit 7 of Reg 17 (0x11)]
        public readonly IOPort CRTControllerIndexMono = new IOPort(0x3B4);
        public readonly IOPort CRTControllerDataColour = new IOPort(0x3D5);
        public readonly IOPort CRTControllerDataMono = new IOPort(0x3B5);

        public readonly IOPort InputStatus0;
        public readonly IOPort InputStatus1Colour; // If Bit 0 (Mono mode) of MiscellaneousOutput set, this is 0x3BA
        public readonly IOPort InputStatus1Mono;

        public VGAIOPorts()
        {
            DACState = DACIndexForReadData;
            InputStatus0 = MiscellaneousOutputWrite;
            InputStatus1Colour = FeatureControlWriteColour;
            InputStatus1Mono = FeatureControlWriteMono;
        }
    }
}
