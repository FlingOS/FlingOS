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
            /* 
             * New stuff to test:
             *   - Sizeof = Structs & sizeof(struct type)
             *   - NewArr/Ldelem/Stelem/Ldlen = Arrays
             *   - NewObj/Initobj/Ldobj/Stobj/Isinst = Objects & types
             */

            BasicConsole.Init();

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

            #region Objects

            TestClass aClass = new TestClass();
            int fld = aClass.aField;

            if (fld != 9)
            {
                UART.Write("Class field wrong\n");
            }
            else
            {
                UART.Write("Class field right\n");
            }

            int arg = 10;
            int arg1 = aClass.aMethodInt(arg);

            if (arg1 != 30)
            {
                UART.Write("Class method int wrong\n");
            }
            else
            {
                UART.Write("Class method int right\n");
            }

            aClass.aMethodVoid();
            int arg2 = aClass.aMethodField(arg);

            if (arg2 != 90)
            {
                UART.Write("Class method field wrong\n");
            }
            else
            {
                UART.Write("Class method field right\n");
            }

            #endregion

            #region String Tests

            BasicConsole.WriteLine("Test BasicConsole write line!");

            int testNum = 5;

            Testing2.String ATestString = "Hello, world!";
            UART.Write(ATestString);
            UART.Write("\n");

            if (ATestString != "Hello, world!")
            {
                UART.Write("String equality does not work!\n");
            }
            else
            {
                UART.Write("String equality works.\n");
            }

            ATestString += " But wait! There's more...";
            BasicConsole.WriteLine("Concatenated.");
            UART.Write(ATestString);

            //ATestString += " We can even append numbers: " + (Testing2.String)testNum;
            //UART.Write(ATestString);

            #endregion

            UART.Write("Okay");

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

    public class TestClass : Testing2.Object
    {
        public int aField = 9;

        [Drivers.Compiler.Attributes.NoGC]
        public int aMethodInt(int arg)
        {
            return arg * 3;
        }

        [Drivers.Compiler.Attributes.NoGC]
        public void aMethodVoid()
        {
            UART.Write("Class method void right\n");
        }

        [Drivers.Compiler.Attributes.NoGC]
        public int aMethodField(int arg)
        {
            return arg * aField;
        }
    }
}
