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

using System.Diagnostics.CodeAnalysis;
using Drivers.Compiler.Attributes;
using Kernel.IO;
using Kernel.Framework.Processes;
using FlagsAttribute = System.FlagsAttribute;
using String = Kernel.Framework.String;

namespace Kernel.Devices.Serial
{
    /// <summary>
    ///     Represents a serial port (e.g. a COM port).
    /// </summary>
    /// <remarks>
    ///     TODO: In a similar fashion to PS2, this driver should use system device claiming for
    ///           claiming COM ports 1 to 4 when required.
    /// </remarks>
    public sealed class Serial : Device
    {
        /// <summary>
        ///     The standard baud rates available for use.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum BaudRates : byte
        {
            _115200 = 0x1,
            _57600 = 0x2,
            _38400 = 0x3,
            _28800 = 0x4,
            _23040 = 0x5,
            _19200 = 0x6,
            _14400 = 0x8,
            _12800 = 0x9,
            _11520 = 0xA,
            _9600 = 0xC,
            _7680 = 0xF,
            _7200 = 0x10,
            _6400 = 0x12,
            _5760 = 0x14,
            _4800 = 0x18,
            _4608 = 0x19,
            _3840 = 0x1E,
            _3600 = 0x20,
            _3200 = 0x24,
            _2880 = 0x28,
            _2560 = 0x2D,
            _2400 = 0x30,
            _2304 = 0x32,
            _1920 = 0x3C,
            _1800 = 0x40,
            _1600 = 0x48,
            _1536 = 0x4B,
            _1440 = 0x50,
            _1280 = 0x5A,
            _1200 = 0x60,
            _1152 = 0x64,
            _960 = 0x78,
            _900 = 0x80,
            _800 = 0x90,
            _768 = 0x96,
            _720 = 0xA0,
            _640 = 0xB4,
            _600 = 0xC0,
            _576 = 0xC8,
            _512 = 0xE1,
            _480 = 0xF0
        }

        /// <summary>
        ///     The standard available (physical) COM ports.
        /// </summary>
        public enum COMPorts : ushort
        {
            COM1 = 0x3F8,
            COM2 = 0x2F8,
            COM3 = 0x3E8,
            COM4 = 0x2E8
        }

        /// <summary>
        ///     The number of bits per byte.
        /// </summary>
        public enum DataBits : byte
        {
            _5 = 0x0,
            _6 = 0x1,
            _7 = 0x2,
            _8 = 0x3
        }

        /// <summary>
        ///     The FIFO control flags used to determine the serial signalling type.
        /// </summary>
        [Flags]
        public enum FIFOControlFlags : byte
        {
            None = 0x0,
            Enable = 0x1,
            ClearReceive = 0x2,
            ClearTransmit = 0x4,
            DMAMode0 = 0x0,
            DMAMode1 = 0x8,
            Enable64ByteFIFO = 0x20,
            TriggerLevel_1Byte = 0x0,
            TriggerLevel_4Bytes = 0x40,
            TriggerLevel_8Byte = 0x80,
            TriggerLevel_14Byte = 0xC0
        }

        /// <summary>
        ///     Flags for enabling various available interrupts from the serial controller.
        /// </summary>
        [Flags]
        public enum Interrupts : byte
        {
            None = 0x0,
            DataAvailable = 0x1,
            TransmitterEmpty = 0x2,
            BreakOrError = 0x4,
            StatusChange = 0x8
        }

        /// <summary>
        ///     Flags for enabling various line control features.
        /// </summary>
        [Flags]
        public enum LineControlFlags : byte
        {
            None = 0x0,
            DivisorLatchAccessBit = 0x80
        }

        /// <summary>
        ///     Flags for determing the various line statuses.
        /// </summary>
        [Flags]
        public enum LineStatusFlags : byte
        {
            DataAvailable = 0x1,
            OverrunError = 0x2,
            ParityError = 0x4,
            FramingError = 0x8,
            BreakSignalReceived = 0x10,
            THREmpty = 0x20,
            THREmptyAndLineIdel = 0x40,
            ErronousDataInFIFO = 0x80
        }

        /// <summary>
        ///     Flags for setting the modem control mode.
        /// </summary>
        [Flags]
        public enum ModemControlFlags : byte
        {
            DTR = 0x1,
            RTS = 0x2,
            AutoFlowControl = 0x10
        }

        /// <summary>
        ///     The number and type of parity bit(s) to use.
        /// </summary>
        public enum ParityBits : byte
        {
            None = 0x00,
            Odd = 0x08,
            Even = 0x18,
            Mark = 0x28,
            Space = 0x38
        }

        /// <summary>
        ///     The number of stop bits to use.
        /// </summary>
        public enum StopBits : byte
        {
            One = 0x0,
            OneAndHalf_Or_Two = 0x2
        }

        /// <summary>
        ///     The default instance for accessing COM port 1. Limited to Kernel process only at the moment.
        /// </summary>
        [Group(Name = "IsolatedKernel_Hardware_Devices")] public static Serial COM1;

        /// <summary>
        ///     The default instance for accessing COM port 2.
        /// </summary>
        public static Serial COM2;
        /// <summary>
        ///     The default instance for accessing COM port 3.
        /// </summary>
        public static Serial COM3;
        private IOPort BaudLSB;
        private IOPort BaudMSB;
        private readonly BaudRates BaudRate;

