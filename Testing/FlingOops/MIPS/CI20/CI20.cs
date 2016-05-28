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
    public static unsafe class CI20
    {
        public const uint ExternalClockRate = 48000000;

        /* CPU and DDR will be configured to use the same clock, so CPU speed divided
         * by DDR speed must be an integer. */
        public const uint CPU_SPEED_HZ = 1200000000 /* 1.2 GHz */;
        public const uint DDR_SPEED_HZ = 400000000 /* 400 MHz */;

        /* Devices */
        public const uint CPM_BASE = 0xb0000000;
        public const uint GPIO_BASE = 0xb0010000;
        
        /* Clock Reset and Power Controller */
        public const uint CPM_CPCCR = (CPM_BASE + 0x0);
        public const uint CPM_CPPCR = (CPM_BASE + 0xC);
        public const uint CPM_CPCSR = (CPM_BASE + 0xD4);
        public const uint CPM_CPAPCR = (CPM_BASE + 0x10);
        public const uint CPM_CPMPCR = (CPM_BASE + 0x14);
        public const uint CPM_CLKGR0 = (CPM_BASE + 0x20);
        public const uint CPM_CLKGR1 = (CPM_BASE + 0x28);
        public const uint CPM_DDRCDR = (CPM_BASE + 0x2C);
        public const uint CPM_DRCG = (CPM_BASE + 0xD0);

        /* Affects CLKGR0 */
        public const uint CLKGR0_DDR1 = (1u << 31);
        public const uint CLKGR0_DDR0 = (1 << 30);

        /* Affects CLKGR1 */
        public const uint CLKGR1_UART4 = (1 << 10);

        /* GPIOs */
        public const uint GPIO_BANK_A = 0x0;
        public const uint GPIO_BANK_B = 0x100;
        public const uint GPIO_BANK_C = 0x200;
        public const uint GPIO_BANK_D = 0x300;
        public const uint GPIO_BANK_E = 0x400;
        public const uint GPIO_BANK_F = 0x500;

        public const uint GPIO_INTC = 0x18;
        public const uint GPIO_MSKS = 0x24;
        public const uint GPIO_MSKC = 0x28;
        public const uint GPIO_PAT1S = 0x34;
        public const uint GPIO_PAT1C = 0x38;
        public const uint GPIO_PAT0S = 0x44;
        public const uint GPIO_PAT0C = 0x48;
        public const uint GPIO_PEC = 0x78;

        /* Pin names: GPIO bank C, mode 2 */
        public const uint PIN_UART4_TXD = (1 << 10);
        public const uint PIN_UART4_RXD = (1 << 20);

        /* Pin names: GPIO bank F */
        public const uint PIN_LED_PIN = (1 << 15);

        public static byte* GPIO(uint bank, uint name)
        {
            return (byte*)(GPIO_BASE + bank + name);
        }
    }
}
#endif