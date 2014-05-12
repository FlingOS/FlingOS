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

            GC.Init();
            Debug.BasicDebug.Init();

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
            Exceptions.AddExceptionHandlerInfo((void*)0, (void*)0);
            
            BasicConsole.WriteLine("Fling OS Running...");

            try
            {
                Paging.Init();
                
                ManagedMain();
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                if (Exceptions.CurrentException._Type == (FOS_System.Type)typeof(FOS_System.Exceptions.PageFaultException))
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
            if (GC.NumObjs > 0)
            {
                BasicConsole.WriteLine("Num Objs > 0");
            }
            else if (GC.NumObjs < 0)
            {
                BasicConsole.WriteLine("Num Objs < 0");
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
                FOS_System.Object[] objArr = new FOS_System.Object[10];
                objArr[0] = new FOS_System.Object();
                objArr[0]._Type.Size = 5;
                if (objArr[0]._Type.Size == 5)
                {
                    Console.WriteLine("Success!");
                }

                int[] testArray = new int[1024];
                testArray[5] = 10;
                int x = testArray[5];

                x = 1;
                int y = 0;
                int z = 0;

                Dummy obj = new Dummy();
                new Dummy();

                obj = new Dummy();

                obj.x = obj.x + obj.y;

                if (obj.x == 21)
                {
                    BasicConsole.WriteLine("Addition success!");
                }

                z = x / y;
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(Exceptions.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);

                if (Exceptions.CurrentException._Type == (FOS_System.Type)typeof(FOS_System.Exceptions.DivideByZeroException))
                {
                    BasicConsole.WriteLine("Handled divide by zero exception.");
                }
                else
                {
                    Exceptions.Rethrow();
                }
            }

            BasicConsole.WriteLine("End of managed main!");
        }
    }

    public class Dummy : FOS_System.Object
    {
        public int x = 10;
        public int y = 11;

        public int Add()
        {
            return x + y;
        }
    }
}
