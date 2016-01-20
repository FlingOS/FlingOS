#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using Kernel.Processes;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Devices;

namespace Kernel.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private class PipeInfo : FOS_System.Object
        {
            public Pipes.Standard.StandardInpoint StdOut;
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

        public static bool Ready
        {
            get
            {
                return ready_count == 3;
            }
        }
        private static int ready_count = 0;

        private static bool CurrentPipeIndex_Changed = false;

        private static uint AcceptedPages_StartAddress = 0;
        private static uint AcceptedPages_Count = 0;
        private static uint AcceptedPages_FromProcessId = 0;

        public static void Main()
        {
            BasicConsole.WriteLine("Window Manager: Started.");

            // Initialise heap & GC
            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();

            // Start thread for calling GC Cleanup method
            if (SystemCalls.StartThread(GCCleanupTask.Main, out GCThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: GC thread failed to create!");
            }

            // Initialise connected pipes list
            ConnectedPipes = new List();

            // Start thread for handling background input processing
            if (SystemCalls.StartThread(InputProcessing, out InputProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: InputProcessing thread failed to create!");
            }
            BasicConsole.Write("WM > InputProcessing thread id: ");
            BasicConsole.WriteLine(InputProcessingThreadId);
            
            // Start thread for other testing
            uint TestThreadId;
            if (SystemCalls.StartThread(TestThread, out TestThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Test thread failed to create!");
            }
            BasicConsole.Write("WM > Test thread id: ");
            BasicConsole.WriteLine(TestThreadId);

            BasicConsole.Write("WM > Register syscall handlers");
            SystemCalls.RegisterSyscallHandler(SystemCallNumbers.RegisterPipeOutpoint, SyscallHandler);
            SystemCalls.RegisterSyscallHandler(SystemCallNumbers.AcceptPages);
            
            // Start thread for handling background output processing
            if (SystemCalls.StartThread(OutputProcessing, out OutputProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: OutputProcessing thread failed to create!");
            }
            
            //TODO: PS2 Keyboard device should be handled by Device Manager Task
            BasicConsole.WriteLine("WM > Init keyboard");
            Keyboard.InitDefault();
            BasicConsole.WriteLine("WM > Register IRQ 1 handler");
            SystemCalls.RegisterIRQHandler(1, HandleIRQ);

            BasicConsole.WriteLine("WM > Wait for pipe to be created");
            // Wait for pipe to be created
            ready_count++;
            SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);

            PipeInfo CurrentPipeInfo = null;

            while (!Terminating)
            {
                try
                {
                    if (CurrentPipeIdx > -1)
                    {
                        if (CurrentPipeIndex_Changed)
                        {
                            CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);
                            CurrentPipeIndex_Changed = false;

                            CurrentPipeInfo.TheConsole.Update();
                        }

                        CurrentPipeInfo.TheConsole.Write(CurrentPipeInfo.StdOut.Read(false));
                    }
                }
                catch
                {
                    if (ExceptionMethods.CurrentException is Pipes.Exceptions.RWFailedException)
                    {
                        SystemCalls.SleepThread(50);
                    }
                    else
                    {
                        BasicConsole.WriteLine("WM > Exception running window manager.");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }
                }
            }
        }

        public static void TestThread()
        {
            int MessageCount = 0;
            while (MessageCount < 5)
            {
                SystemCalls.SleepThread(3000);
                if (AcceptedPages_Count > 0)
                {
                    char* TextPtr = (char*)AcceptedPages_StartAddress;
                    while (*TextPtr == '\0')
                    {
                        SystemCalls.SleepThread(50);
                    }
                    TextPtr++;
                    while (*TextPtr != '\0')
                    {
                        BasicConsole.Write(*TextPtr++);
                    }
                    TextPtr = (char*)AcceptedPages_StartAddress;
                    *TextPtr = '\0';
                    MessageCount++;
                }
            }

            {
                char* TextPtr = (char*)AcceptedPages_StartAddress;
                while (*TextPtr == '\0')
                {
                    SystemCalls.SleepThread(50);
                }

                int* IdPtr = (int*)(TextPtr + 1);
                int SemaphoreId = *IdPtr;
                BasicConsole.WriteLine("Waiting on semaphore");
                SystemCalls.WaitSemaphore(SemaphoreId);
                BasicConsole.WriteLine("Wait completed");
            }

            SystemCalls.UnmapPages(AcceptedPages_StartAddress, AcceptedPages_Count);
            BasicConsole.WriteLine("Test thread exiting.");
        }

        public static void InputProcessing()
        {
            ready_count++;

            while (!Terminating)
            {
                //BasicConsole.WriteLine("WM > IP : (0)");

                if (!InputProcessingThreadAwake)
                {
                    SystemCalls.SleepThread(10000);
                }
                InputProcessingThreadAwake = false;

                BasicConsole.WriteLine("WM > InputProcessing thread running...");

                int numOutpoints;
                SystemCallResults SysCallResult;
                Pipes.BasicOutpoint.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out);

                //BasicConsole.WriteLine("WM > IP : (1)");

                if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                {
                    //BasicConsole.WriteLine("WM > IP : (2)");

                    Pipes.PipeOutpointDescriptor[] OutpointDescriptors;
                    Pipes.BasicOutpoint.GetOutpointDescriptors(numOutpoints, out SysCallResult, out OutpointDescriptors, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out);

                    //BasicConsole.WriteLine("WM > IP : (3)");

                    if (SysCallResult == SystemCallResults.OK)
                    {
                        //BasicConsole.WriteLine("WM > IP : (4)");

                        for (int i = 0; i < OutpointDescriptors.Length; i++)
                        {
                            //BasicConsole.WriteLine("WM > IP : (5)");

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
                                    //BasicConsole.WriteLine("WM > IP : (6)");

                                    PipeInfo NewPipeInfo = new PipeInfo();
                                    NewPipeInfo.StdOut = new Pipes.Standard.StandardInpoint(Descriptor.ProcessId, true); // 2000 ASCII characters = 2000 bytes

                                    //BasicConsole.WriteLine("WM > IP : (7)");

                                    ConnectedPipes.Add(NewPipeInfo);

                                    if (CurrentPipeIdx == -1)
                                    {
                                        CurrentPipeIdx = 0;
                                        CurrentPipeIndex_Changed = true;

                                        SystemCalls.WakeThread(MainThreadId);
                                        SystemCalls.WakeThread(OutputProcessingThreadId);
                                    }

                                    //BasicConsole.WriteLine("WM > IP : (8)");

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
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.RegisterPipeOutpoint:
                    {
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
                    }
                    break;
                case SystemCallNumbers.AcceptPages:
                    {
                        BasicConsole.WriteLine("WM > Accept pages");
                        AcceptedPages_StartAddress = param1;
                        AcceptedPages_Count = param2;
                        AcceptedPages_FromProcessId = callerProcessId;
                        result = SystemCallResults.OK;
                    }
                    break;
            }

            return (int)result;
        }

        public static void OutputProcessing()
        {
            ready_count++;

            // Wait for pipe to be created
            SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);

            PipeInfo CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);

            while (!Terminating)
            {
                try
                {
                    //BasicConsole.WriteLine("WM > OP : (0)");

                    bool AltPressed = Keyboard.Default.AltPressed;
                    uint Scancode;
                    bool GotScancode = Keyboard.Default.GetScancode(out Scancode);
                    if (GotScancode)
                    {
                        //BasicConsole.WriteLine("WM > OP : (1)");

                        KeyboardKey Key;
                        if (Keyboard.Default.GetKeyValue(Scancode, out Key))
                        {
                            //BasicConsole.WriteLine("WM > OP : (2)");

                            if (AltPressed && Key == KeyboardKey.Tab)
                            {
                                //BasicConsole.WriteLine("WM > OP : (3)");

                                CurrentPipeIdx++;
                                if (CurrentPipeIdx >= ConnectedPipes.Count)
                                {
                                    CurrentPipeIdx = 0;
                                }

                                CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);
                                CurrentPipeIndex_Changed = true;

                                //BasicConsole.WriteLine("WM > OP : (4)");
                            }
                            else
                            {
                                //BasicConsole.WriteLine("WM > OP : (5)");

                                SystemCalls.SendMessage(((PipeInfo)ConnectedPipes[CurrentPipeIdx]).StdOut.OutProcessId, Scancode, 0);

                                //BasicConsole.WriteLine("WM > OP : (6)");
                            }
                        }
                    }
                    else
                    {
                        SystemCalls.SleepThread(50);
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
