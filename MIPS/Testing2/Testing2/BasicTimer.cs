using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing2
{
    public static unsafe class BasicTimer
    {
        public static UInt64 CounterValue
        {
            get
            {
                //return 0;
                return *(uint*)Timer.TCU_OSTCNTL | ((UInt64)(*(uint*)Timer.TCU_OSTCNTHBUF) << 32);
            }
        }

        public static void Init()
        {
            /* Timer keeps counting, ignoring comparison value */
            *(uint*)Timer.TCU_OSTCSR = Timer.OSTCSR_CNT_MD | Timer.OSTCSR_PRESCALE_16;

            /* Counter initial value. */
            *(uint*)Timer.TCU_OSTCNTH = 0;
            *(uint*)Timer.TCU_OSTCNTL = 0;

            /* Use EXTCLK as the clock source */
            *(uint*)Timer.TCU_OSTCSR |= Timer.OSTCSR_EXT_EN;

            /* Enable the timer*/
            *(uint*)Timer.TESR = Timer.TER_OSTEN;
        }

        public static void Sleep(UInt64 usec)
        {
            /* Doesn't handle the wrap case, which, with 64-bit values and 3 million
             * ticks per second, may crop up once every 194980 years. */
            UInt64 target = CounterValue + (usec * Timer.OS_TIMER_USEC_DIV);

            while (CounterValue < target)
                ;
        }
    }
}
