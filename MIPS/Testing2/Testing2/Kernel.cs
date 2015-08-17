using System;

namespace Testing2
{
    public static class Kernel
    {
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = "ASM\\Kernel")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MinValue)]
        public static void Boot()
        {
        }

        //static void method(int argument)
        //{
        //    if (argument != 123)
        //    {
        //        UART.Write("Argument bad");
        //    }
        //    else
        //    {
        //        UART.Write("Argument good");
        //    }
        //}

        [Drivers.Compiler.Attributes.MainMethod]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void Main()
        {
            LED.Blue();
            DelayLong();
            LED.Red();
            DelayLong();

            UART.Init();
            BasicTimer.Init();

            UART.Write("Hello, world!\r\n");

            UInt64 lastCount = BasicTimer.CounterValue;
            UART.Write("Waiting 1 second(s) [");
            for(int i = 0; i < 10; i++)
            {
                BasicTimer.Sleep(100000u);
                UART.Write(".");
            }
            UART.Write("]\n");

            // BEGIN - Test: Right logical shift
            UInt64 bitString = 576460752303423488;
            int dist = 10;
            bitString = bitString >> dist;

            if (bitString != 562949953421312)
            {
                UART.Write("Bad right LOGICAL shift");
            }
            else
            {
                UART.Write("Good right LOGICAL shift");
            }
            // END - Test: Right logical shift

            //0xffffffff = -1 (32-bit)
            //0x7fffffff = 2147483647 (largest +ve 32-bit)
            //0x80000000 = -2147483648 (largest -ve 32-bit)
            
            //0xffffffff = 4294967295 (64-bit)
            //0xffffffffffffffff = -1 (64-bit)
            //0x80000000 = 2147483648 (64-bit)
            //0x7fffffffffffffff = 9223372036854775807 (largest +ve 64-bit)
            //0x8000000000000000 = -9223372036854775808 (largest -ve 64-bit)

            //Int64 a = -9223372036854775808;
            //Int64 b = -a;

            //if (b != -9223372036854775808)
            //{
            //    UART.Write("Neg64 res NOT correct");
            //}
            //else
            //{
            //    UART.Write("Neg64 res is correct");
            //}

            //int a1 = 0;
            //int b1 = 1;
            //int c1 = 2;
            //int res = a1 + b1 + c1;

            //switch(res)
            //{
            //    case 0:
            //        UART.Write("Case 0");
            //        break;
            //    case 1:
            //        UART.Write("Case 1");
            //        break;
            //    case 2:
            //        UART.Write("Case 2");
            //        break;
            //    default:
            //        UART.Write("Case default");
            //        break;
            //}

            //int arg = 123;
            //method(arg);

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
            //for (int j = 0; j < 100; j++)
            //{
            //    ;
            //}
            BasicTimer.Sleep(10ul);
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
