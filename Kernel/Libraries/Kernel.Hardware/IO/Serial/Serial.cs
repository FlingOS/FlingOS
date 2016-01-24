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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.FOS_System.Processes;

namespace Kernel.Hardware.IO.Serial
{
    public class Serial : Device
    {
        public enum COMPorts : ushort
        {
            COM1 = 0x3F8,
            COM2 = 0x2F8,
            COM3 = 0x3E8,
            COM4 = 0x2E8
        }
        public enum ParityBits : byte
        {
            None = 0x00,
            Odd = 0x08,
            Even = 0x18,
            Mark = 0x28,
            Space = 0x38
        }
        [Flags]
        public enum Interrupts : byte
        {
            None = 0x0,
            DataAvailable = 0x1,
            TransmitterEmpty = 0x2,
            BreakOrError = 0x4,
            StatusChange = 0x8
        }

        [Flags]
        public enum LineControlFlags : byte
        {
            None = 0x0,
            DivisorLatchAccessBit = 0x80
        }

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

        public enum DataBits : byte
        {
            _5 = 0x0,
            _6 = 0x1,
            _7 = 0x2,
            _8 = 0x3
        }
        public enum StopBits : byte
        {
            One = 0x0,
            OneAndHalf_Or_Two = 0x2
        }

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

        [Flags]
        public enum ModemControlFlags : byte
        {
            DTR = 0x1,
            RTS = 0x2,
            AutoFlowControl = 0x10
        }

        protected COMPorts port;
        protected DataBits dataBits;
        protected ParityBits parityBits;
        protected StopBits stopBits;
        protected BaudRates baudRate;
        protected Interrupts interrupts;
        protected FIFOControlFlags FIFOTriggerLevel = FIFOControlFlags.TriggerLevel_14Byte;

        protected IOPort Data;
        protected IOPort InterruptEnable;
        protected IOPort BaudLSB;
        protected IOPort BaudMSB;
        protected IOPort FIFOControl;
        protected IOPort LineControl;
        protected IOPort ModemControl;
        protected IOPort LineStatus;
        protected IOPort ModemStatus;
        protected IOPort Scratch;

        protected bool TransmitReady
        {
            [Drivers.Compiler.Attributes.NoGC]
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return (LineStatus.Read_Byte() & (byte)LineStatusFlags.THREmpty) != 0;
            }
        }
        protected bool ReceiveReady
        {
            [Drivers.Compiler.Attributes.NoGC]
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return (LineStatus.Read_Byte() & (byte)LineStatusFlags.DataAvailable) != 0;
            }
        }

        public Serial(COMPorts port, DataBits dataBits, ParityBits parityBits, StopBits stopBits, BaudRates baudRate, Interrupts interrupts)
        {
            this.port = port;
            this.dataBits = dataBits;
            this.parityBits = parityBits;
            this.stopBits = stopBits;
            this.baudRate = baudRate;
            this.interrupts = interrupts;

            Init();
        }

        protected void Init()
        {
            Data = new IOPort((ushort)(port + 0));
            InterruptEnable = new IOPort((ushort)(port + 1));
            BaudLSB = new IOPort((ushort)(port + 0));
            BaudMSB = new IOPort((ushort)(port + 1));
            FIFOControl = new IOPort((ushort)(port + 2));
            LineControl = new IOPort((ushort)(port + 3));
            ModemControl = new IOPort((ushort)(port + 4));
            LineStatus = new IOPort((ushort)(port + 5));
            ModemStatus = new IOPort((ushort)(port + 6));
            Scratch = new IOPort((ushort)(port + 7));

            InterruptEnable.Write_Byte((byte)interrupts);
            
            LineControl.Write_Byte((byte)LineControlFlags.DivisorLatchAccessBit);
            BaudLSB.Write_Byte((byte)((byte)baudRate & 0x00FF));
            BaudMSB.Write_Byte((byte)((byte)baudRate & 0xFF00));
            
            // This also clears the DLAB flag
            LineControl.Write_Byte((byte)((byte)dataBits | (byte)stopBits | (byte)parityBits));
            FIFOControl.Write_Byte((byte)(FIFOControlFlags.Enable | 
                                          FIFOControlFlags.ClearReceive | FIFOControlFlags.ClearTransmit |
                                          FIFOTriggerLevel));
            ModemControl.Write_Byte((byte)(ModemControlFlags.DTR | ModemControlFlags.RTS));
        }

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public void Write(byte val)
        {
            while (!TransmitReady)
            {
                SystemCalls.SleepThread(10);
                //Hardware.Devices.Timer.Default.Wait(10);
            }
            Data.Write_Byte(val);
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public byte Read()
        {
            while (!ReceiveReady)
            {
                SystemCalls.SleepThread(10);
                //Hardware.Devices.Timer.Default.Wait(10);
            }
            return Data.Read_Byte();
        }

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public void Write(FOS_System.String str)
        {
            for (int i = 0; i < str.length; i++)
            {
                Write((byte)str[i]);
            }
        }

        public static Serial COM1;
        public static void InitCOM1()
        {
            if (COM1 == null)
            {
                COM1 = new Serial(COMPorts.COM1, DataBits._8, ParityBits.None, StopBits.One, BaudRates._115200, Interrupts.None);
            }
        }
        public static Serial COM2;
        public static void InitCOM2()
        {
            if (COM2 == null)
            {
                COM2 = new Serial(COMPorts.COM2, DataBits._8, ParityBits.None, StopBits.One, BaudRates._19200, Interrupts.None);
            }
        }
        public static Serial COM3;
        public static void InitCOM3()
        {
            if (COM3 == null)
            {
                COM3 = new Serial(COMPorts.COM3, DataBits._8, ParityBits.None, StopBits.One, BaudRates._19200, Interrupts.None);
            }
        }
    }
}
