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
            public Pipes.Standard.StandardInpoint StdOut;
            public int StdInPipeId;
            public Console TheConsole = new Consoles.AdvancedConsole();
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
        private static uint InputProcessingThreadId;
        private static bool InputProcessingThreadAwake = false;
        private static uint OutputProcessingThreadId;

        private static Pipes.Standard.StandardOutpoint StdIn;

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

            // Start thread for handling background input processing
            if (SystemCallMethods.StartThread(InputProcessing, out InputProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: InputProcessing thread failed to create!");
            }

            // Start thread for handling background output processing
            if (SystemCallMethods.StartThread(OutputProcessing, out OutputProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: OutputProcessing thread failed to create!");
            }

            BasicConsole.Write("WM > InputProcessing thread id: ");
            BasicConsole.WriteLine(InputProcessingThreadId);

            StdIn = new Pipes.Standard.StandardOutpoint(false);

            Keyboard.InitDefault();
            SystemCallMethods.RegisterIRQHandler(1, HandleIRQ);

            SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.RegisterPipeOutpoint, SyscallHandler);
            
            // Wait for pipe to be created
            SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);

            while (!Terminating)
            {
                try
                {
                    if (CurrentPipeIdx > -1)
                    {
                        PipeInfo CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);

                        CurrentPipeInfo.TheConsole.Write(CurrentPipeInfo.StdOut.Read());
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("WM > Exception running window manager.");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        public static void InputProcessing()
        {
            while (!Terminating)
            {
                if (!InputProcessingThreadAwake)
                {
                    SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);
                }
                InputProcessingThreadAwake = false;

                BasicConsole.WriteLine("WM > InputProcessing thread runnning...");

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
                                if (ExistingPipeInfo.StdOut.OutProcessId == Descriptor.ProcessId)
                                {
                                    PipeExists = true;
                                    break;
                                }
                            }

                            if(!PipeExists)
                            {
                                try
                                {
                                    PipeInfo NewPipeInfo = new PipeInfo();
                                    NewPipeInfo.StdOut = new Pipes.Standard.StandardInpoint(Descriptor.ProcessId, true); // 2000 ASCII characters = 2000 bytes
                                    NewPipeInfo.StdInPipeId = StdIn.WaitForConnect();

                                    ConnectedPipes.Add(NewPipeInfo);

                                    if (CurrentPipeIdx == -1)
                                    {
                                        CurrentPipeIdx = 0;
                                        SystemCallMethods.WakeThread(MainThreadId);
                                        SystemCallMethods.WakeThread(OutputProcessingThreadId);
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
                        Return2 = InputProcessingThreadId;
                        InputProcessingThreadAwake = true;
                    }
                    break;
            }

            return (int)result;
        }

        public static void OutputProcessing()
        {
            // Wait for pipe to be created
            SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);

            FOS_System.String line = "";

            PipeInfo CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);

            while (!Terminating)
            {
                try
                {
                    bool AltPressed = Keyboard.Default.AltPressed;
                    uint Scancode;
                    bool GotScancode = Keyboard.Default.GetScancode(out Scancode);
                    if (GotScancode)
                    {
                        KeyboardKey Key;
                        if (Keyboard.Default.GetKeyValue(Scancode, out Key))
                        {
                            if (AltPressed && Key == KeyboardKey.Tab)
                            {
                                CurrentPipeIdx++;
                                if (CurrentPipeIdx >= ConnectedPipes.Count)
                                {
                                    CurrentPipeIdx = 0;
                                }

                                CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);
                                SystemCallMethods.WakeThread(1);
                            }
                            else
                            {
                                char Character;
                                if (Keyboard.Default.GetCharValue(Scancode, out Character))
                                {
                                    line += Character;

                                    if (line.length > 0 && line[line.length - 1] == '\n')
                                    {
                                        StdIn.Write(CurrentPipeInfo.StdInPipeId, line);
                                        line = "";
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("WM > Error during output processing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
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
