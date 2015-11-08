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

        private static Serial MsgPort;
        private static Serial NotifPort;

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

#if DEBUG
                    bool ShouldSuspend = ThreadsToSuspend.IndexOf(ProcessThreadId) > -1 ||
                                         ExceptionMethods.CurrentException != null;
#else
                    bool ShouldSuspend = ThreadsToSuspend.IndexOf(ProcessThreadId) > -1;
#endif

                    if (ShouldSuspend)
                    {
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > Pausing");
                        PrintCurrentThread();
#endif

                        ProcessManager.CurrentThread.Debug_Suspend = true;
                        NotifPort.Write(0xFE);
                        
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
            MsgPort.Write(" ");
            MsgPort.Write(ProcessManager.CurrentThread.Name);
            MsgPort.Write(" thread of ");
            MsgPort.Write(ProcessManager.CurrentProcess.Name);
            MsgPort.Write("\n");
        }
        
        [NoDebug]
        [NoGC]
        public static void Main()
        {
            MsgPort = Serial.COM2;
            NotifPort = Serial.COM3;

            MsgPort.Write("Debug thread :D\n");

            //MsgPort.Write("Enabling...\n");
            Enabled = true;
            //MsgPort.Write("Enabled.\n");

            while (!Terminating)
            {
                FOS_System.String line = "";
                char c = (char)MsgPort.Read();
                while (c != '\n' && c != '\r')
                {
                    line += c;
                    c = (char)MsgPort.Read();
                }
                
                MsgPort.Write("START OF COMMAND\n");
                MsgPort.Write(line);
                MsgPort.Write("\n");

                line = line.Trim().ToLower();
                
                List lineParts = line.Split(' ');
                if (lineParts.Count > 0)
                {
                    FOS_System.String cmd = (FOS_System.String)lineParts[0];

                    if (cmd == "ping")
                    {
                        MsgPort.Write("pong\n");
                    }
                    else if (cmd == "help")
                    {
                        MsgPort.Write("Available commands:\n");
                        MsgPort.Write(" - ping\n");
                        MsgPort.Write(" - threads\n");
                        MsgPort.Write(" - suspend (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - resume (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - step (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - ss (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - bps (address:hex)\n");
                        MsgPort.Write(" - bpc (address:hex)\n");
                        MsgPort.Write(" - regs (processId) (threadId)\n");
                        MsgPort.Write(" - memory (processId) (address:hex) (length) (units:1,2,4)\n");
                    }
                    else if (cmd == "threads")
                    {
                        for (int i = 0; i < ProcessManager.Processes.Count; i++)
                        {
                            Process AProcess = (Process)ProcessManager.Processes[i];
                            MsgPort.Write(" - Process : ");
                            MsgPort.Write((FOS_System.String)AProcess.Id);
                            MsgPort.Write(" : ");
                            MsgPort.Write(AProcess.Name);
                            MsgPort.Write("\n");
                            for (int j = 0; j < AProcess.Threads.Count; j++)
                            {
                                Thread AThread = (Thread)AProcess.Threads[j];

                                MsgPort.Write("      - Thread : ");
                                MsgPort.Write((FOS_System.String)AThread.Id);
                                MsgPort.Write(" : ");
                                if (AThread.Debug_Suspend)
                                {
                                    MsgPort.Write("Suspended");
                                }
                                else if (AThread.TimeToSleep == -1)
                                {
                                    MsgPort.Write("Waiting  ");
                                }
                                else if (AThread.TimeToSleep > 0)
                                {
                                    MsgPort.Write("Sleeping ");
                                }
                                else
                                {
                                    MsgPort.Write("Running  ");
                                }
                                MsgPort.Write(" : ");
                                MsgPort.Write(AThread.Name);
                                MsgPort.Write("\n");
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

                                            MsgPort.Write(" > Suspended ");
                                            MsgPort.Write(AThread.Name);
                                            MsgPort.Write(" thread\n");
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
                                        if (TheThread != MainThread)
                                        {
                                            TheThread.Debug_Suspend = true;

                                            MsgPort.Write(" > Suspended ");
                                            MsgPort.Write(TheThread.Name);
                                            MsgPort.Write(" thread\n");
                                        }
                                    }
                                    else
                                    {
                                        MsgPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                MsgPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
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

                                        MsgPort.Write(" > Resumed ");
                                        MsgPort.Write(AThread.Name);
                                        MsgPort.Write(" thread\n");
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

                                        MsgPort.Write(" > Resumed ");
                                        MsgPort.Write(TheThread.Name);
                                        MsgPort.Write(" thread\n");
                                    }
                                    else
                                    {
                                        MsgPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                MsgPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
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

                                            MsgPort.Write(" > Stepping ");
                                            MsgPort.Write(AThread.Name);
                                            MsgPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            MsgPort.Write("Thread must be suspended first.\n");
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

                                            MsgPort.Write(" > Stepping ");
                                            MsgPort.Write(TheThread.Name);
                                            MsgPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            MsgPort.Write("Thread must be suspended first.\n");
                                        }
                                    }
                                    else
                                    {
                                        MsgPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                MsgPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
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

                                            MsgPort.Write(" > Single stepping ");
                                            MsgPort.Write(AThread.Name);
                                            MsgPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            MsgPort.Write("Thread must be suspended first.\n");
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

                                            MsgPort.Write(" > Single stepping ");
                                            MsgPort.Write(TheThread.Name);
                                            MsgPort.Write(" thread\n");
                                        }
                                        else
                                        {
                                            MsgPort.Write("Thread must be suspended first.\n");
                                        }
                                    }
                                    else
                                    {
                                        MsgPort.Write("Thread not found.\n");
                                    }
                                }
                            }
                            else
                            {
                                MsgPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "bps")
                    {
                        if (lineParts.Count == 2)
                        {
                            uint Address = FOS_System.Int32.Parse_HexadecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            
                            MsgPort.Write(" > Breakpoint to be set at ");
                            MsgPort.Write(Address);
                            MsgPort.Write("\n");
                            
                            *((byte*)Address) = 0xCC;
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else if (cmd == "bpc")
                    {
                        if (lineParts.Count == 2)
                        {
                            uint Address = FOS_System.Int32.Parse_HexadecimalUnsigned((FOS_System.String)lineParts[1], 0);

                            MsgPort.Write(" > Breakpoint to be cleared at ");
                            MsgPort.Write(Address);
                            MsgPort.Write("\n");

                            *((byte*)Address) = 0x90;
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
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
                                        MsgPort.Write("Registers of ");
                                        MsgPort.Write(TheThread.Name);
                                        MsgPort.Write(" : \n");

                                        MsgPort.Write(" > EAX : ");
                                        MsgPort.Write((FOS_System.String)TheThread.EAXFromInterruptStack);
                                        MsgPort.Write("\n > EBX : ");
                                        MsgPort.Write((FOS_System.String)TheThread.EBXFromInterruptStack);
                                        MsgPort.Write("\n > ECX : ");
                                        MsgPort.Write((FOS_System.String)TheThread.ECXFromInterruptStack);
                                        MsgPort.Write("\n > EDX : ");
                                        MsgPort.Write((FOS_System.String)TheThread.EDXFromInterruptStack);
                                        MsgPort.Write("\n");

                                        MsgPort.Write("\n > ESP : ");
                                        MsgPort.Write((FOS_System.String)TheThread.ESPFromInterruptStack);
                                        MsgPort.Write("\n > EBP : ");
                                        MsgPort.Write((FOS_System.String)TheThread.EBPFromInterruptStack);
                                        MsgPort.Write("\n > EIP : ");
                                        MsgPort.Write((FOS_System.String)TheThread.EIPFromInterruptStack);
                                        MsgPort.Write("\n");
                                    }
                                    else
                                    {
                                        MsgPort.Write("Thread must be suspended first.\n");
                                    }
                                }
                                else
                                {
                                    MsgPort.Write("Thread not found.\n");
                                }
                            }
                            else
                            {
                                MsgPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
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

                                MsgPort.Write(" Memory from ");
                                MsgPort.Write(Address);
                                MsgPort.Write(" for ");
                                MsgPort.Write(length);
                                MsgPort.Write(" units with ");
                                MsgPort.Write(units);
                                MsgPort.Write(" bytes/unit:\n");

                                if (units == 1)
                                {
                                    byte* AddrPtr = (byte*)Address;
                                    for (int i = 0; i < length; i++)
                                    {
                                        MsgPort.Write((FOS_System.String)AddrPtr[i]);
                                        MsgPort.Write(" ");
                                    }
                                }
                                else if (units == 2)
                                {
                                    ushort* AddrPtr = (ushort*)Address;
                                    for (int i = 0; i < length; i++)
                                    {
                                        MsgPort.Write((FOS_System.String)AddrPtr[i]);
                                        MsgPort.Write(" ");
                                    }
                                }
                                else if (units == 4)
                                {
                                    uint* AddrPtr = (uint*)Address;
                                    for (int i = 0; i < length; i++)
                                    {
                                        MsgPort.Write((FOS_System.String)AddrPtr[i]);
                                        MsgPort.Write(" ");
                                    }
                                }

                                MsgPort.Write("\n");

                                SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);
                            }
                            else
                            {
                                MsgPort.Write("Process not found.\n");
                            }
                        }
                        else
                        {
                            MsgPort.Write("Incorrect arguments, see help.\n");
                        }
                    }
                    else
                    {
                        MsgPort.Write("Unrecognised command. (Note: Backspace not supported!)\n");
                    }
                }

                MsgPort.Write("END OF COMMAND\n");
            }
        }
        
    }
}
