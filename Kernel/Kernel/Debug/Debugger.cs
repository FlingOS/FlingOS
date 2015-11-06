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

#define DEBUGGER_INTERUPT_TRACE
#undef DEBUGGER_INTERUPT_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.IO.Serial;
using Kernel.Hardware.Processes;
using Kernel.Hardware.VirtMem;
using Kernel.Processes;

using NoDebug = Drivers.Compiler.Attributes.NoDebugAttribute;
using NoGC = Drivers.Compiler.Attributes.NoGCAttribute;

namespace Kernel.Debug
{
    public static unsafe class Debugger
    {
        public static bool Terminating = false;
        private static bool Enabled = false;

        private static Serial DebugPort;

        public static Thread MainThread;

        private static UInt64List ThreadsToSuspend = new UInt64List();

        [NoDebug]
        [NoGC]
        public static void Int1()
        {
            // Clear trap flag
            ProcessManager.CurrentThread.EFLAGSFromInterruptStack &= ~0x0100u;

            PauseCurrentThread();
        }
        [NoDebug]
        [NoGC]
        public static void Int3()
        {
            PauseCurrentThread();
        }
        [NoDebug]
        [NoGC]
        private static void PauseCurrentThread()
        {
            Hardware.Interrupts.Interrupts.InsideCriticalHandler = true;

            try
            {
#if DEBUGGER_INTERUPT_TRACE
                DebugPort.Write("Pause current thread:\n");

                DebugPort.Write(" > Current process: ");
                DebugPort.Write(ProcessManager.CurrentProcess.Name);
                DebugPort.Write("\n");

                FOS_System.String espStr = " > Current ESP : 0x--------\n";
                ExceptionMethods.FillString((uint)ExceptionMethods.StackPointer, 26, espStr);
                DebugPort.Write(espStr);
                FOS_System.String ebpStr = " > Current EBP : 0x--------\n";
                ExceptionMethods.FillString((uint)ExceptionMethods.BasePointer, 26, ebpStr);
                DebugPort.Write(ebpStr);

                DebugPort.Write(" > Checking thread is not debugger thread...\n");
#endif
                if (ProcessManager.CurrentThread != MainThread)
                {
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Disabling debugger\n");
#endif
                    Enabled = false;
                    
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Calculating process id\n");
#endif
                    UInt64 ProcessThreadId = (((UInt64)ProcessManager.CurrentProcess.Id) << 32) | ProcessManager.CurrentThread.Id;
                    
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Checking suspended threads list\n");
#endif
                    if (ThreadsToSuspend.IndexOf(ProcessThreadId) > -1)
                    {
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > Pausing");
                        PrintCurrentThread();
#endif

                        ProcessManager.CurrentThread.Debug_Suspend = true;
                        
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > Doing scheduler update...\n");
#endif 

                        Scheduler.UpdateCurrentState();
                        
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > New");
                        PrintCurrentThread();
#endif
                    }
                    
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Enabling debugger\n");
#endif
                    Enabled = true;
                }
            }
            finally
            {
                Hardware.Interrupts.Interrupts.InsideCriticalHandler = false;
            }
        }
        [NoDebug]
        [NoGC]
        private static void PrintCurrentThread()
        {
            DebugPort.Write(" ");
            DebugPort.Write(ProcessManager.CurrentThread.Name);
            DebugPort.Write(" thread of ");
            DebugPort.Write(ProcessManager.CurrentProcess.Name);
            DebugPort.Write("\n");
        }
        
