#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
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
        [Compiler.NoDebug]
        static Kernel()
        {
            BasicConsole.Init();
            BasicConsole.Clear();

#if DEBUG
            Debug.BasicDebug.Init();
#endif
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
        [Compiler.NoDebug]
        static unsafe void Main()
        {
            //Necessary for exception handling stuff to work
            //  - We must have an intial, empty handler to always 
            //    "return" to.
            ExceptionMethods.AddExceptionHandlerInfo((void*)0, (void*)0);
            
            BasicConsole.WriteLine("Fling OS Running...");

            try
            {
                Hardware.VirtMemManager.Init();
                Hardware.Devices.CPU.InitDefault();
                Hardware.Devices.Timer.InitDefault();
                Hardware.Devices.Keyboard.InitDefault();

                Core.Processes.Process ManagedMainProcess = 
                    Core.Processes.ProcessManager.CreateProcess(GetManagedMainMethodPtr(), "Managed Main");
                Core.Processes.Process SampleProcess =
                    Core.Processes.ProcessManager.LoadSampleProcess();

                Core.Processes.Thread ManagedMain_MainThread = ((Core.Processes.Thread)ManagedMainProcess.Threads[0]);
                Heap.Free(ManagedMain_MainThread.ThreadStackTop);
                ManagedMain_MainThread.ThreadStackTop = GetKernelStackPtr();
                ManagedMain_MainThread.ESP = (uint)ManagedMain_MainThread.ThreadStackTop;

                Core.Processes.ProcessManager.RegisterProcess(ManagedMainProcess);
                Core.Processes.ProcessManager.RegisterProcess(SampleProcess);

                ManagedMain();
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    BasicConsole.WriteLine("Page fault exception unhandled!");
                }
                else
                {
                    BasicConsole.WriteLine("Startup error! " + ExceptionMethods.CurrentException.Message);
                }
                BasicConsole.WriteLine("Fling OS forced to halt!");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("Cleaning up...");
            FOS_System.GC.Cleanup();

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.Write("GC num objs: ");
            BasicConsole.WriteLine(FOS_System.GC.NumObjs);
            BasicConsole.Write("GC num strings: ");
            BasicConsole.WriteLine(FOS_System.GC.NumStrings);
            BasicConsole.Write("Heap memory use: ");
            BasicConsole.Write(Heap.FBlock->used * Heap.FBlock->bsize);
            BasicConsole.Write(" / ");
            BasicConsole.WriteLine(Heap.FBlock->size);
            BasicConsole.SetTextColour(BasicConsole.default_colour);

            BasicConsole.WriteLine("Fling OS Ended.");

            //Necessary - no way of returning from this method since add exception info 
            //            at start cannot be "undone" so stack is "corrupted" if we try
            //            to "ret"
            //So we just halt the CPU for want of a better solution later when ACPI is 
            //implemented.
            ExceptionMethods.HaltReason = "End of Main";
            Halt(0xFFFFFFFF);
            //TODO: Proper shutdown method
        }

        /// <summary>
        /// Halts the kernel and halts the CPU.
        /// </summary>
        /// <param name="lastAddress">The address of the last line of code which ran or 0xFFFFFFFF.</param>
        [Compiler.HaltMethod]
        [Compiler.NoGC]
        public static void Halt(uint lastAddress)
        {
            try
            {
                Hardware.Devices.Keyboard.CleanDefault();
                Hardware.Devices.Timer.CleanDefault();
            }
            catch
            {
            }

            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.Write("Halt Reason: ");
            BasicConsole.WriteLine(ExceptionMethods.HaltReason);
            //BasicConsole.Write("Last address: ");
            //BasicConsole.WriteLine(lastAddress);
            BasicConsole.SetTextColour(BasicConsole.default_colour);

            if (ExceptionMethods.CurrentException != null)
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    BasicConsole.Write("Address: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).address);
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).errorCode);
                }
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Kernel halting!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            PreReqs.Reset();
        }

        /// <summary>
        /// The actual main method for the kernel - by this point, all memory management, exception handling 
        /// etc has been set up properly.
        /// </summary>
        [Compiler.NoDebug]
        private static unsafe void ManagedMain()
        {
            try
            {
                Core.Shell.InitDefault();
                Core.Shell.Default.Execute();

                if (!Core.Shell.Default.Terminating)
                {
                    Core.Console.Default.WarningColour();
                    Core.Console.Default.WriteLine("Abnormal shell shutdown!");
                    Core.Console.Default.DefaultColour();
                }
                else
                {
                    Core.Console.Default.Clear();
                }

                //TimerTest();
                //PCBeepTest();
                //KeyboardTest();
                //AdvancedConsoleTest();
                
                //InitATA();

                //OutputDivider();

                //InitPCI();

                //OutputDivider();

                //try
                //{
                //    OutputPCIInfo();
                //    OutputDivider();
                //}
                //catch
                //{
                //    OutputCurrentExceptionInfo();
                //}

                //FOS_System.GC.Cleanup();

                //InitUSB();

                //OutputDivider();

                //if (Hardware.DeviceManager.Devices.Count > 0)
                //{
                //    //try
                //    //{
                //    //    OutputATAInfo();
                //    //}
                //    //catch
                //    //{
                //    //    OutputCurrentExceptionInfo();
                //    //}

                //    InitFileSystem();

                //    OutputDivider();

                //    CheckDiskFormatting(HDD0);

                //    OutputDivider();

                //    try
                //    {
                //        OutputFileSystemsInfo();
                //    }
                //    catch
                //    {
                //        OutputCurrentExceptionInfo();
                //    }

                //    try
                //    {
                //        OutputFileContents("A:/Doc in Root Dir.txt");
                //        OutputFileContents("A:/Test Dir/Doc in Test Dir.txt");
                //    }
                //    catch
                //    {
                //        OutputCurrentExceptionInfo();
                //    }

                //    FileSystemTests();
                //}
            }
            catch
            {
                OutputCurrentExceptionInfo();
            }
            
            BasicConsole.WriteLine();
            OutputDivider();
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of managed main.");
        }

        /// <summary>
        /// Outputs the current exception information.
        /// </summary>
        [Compiler.NoDebug]
        private static void OutputCurrentExceptionInfo()
        {
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);

            BasicConsole.SetTextColour(BasicConsole.default_colour);

            ExceptionMethods.CurrentException = null;
        }

        /// <summary>
        /// Outputs a divider line.
        /// </summary>
        private static void OutputDivider()
        {
            BasicConsole.WriteLine("---------------------");
        }

        private static unsafe void* GetManagedMainMethodPtr()
        {
            return null;
        }
        private static unsafe byte* GetKernelStackPtr()
        {
            return null;
        }
    }
}
