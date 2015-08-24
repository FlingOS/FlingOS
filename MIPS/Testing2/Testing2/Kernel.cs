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

            //BEGIN - Test: Div
            UInt32 a1 = 9;
            UInt32 b1 = 3;
            a1 = a1 / b1;

            if (a1 != 3)
            {
                UART.Write("Bad unsigned div no rem\n");
            }
            else
            {
                UART.Write("Good unsigned div no rem\n");
            }

            UInt32 a2 = 10;
            UInt32 b2 = 3;
            a2 = a2 / b2;

            if (a2 != 3)
            {
                UART.Write("Bad unsigned div with rem\n");
            }
            else
            {
                UART.Write("Good unsigned div with rem\n");
            }

            Int32 a3 = -9;
            Int32 b3 = 3;
            a3 = a3 / b3;

            if (a3 != -3)
            {
                UART.Write("Bad signed div - + no rem\n");
            }
            else
            {
                UART.Write("Good signed div - + no rem\n");
            }

            Int32 a4 = 9;
            Int32 b4 = -3;
            a4 = a4 / b4;

            if (a4 != -3)
            {
                UART.Write("Bad signed div + - no rem\n");
            }
            else
            {
                UART.Write("Good signed div + - no rem\n");
            }

            Int32 a5 = -9;
            Int32 b5 = -3;
            a5 = a5 / b5;

            if (a5 != 3)
            {
                UART.Write("Bad signed div - - no rem\n");
            }
            else
            {
                UART.Write("Good signed div - - no rem\n");
            }

            Int32 a6 = 9;
            Int32 b6 = 3;
            a6 = a6 / b6;

            if (a5 != 3)
            {
                UART.Write("Bad signed div + + no rem\n");
            }
            else
            {
                UART.Write("Good signed div + + no rem\n");
            }
            //--

            Int32 a7 = -10;
            Int32 b7 = 3;
            a7 = a7 / b7;

            if (a7 != -3)
            {
                UART.Write("Bad signed div - + with rem\n");
            }
            else
            {
                UART.Write("Good signed div - + with rem\n");
            }

            Int32 a8 = 10;
            Int32 b8 = -3;
            a8 = a8 / b8;

            if (a8 != -3)
            {
                UART.Write("Bad signed div + - with rem\n");
            }
            else
            {
                UART.Write("Good signed div + - with rem\n");
            }

            Int32 a9 = -10;
            Int32 b9 = -3;
            a9 = a9 / b9;

            if (a9 != 3)
            {
                UART.Write("Bad signed div - - with rem\n");
            }
            else
            {
                UART.Write("Good signed div - - with rem\n");
            }

            Int32 a10 = 10;
            Int32 b10 = 3;
            a10 = a10 / b10;

            if (a10 != 3)
            {
                UART.Write("Bad signed div + + with rem\n");
            }
            else
            {
                UART.Write("Good signed div + + with rem\n");
            }
            //END - Test: Div

            ////BEGIN - Test: Sub
            //UInt32 a1 = 9;
            //UInt32 b1 = 4;
            //a1 = a1 - b1;

            //if (a1 != 5)
            //{
            //    UART.Write("Bad unsigned 32-bit - 32-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good unsigned 32-bit - 32-bit\n");
            //}

            //Int32 a2 = -9;
            //Int32 b2 = 4;
            //a2 = a2 - b2;

            //if (a2 != -13)
            //{
            //    UART.Write("Bad signed (-)32-bit - (+)32-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (-)32-bit - (+)32-bit\n");
            //}

            //Int32 a3 = -9;
            //Int32 b3 = -4;
            //a3 = a3 - b3;

            //if (a3 != -5)
            //{
            //    UART.Write("Bad signed (-)32-bit - (-)32-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (-)32-bit - (-)32-bit\n");
            //}

            //Int32 a10 = 9;
            //Int32 b10 = -4;
            //a10 = a10 - b10;

            //if (a10 != 13)
            //{
            //    UART.Write("Bad signed (+)32-bit - (-)32-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (+)32-bit - (-)32-bit\n");
            //}

            //Int32 a11 = 9;
            //Int32 b11 = 4;
            //a11 = a11 - b11;

            //if (a11 != 5)
            //{
            //    UART.Write("Bad signed (+)32-bit - (+)32-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (+)32-bit - (+)32-bit\n");
            //}

            ////---
            //Int64 a4 = 1080863910568919040;
            //Int32 b4 = 4;
            //a4 = a4 - b4;

            //if (a4 != 1080863910568919036)
            //{
            //    UART.Write("***Bad unsigned 64-bit - 32-bit\n");
            //}
            //else
            //{
            //    UART.Write("***Good unsigned 64-bit - 32-bit\n");
            //}

            //Int64 a12 = 0;
            //Int32 b12 = 4;
            //a12 = a12 - b12;

            //if (a12 != -4)
            //{
            //    UART.Write("***Bad unsigned zero 64-bit - 32-bit\n");
            //}
            //else
            //{
            //    UART.Write("***Good unsigned zero 64-bit - 32-bit\n");
            //}

            //Int64 a13 = 1080863910568919040;
            //Int64 b13 = 4;
            //a13 = a13 - b13;

            //if (a13 != 1080863910568919036)
            //{
            //    UART.Write("***Bad unsigned 64-bit - 64-bit\n");
            //}
            //else
            //{
            //    UART.Write("***Good unsigned 64-bit - 64-bit\n");
            //}

            //Int64 a14 = 0;
            //Int64 b14 = 4;
            //a14 = a14 - b14;

            //if (a14 != -4)
            //{
            //    UART.Write("***Bad unsigned zero 64-bit - 64-bit\n");
            //}
            //else
            //{
            //    UART.Write("***Good unsigned zero 64-bit - 64-bit\n");
            //}

            ////---

            //UInt64 a5 = 1080863910568919040;
            //UInt64 b5 = 844424930131968;
            //a5 = a5 - b5;

            //if (a5 != 1080019485638787072)
            //{
            //    UART.Write("Bad unsigned 64-bit - 64-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good unsigned 64-bit - 64-bit\n");
            //}

            //Int64 a6 = -1080863910568919040;
            //Int64 b6 = 844424930131968;
            //a6 = a6 - b6;

            //if (a6 != -1081708335499051008)
            //{
            //    UART.Write("Bad signed (-)64-bit - (+)64-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (-)64-bit - (+)64-bit\n");
            //}

            //Int64 a7 = -1080863910568919040;
            //Int64 b7 = -844424930131968;
            //a7 = a7 - b7;

            //if (a7 != -1080019485638787072)
            //{
            //    UART.Write("Bad signed (-)64-bit - (-)64-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (-)64-bit - (-)64-bit\n");
            //}

            //Int64 a8 = 1080863910568919040;
            //Int64 b8 = -844424930131968;
            //a8 = a8 - b8;

            //if (a8 != 1081708335499051008)
            //{
            //    UART.Write("Bad signed (+)64-bit - (-)64-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (+)64-bit - (-)64-bit\n");
            //}

            //Int64 a9 = 1080863910568919040;
            //Int64 b9 = 844424930131968;
            //a9 = a9 - b9;

            //if (a9 != 1080019485638787072)
            //{
            //    UART.Write("Bad signed (+)64-bit - (+)64-bit\n");
            //}
            //else
            //{
            //    UART.Write("Good signed (+)64-bit - (+)64-bit\n");
            //}

            // END - Test: Sub

            // BEGIN - Test: Right logical shift

            //UInt64 bitString = 576460752303423488;
            //int dist = 10;
            //bitString = bitString >> dist;

            //if (bitString != 562949953421312)
            //{
            //    UART.Write("Bad right LOGICAL shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right LOGICAL shift\n");
            //}

            ////Signed 4-4
            //Int32 s44String = -28416;    
            //Int32 s44Dist = 6;         
            //s44String = s44String >> s44Dist;

            //if (s44String != -444)      
            //{
            //    UART.Write("Bad right s44 shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right s44 shift\n");
            //}

            ////Unsigned 4-4
            //UInt32 u44String = 4352;
            //Int32 u44Dist = 6;
            //u44String = u44String >> u44Dist;

            //if (u44String != 68)
            //{
            //    UART.Write("Bad right u44 shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right u44 shift\n");
            //}

            ////Signed 8-4 $t2<32
            //Int64 s84StringL = -9185091440022126524;
            //Int32 s84DistL = 6;
            //s84StringL = s84StringL >> s84DistL;

            //if (s84StringL != -143517053750345727)
            //{
            //    UART.Write("Bad right s84L shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right s84L shift\n");
            //}

            ////Signed 8-4 $t2>=32
            //Int64 s84StringG = -9187343239835811840;
            //Int32 s84DistG = 40;
            //s84StringG = s84StringG >> s84DistG;

            //if (s84StringG != -8355840)
            //{
            //    UART.Write("Bad right s84G shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right s84G shift\n");
            //}

            ////Signed 8-4 $t2>=32 right shift max +ve 
            //Int64 s84StringP = 9223372036854775807;
            //Int32 s84DistP = 40;
            //s84StringP = s84StringP >> s84DistP;

            //if (s84StringP != 8388607)
            //{
            //    UART.Write("Bad right s84G shift max +ve\n");
            //}
            //else
            //{
            //    UART.Write("Good right s84G shift max +ve\n");
            //}

            ////Signed 8-4 $t2>=32 right shift max -ve 
            //Int64 s84StringN = -9223372036854775808;
            //Int32 s84DistN = 40;
            //s84StringN = s84StringN >> s84DistN;

            //if (s84StringN != -8388608)
            //{
            //    UART.Write("Bad right s84G shift max -ve\n");
            //}
            //else
            //{
            //    UART.Write("Good right s84G shift max -ve\n");
            //}

            ////Signed 63-bit right shift of all 1s 
            //Int64 sNStringN = -1;
            //Int32 sNDistN = 63;     
            //sNStringN = sNStringN >> sNDistN;

            //if (sNStringN != -1)
            //{
            //    UART.Write("Bad 63-bit right shift\n");
            //}
            //else
            //{
            //    UART.Write("Good 63-bit right shift\n");
            //}

            ////Unsigned 63-bit right shift of all 1s
            //UInt64 uNStringN = 18446744073709551615;
            //Int32 uNDistN = 63;     
            //uNStringN = uNStringN >> uNDistN;

            //if (uNStringN != 1)
            //{
            //    UART.Write("Bad 63-bit right shift\n");
            //}
            //else
            //{
            //    UART.Write("Good 63-bit right shift\n");
            //}

            ////Signed 8-8 $t2<32
            //Int64 s88stringL = -8646911284551352320;
            //Int32 s88distL = 6;       
            //s88stringL = s88stringL >> s88distL;

            //if (s88stringL != -135107988821114880)
            //{
            //    UART.Write("Bad right s88l shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right s88l shift\n");
            //}

            ////Signed 8-8 $t2>=32
            //Int64 s88StringG = -123456789123456789;
            //Int32 s88DistG = 40;       
            //s88StringG = s88StringG >> s88DistG;

            //if (s88StringG != -112284)
            //{
            //    UART.Write("Bad right s88G shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right s88G shift\n");
            //}

            ////Unsigned 8-4 $t2<32
            //UInt64 u84StringL = 4899916394579099648;
            //Int32 u84DistL = 6;
            //u84StringL = u84StringL >> u84DistL;

            //if (u84StringL != 76561193665298432)
            //{
            //    UART.Write("Bad right u84L shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right u84L shift\n");
            //}

            ////Unsigned 8-4 $t2>=32
            //UInt64 u84StringG = 4899916394579099648;
            //Int32 u84DistG = 40;
            //u84StringG = u84StringG >> u84DistG;

            //if (u84StringG != 4456448)
            //{
            //    UART.Write("Bad right u84G shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right u84G shift\n");
            //}

            ////Unsigned 8-8 $t2<32
            //UInt64 u88StringL = 4899916394579099648;
            //Int32 u88DistL = 6;     
            //u88StringL = u88StringL >> u88DistL;

            //if (u88StringL != 76561193665298432)
            //{
            //    UART.Write("Bad right u88L shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right u88L shift\n");
            //}

            ////Unsigned 8-8 $t2>=32
            //UInt64 u88StringG = 4899916394579099648;
            //Int32 u88DistG = 40;
            //u88StringG = u88StringG >> u88DistG;

            //if (u88StringG != 4456448)
            //{
            //    UART.Write("Bad right u88G shift\n");
            //}
            //else
            //{
            //    UART.Write("Good right u88G shift\n");
            //}

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
