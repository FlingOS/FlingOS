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

using System.Diagnostics.CodeAnalysis;
using Drivers.Compiler.Attributes;
using Kernel.Consoles;
using Kernel.Devices;
using Kernel.Devices.Keyboards;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes;
using Kernel.Pipes.Exceptions;
using Kernel.Pipes.Standard;
using Kernel.VGA.Configurations.Graphical;
using Kernel.VGA.Fonts;

namespace Kernel.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private static readonly bool Terminating = false;

        private static List ConnectedPipes;
        private static int CurrentPipeIdx = -1;
        
        private static uint GCThreadId;
        private static int NewOutpointAvailable_SemaphoreId;
        private static int NewPipeConnected_SemaphoreId;
        private static int NewClientReady_SemaphoreId;

        [Group(Name = "IsolatedKernel")] private static int ready_count;

        private static bool CurrentPipeIndex_Changed;

        private static uint AcceptedPages_StartAddress;
        private static uint AcceptedPages_Count;
        private static uint AcceptedPages_FromProcessId;

        private static int ConsoleAccessSemaphoreId;

        public static bool Ready
        {
            get { return ready_count == 3; }
        }

        public static void Main()
        {
            Helpers.ProcessInit("Window Manager", out GCThreadId);

            DeviceManager.InitForProcess();
            
            // Initialise connected pipes list
            ConnectedPipes = new List();

            if (SystemCalls.CreateSemaphore(-1, out NewOutpointAvailable_SemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("WM > Failed to create a semaphore! (1)");
            }
            if (SystemCalls.CreateSemaphore(-1, out NewPipeConnected_SemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("WM > Failed to create a semaphore! (2)");
            }
            if (SystemCalls.CreateSemaphore(-1, out NewClientReady_SemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("WM > Failed to create a semaphore! (3)");
            }
            if (SystemCalls.CreateSemaphore(1, out ConsoleAccessSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("WM > Failed to create a semaphore! (4)");
            }

            BasicConsole.WriteLine("WM > Init keyboard");
            PS2.Init();
            Keyboard.Default = PS2.SingletonPS2;

            // Start thread for handling background input processing
            uint InputProcessingThreadId;
            if (SystemCalls.StartThread(InputProcessing, out InputProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: InputProcessing thread failed to create!");
            }
            BasicConsole.Write("WM > InputProcessing thread id: ");
            BasicConsole.WriteLine(InputProcessingThreadId);

            BasicConsole.WriteLine("WM > Register syscall handlers");
            SystemCalls.RegisterSyscallHandler(SystemCallNumbers.RegisterPipeOutpoint, SyscallHandler);

            // Start thread for other testing
            //uint TestThreadId;
            //if (SystemCalls.StartThread(TestThread, out TestThreadId) != SystemCallResults.OK)
            //{
            //    BasicConsole.WriteLine("Window Manager: Test thread failed to create!");
            //}
            //BasicConsole.Write("WM > Test thread id: ");
            //BasicConsole.WriteLine(TestThreadId);
            //SystemCalls.RegisterSyscallHandler(SystemCallNumbers.AcceptPages);


            // Start thread for handling background output processing
            uint OutputProcessingThreadId;
            if (SystemCalls.StartThread(OutputProcessing, out OutputProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: OutputProcessing thread failed to create!");
            }

            BasicConsole.WriteLine("WM > Register IRQ 1 handler");
            SystemCalls.RegisterIRQHandler(1, HandleIRQ);


            BasicConsole.WriteLine("Setting up VGA...");
            VGA.VGA.SetConfiguration(T_80x25.Instance, Jupitor.Instance);

            //BasicConsole.WriteLine("Testing VGA Graphical modes...");
            //TheVGA.TestMode_G_640x480x4();
            //TheVGA.TestMode_G_720x480x4();
            //VGA.Buffering.FourPlaneBuffersTest.Test1();
            //BasicConsole.WriteLine("Test finished.");

            //BasicConsole.WriteLine("Resetting graphics adapter...");
            //TheVGA.LoadConfiguration(T_80x25.Instance);
            //TheVGA.LoadFont(Jupitor.Instance);
            //TheVGA.SetCGAPalette();


            BasicConsole.WriteLine("WM > Wait for pipe to be created");
            // Wait for pipe to be created
            ready_count++;
            SystemCalls.WaitSemaphore(NewClientReady_SemaphoreId);

            PipeInfo CurrentPipeInfo = null;

            while (!Terminating)
            {
                try
                {
                    if (CurrentPipeIdx > -1)
                    {
                        SystemCalls.WaitSemaphore(ConsoleAccessSemaphoreId);
                        try
                        {
                            if (CurrentPipeIndex_Changed)
                            {
                                CurrentPipeInfo = (PipeInfo)ConnectedPipes[CurrentPipeIdx];
                                CurrentPipeIndex_Changed = false;

                                CurrentPipeInfo.TheConsole.Update();
                            }
                        }
                        finally
                        {
                            SystemCalls.SignalSemaphore(ConsoleAccessSemaphoreId);
                        }

                        if (CurrentPipeInfo != null)
                        {
                            //BasicConsole.WriteLine((String)"WM > Start blocking read... PipeId: " + CurrentPipeInfo.StdOut.PipeId);
                            String str = CurrentPipeInfo.StdOut.Read(true);
                            //BasicConsole.WriteLine("WM > Blocking read ended.");

                            SystemCalls.WaitSemaphore(ConsoleAccessSemaphoreId);
                            try
                            {
                                CurrentPipeInfo.TheConsole.Scroll(CurrentPipeInfo.TheConsole.ScreenHeight);
                                CurrentPipeInfo.TheConsole.Write(str);
                            }
                            finally
                            {
                                SystemCalls.SignalSemaphore(ConsoleAccessSemaphoreId);
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("WM > CurrentPipeInfo null but CurrentPipeIdx > -1??");
                            SystemCalls.SleepThread(50);
                        }
                    }
                }
                catch
                {
                    if (ExceptionMethods.CurrentException is RWFailedException)
                    {
                        //BasicConsole.WriteLine("WM > Read/Write Exception handled (expected due to abort?)");
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

        [SuppressMessage("ReSharper", "LoopVariableIsNeverChangedInsideLoop")]
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

                SystemCalls.WaitSemaphore(NewOutpointAvailable_SemaphoreId);

                //BasicConsole.WriteLine("WM > InputProcessing thread running...");

                int numOutpoints;
                SystemCallResults SysCallResult;
                BasicOutpoint.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, PipeClasses.Standard,
                    PipeSubclasses.Standard_Out);

                //BasicConsole.WriteLine("WM > IP : (1)");

                if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                {
                    //BasicConsole.WriteLine("WM > IP : (2)");

                    PipeOutpointDescriptor[] OutpointDescriptors;
                    BasicOutpoint.GetOutpointDescriptors(numOutpoints, out SysCallResult, out OutpointDescriptors,
                        PipeClasses.Standard, PipeSubclasses.Standard_Out);

                    //BasicConsole.WriteLine("WM > IP : (3)");

                    if (SysCallResult == SystemCallResults.OK)
                    {
                        //BasicConsole.WriteLine("WM > IP : (4)");

                        for (int i = 0; i < OutpointDescriptors.Length; i++)
                        {
                            //BasicConsole.WriteLine("WM > IP : (5)");

                            PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
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

                            if (!PipeExists)
                            {
                                try
                                {
                                    //BasicConsole.WriteLine("WM > IP : (6)");

                                    PipeInfo NewPipeInfo = new PipeInfo();
                                    NewPipeInfo.StdOut = new StandardInpoint(Descriptor.ProcessId, true);
                                    // 2000 ASCII characters = 2000 bytes

                                    //BasicConsole.WriteLine("WM > IP : (7)");

                                    ConnectedPipes.Add(NewPipeInfo);

                                    if (CurrentPipeIdx == -1)
                                    {
                                        CurrentPipeIdx = 0;
                                        CurrentPipeIndex_Changed = true;

                                        SystemCalls.SignalSemaphore(NewClientReady_SemaphoreId);
                                        SystemCalls.SignalSemaphore(NewPipeConnected_SemaphoreId);
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
                    //BasicConsole.WriteLine("WM > IH > Actioning Register Pipe Outpoint system call...");
                    PipeClasses Class = (PipeClasses)param1;
                    PipeSubclasses Subclass = (PipeSubclasses)param2;
                    if (Class == PipeClasses.Standard &&
                        Subclass == PipeSubclasses.Standard_Out)
                    {
                        //BasicConsole.WriteLine("WM > IH > Register Pipe Outpoint has desired pipe class and subclass.");
                        result = SystemCallResults.RequestAction_SignalSemaphore;
                        Return2 = (uint)NewOutpointAvailable_SemaphoreId;
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
            SystemCalls.WaitSemaphore(NewPipeConnected_SemaphoreId);

            PipeInfo CurrentPipeInfo = (PipeInfo)ConnectedPipes[CurrentPipeIdx];

            uint[] TabKeyScancodes = Keyboard.Default.GetScancodesForKey(KeyboardKey.Tab);
            uint[] UpArrowKeyScancodes = Keyboard.Default.GetScancodesForKey(KeyboardKey.UpArrow);
            uint[] DownArrowKeyScancodes = Keyboard.Default.GetScancodesForKey(KeyboardKey.DownArrow);

            while (!Terminating)
            {
                try
                {
                    //BasicConsole.WriteLine("WM > OP : (0)");
                    
                    uint Scancode = Keyboard.Default.ReadScancode();
                    bool AltPressed = Keyboard.Default.AltPressed;
                    //BasicConsole.WriteLine("WM > OP : (1)");
                    
                        //BasicConsole.WriteLine("WM > OP : (2)");

                    if (AltPressed && ContainsScancode(TabKeyScancodes, Scancode))
                    {
                        //BasicConsole.WriteLine("WM > OP : (3)");

                        CurrentPipeIdx++;
                        if (CurrentPipeIdx >= ConnectedPipes.Count)
                        {
                            CurrentPipeIdx = 0;
                        }

                        SystemCalls.WaitSemaphore(ConsoleAccessSemaphoreId);

                        // Abort the blocking read (if any) that was issued on the current pipe
                        //BasicConsole.WriteLine((String)"WM > Issuing abort blocking read... PipeId: " + CurrentPipeInfo.StdOut.PipeId);
                        SystemCallResults Results = SystemCalls.AbortPipeReadWrite(CurrentPipeInfo.StdOut.PipeId);
                        //BasicConsole.WriteLine((String)"WM > Abort blocking read complete: " + (Results == SystemCallResults.OK ? "Aborted." : "Failed to abort."));

                        try
                        {
                            CurrentPipeInfo = (PipeInfo)ConnectedPipes[CurrentPipeIdx];
                            CurrentPipeIndex_Changed = true;

                            //BasicConsole.WriteLine("WM > Window switched.");
                        }
                        finally
                        {
                            SystemCalls.SignalSemaphore(ConsoleAccessSemaphoreId);
                        }

                        //BasicConsole.WriteLine("WM > OP : (4)");
                    }
                    else
                    {
                        if (ContainsScancode(UpArrowKeyScancodes, Scancode))
                        {
                            SystemCalls.WaitSemaphore(ConsoleAccessSemaphoreId);
                            try
                            {
                                CurrentPipeInfo.TheConsole.Scroll(-1);
                            }
                            finally
                            {
                                SystemCalls.SignalSemaphore(ConsoleAccessSemaphoreId);
                            }
                        }
                        else if (ContainsScancode(DownArrowKeyScancodes, Scancode))
                        {
                            SystemCalls.WaitSemaphore(ConsoleAccessSemaphoreId);
                            try
                            {
                                CurrentPipeInfo.TheConsole.Scroll(+1);
                            }
                            finally
                            {
                                SystemCalls.SignalSemaphore(ConsoleAccessSemaphoreId);
                            }
                        }

                        //BasicConsole.WriteLine("WM > OP : (5)");

                        SystemCalls.SendMessage(
                            ((PipeInfo)ConnectedPipes[CurrentPipeIdx]).StdOut.OutProcessId, Scancode, 0);

                        //BasicConsole.WriteLine("WM > OP : (6)");
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("WM > Error during output processing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        [NoGC]
        private static bool ContainsScancode(uint[] Scancodes, uint Scancode)
        {
            for (int i = 0; i < Scancodes.Length; i++)
            {
                if (Scancodes[i] == Scancode)
                {
                    return true;
                }
            }
            return false;
        }

        private static int HandleIRQ(uint IRQNum)
        {
            if (IRQNum == 1)
            {
                //BasicConsole.WriteLine("Keyboard interrupt");
                ((PS2)Keyboard.Default).InterruptHandler();
                return 0;
            }
            return -1;
        }

        private class PipeInfo : Object
        {
            public readonly Console TheConsole = new VGAConsole();
            public StandardInpoint StdOut;
        }
    }
}