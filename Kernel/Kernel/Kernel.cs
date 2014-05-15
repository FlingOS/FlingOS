using Kernel.FOS_System;
using System;

namespace Kernel
{
    /// <summary>
    /// The main class (containing the kernel entry point) for the Fling OS kernel.
    /// </summary>
    [Compiler.PluggedClass]
    public static class Kernel
    {
        /// <summary>
        /// Initialises static stuff within the kernel (such as calling GC.Init and BasicDebug.Init)
        /// </summary>
        static Kernel()
        {
            BasicConsole.Init();
            BasicConsole.Clear();

            Debug.BasicDebug.Init();
            FOS_System.GC.Init();

            BasicConsole.WriteLine();
        }

        /// <summary>
        /// Filled-in by the compiler.
        /// </summary>
        [Compiler.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        /// <summary>
        /// Main kernel entry point
        /// </summary>
        [Compiler.KernelMainMethod]
        [Compiler.NoGC]
        static unsafe void Main()
        {
            //Necessary for exception handling stuff to work
            //  - We must have an intial, empty handler to always 
            //    "return" to.
            ExceptionMethods.AddExceptionHandlerInfo((void*)0, (void*)0);
            
            BasicConsole.WriteLine("Fling OS Running...");

            try
            {
                Paging.Init();
                
                ManagedMain();

                FOS_System.GC.Cleanup();
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                FOS_System.Type currExceptionType = ExceptionMethods.CurrentException._Type;
                if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.PageFaultException))
                {
                    BasicConsole.WriteLine("Page fault exception unhandled!");
                }
                else
                {
                    BasicConsole.WriteLine("Unhandled exception caught in Main()!");
                }
                BasicConsole.WriteLine("Fling OS forced to halt!");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            if (FOS_System.GC.NumObjs > 3)
            {
                BasicConsole.WriteLine("Num Objs > 3");
            } 
            else if (FOS_System.GC.NumObjs > 2)
            {
                BasicConsole.WriteLine("Num Objs > 2");
            }
            else if (FOS_System.GC.NumObjs > 1)
            {
                BasicConsole.WriteLine("Num Objs > 1");
            }
            else if (FOS_System.GC.NumObjs > 0)
            {
                BasicConsole.WriteLine("Num Objs > 0");
            }
            else if (FOS_System.GC.NumObjs < 0)
            {
                BasicConsole.WriteLine("Num Objs < 0");
            }
            if (FOS_System.GC.numStrings > 0)
            {
                BasicConsole.WriteLine("Num strings > 0");
            }
            else if (FOS_System.GC.numStrings < 0)
            {
                BasicConsole.WriteLine("Num strings < 0");
            }
            BasicConsole.SetTextColour(BasicConsole.default_colour);

            BasicConsole.WriteLine("Fling OS Ended.");

            //Necessary - no way of returning from this method since add exception info 
            //            at start cannot be "undone" so stack is "corrupted" if we try
            //            to "ret"
            //So we just halt the CPU for want of a better solution later when ACPI is 
            //implemented.
            Halt();
        }

        /// <summary>
        /// Halts the kernel and halts the CPU.
        /// </summary>
        [Compiler.HaltMethod]
        [Compiler.NoGC]
        public static void Halt()
        {
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Kernel halting!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            PreReqs.Reset();
        }

        /// <summary>
        /// The actual main method for the kernel - by this point, all memory management, exception handling 
        /// etc has been set up properly.
        /// </summary>
        private static void ManagedMain()
        {
            try
            {
                FOS_System.String testStr = FOS_System.String.Concat("test1", " test2");
                BasicConsole.WriteLine(testStr);

                FOS_System.Object[] objArr = new FOS_System.Object[10];
                objArr[0] = new FOS_System.Object();
                objArr[0]._Type.Size = 5;
                if (objArr[0] != null)
                {
                    BasicConsole.WriteLine("Set object in array success!");
                }

                int[] testArray = new int[1024];
                testArray[5] = 10;
                int q = testArray[5];

                Dummy obj = new Dummy();
                new Dummy();
                obj = new Dummy();
                obj.x = obj.x + obj.y;
                if (obj.x == 21)
                {
                    BasicConsole.WriteLine("Addition success!");
                }

                if (obj.testEnum == Dummy.TestEnum.First)
                {
                    BasicConsole.WriteLine("TestEnum.First pre-assigned.");
                }
                obj.testEnum = Dummy.TestEnum.Second;
                if (obj.testEnum == Dummy.TestEnum.Second)
                {
                    BasicConsole.WriteLine("TestEnum.Second assignment worked.");
                }

                int x = 0;
                int y = 0;
                int z = 0;
                z = x / y;
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);

                FOS_System.Type currExceptionType = ExceptionMethods.CurrentException._Type;
                if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.DivideByZeroException))
                {
                    BasicConsole.WriteLine("Handled divide by zero exception.");
                }
                else if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.ArgumentException))
                {
                    BasicConsole.WriteLine("Handled argument exception.");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.ArgumentException)(ExceptionMethods.CurrentException)).ExtendedMessage);
                }
                else
                {
                    ExceptionMethods.Rethrow();
                }
            }

            BasicConsole.WriteLine("End of managed main!");
        }
    }

    public class Dummy : FOS_System.Object
    {
        public enum TestEnum
        {
            First = 1,
            Second = 2,
            Third = 3,
            NULL = 0
        }

        public TestEnum testEnum = TestEnum.First;

        public int x = 10;
        public int y = 11;

        public int Add()
        {
            return x + y;
        }
    }
}
