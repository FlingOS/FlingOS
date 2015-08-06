using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing1
{
    public static class Timer
    {
        /* TCU, the Timer and Counter Unit peripheral */
        public const uint TCU_BASE = 0xb0002000;

        /* Individual registers */

        public const uint TSTR = (TCU_BASE + 0xF0);
        public const uint TSTSR = (TCU_BASE + 0xF4);
        public const uint TSTCR = (TCU_BASE + 0xF8);
        public const uint TSR = (TCU_BASE + 0x1C);
        public const uint TSSR = (TCU_BASE + 0x2C);
        public const uint TSCR = (TCU_BASE + 0x3C);
        public const uint TER = (TCU_BASE + 0x10);
        public const uint TESR = (TCU_BASE + 0x14);
        public const uint TECR = (TCU_BASE + 0x18);
        public const uint TFR = (TCU_BASE + 0x20);
        public const uint TFSR = (TCU_BASE + 0x24);
        public const uint TFCR = (TCU_BASE + 0x28);
        public const uint TMR = (TCU_BASE + 0x30);
        public const uint TMSR = (TCU_BASE + 0x34);
        public const uint TMCR = (TCU_BASE + 0x38);
        public const uint TDFR0 = (TCU_BASE + 0x40);
        public const uint TDHR0 = (TCU_BASE + 0x44);
        public const uint TCNT0 = (TCU_BASE + 0x48);
        public const uint TCSR0 = (TCU_BASE + 0x4C);
        public const uint TDFR1 = (TCU_BASE + 0x50);
        public const uint TDHR1 = (TCU_BASE + 0x54);
        public const uint TCNT1 = (TCU_BASE + 0x58);
        public const uint TCSR1 = (TCU_BASE + 0x5C);
        public const uint TDFR2 = (TCU_BASE + 0x60);
        public const uint TDHR2 = (TCU_BASE + 0x64);
        public const uint TCNT2 = (TCU_BASE + 0x68);
        public const uint TCSR2 = (TCU_BASE + 0x6C);
        public const uint TDFR3 = (TCU_BASE + 0x70);
        public const uint TDHR3 = (TCU_BASE + 0x74);
        public const uint TCNT3 = (TCU_BASE + 0x78);
        public const uint TCSR3 = (TCU_BASE + 0x7C);
        public const uint TDFR4 = (TCU_BASE + 0x80);
        public const uint TDHR4 = (TCU_BASE + 0x84);
        public const uint TCNT4 = (TCU_BASE + 0x88);
        public const uint TCSR4 = (TCU_BASE + 0x8C);
        public const uint TDFR5 = (TCU_BASE + 0x90);
        public const uint TDHR5 = (TCU_BASE + 0x94);
        public const uint TCNT5 = (TCU_BASE + 0x98);
        public const uint TCSR5 = (TCU_BASE + 0x9C);
        public const uint TDFR6 = (TCU_BASE + 0xA0);
        public const uint TDHR6 = (TCU_BASE + 0xA4);
        public const uint TCNT6 = (TCU_BASE + 0xA8);
        public const uint TCSR6 = (TCU_BASE + 0xAC);
        public const uint TDFR7 = (TCU_BASE + 0xB0);
        public const uint TDHR7 = (TCU_BASE + 0xB4);
        public const uint TCNT7 = (TCU_BASE + 0xB8);
        public const uint TCSR7 = (TCU_BASE + 0xBC);
        public const uint TCUMOD0 = (TCU_BASE + 0x00);
        public const uint TCUMOD3 = (TCU_BASE + 0x10);
        public const uint TCUMOD4 = (TCU_BASE + 0x20);
        public const uint TCUMOD5 = (TCU_BASE + 0x30);
        public const uint TFWD0 = (TCU_BASE + 0x04);
        public const uint TFWD3 = (TCU_BASE + 0x14);
        public const uint TFWD4 = (TCU_BASE + 0x24);
        public const uint TFWD5 = (TCU_BASE + 0x34);
        public const uint TFIFOSR0 = (TCU_BASE + 0x08);
        public const uint TFIFOSR3 = (TCU_BASE + 0x18);
        public const uint TFIFOSR4 = (TCU_BASE + 0x28);
        public const uint TFIFOSR5 = (TCU_BASE + 0x38);

        /* TCU OS Timer sub-unit */
        public const uint TCU_OSTCSR = (TCU_BASE + 0xec);
        public const uint TCU_OSTDR = (TCU_BASE + 0xe0);
        public const uint TCU_OSTCNTH = (TCU_BASE + 0xe8);
        public const uint TCU_OSTCNTL = (TCU_BASE + 0xe4);
        public const uint TCU_OSTCNTHBUF = (TCU_BASE + 0xfc);

        /* Timer control register */
        public const uint TCSR_BYPASS = (1 << 11);
        public const uint TCSR_CLRZ = (1 << 10);
        public const uint TCSR_SD = (1 << 9);
        public const uint TCSR_INITL = (1 << 8);
        public const uint TCSR_PWM_EN = (1 << 7);
        public const uint TCSR_PWM_IN_EN = (1 << 6);
        public const uint TCSR_PRESCALE_DIV1 = (0 << 3);
        public const uint TCSR_PRESCALE_DIV4 = (1 << 3);
        public const uint TCSR_PRESCALE_DIV16 = (2 << 3);
        public const uint TCSR_PRESCALE_DIV64 = (3 << 3);
        public const uint TCSR_PRESCALE_DIV256 = (4 << 3);
        public const uint TCSR_PRESCALE_DIV1024 = (5 << 3);
        public const uint TCSR_EXT_EN = (1 << 2);
        public const uint TCSR_RTC_EN = (1 << 1);
        public const uint TCSR_PCK_EN = (1 << 0);

        /* Timer enable register */
        public const uint TER_OSTEN = (1 << 15) /* OS Timer */;
        public const uint TER_TCEN7 = (1 << 7)  /* Timer 7 */;
        public const uint TER_TCEN6 = (1 << 6)  /* Timer 6 */;
        public const uint TER_TCEN5 = (1 << 5)  /* Timer 5 */;
        public const uint TER_TCEN4 = (1 << 4)  /* Timer 4 */;
        public const uint TER_TCEN3 = (1 << 3)  /* Timer 3 */;
        public const uint TER_TCEN2 = (1 << 2)  /* Timer 2 */;
        public const uint TER_TCEN1 = (1 << 1)  /* Timer 1 */;
        public const uint TER_TCEN0 = (1 << 0)  /* Timer 0 */;

        /* Timer mask register */
        public const uint TMR_OSTMASK = (1 << 15) /* OS timer */;

        /* Timer flag register */
        public const uint TFR_OSTFLAG = (1 << 15) /* OS timer */;

        /* OS Timer control register */
        public const uint OSTCSR_CNT_MD = (1 << 15) /* 0: reset to 0 at compare value; 1: ignore compare value */;
        public const uint OSTCSR_SD = (1 << 9)  /* 0: graceful shutdown; 1: abrupt shutdown */;
        public const uint OSTCSR_PRESCALE_1 = 0         /* Don't divide the clock */;
        public const uint OSTCSR_PRESCALE_4 = (1 << 3)  /* Prescale by 4 */;
        public const uint OSTCSR_PRESCALE_16 = (2 << 3)  /* Prescale by 16 */;
        public const uint OSTCSR_PRESCALE_64 = (3 << 3)  /* ... */;
        public const uint OSTCSR_PRESCALE_256 = (4 << 3);
        public const uint OSTCSR_PRESCALE_1024 = (5 << 3);
        public const uint OSTCSR_EXT_EN = (1 << 2)  /* Use EXTAL as clock source */;
        public const uint OSTCSR_RTC_EN = (1 << 1)  /* Use RTCCLK as clock source */;
        public const uint OSTCSR_PCK_EN = (1 << 0)  /* Use PCLK as clock source */;

        /* We prescale the OS timer by 16, which, with a 48MHz EXTCLK, means our OS
         * timer increments 3000000 times a second.
        */

        public const uint OS_TIMER_HZ = (CI20.ExternalClockRate / 16);

        /* Multiply a value in usec by this to get the number of ticks to wait for
         * the desired amount of usecs. For this to be at all meaningful, OS_TIMER_HZ
         * must be an integer multiple of 1000000. */
        public const uint OS_TIMER_USEC_DIV = (OS_TIMER_HZ / 1000000);
    }
}
