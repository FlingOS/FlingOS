using System; 

namespace Testing2
{
    public struct AStruct
    {
        public byte a;      // 1 byte - 1 byte on heap
        public short b;     // 2 bytes - 2 bytes on heap
        public int c;       // 4 bytes - 4 bytes on heap
        public long d;      // 8 bytes - 8 bytes on heap
                            // Total : 15 bytes
    }

    public static unsafe class Kernel
    {
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = "ASM\\Kernel")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MinValue)]
        public static void Boot()
        {
        }

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void EnableInterrupts()
        {
        }

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static byte* GetExceptionHandlerStart()
        {
            return null;
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static byte* GetExceptionHandlerEnd()
        {
            return null;
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static byte* GetIRQHandlerStart()
        {
            return null;
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static byte* GetIRQHandlerEnd()
        {
            return null;
        }

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void MainInterruptHandler()
        {
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void DoSyscall()
        {
        }

        public static void MemSet(byte val, byte* to, uint length)
        {
            BasicConsole.WriteLine(((Testing2.String)"Setting ") + (uint)to + " to " + val + ", size: " + length);
            BasicConsole.DumpMemory(to, (int)length);

            uint tempLength = length;
            while (tempLength-- > 0)
            {
                *to++ = val;
            }

            BasicConsole.DumpMemory(to - length, (int)length);
        }
        public static void MemCpy(byte* from, byte* to, uint length)
        {
            BasicConsole.WriteLine(((Testing2.String)"Copying from ") + (uint)from + " to " + (uint)to + ", size: " + length);
            BasicConsole.DumpMemory(from, (int)length);
            BasicConsole.DumpMemory(to, (int)length);

            uint tempLength = length;
            while (tempLength-- > 0)
            {
                *to++ = *from++;
            }

            BasicConsole.DumpMemory(to - length, (int)length);
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
        public static void Main()
        {
            ExceptionMethods.AddExceptionHandlerInfo(null, null);

            BasicConsole.Init();
            BasicConsole.WriteLine("Kernel executing...");

            try
            {
                MemSet(0x0, (byte*)0x80000000, 0x1000);

                BasicConsole.WriteLine("Copying exception handler...");
                byte* ExHndlrStart = GetExceptionHandlerStart();
                byte* ExHndlrEnd = GetExceptionHandlerEnd();
                MemCpy(ExHndlrStart, (byte*)0x80000180, (uint)ExHndlrEnd - (uint)ExHndlrStart);

                BasicConsole.WriteLine("Copying IRQ handler...");
                byte* IRQHndlrStart = GetIRQHandlerStart();
                byte* IRQHndlrEnd = GetIRQHandlerEnd();
                MemCpy(IRQHndlrStart, (byte*)0x80000200, (uint)IRQHndlrEnd - (uint)IRQHndlrStart);

                BasicConsole.WriteLine("Enabling interrupts...");

                for (int i = 0; i < 100000; i++)
                {
                    LED.Blue();
                    DelayShort();
                    LED.Red();
                    DelayShort();
                }
                LED.Blue();
                for (int i = 0; i < 100; i++)
                {
                    DelayLong();
                }

                //ExceptionMethods.ArbitaryReturn((uint)ExceptionMethods.BasePointer, (uint)ExceptionMethods.StackPointer, (byte*)0x80000200);

                EnableInterrupts();

                BasicConsole.WriteLine("Testing interrupts...");

                DoSyscall();

                BasicConsole.WriteLine("Continuing execution.");

                #region Struct Tests

                //int size = sizeof(AStruct);

                //if (size != 15)
                //{
                //    UART.Write("Bad size\n");
                //}
                //else
                //{
                //    UART.Write("Good size\n");
                //}

                //AStruct Inst = new AStruct();
                //Inst.a = 1;
                //Inst.b = 2;
                //Inst.c = 4;
                //Inst.d = 8;

                //if (Inst.a != 1)
                //{
                //    UART.Write("Inst.a wrong\n");
                //}
                //else
                //{
                //    UART.Write("Inst.a right\n");
                //}
                //if (Inst.b != 2)
                //{
                //    UART.Write("Inst.b wrong\n");
                //}
                //else
                //{
                //    UART.Write("Inst.b right\n");
                //}
                //if (Inst.c != 4)
                //{
                //    UART.Write("Inst.c wrong\n");
                //}
                //else
                //{
                //    UART.Write("Inst.c right\n");
                //}
                //if (Inst.d != 8)
                //{
                //    UART.Write("Inst.d wrong\n");
                //}
                //else
                //{
                //    UART.Write("Inst.d right\n");
                //}

                //AStruct* HeapInst = (AStruct*)Heap.AllocZeroed((uint)sizeof(AStruct), "Kernel:Main");
                //if (HeapInst == null)
                //{
                //    UART.Write("HeapInst null\n");
                //}
                //else
                //{
                //    UART.Write("HeapInst not null\n");
                //}

                //HeapInst->a = 1;
                //HeapInst->b = 2;
                //HeapInst->c = 4;
                //HeapInst->d = 8;

                //if (HeapInst->a != 1)
                //{
                //    UART.Write("HeapInst->a wrong\n");
                //}
                //else
                //{
                //    UART.Write("HeapInst->a right\n");
                //}
                //if (HeapInst->b != 2)
                //{
                //    UART.Write("HeapInst->b wrong\n");
                //}
                //else
                //{
                //    UART.Write("HeapInst->b right\n");
                //}
                //if (HeapInst->c != 4)
                //{
                //    UART.Write("HeapInst->c wrong\n");
                //}
                //else
                //{
                //    UART.Write("HeapInst->c right\n");
                //}
                //if (HeapInst->d != 8)
                //{
                //    UART.Write("HeapInst->d wrong\n");
                //}
                //else
                //{
                //    UART.Write("HeapInst->d right\n");
                //}

                #endregion

                #region Array Tests Value Types

                //int[] array = new int[4];

                //int len = array.Length;

                //if (len != 4)
                //{
                //    UART.Write("Array length wrong\n");
                //}
                //else
                //{
                //    UART.Write("Array length right\n");
                //}

                //array[0] = 5;
                //array[1] = 10;
                //array[2] = 15;
                //array[3] = 20;

                //if (array[0] != 5)
                //{
                //    UART.Write("array[0] wrong\n");
                //}
                //else
                //{
                //    UART.Write("array[0] right\n");
                //}

                //if (array[1] != 10)
                //{
                //    UART.Write("array[1] wrong\n");
                //}
                //else
                //{
                //    UART.Write("array[1] right\n");
                //}

                //if (array[2] != 15)
                //{
                //    UART.Write("array[2] wrong\n");
                //}
                //else
                //{
                //    UART.Write("array[2] right\n");
                //}

                //if (array[3] != 20)
                //{
                //    UART.Write("array[3] wrong\n");
                //}
                //else
                //{
                //    UART.Write("array[3] right\n");
                //}

                #endregion

                #region Array Tests Using Structs

                //AStruct[] arr = new AStruct[3];
                //int length = arr.Length;

                //if (length != 3)
                //{
                //    UART.Write("Struct array length wrong\n");
                //}
                //else
                //{
                //    UART.Write("Struct array length right\n");
                //}

                //arr[0].a = 255;
                //arr[0].b = 32767;
                //arr[0].c = 2147483647;
                //arr[0].d = 9223372036854775807;

                //if (arr[0].a != 255)
                //{
                //    UART.Write("arr[0].a wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[0].a right\n");
                //}
                //if (arr[0].b != 32767)
                //{
                //    UART.Write("arr[0].b wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[0].b right\n");
                //}
                //if (arr[0].c != 2147483647)
                //{
                //    UART.Write("arr[0].c wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[0].c right\n");
                //}
                //if (arr[0].d != 9223372036854775807)
                //{
                //    UART.Write("arr[0].d wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[0].d right\n");
                //}

                //arr[1].a = 0;
                //arr[1].b = 1;
                //arr[1].c = 2;
                //arr[1].d = 3;

                //if (arr[1].a != 0)
                //{
                //    UART.Write("arr[1].a wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[1].a right\n");
                //}
                //if (arr[1].b != 1)
                //{
                //    UART.Write("arr[1].b wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[1].b right\n");
                //}
                //if (arr[1].c != 2)
                //{
                //    UART.Write("arr[1].c wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[1].c right\n");
                //}
                //if (arr[1].d != 3)
                //{
                //    UART.Write("arr[1].d wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[1].d right\n");
                //}

                //arr[2].a = 100;
                //arr[2].b = 3000;
                //arr[2].c = 5777;
                //arr[2].d = 99876;

                //if (arr[2].a != 100)
                //{
                //    UART.Write("arr[2].a wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[2].a right\n");
                //}
                //if (arr[2].b != 3000)
                //{
                //    UART.Write("arr[2].b wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[2].b right\n");
                //}
                //if (arr[2].c != 5777)
                //{
                //    UART.Write("arr[2].c wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[2].c right\n");
                //}
                //if (arr[2].d != 99876)
                //{
                //    UART.Write("arr[2].d wrong\n");
                //}
                //else
                //{
                //    UART.Write("arr[2].d right\n");
                //}

                #endregion

                #region String Tests

                //BasicConsole.WriteLine("Test BasicConsole write line!");

                //int testNum = 5;

                //Testing2.String ATestString = "Hello, world!";
                //BasicConsole.WriteLine(ATestString);

                //if (ATestString != "Hello, world!")
                //{
                //    BasicConsole.WriteLine("String equality does not work!");
                //}
                //else
                //{
                //    BasicConsole.WriteLine("String equality works.");
                //}

                //ATestString += " But wait! There's more...";
                //BasicConsole.WriteLine(ATestString);

                //ATestString += " We can even append numbers: " + (Testing2.String)testNum;
                //BasicConsole.WriteLine(ATestString);

                //BasicConsole.DumpMemory((byte*)Utilities.ObjectUtilities.GetHandle(ATestString) - sizeof(GCHeader), (int)(ATestString.length + Testing2.String.FieldsBytesSize + sizeof(GCHeader)));

                #endregion

                #region Objects

                //TestClass aClass = new TestClass();
                //BasicConsole.WriteLine("Object created!");
                //int fld = aClass.aField;

                //if (fld != 9)
                //{
                //    UART.Write("Class field wrong\n");
                //}
                //else
                //{
                //    UART.Write("Class field right\n");
                //}

                //int arg = 10;
                //int arg1 = aClass.aMethodInt(arg);

                //if (arg1 != 30)
                //{
                //    UART.Write("Class method int wrong\n");
                //}
                //else
                //{
                //    UART.Write("Class method int right\n");
                //}

                //aClass.aMethodVoid();
                //int arg2 = aClass.aMethodField(arg);

                //if (arg2 != 90)
                //{
                //    UART.Write("Class method field wrong\n");
                //}
                //else
                //{
                //    UART.Write("Class method field right\n");
                //}

                #endregion

                BasicConsole.WriteLine("Okay");

            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine("Startup error! " + ExceptionMethods.CurrentException.Message);
                BasicConsole.WriteLine("FlingOS forced to halt!");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            bool OK = true;
            while (OK)
            {
                ;
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
            //BasicTimer.Sleep(10ul);
        }
    }

    public class TestClass : Testing2.Object
    {
        public int aField = 9;

        public int aMethodInt(int arg)
        {
            return arg * 3;
        }

        public void aMethodVoid()
        {
            UART.Write("Class method void right\n");
        }

        public int aMethodField(int arg)
        {
            return arg * aField;
        }
    }
}
