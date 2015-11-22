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
    /// <summary>
    /// The debugger implementation for kernel level debugging.
    /// </summary>
    public static unsafe class Debugger
    {
        /// <summary>
        /// Set to true to terminate the debugger.
        /// </summary>
        public static bool Terminating = false;

        //The compiler complains about this variable only ever being assigned to BUT
        //  it is used in the assembly code (which MSBuild doesn't know about / can't detect).
#pragma warning disable 0414
        /// <summary>
        /// Whether the debugger is currently enabled or not.
        /// </summary>
        /// <remarks>
        /// MSBuild complains that this variable is unused (because in the C# code it is only ever assigned to).
        /// However, in reality it is used - in the assembly code for the Int1 and Int3 interrupt routines!
        /// </remarks>
        private static bool Enabled = false;
#pragma warning restore 0414

        /// <summary>
        /// The serial port used for sending and receiving synchronous messages to/from the host.
        /// </summary>
        private static Serial MsgPort;
        /// <summary>
        /// The serial port used for sending asynchronous notifications to the host.
        /// </summary>
        /// <remarks>
        /// Send 0xFE to indicate a thread has been suspended that wasn't previously.
        /// </remarks>
        private static Serial NotifPort;

        /// <summary>
        /// The main thread of the debugger.
        /// </summary>
        /// <remarks>
        /// Set by the kernel task when the debugger is started. Used to prevent the host from suspending the debugger thread (which would result
        /// in total debugger lock-up and potentially irrecoverable system freeze!)
        /// </remarks>
        public static Thread MainThread;

        /// <summary>
        /// List of threads (specified as ProcessId:ThreadId) to suspend if they hit a breakpoint.
        /// </summary>
        private static UInt64List ThreadsToSuspend = new UInt64List();
        /// <summary>
        /// Dictionary of threads (key specified as ProcessId:ThreadId) and addresses to suspend them at.
        /// </summary>
        private static UInt64Dictionary ThreadsToSuspendAtAddresses = new UInt64Dictionary();

        /// <summary>
        /// Main C# handler function for interrupt 1s (single steps).
        /// </summary>
        /// <remarks>
        /// Called by the ASM handler.
        /// </remarks>
        [NoDebug]
        [NoGC]
        public static void Int1()
        {
            // Clear trap flag
            ProcessManager.CurrentThread.EFLAGSFromInterruptStack &= ~0x0100u;

            PauseCurrentThread();
        }
        /// <summary>
        /// Main C# handler function for interrupt 3s (breakpoints).
        /// </summary>
        /// <remarks>
        /// Called by the ASM handler.
        /// </remarks>
        [NoDebug]
        [NoGC]
        public static void Int3()
        {
            PauseCurrentThread();
        }
        /// <summary>
        /// Handles processing a debug interrupt including suspending the current thread if it is supposed/allowed to be.
        /// </summary>
        /// <remarks>
        /// This is a critical interrupt handler, so usual restrictions apply.
        /// </remarks>
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
                // Prevent pausing of the debugger thread (which can otherwise result in total system lock-up)
                if (ProcessManager.CurrentThread != MainThread)
                {
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Disabling debugger\n");
#endif
                    // Temporarily disable the debugger
                    Enabled = false;
                    
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Calculating process id\n");
#endif
                    // Generate a unique identifier for the current process / thread pair.
                    UInt64 ProcessThreadId = (((UInt64)ProcessManager.CurrentProcess.Id) << 32) | ProcessManager.CurrentThread.Id;
                    
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Checking suspended threads list\n");
#endif

#if DEBUG
                    bool SuspendAtAddessesContains = ThreadsToSuspendAtAddresses.Contains(ProcessThreadId);
                    // Should suspend the thread if EITHER: It is in the suspend list and not the suspend at addresses list 
                    //                                  OR: It is in the suspend at addresses list and the process's current instruction address matches
                    //      Note: Current instruction address of the paused process not the current interrupt handler routine
                    bool ShouldSuspend = (!SuspendAtAddessesContains && ThreadsToSuspend.IndexOf(ProcessThreadId) > -1) ||
                                         (SuspendAtAddessesContains && ThreadsToSuspendAtAddresses[ProcessThreadId] == ProcessManager.CurrentThread.EIPFromInterruptStack);
#else
                    bool ShouldSuspend = ThreadsToSuspend.IndexOf(ProcessThreadId) > -1;
#endif

                    if (ShouldSuspend)
                    {
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > Pausing");
                        PrintCurrentThread();
#endif

                        if (SuspendAtAddessesContains)
                        {
                            // Prevent recurring breaks
                            ThreadsToSuspendAtAddresses.Remove(ProcessThreadId);
                        }

                        // Suspend the thread
                        ProcessManager.CurrentThread.Debug_Suspend = true;
                        // Notify the host debugger that a thread has been suspended
                        NotifPort.Write(0xFE);
                        
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > Doing scheduler update...\n");
#endif 

                        // Get the scheduler to do an update so we don't return to the same thread
                        Scheduler.UpdateCurrentState();
                        
#if DEBUGGER_INTERUPT_TRACE
                        DebugPort.Write(" > New");
                        PrintCurrentThread();
#endif
                    }
                    // If the thread shouldn't be suspended, we might still be single-stepping to a particular address
                    else if (SuspendAtAddessesContains)
                    {
                        // If we are single stepping, (re)set the single step flag (Note: EFLAGS restored by IRet)
                        ProcessManager.CurrentThread.EFLAGSFromInterruptStack |= 0x0100u;
                    }
                    
#if DEBUGGER_INTERUPT_TRACE
                    DebugPort.Write(" > Enabling debugger\n");
#endif
                    // Re-enable the debugger
                    Enabled = true;
                }
            }
            finally
            {
                Hardware.Interrupts.Interrupts.InsideCriticalHandler = false;
            }
        }
        
        /// <summary>
        /// Main method for the debugger.
        /// </summary>
        [NoDebug]
        [NoGC]
        public static void Main()
        {
            // The serial ports should have already been initialised
            MsgPort = Serial.COM2;
            NotifPort = Serial.COM3;

            // This looks silly, but the host debugger is actually waiting for this
            //  to determine the OS has connected and is ready
            MsgPort.Write("Debug thread :D\n");

            //MsgPort.Write("Enabling...\n");
            // Everything is initialised and connected, so enable the debugger
            Enabled = true;
            //MsgPort.Write("Enabled.\n");

            // Note: It doesn't actually matter if the host hasn't connected
            //  because the debugger has near-zero effective cost to the rest of the system
            //  unless it is sent a command (unlike the old debugger, which slowed everything
            //  down to the point of being unusable).

            while (!Terminating)
            {
                // Read in a line from the host
                FOS_System.String line = "";
                char c = (char)MsgPort.Read();
                while (c != '\n' && c != '\r')
                {
                    line += c;
                    c = (char)MsgPort.Read();
                }
                
                // Echo the command back to the host for verification
                MsgPort.Write("START OF COMMAND\n");
                MsgPort.Write(line);
                MsgPort.Write("\n");

                // Filter the line
                line = line.Trim().ToLower();
                
                // Split it up into relevant parts
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
                        #region Help

                        MsgPort.Write("Available commands:\n");
                        MsgPort.Write(" - ping\n");
                        MsgPort.Write(" - threads\n");
                        MsgPort.Write(" - suspend (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - resume (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - step (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - ss (processId) (threadId | -1 for all threads)\n");
                        MsgPort.Write(" - sta (processId) (threadId | -1 for all threads) (address)\n");
                        MsgPort.Write(" - bps (address:hex)\n");
                        MsgPort.Write(" - bpc (address:hex)\n");
                        MsgPort.Write(" - regs (processId) (threadId)\n");
                        MsgPort.Write(" - memory (processId) (address:hex) (length) (units:1,2,4)\n");

                        #endregion
                    }
                    else if (cmd == "threads")
                    {
                        #region Threads

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

                        #endregion
                    }
                    else if (cmd == "suspend")
                    {
                        #region Suspend

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

                        #endregion
                    }
                    else if (cmd == "resume")
                    {
                        #region Resume 

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

                        #endregion
                    }
                    else if (cmd == "step")
                    {
                        #region Step

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

                        #endregion
                    }
                    else if (cmd == "ss")
                    {
                        #region Single Step

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

                        #endregion
                    }
                    else if (cmd == "sta")
                    {
                        #region Step To Address

                        if (lineParts.Count == 4)
                        {
                            uint ProcessId = FOS_System.Int32.Parse_DecimalUnsigned((FOS_System.String)lineParts[1], 0);
                            int ThreadId = FOS_System.Int32.Parse_DecimalSigned((FOS_System.String)lineParts[2]);
                            uint Address = FOS_System.Int32.Parse_HexadecimalUnsigned((FOS_System.String)lineParts[3], 0);

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
                                            UInt64 ProcessThreadId = (((UInt64)ProcessId) << 32) | (uint)ThreadId;
                                            ThreadsToSuspendAtAddresses.Add(ProcessThreadId, Address);
                                            AThread.EFLAGSFromInterruptStack |= 0x0100;
                                            AThread.Debug_Suspend = false;

                                            MsgPort.Write(" > Single stepping ");
                                            MsgPort.Write(AThread.Name);
                                            MsgPort.Write(" thread to address ");
                                            MsgPort.Write(Address);
                                            MsgPort.Write("\n");
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
                                            UInt64 ProcessThreadId = (((UInt64)ProcessId) << 32) | (uint)ThreadId;
                                            ThreadsToSuspendAtAddresses.Add(ProcessThreadId, Address);
                                            TheThread.EFLAGSFromInterruptStack |= 0x0100;
                                            TheThread.Debug_Suspend = false;

                                            MsgPort.Write(" > Single stepping ");
                                            MsgPort.Write(TheThread.Name);
                                            MsgPort.Write(" thread to address ");
                                            MsgPort.Write(Address);
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

                        #endregion
                    }
                    else if (cmd == "bps")
                    {
                        #region Set Breakpoint

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

                        #endregion
                    }
                    else if (cmd == "bpc")
                    {
                        #region Clear Breakpoint

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

                        #endregion
                    }
                    else if (cmd == "regs")
                    {
                        #region Registers

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

                        #endregion
                    }
                    else if (cmd == "mem")
                    {
                        #region Memory

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

                        #endregion
                    }
                    else
                    {
                        MsgPort.Write("Unrecognised command. (Note: Backspace not supported!)\n");
                    }
                }

                // Always issue end of command signal, even if something else went wrong
                //  - Keeps the host in sync.
                MsgPort.Write("END OF COMMAND\n");
            }
        }        
    }
}
