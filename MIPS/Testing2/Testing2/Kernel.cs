using System;

namespace Testing2
{
    public static class Kernel
    {
        static bool x = false;

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = "ASM\\Kernel")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MinValue)]
        public static void Boot()
        {
        }

        [Drivers.Compiler.Attributes.MainMethod]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void Main()
        {
            LED.Blue();
            DelayLong();
            LED.Red();
            DelayLong();

            UART.Init();
            //BasicTimer.Init();

            UART.Write("Hello, world!\r\n");

            while (true)
            {
                char c = UART.ReadChar();

                if (c == 'r' || c == 'R')
                {
                    LED.Red();
                }
                else if (c == 'b' || c == 'B')
                {
                    LED.Blue();
                }
                else if (c == 'p' || c == 'P')
                {
                    for (int i = 0; i < 10000 * 2; i++)
                    {
                        LED.Red();
                        DelayShort();
                        LED.Blue();
                        DelayShort();
                    }
                }
                else if (c == 'f' || c == 'F')
                {
                    int bProp = 0;
                    bool dir = false;
                    for (int dirSwitches = 0; dirSwitches < 2; dirSwitches++)
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            for (int x = 0; x < 1000; x++)
                            {
                                DelayShort();
                                if (dir)
                                {
                                    if (x < bProp)
                                    {
                                        LED.Blue();
                                    }
                                    else
                                    {
                                        LED.Red();
                                    }
                                }
                                else
                                {
                                    if (bProp < x)
                                    {
                                        LED.Blue();
                                    }
                                    else
                                    {
                                        LED.Red();
                                    }
                                }
                            }
                            bProp++;
                        }
                        dir = !dir;
                        bProp = 0;
                    }
                }
                else
                {
                    LED.Red();
                    DelayLong();
                    LED.Blue();
                    DelayLong();
                }

                UART.Write(c);
            }
        }
        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        private static void DelayLong()
        {
            for (int i = 0; i < 1000; i++)
            {
                DelayShort();
            }
        }
        private static void DelayShort()
        {
            for (int j = 0; j < 100; j++)
            {
                ;
            }
        }

        [Drivers.Compiler.Attributes.ThrowNullReferenceExceptionMethod]
        private static void ThrowNullReferenceException(uint eip)
        {
            UART.Write('N');
            UART.Write('u');
            UART.Write('l');
            UART.Write('l');
            UART.Write(' ');
            UART.Write('r');
            UART.Write('e');
            UART.Write('f');
            UART.Write('\r');
            UART.Write('\n');

            while (true)
            {
                LED.Red();
                LED.Blue();
            }
        }

        [Drivers.Compiler.Attributes.ThrowIndexOutOfRangeExceptionMethod]
        private static void ThrowIndexOutOfRangeException(uint eip)
        {
            UART.Write('I');
            UART.Write('n');
            UART.Write('d');
            UART.Write('e');
            UART.Write('x');
            UART.Write(' ');
            UART.Write('O');
            UART.Write('O');
            UART.Write('R');
            UART.Write('\r');
            UART.Write('\n');

            while (true)
            {
                LED.Red();
                LED.Blue();
            }
        }
    }
}
