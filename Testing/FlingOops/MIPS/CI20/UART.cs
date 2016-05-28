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


#if MIPS
using System;

namespace FlingOops.MIPS.CI20
{
    public static unsafe class UART
    {
        public const uint UART_BASE = 0xb0030000;

        /* Uart registers */
        public const uint URBR = 0x0;
        public const uint UTHR = 0x0;
        public const uint UDLLR = 0x0;
        public const uint UDLHR = 0x4;
        public const uint UIER = 0x4;
        public const uint UIIR = 0x8;
        public const uint UFCR = 0x8;
        public const uint ULCR = 0x0c;
        public const uint UMCR = 0x10;
        public const uint ULSR = 0x14;
        public const uint UMSR = 0x18;
        public const uint USPR = 0x1c;
        public const uint ISR = 0x20;
        public const uint UMR = 0x24;
        public const uint UACR = 0x28;
        public const uint URCR = 0x40;
        public const uint UTCR = 0x44;

        /* Fields within registers */
        public const uint ULCR_DLAB = (1 << 7) /* Divisor latch access */;
        public const uint ULCR_SBK = (1 << 6) /* Set break */;
        public const uint ULCR_STPAR = (1 << 5) /* Stick parity*/;
        public const uint ULCR_PARM = (1 << 4) /* 1 = even parity, 0 = odd parity */;
        public const uint ULCR_PARE = (1 << 3) /* Parity enable */;
        public const uint ULCR_SBLS = (1 << 2) /* 0 = 1 stop bit, 1 = 2 stop bits */;
        public const uint ULCR_WLS_5 = (0) /* 5-bit words */;
        public const uint ULCR_WLS_6 = (1) /* 6-bit words */;
        public const uint ULCR_WLS_7 = (2) /* 7-bit words */;
        public const uint ULCR_WLS_8 = (3) /* 8-bit words */;

        public const uint UMCR_MDCE = (1 << 7) /* Modem control enable */;
        public const uint UMCR_FCM = (1 << 6) /* 1 = hardware, 0 = software */;
        public const uint UMCR_LOOP = (1 << 4) /* Loopback testing mode */;
        public const uint UMCR_RTS = (1 << 1) /* RTS */;

        public const uint UFCR_RTDR_1 = (0 << 6);
        public const uint UFCR_RTDR_16 = (1 << 6);
        public const uint UFCR_RTDR_32 = (2 << 6);
        public const uint UFCR_RTDR_60 = (3 << 6);
        public const uint UFCR_UME = (1 << 4) /* Enable the UART */;
        public const uint UFCR_DMR = (1 << 3) /* DMA enable */;
        public const uint UFCR_TFRT = (1 << 2) /* Transmit holding register reset */;
        public const uint UFCR_RFRT = (1 << 1) /* Receive buffer reset */;
        public const uint UFCR_FME = (1 << 0) /* FIFO mode enable */;

        public const uint ULSR_FIFOE = (1 << 7) /* FIFO Error Status. (FIFO mode only) */;
        public const uint ULSR_TEMP = (1 << 6) /* Transmit Holding Register Empty. */;
        public const uint ULSR_TDRQ = (1 << 5) /* Transmit Data Request. */;
        public const uint ULSR_BI = (1 << 4) /* Break Interrupt. */;
        public const uint ULSR_FMER = (1 << 3) /* Framing Error. */;
        public const uint ULSR_PARER = (1 << 2) /* Parity Error. */;
        public const uint ULSR_OVER = (1 << 1) /* Overrun Error. */;
        public const uint ULSR_DRY = (1 << 0) /* Data Ready. */;

        private const uint ExternalClockDivisor = CI20.ExternalClockRate / 16 / 115200;
        private const uint BitsPerSecond = 115200;
        
        private static uint UART_Num = 4;
        
        private static void InitBoard()
        {
            /* Enable UART 4 GPIOs */
            *(uint*)CI20.GPIO(CI20.GPIO_BANK_C, CI20.GPIO_INTC) = CI20.PIN_UART4_TXD | CI20.PIN_UART4_RXD;
            *(uint*)CI20.GPIO(CI20.GPIO_BANK_C, CI20.GPIO_MSKC) = CI20.PIN_UART4_TXD | CI20.PIN_UART4_RXD;
            *(uint*)CI20.GPIO(CI20.GPIO_BANK_C, CI20.GPIO_PAT1S) = CI20.PIN_UART4_TXD | CI20.PIN_UART4_RXD;
            *(uint*)CI20.GPIO(CI20.GPIO_BANK_C, CI20.GPIO_PAT0C) = CI20.PIN_UART4_TXD | CI20.PIN_UART4_RXD;

            /* Enable UART4 clock. UARTs are clocked from EXTCLK: no PLL required. */
            *(uint*)CI20.CPM_CLKGR1 = (*(uint*)CI20.CPM_CLKGR1) & ~CI20.CLKGR1_UART4;
        }
        public static void Init()
        {
            InitBoard();

            /* Disable UART4 interrupts */
            WriteRegister(UIER, 0);

            /* Modem control: RTS */
            WriteRegister(UMCR, UMCR_RTS);

            /* Enable FIFO and reset rx and tx, and enable the module (this last is
             * jz47xx-specific). */
            WriteRegister(UFCR, UFCR_FME | UFCR_TFRT | UFCR_RFRT | UFCR_UME);

            /* Enable banking and set the baud rate */
            WriteRegister(ULCR, ULCR_DLAB | ULCR_WLS_8);
            WriteRegister(UDLLR, ExternalClockDivisor & 0xff);
            WriteRegister(UDLHR, (ExternalClockDivisor & 0xff00) >> 8);

            /* Disable banking again */
            WriteRegister(ULCR, ULCR_WLS_8); 
        }

        [Drivers.Compiler.Attributes.NoGC]
        public static void Write(FlingOops.String str)
        {
            for (int i = 0; i < str.length; i++)
            {
                Write(str[i]);
            }
        }
        public static void Write(char c)
        {
            while ((ReadRegister(ULSR) & ULSR_TDRQ) == 0) /* Transmit-hold register empty */
                ;

            WriteRegister(UTHR, c);
        }

        public static char ReadChar()
        {
            while ((ReadRegister(ULSR) & ULSR_DRY) == 0) /* Data-ready register not set */
                 ;

            return (char)(ReadRegister(URBR) & 0xFF);
        }

        private static byte* Registers(uint uartNum)
        {
            return (byte*)(UART_BASE + (uartNum * 0x1000));
        }
        private static void WriteRegister(uint addr, uint val)
        {
            *(uint*)(Registers(UART_Num) + addr) = val;
        }
        private static uint ReadRegister(uint addr)
        {
            return *(uint*)(Registers(UART_Num) + addr);
        }

        public static void SetColour_Red()
        {
            Write((char)27);
            Write("[0;31m");
        }
        public static void SetColour_Yellow()
        {
            Write((char)27);
            Write("[1;33m");
        }
        public static void SetColour_Green()
        {
            Write((char)27);
            Write("[0;32m");
        }
        public static void SetColour_White()
        {
            Write((char)27);
            Write("[1;37m");
        }
        public static void SetColour_Black()
        {
            Write((char)27);
            Write("[0;30m");
        }
    }
}
#endif