        [NoDebug]
        [NoGC]
        public static void Main()
        {
            DebugPort = Serial.COM2;

            DebugPort.Write("Enabling...\n");
            Enabled = true;
            DebugPort.Write("Enabled.\n");

            DebugPort.Write("Debug thread :D\n");

            while (!Terminating)
            {
                FOS_System.String line = "";
                char c = (char)DebugPort.Read();
                while (c != '\n' && c != '\r')
                {
                    line += c;
                    DebugPort.Write((byte)c);

                    c = (char)DebugPort.Read();
                }
                DebugPort.Write("\n");

                line = line.Trim().ToLower();
                
                List lineParts = line.Split(' ');
                if (lineParts.Count > 0)
                {
                    FOS_System.String cmd = (FOS_System.String)lineParts[0];

                    if (cmd == "ping")
                    {
                        DebugPort.Write("pong\n");
                    }
                    else if (cmd == "help")
                    {
                        DebugPort.Write("Available commands:\n");
                        DebugPort.Write(" - ping\n");
                        DebugPort.Write(" - threads\n");
                        DebugPort.Write(" - suspend (processId) (threadId | -1 for all threads)\n");
                        DebugPort.Write(" - resume (processId) (threadId | -1 for all threads)\n");
                        DebugPort.Write(" - step (processId) (threadId | -1 for all threads)\n");
                        DebugPort.Write(" - ss (processId) (threadId | -1 for all threads)\n");
                        DebugPort.Write(" - bps (address:hex)\n");
                        DebugPort.Write(" - bpc (address:hex)\n");
                        DebugPort.Write(" - regs (processId) (threadId)\n");
                        DebugPort.Write(" - memory (processId) (address:hex) (length) (units:1,2,4)\n");
                    }
                    else if (cmd == "threads")
                    {
                        for (int i = 0; i < ProcessManager.Processes.Count; i++)
                        {
                            Process AProcess = (Process)ProcessManager.Processes[i];
                            DebugPort.Write(" - Process : ");
                            DebugPort.Write((FOS_System.String)AProcess.Id);
                            DebugPort.Write(" : ");
                            DebugPort.Write(AProcess.Name);
                            DebugPort.Write("\n");
                            for (int j = 0; j < AProcess.Threads.Count; j++)
                            {
                                Thread AThread = (Thread)AProcess.Threads[j];

                                DebugPort.Write("      - Thread : ");
                                DebugPort.Write((FOS_System.String)AThread.Id);
                                DebugPort.Write(" : ");
                                if (AThread.Debug_Suspend)
                                {
                                    DebugPort.Write("Suspended");
                                }
                                else if (AThread.TimeToSleep == -1)
                                {
                                    DebugPort.Write("Waiting  ");
                                }
                                else if (AThread.TimeToSleep > 0)
                                {
                                    DebugPort.Write("Sleeping ");
                                }
                                else
                                {
                                    DebugPort.Write("Running  ");
                                }
                                DebugPort.Write(" : ");
                                DebugPort.Write(AThread.Name);
                                DebugPort.Write("\n");
                            }
                        }
                    }
                    else if (cmd == "suspend")
                    {
                        if (lineParts.Count == 3)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            int ThreadId = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[2]);

                            Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                            if (TheProcess != null)
                            {
                                if (ThreadId == -1)
                                {
                                    for (int i = 0; i < TheProcess.Threads.Count; i++)
                                    {
                                        Thread AThread = ((Thread)TheProcess.Threads[i]);

                                        if (AThread != MainThread)
                                        {
                                            UInt64 ProcessThreadId = (((UInt64)ProcessId) << 32) | AThread.Id;
                                            if (ThreadsToSuspend.IndexOf(ProcessThreadId) == -1)
                                            {
                                                ThreadsToSuspend.Add(ProcessThreadId);
                                            }

                                            AThread.Debug_Suspend = true;

                                            DebugPort.Write(" > Suspended ");
                                            DebugPort.Write(AThread.Name);
                                            DebugPort.Write(" thread\n");
                                        }
                                    }
                                }
                                else
                                {
                                    UInt64 ProcessThreadId = (((UInt64)ProcessId) << 32) | (uint)ThreadId;
                                    if (ThreadsToSuspend.IndexOf(ProcessThreadId) == -1)
                                    {
                                        ThreadsToSuspend.Add(ProcessThreadId);
                                    }

                                    Thread TheThread = ProcessManager.GetThreadById((uint)ThreadId, TheProcess);

                                    if (TheThread != null)
                                    {
                                        TheThread.Debug_Suspend = true;

                                        DebugPort.Write(" > Suspended ");
                                        DebugPort.Write(TheThread.Name);
                                        DebugPort.Write(" thread\n");
                                    }
                                    else
                                    {
                                        DebugPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                DebugPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "resume")
                    {
                        if (lineParts.Count == 3)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            int ThreadId = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[2]);

                            Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                            if (TheProcess != null)
                            {
                                if (ThreadId == -1)
                                {
                                    for (int i = 0; i < TheProcess.Threads.Count; i++)
                                    {
                                        Thread AThread = ((Thread)TheProcess.Threads[i]);

                                        UInt64 ProcessThreadId = (((UInt64)ProcessId) << 32) | AThread.Id;
                                        ThreadsToSuspend.Remove(ProcessThreadId);

                                        AThread.Debug_Suspend = false;

                                        DebugPort.Write(" > Resumed ");
                                        DebugPort.Write(AThread.Name);
                                        DebugPort.Write(" thread\n");
                                    }
                                }
                                else
                                {
                                    UInt64 ProcessThreadId = (((UInt64)ProcessId) << 32) | (uint)ThreadId;
                                    ThreadsToSuspend.Remove(ProcessThreadId);

                                    Thread TheThread = ProcessManager.GetThreadById((uint)ThreadId, TheProcess);

                                    if (TheThread != null)
                                    {
                                        TheThread.Debug_Suspend = false;

                                        DebugPort.Write(" > Resumed ");
                                        DebugPort.Write(TheThread.Name);
                                        DebugPort.Write(" thread\n");
                                    }
                                    else
                                    {
                                        DebugPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                DebugPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "step")
                    {
                        if (lineParts.Count == 3)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            int ThreadId = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[2]);

                            Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                            if (TheProcess != null)
                            {
                                if (ThreadId == -1)
                                {
                                    for (int i = 0; i < TheProcess.Threads.Count; i++)
                                    {
                                        Thread AThread = ((Thread)TheProcess.Threads[i]);

                                        if (AThread.Debug_Suspend)
                                        {
                                            AThread.Debug_Suspend = false;

                                            DebugPort.Write(" > Stepping ");
                                            DebugPort.Write(AThread.Name);
                                            DebugPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            DebugPort.Write("Thread must be suspended first.\n");
                                        }
                                    }
                                }
                                else
                                {
                                    Thread TheThread = ProcessManager.GetThreadById((uint)ThreadId, TheProcess);

                                    if (TheThread != null)
                                    {
                                        if (TheThread.Debug_Suspend)
                                        {
                                            TheThread.Debug_Suspend = false;

                                            DebugPort.Write(" > Stepping ");
                                            DebugPort.Write(TheThread.Name);
                                            DebugPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            DebugPort.Write("Thread must be suspended first.\n");
                                        }
                                    }
                                    else
                                    {
                                        DebugPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                DebugPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "ss")
                    {
                        if (lineParts.Count == 3)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            int ThreadId = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[2]);

                            Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                            if (TheProcess != null)
                            {
                                if (ThreadId == -1)
                                {
                                    for (int i = 0; i < TheProcess.Threads.Count; i++)
                                    {
                                        Thread AThread = ((Thread)TheProcess.Threads[i]);

                                        if (AThread.Debug_Suspend)
                                        {
                                            // Set trap flag (int1)
                                            AThread.EFLAGSFromInterruptStack |= 0x0100;
                                            AThread.Debug_Suspend = false;

                                            DebugPort.Write(" > Single stepping ");
                                            DebugPort.Write(AThread.Name);
                                            DebugPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            DebugPort.Write("Thread must be suspended first.\n");
                                        }
                                    }
                                }
                                else
                                {
                                    Thread TheThread = ProcessManager.GetThreadById((uint)ThreadId, TheProcess);

                                    if (TheThread != null)
                                    {
                                        if (TheThread.Debug_Suspend)
                                        {
                                            // Set trap flag (int1)
                                            TheThread.EFLAGSFromInterruptStack |= 0x0100;
                                            TheThread.Debug_Suspend = false;

                                            DebugPort.Write(" > Single stepping ");
                                            DebugPort.Write(TheThread.Name);
                                            DebugPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            DebugPort.Write("Thread must be suspended first.\n");
                                        }
                                    }
                                    else
                                    {
                                        DebugPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                DebugPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "bps")
                    {
                        if (lineParts.Count == 2)
                        {
                            uint Address = FOS_System.Int32.Parse_HexadecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            
                            DebugPort.Write(" > Breakpoint to be set at ");
                            DebugPort.Write(Address);
                            DebugPort.Write("\n");
                            
                            *((byte*)Address) = 0xCC;
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "bpc")
                    {
                        if (lineParts.Count == 2)
                        {
                            uint Address = FOS_System.Int32.Parse_HexadecimalUnsigned((FOS_System.String)lineParts[1], 0);

                            DebugPort.Write(" > Breakpoint to be cleared at ");
                            DebugPort.Write(Address);
                            DebugPort.Write("\n");

                            *((byte*)Address) = 0x90;
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "regs")
                    {
                        if (lineParts.Count == 3)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            uint ThreadId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[2], 0);

                            Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                            if (TheProcess != null)
                            {
                                Thread TheThread = ProcessManager.GetThreadById((uint)ThreadId, TheProcess);

                                if (TheThread != null)
                                {
                                    if (TheThread.Debug_Suspend)
                                    {
                                        DebugPort.Write("Registers of ");
                                        DebugPort.Write(TheThread.Name);
                                        DebugPort.Write(" : \n");

                                        DebugPort.Write(" > EAX : ");
                                        DebugPort.Write((FOS_System.String)TheThread.EAXFromInterruptStack);
                                        DebugPort.Write("\n > EBX : ");
                                        DebugPort.Write((FOS_System.String)TheThread.EBXFromInterruptStack);
                                        DebugPort.Write("\n > ECX : ");
                                        DebugPort.Write((FOS_System.String)TheThread.ECXFromInterruptStack);
                                        DebugPort.Write("\n > EDX : ");
                                        DebugPort.Write((FOS_System.String)TheThread.EDXFromInterruptStack);
                                        DebugPort.Write("\n");

                                        DebugPort.Write("\n > ESP : ");
                                        DebugPort.Write((FOS_System.String)TheThread.ESPFromInterruptStack);
                                        DebugPort.Write("\n > EBP : ");
                                        DebugPort.Write((FOS_System.String)TheThread.EBPFromInterruptStack);
                                        DebugPort.Write("\n > EIP : ");
                                        DebugPort.Write((FOS_System.String)TheThread.EIPFromInterruptStack);
                                        DebugPort.Write("\n");
                                    }
                                    else
                                    {
                                        DebugPort.Write("Thread must be suspended first.\n");
                                    }
                                }
                                else
                                {
                                    DebugPort.Write("Thread not found.\n");
                                }
                            }
                            else
                            {
                                DebugPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "mem")
                    {
                        if (lineParts.Count == 5)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            
                            Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                            if (TheProcess != null)
                            {
                                // Need access to specified process' memory
                                MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(TheProcess);

                                uint Address = FOS_System.Int32.Parse_HexadecimalUnsigned((FOS_System.String)lineParts[2], 0);
                                int length = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[3]);
                                int units = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[4]);

                                DebugPort.Write(" Memory from ");
                                DebugPort.Write(Address);
                                DebugPort.Write(" for ");
                                DebugPort.Write(length);
                                DebugPort.Write(" units with ");
                                DebugPort.Write(units);
                                DebugPort.Write(" bytes/unit:\n");

                                if (units == 1)
                                {
                                    byte* AddrPtr = (byte*)Address;
                                    for (int i = 0; i < length; i++)
                                    {
                                        DebugPort.Write((FOS_System.String)AddrPtr[i]);
                                        DebugPort.Write(" ");
                                    }
                                }
                                else if (units == 2)
                                {
                                    ushort* AddrPtr = (ushort*)Address;
                                    for (int i = 0; i < length; i++)
                                    {
                                        DebugPort.Write((FOS_System.String)AddrPtr[i]);
                                        DebugPort.Write(" ");
                                    }
                                }
                                else if (units == 4)
                                {
                                    uint* AddrPtr = (uint*)Address;
                                    for (int i = 0; i < length; i++)
                                    {
                                        DebugPort.Write((FOS_System.String)AddrPtr[i]);
                                        DebugPort.Write(" ");
                                    }
                                }

                                DebugPort.Write("\n");

                                SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);
                            }
                            else
                            {
                                DebugPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            DebugPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else
                    {
                        DebugPort.Write("Unrecognised command. (Note: Backspace not supported!)\n");
                    }
                }

                DebugPort.Write("END OF COMMAND\n");
            }
        }
        
    }
}
