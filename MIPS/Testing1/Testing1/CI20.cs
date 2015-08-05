using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing1
{
    public static unsafe class CI20
    {
        /* CPU and DDR will be configured to use the same clock, so CPU speed divided
         * by DDR speed must be an integer. */
        public const uint CPU_SPEED_HZ = 1200000000 /* 1.2 GHz */;
        public const uint DDR_SPEED_HZ = 400000000 /* 400 MHz */;

        /* Devices */
        public const uint CPM_BASE = 0xb0000000;
        public const uint INTC_BASE = 0xb0001000;
        public const uint GPIO_BASE = 0xb0010000;
        public const uint DDR_BASE = 0xb3010000;
        public const uint DDRP_BASE = 0xb3011000;
        public const uint MSC0_BASE = 0xb3450000;
        public const uint MSC1_BASE = 0xb3460000;
        public const uint MSC2_BASE = 0xb3470000;

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

        /* Affects CPCCR */
        public const uint CPCCR_SEL_SRC_APLL = (1 << 30);
        public const uint CPCCR_SEL_SRC_EXTCLK = (2u << 30);
        public const uint CPCCR_SEL_SRC_RTCLK = (3u << 30);
        public const uint CPCCR_SEL_CPLL_SCLK_A = (1 << 28);
        public const uint CPCCR_SEL_CPLL_MPLL = (2 << 28);
        public const uint CPCCR_SEL_CPLL_EPLL = (3 << 28);
        public const uint CPCCR_SEL_H0PLL_SCLK_A = (1 << 26);
        public const uint CPCCR_SEL_H0PLL_MPLL = (2 << 26);
        public const uint CPCCR_SEL_H0PLL_EPLL = (3 << 26);
        public const uint CPCCR_SEL_H2PLL_SCLK_A = (1 << 24);
        public const uint CPCCR_SEL_H2PLL_MPLL = (2 << 24);
        public const uint CPCCR_SEL_H2PLL_TCK = (3 << 24);
        public const uint CPCCR_CE_CPU = (1 << 22);
        public const uint CPCCR_CE_AHB0 = (1 << 21);
        public const uint CPCCR_CE_AHB2 = (1 << 20);
        public const uint CPCCR_PDIV_SHIFT = 16;
        public const uint CPCCR_H2DIV_SHIFT = 12;
        public const uint CPCCR_H0DIV_SHIFT = 8;
        public const uint CPCCR_L2CDIV_SHIFT = 4;
        public const uint CPCCR_CDIV_SHIFT = 0;
        public const uint CPCCR_CLOCK_SRC_MASK = 0xFF000000;

        /* Affects CPPCR */
        public const uint CPPCR_BWADJ_MASK = 0x000FFF00;
        public const uint CPPCR_BWADJ_SHIFT = 8;

        /* Affects CPCSR */
        public const uint CPCSR_H2DIV_BUSY = (1 << 2);
        public const uint CPCSR_H0DIV_BUSY = (1 << 1);
        public const uint CPCSR_CDIV_BUSY = (1 << 0);

        /* Affects CPAPCR, CPMPCR */
        public const uint PLL_M_SHIFT = 19;
        public const uint PLL_N_SHIFT = 13;
        public const uint PLL_OD_SHIFT = 9;
        public const uint PLL_MNOD_MASK = 0xFFFFFF00;
        public const uint PLL_ON_BIT = (1 << 4);
        public const uint PLL_ENABLE_BIT = (1 << 0);

        /* Affects CLKGR0 */
        public const uint CLKGR0_DDR1 = (1u << 31);
        public const uint CLKGR0_DDR0 = (1 << 30);

        /* Affects CLKGR1 */
        public const uint CLKGR1_UART4 = (1 << 10);

        /* INTC - interrupt controller */
        public const uint INTC_ICSR0 = (INTC_BASE + 0x0);
        public const uint INTC_ICMSR0 = (INTC_BASE + 0x8);
        public const uint INTC_ICMCR0 = (INTC_BASE + 0xC);
        public const uint INTC_ICPR0 = (INTC_BASE + 0x10);
        public const uint INTC_ICSR1 = (INTC_BASE + 0x20);
        public const uint INTC_ICPR1 = (INTC_BASE + 0x30);

        public const uint INTC_ICMR0_TCU0 = (1 << 27);
        public const uint INTC_ICMR0_TCU1 = (1 << 26);
        public const uint INTC_ICMR0_TCU2 = (1 << 25);

        /* Affects DDRCDR */
        public const uint DDRCDR_DCS_STOP = (0 << 30);
        public const uint DDRCDR_DCS_SCLK_A = (1 << 30);
        public const uint DDRCDR_DCS_MPLL = (2u << 30);
        public const uint DDRCDR_CE_DDR = (1 << 29);
        public const uint DDRCDR_DDR_BUSY = (1 << 28);
        public const uint DDRCDR_DDR_STOP = (1 << 27);
        public const uint DDRCDR_CDR_SHIFT = 0;

        /* DDR controller */
        public const uint DDR_DSTATUS = (DDR_BASE + 0x0);
        public const uint DDR_DCFG = (DDR_BASE + 0x4);
        public const uint DDR_DCTRL = (DDR_BASE + 0x8);
        public const uint DDR_DLMR = (DDR_BASE + 0xc);
        public const uint DDR_DREFCNT = (DDR_BASE + 0x18);
        public const uint DDR_DMMAP0 = (DDR_BASE + 0x24);
        public const uint DDR_DMMAP1 = (DDR_BASE + 0x28);
        public const uint DDR_DTIMING1 = (DDR_BASE + 0x60);
        public const uint DDR_DTIMING2 = (DDR_BASE + 0x64);
        public const uint DDR_DTIMING3 = (DDR_BASE + 0x68);
        public const uint DDR_DTIMING4 = (DDR_BASE + 0x6c);
        public const uint DDR_DTIMING5 = (DDR_BASE + 0x70);
        public const uint DDR_DTIMING6 = (DDR_BASE + 0x74);
        public const uint DDR_DREMAP1 = (DDR_BASE + 0x9c);
        public const uint DDR_DREMAP2 = (DDR_BASE + 0xa0);
        public const uint DDR_DREMAP3 = (DDR_BASE + 0xa4);
        public const uint DDR_DREMAP4 = (DDR_BASE + 0xa8);
        public const uint DDR_DREMAP5 = (DDR_BASE + 0xac);
        public const uint DDR_DDLP = (DDR_BASE + 0xbc);
        public const uint DDR_WCMDCTRL1 = (DDR_BASE + 0x100);
        public const uint DDR_RCMDCTRL0 = (DDR_BASE + 0x104);
        public const uint DDR_RCMDCTRL1 = (DDR_BASE + 0x108);
        public const uint DDR_WDATTHD0 = (DDR_BASE + 0x114);
        public const uint DDR_WDATTHD1 = (DDR_BASE + 0x118);
        public const uint DDR_IPORTPRI = (DDR_BASE + 0x128);

        /* DDR PHY */
        public const uint DDRP_PIR = (DDRP_BASE + 0x04);
        public const uint DDRP_PGCR = (DDRP_BASE + 0x08);
        public const uint DDRP_PGSR = (DDRP_BASE + 0x0c);
        public const uint DDRP_PTR0 = (DDRP_BASE + 0x18);
        public const uint DDRP_PTR1 = (DDRP_BASE + 0x1c);
        public const uint DDRP_PTR2 = (DDRP_BASE + 0x20);
        public const uint DDRP_ACIOCR = (DDRP_BASE + 0x24);
        public const uint DDRP_DXCCR = (DDRP_BASE + 0x28);
        public const uint DDRP_DSGCR = (DDRP_BASE + 0x2c);
        public const uint DDRP_DCR = (DDRP_BASE + 0x30);
        public const uint DDRP_DTPR0 = (DDRP_BASE + 0x34);
        public const uint DDRP_DTPR1 = (DDRP_BASE + 0x38);
        public const uint DDRP_DTPR2 = (DDRP_BASE + 0x3c);
        public const uint DDRP_MR0 = (DDRP_BASE + 0x40);
        public const uint DDRP_MR1 = (DDRP_BASE + 0x44);
        public const uint DDRP_MR2 = (DDRP_BASE + 0x48);
        public const uint DDRP_MR3 = (DDRP_BASE + 0x4c);
        public const uint DDRP_ODTCR = (DDRP_BASE + 0x50);
        public const uint DDRP_DTAR = (DDRP_BASE + 0x54);
        public const uint DDRP_DTDR0 = (DDRP_BASE + 0x58);
        public const uint DDRP_DTDR1 = (DDRP_BASE + 0x5c);
        public const uint DDRP_DCUAR = (DDRP_BASE + 0xc0);
        public const uint DDRP_DCUDR = (DDRP_BASE + 0xc4);
        public const uint DDRP_DCURR = (DDRP_BASE + 0xc8);
        public const uint DDRP_DCULR = (DDRP_BASE + 0xcc);
        public const uint DDRP_DCUGCR = (DDRP_BASE + 0xd0);
        public const uint DDRP_DCUTPR = (DDRP_BASE + 0xd4);
        public const uint DDRP_DCUSR0 = (DDRP_BASE + 0xd8);
        public const uint DDRP_DCUSR1 = (DDRP_BASE + 0xdc);
        public const uint DDRP_ZQXCR0 = (DDRP_BASE + 0x180);
        public const uint DDRP_ZQXCR1 = (DDRP_BASE + 0x184);
        public const uint DDRP_DXGCR0 = (DDRP_BASE + 0x1c0 + (0x40 * 0));
        public const uint DDRP_DXGCR1 = (DDRP_BASE + 0x1c0 + (0x40 * 1));
        public const uint DDRP_DXGCR2 = (DDRP_BASE + 0x1c0 + (0x40 * 2));
        public const uint DDRP_DXGCR3 = (DDRP_BASE + 0x1c0 + (0x40 * 3));
        public const uint DDRP_DXGCR4 = (DDRP_BASE + 0x1c0 + (0x40 * 4));
        public const uint DDRP_DXGCR5 = (DDRP_BASE + 0x1c0 + (0x40 * 5));
        public const uint DDRP_DXGCR6 = (DDRP_BASE + 0x1c0 + (0x40 * 6));
        public const uint DDRP_DXGCR7 = (DDRP_BASE + 0x1c0 + (0x40 * 7));

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

        public static uint* GPIO(uint bank, uint name)
        {
            return (uint*)(GPIO_BASE + bank + name);
        }
    }
}