        private IOPort Data;
        private readonly DataBits _dataBits;
        private IOPort FIFOControl;
        private FIFOControlFlags FIFOTriggerLevel = FIFOControlFlags.TriggerLevel_14Byte;
        private IOPort InterruptEnable;
        private readonly Interrupts _interrupts;
        private IOPort LineControl;
        private IOPort LineStatus;
        private IOPort ModemControl;
        private IOPort ModemStatus;
        private readonly ParityBits _parityBits;

        private readonly COMPorts Port;
        private IOPort Scratch;
        private readonly StopBits _stopBits;

        private bool TransmitReady
        {
            [NoGC] [NoDebug] get { return (LineStatus.Read_Byte() & (byte)LineStatusFlags.THREmpty) != 0; }
        }

        private bool ReceiveReady
        {
            [NoGC] [NoDebug] get { return (LineStatus.Read_Byte() & (byte)LineStatusFlags.DataAvailable) != 0; }
        }

        /// <summary>
        ///     Initialises a new serial port device with the specified information.
        /// </summary>
        /// <param name="Port">The COM port to access.</param>
        /// <param name="DataBits">The number of bits per byte to use.</param>
        /// <param name="ParityBits">The number of parity bits to use.</param>
        /// <param name="StopBits">The number of stop bits to use.</param>
        /// <param name="BaudRate">The baud rate to use.</param>
        /// <param name="Interrupts">Which interrupts to enable.</param>
        public Serial(COMPorts Port, DataBits DataBits, ParityBits ParityBits, StopBits StopBits, BaudRates BaudRate,
            Interrupts Interrupts)
        {
            this.Port = Port;
            this._dataBits = DataBits;
            this._parityBits = ParityBits;
            this._stopBits = StopBits;
            this.BaudRate = BaudRate;
            this._interrupts = Interrupts;

            Init();
        }

        private void Init()
        {
            Data = new IOPort((ushort)(Port + 0));
            InterruptEnable = new IOPort((ushort)(Port + 1));
            BaudLSB = new IOPort((ushort)(Port + 0));
            BaudMSB = new IOPort((ushort)(Port + 1));
            FIFOControl = new IOPort((ushort)(Port + 2));
            LineControl = new IOPort((ushort)(Port + 3));
            ModemControl = new IOPort((ushort)(Port + 4));
            LineStatus = new IOPort((ushort)(Port + 5));
            ModemStatus = new IOPort((ushort)(Port + 6));
            Scratch = new IOPort((ushort)(Port + 7));

            InterruptEnable.Write_Byte((byte)_interrupts);

            LineControl.Write_Byte((byte)LineControlFlags.DivisorLatchAccessBit);
            BaudLSB.Write_Byte((byte)((byte)BaudRate & 0x00FF));
            BaudMSB.Write_Byte((byte)((byte)BaudRate & 0xFF00));

            // This also clears the DLAB flag
            LineControl.Write_Byte((byte)((byte)_dataBits | (byte)_stopBits | (byte)_parityBits));
            FIFOControl.Write_Byte((byte)(FIFOControlFlags.Enable |
                                          FIFOControlFlags.ClearReceive | FIFOControlFlags.ClearTransmit |
                                          FIFOTriggerLevel));
            ModemControl.Write_Byte((byte)(ModemControlFlags.DTR | ModemControlFlags.RTS));
        }

        /// <summary>
        ///     Writes the specified byte to the serial connection. This is a blocking call.
        /// </summary>
        /// <param name="Value">The byte to write.</param>
        [NoGC]
        [NoDebug]
        public void Write(byte Value)
        {
            while (!TransmitReady)
            {
                SystemCalls.SleepThread(10);
                //Hardware.Devices.Timer.Default.Wait(10);
            }
            Data.Write_Byte(Value);
        }

        /// <summary>
        ///     Reads a byte from the serial connection. This is a blocking call.
        /// </summary>
        /// <returns></returns>
        [NoGC]
        [NoDebug]
        public byte Read()
        {
            while (!ReceiveReady)
            {
                SystemCalls.SleepThread(10);
                //Hardware.Devices.Timer.Default.Wait(10);
            }
            return Data.Read_Byte();
        }

        /// <summary>
        ///     Writes the specified string to the serial connection. This is a blocking call.
        /// </summary>
        /// <param name="Value">The string to write.</param>
        [NoGC]
        [NoDebug]
        public void Write(String Value)
        {
            for (int i = 0; i < Value.Length; i++)
            {
                Write((byte)Value[i]);
            }
        }

        /// <summary>
        ///     Initialises the default instance for COM port 1, if it has not already been initialised. This should only be called from the Kernel process at the moment.
        /// </summary>
        public static void InitCOM1()
        {
            if (COM1 == null)
            {
                COM1 = new Serial(COMPorts.COM1, DataBits._8, ParityBits.None, StopBits.One, BaudRates._115200,
                    Interrupts.None);
            }
        }

        /// <summary>
        ///     Initialises the default instance for COM port 2, if it has not already been initialised.
        /// </summary>
        public static void InitCOM2()
        {
            if (COM2 == null)
            {
                COM2 = new Serial(COMPorts.COM2, DataBits._8, ParityBits.None, StopBits.One, BaudRates._19200,
                    Interrupts.None);
            }
        }

        /// <summary>
        ///     Initialises the default instance for COM port 3, if it has not already been initialised.
        /// </summary>
        public static void InitCOM3()
        {
            if (COM3 == null)
            {
                COM3 = new Serial(COMPorts.COM3, DataBits._8, ParityBits.None, StopBits.One, BaudRates._19200,
                    Interrupts.None);
            }
        }
    }
}