using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlingOops.MIPS.CI20
{
    public static unsafe class LED
    {
        public const uint GPIO_F_SET = 0xb0010544;
        public const uint GPIO_F_CLEAR = 0xb0010548;

        public const uint GPIO_F_LED_PIN = (1 << 15);

        public static void Red()
        {
            *(uint*)GPIO_F_SET = GPIO_F_LED_PIN;
        }
        public static void Blue()
        {
            *(uint*)GPIO_F_CLEAR = GPIO_F_LED_PIN;
        }
    }
}
