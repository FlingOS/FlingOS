using System;

namespace Testing1
{
    public static class Kernel
    {
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = "ASM\\Kernel")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MinValue)]
        public static void Boot()
        {
        }

        [Drivers.Compiler.Attributes.MainMethod]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Main()
        {
            UART.Init();

            //UART.Write("Hello, world!\n");
            UART.Write('H');
            UART.Write('e');
            UART.Write('l');
            UART.Write('l');
            UART.Write('o');
            UART.Write(',');
            UART.Write(' ');
            UART.Write('w');
            UART.Write('o');
            UART.Write('r');
            UART.Write('l');
            UART.Write('d');
            UART.Write('!');

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
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        LED.Red();
                        Delay();
                        LED.Blue();
                        Delay();
                    }
                }
            }
        }
        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        private static void Delay()
        {
            for (int i = 0; i < 100; i++)
            {
                ;
            }
        }
    }
}
