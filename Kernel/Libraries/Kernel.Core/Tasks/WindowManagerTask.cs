using System;
using Kernel.Core.Processes;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Devices;

namespace Kernel.Core.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private class PipeInfo : FOS_System.Object
        {
            public Pipes.Standard.StandardInpoint StdIn;
            public Console ScreenOutput = new Consoles.AdvancedConsole();
        }

        private static bool Terminating = false;

        private static List ConnectedPipes;
        private static int CurrentPipeIdx = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This is guaranteed to be one.
        /// </remarks>
        private static uint MainThreadId = 1;
        private static uint GCThreadId;
        private static uint ProcessingThreadId;
        private static bool ProcessingThreadAwake = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Window Manager: Started.");

            // Initialise heap & GC
            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            
            // Start thread for calling GC Cleanup method
            if (SystemCallMethods.StartThread(GCCleanupTask.Main, out GCThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: GC thread failed to create!");
            }

            // Initialise connected pipes list
            ConnectedPipes = new List();

            // Start thread for handling background processing
            if (SystemCallMethods.StartThread(Processing, out ProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Processing thread failed to create!");
            }

            BasicConsole.Write("WM > Processing thread id: ");
            BasicConsole.WriteLine(ProcessingThreadId);

            SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.RegisterPipeOutpoint, SyscallHandler);

            // Wait for pipe to be created
            SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);

            SystemCallMethods.RegisterIRQHandler(1, HandleIRQ);
            Keyboard.InitDefault();

            while (!Terminating)
            {
                try
                {
                    if (CurrentPipeIdx > -1)
                    {
                        PipeInfo CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);

                        CurrentPipeInfo.ScreenOutput.Write(CurrentPipeInfo.StdIn.Read());

                        bool AltPressed = Keyboard.Default.AltPressed;
                        KeyboardKey Key;
                        bool GotKey = Keyboard.Default.GetKey(out Key);
                        if (GotKey)
                        {
                            if (AltPressed && Key == KeyboardKey.Tab)
                            {
                                CurrentPipeIdx++;
                                if (CurrentPipeIdx >= ConnectedPipes.Count)
                                {
                                    CurrentPipeIdx = 0;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("WM > Exception running window manager.");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        public static void Processing()
        {
            while (!Terminating)
            {
                if (!ProcessingThreadAwake)
                {
                    SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);
                }
                ProcessingThreadAwake = false;

                BasicConsole.WriteLine("WM > Processing thread runnning...");

                // Delay to allow time for Register Outpoint Deferred System Call to complete
                SystemCallMethods.SleepThread(500);

                int numOutpoints;
                SystemCallResults SysCallResult;
                Pipes.BasicServerHelpers.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out);

                if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                {
                    Pipes.PipeOutpointDescriptor[] OutpointDescriptors;
                    Pipes.BasicServerHelpers.GetOutpointDescriptors(numOutpoints, ref SysCallResult, out OutpointDescriptors, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out);

                    if (SysCallResult == SystemCallResults.OK)
                    {
                        for (int i = 0; i < OutpointDescriptors.Length; i++)
                        {
                            Pipes.PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
                            bool PipeExists = false;

                            for (int j = 0; j < ConnectedPipes.Count; j++)
                            {
                                PipeInfo ExistingPipeInfo = (PipeInfo)ConnectedPipes[j];
                                if (ExistingPipeInfo.StdIn.OutProcessId == Descriptor.ProcessId)
                                {
                                    PipeExists = true;
                                    break;
                                }
                            }

                            if(!PipeExists)
                            {
                                try
                                {
                                    ConnectedPipes.Add(new PipeInfo()
                                    {
                                        StdIn = new Pipes.Standard.StandardInpoint(Descriptor.ProcessId, 2000, true), // 2000 ASCII characters = 2000 bytes
                                    });

                                    if (CurrentPipeIdx == -1)
                                    {
                                        CurrentPipeIdx = 0;
                                        SystemCallMethods.WakeThread(MainThreadId);
                                    }
                                }
                                catch
                                {
                                    BasicConsole.WriteLine("WM > Error creating new pipe!");
                                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        BasicConsole.WriteLine("WM > Couldn't get outpoint descriptors!");
                    }
                }
                else
                {
                    BasicConsole.WriteLine("WM > Cannot get outpoints!");
                }
            }
        }
        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.RegisterPipeOutpoint:
                    BasicConsole.WriteLine("WM > IH > Actioning Register Pipe Outpoint system call...");
                    Pipes.PipeClasses Class = (Pipes.PipeClasses)param1;
                    Pipes.PipeSubclasses Subclass = (Pipes.PipeSubclasses)param2;
                    if (Class == Pipes.PipeClasses.Standard &&
                        Subclass == Pipes.PipeSubclasses.Standard_Out)
                    {
                        BasicConsole.WriteLine("WM > IH > Register Pipe Outpoint has desired pipe class and subclass.");
                        result = SystemCallResults.RequestAction_WakeThread;
                        Return2 = ProcessingThreadId;
                        ProcessingThreadAwake = true;
                    }
                    break;
            }

            return (int)result;
        }

        private static int HandleIRQ(uint IRQNum)
        {
            if (IRQNum == 1)
            {
                ((Hardware.Keyboards.PS2)Keyboard.Default).InterruptHandler();
                return 0;
            }
            return -1;
        }
    }
}
