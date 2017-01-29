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

using System.Security.Principal;
using Drivers.Compiler.Attributes;
using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Devices.Serial;
using Kernel.Multiprocessing;
using x86Interrupts = Kernel.Interrupts.Interrupts;
using NoDebug = Drivers.Compiler.Attributes.NoDebugAttribute;
using NoGC = Drivers.Compiler.Attributes.NoGCAttribute;

namespace Kernel.Debug
{
    /// <summary>
    ///     The debugger implementation for kernel level debugging.
    /// </summary>
    public static unsafe class Debugger
    {
        /// <summary>
        ///     Set to true to terminate the debugger.
        /// </summary>
        [Group(Name = "IsolatedKernel")] public static bool Terminating = false;

        //The compiler complains about this variable only ever being assigned to BUT
        //  it is used in the assembly code (which MSBuild doesn't know about / can't detect).
#pragma warning disable 0414
        /// <summary>
        ///     Whether the debugger is currently enabled or not.
        /// </summary>
        /// <remarks>
        ///     MSBuild complains that this variable is unused (because in the C# code it is only ever assigned to).
        ///     However, in reality it is used - in the assembly code for the Int1 and Int3 interrupt routines!
        /// </remarks>
        [Group(Name = "IsolatedKernel")] private static bool Enabled;
#pragma warning restore 0414

        /// <summary>
        ///     The serial port used for sending and receiving synchronous messages to/from the host.
        /// </summary>
        [Group(Name = "IsolatedKernel")] private static Serial MsgPort;

        /// <summary>
        ///     The serial port used for sending asynchronous notifications to the host.
        /// </summary>
        /// <remarks>
        ///     Send 0xFE to indicate a thread has been suspended that wasn't previously.
        /// </remarks>
        [Group(Name = "IsolatedKernel")] private static Serial NotifPort;

        /// <summary>
        ///     The main thread of the debugger.
        /// </summary>
        /// <remarks>
        ///     Set by the kernel task when the debugger is started. Used to prevent the host from suspending the debugger thread
        ///     (which would result
        ///     in total debugger lock-up and potentially irrecoverable system freeze!)
        /// </remarks>
        [Group(Name = "IsolatedKernel")] public static Thread MainThread;

        /// <summary>
        ///     List of threads (specified as ProcessId:ThreadId) to suspend if they hit a breakpoint.
        /// </summary>
        [Group(Name = "IsolatedKernel")] private static readonly UInt64List ThreadsToSuspend = new UInt64List();

        /// <summary>
        ///     Dictionary of threads (key specified as ProcessId:ThreadId) and addresses to suspend them at.
        /// </summary>
        [Group(Name = "IsolatedKernel")] private static readonly UInt64Dictionary ThreadsToSuspendAtAddresses =
            new UInt64Dictionary();

        /// <summary>
        ///     Main C# handler function for interrupt 1s (single steps).
        /// </summary>
        /// <remarks>
        ///     Called by the ASM handler.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Int1()
        {
            // Clear trap flag
            ProcessManager.CurrentThread.EFLAGSFromInterruptStack &= ~0x0100u;

            PauseCurrentThread();
        }

        /// <summary>
        ///     Main C# handler function for interrupt 3s (breakpoints).
        /// </summary>
        /// <remarks>
        ///     Called by the ASM handler.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Int3()
        {
            //BasicConsole.WriteLine("Debugger > Int3 occurred.");

            PauseCurrentThread();
        }

        /// <summary>
        ///     Handles processing a debug interrupt including suspending the current thread if it is supposed/allowed to be.
        /// </summary>
        /// <remarks>
        ///     This is a critical interrupt handler, so usual restrictions apply.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        private static void PauseCurrentThread()
        {
            // Note: This must be OUTSIDE any exception block.
            x86Interrupts.InsideCriticalHandler = true;

            try
            {
#if DEBUGGER_INTERUPT_TRACE
                BasicConsole.Write("Pause current thread:\n");

                BasicConsole.Write(" > Current process: ");
                BasicConsole.Write(ProcessManager.CurrentProcess.Name);
                BasicConsole.Write("\n");

                Framework.String espStr = " > Current ESP : 0x--------\n";
                ExceptionMethods.FillString((uint)ExceptionMethods.StackPointer, 26, espStr);
                BasicConsole.Write(espStr);
                Framework.String ebpStr = " > Current EBP : 0x--------\n";
                ExceptionMethods.FillString((uint)ExceptionMethods.BasePointer, 26, ebpStr);
                BasicConsole.Write(ebpStr);

                BasicConsole.Write(" > Checking thread is not debugger thread...\n");
#endif
                // Prevent pausing of the debugger thread (which can otherwise result in total system lock-up)
                if (ProcessManager.CurrentThread != MainThread)
                {
#if DEBUGGER_INTERUPT_TRACE
                    BasicConsole.Write(" > Disabling debugger\n");
#endif
                    // Temporarily disable the debugger
                    Enabled = false;

#if DEBUGGER_INTERUPT_TRACE
                    BasicConsole.Write(" > Calculating process id\n");
#endif
                    // Generate a unique identifier for the current process / thread pair.
                    ulong ProcessThreadId = ((ulong)ProcessManager.CurrentProcess.Id << 32) |
                                            ProcessManager.CurrentThread.Id;

#if DEBUGGER_INTERUPT_TRACE
                    BasicConsole.Write(" > Checking suspended threads list\n");
#endif

                    bool SuspendAtAddessesContains = ThreadsToSuspendAtAddresses.Contains(ProcessThreadId);
                    // Should suspend the thread if EITHER: It is in the suspend list and not the suspend at addresses list 
                    //                                  OR: It is in the suspend at addresses list and the process's current instruction address matches
                    //      Note: Current instruction address of the paused process not the current interrupt handler routine
                    bool ShouldSuspend = (!SuspendAtAddessesContains && ThreadsToSuspend.IndexOf(ProcessThreadId) > -1) ||
                                         (SuspendAtAddessesContains &&
                                          ThreadsToSuspendAtAddresses[ProcessThreadId] ==
                                          ProcessManager.CurrentThread.EIPFromInterruptStack);

                    if (ShouldSuspend)
                    {
#if DEBUGGER_INTERUPT_TRACE
                        BasicConsole.Write(" > Pausing");
                        BasicConsole.WriteLine(ProcessManager.CurrentThread.Name);
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
                        BasicConsole.Write(" > Doing scheduler update...\n");
#endif

                        // Get the scheduler to do an update so we don't return to the same thread
                        Scheduler.UpdateCurrentState();

#if DEBUGGER_INTERUPT_TRACE
                        BasicConsole.Write(" > New");
                        BasicConsole.WriteLine(ProcessManager.CurrentThread.Name);
#endif
                    }
                    // If the thread shouldn't be suspended, we might still be single-stepping to a particular address
                    else if (SuspendAtAddessesContains)
                    {
                        // If we are single stepping, (re)set the single step flag (Note: EFLAGS restored by IRet)
                        ProcessManager.CurrentThread.EFLAGSFromInterruptStack |= 0x0100u;
                    }

#if DEBUGGER_INTERUPT_TRACE
                    BasicConsole.Write(" > Enabling debugger\n");
#endif
                    // Re-enable the debugger
                    Enabled = true;
                }
            }
            catch
            {
                BasicConsole.WriteLine("Error in Debugger.PauseCurrentThread!");
                if (ExceptionMethods.CurrentException != null)
                {
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }

            // Note: This must be OUTSIDE any exception block.
            x86Interrupts.InsideCriticalHandler = false;

            //BasicConsole.WriteLine("Debugger > PauseCurrentThread end.");
        }

        /// <summary>
        ///     Main method for the debugger.
        /// </summary>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Main()
        {
            DeviceManager.InitForProcess();

            Serial.InitCOM2();
            Serial.InitCOM3();

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
                String line = "";
                char c = (char)MsgPort.Read();
                while (c != '\n' && c != '\r')
                {
                    line += c;
                    c = (char)MsgPort.Read();
                }

                BasicConsole.WriteLine("Debugger > Command received: " + line);

                // Echo the command back to the host for verification
                MsgPort.Write("START OF COMMAND\n");
                MsgPort.Write(line);
                MsgPort.Write("\n");

                // Filter the line
                line = line.Trim().ToLower();

                try
                {
                    // Split it up into relevant parts
                    List lineParts = line.Split(' ');
                    if (lineParts.Count > 0)
                    {
                        String cmd = (String)lineParts[0];

                        if (cmd == "ping")
                        {
                            BasicConsole.WriteLine("Debugger > Processing `ping` command.");

                            MsgPort.Write("pong\n");
                        }
                        else if (cmd == "help")
                        {
                            #region Help

                            BasicConsole.WriteLine("Debugger > Processing `help` command.");
                            MsgPort.Write("Available commands:\n");
                            MsgPort.Write(" - ping\n");
                            MsgPort.Write(" - threads\n");
                            MsgPort.Write(" - suspend (processId) (threadId | -1 for all threads)\n");
                            MsgPort.Write(" - resume (processId) (threadId | -1 for all threads)\n");
                            MsgPort.Write(" - step (processId) (threadId | -1 for all threads)\n");
                            MsgPort.Write(" - ss (processId) (threadId | -1 for all threads)\n");
                            MsgPort.Write(" - sta (processId) (threadId | -1 for all threads) (address)\n");
                            MsgPort.Write(" - bps (processId) (address:hex)\n");
                            MsgPort.Write(" - bpc (processId) (address:hex)\n");
                            MsgPort.Write(" - regs (processId) (threadId)\n");
                            MsgPort.Write(" - memory (processId) (address:hex) (length) (units:1,2,4)\n");

                            #endregion
                        }
                        else if (cmd == "threads")
                        {
                            #region Threads

                            BasicConsole.WriteLine("Debugger > Processing `threads` command.");

                            for (int i = 0; i < ProcessManager.Processes.Count; i++)
                            {
                                Process AProcess = (Process)ProcessManager.Processes[i];
                                MsgPort.Write(" - Process : ");
                                MsgPort.Write(AProcess.Id);
                                MsgPort.Write(" : ");
                                MsgPort.Write(AProcess.Name);
                                MsgPort.Write(" : ");
                                switch (AProcess.Priority)
                                {
                                    case Scheduler.Priority.ZeroTimed:
                                        MsgPort.Write("Zero Timed");
                                        break;
                                    case Scheduler.Priority.Low:
                                        MsgPort.Write("Low");
                                        break;
                                    case Scheduler.Priority.Normal:
                                        MsgPort.Write("Normal");
                                        break;
                                    case Scheduler.Priority.High:
                                        MsgPort.Write("High");
                                        break;
                                }
                                MsgPort.Write("\n");
                                for (int j = 0; j < AProcess.Threads.Count; j++)
                                {
                                    Thread AThread = (Thread)AProcess.Threads[j];

                                    MsgPort.Write("      - Thread : ");
                                    MsgPort.Write(AThread.Id);
                                    MsgPort.Write(" : ");

                                    switch (AThread.ActiveState)
                                    {
                                        case Thread.ActiveStates.Active:
                                            MsgPort.Write("Active");
                                            break;
                                        case Thread.ActiveStates.Inactive:
                                            MsgPort.Write("Inactive");
                                            break;
                                        case Thread.ActiveStates.NotStarted:
                                            MsgPort.Write("Not Started");
                                            break;
                                        case Thread.ActiveStates.Suspended:
                                            if (AThread.Debug_Suspend)
                                            {
                                                MsgPort.Write("Debugging");
                                            }
                                            else
                                            {
                                                MsgPort.Write("Suspended");
                                            }
                                            break;
                                        case Thread.ActiveStates.Terminated:
                                            MsgPort.Write("Terminated");
                                            break;
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

                            BasicConsole.WriteLine("Debugger > Processing `suspend` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);
                                int ThreadId = Int32.Parse_DecimalSigned((String)lineParts[2]);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    if (ThreadId == -1)
                                    {
                                        for (int i = 0; i < TheProcess.Threads.Count; i++)
                                        {
                                            Thread AThread = (Thread)TheProcess.Threads[i];

                                            if (AThread != MainThread)
                                            {
                                                ulong ProcessThreadId = ((ulong)ProcessId << 32) | AThread.Id;
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
                                        ulong ProcessThreadId = ((ulong)ProcessId << 32) | (uint)ThreadId;
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

                            BasicConsole.WriteLine("Debugger > Processing `resume` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);
                                int ThreadId = Int32.Parse_DecimalSigned((String)lineParts[2]);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    if (ThreadId == -1)
                                    {
                                        for (int i = 0; i < TheProcess.Threads.Count; i++)
                                        {
                                            Thread AThread = (Thread)TheProcess.Threads[i];

                                            ulong ProcessThreadId = ((ulong)ProcessId << 32) | AThread.Id;
                                            ThreadsToSuspend.Remove(ProcessThreadId);

                                            AThread.Debug_Suspend = false;

                                            MsgPort.Write(" > Resumed ");
                                            MsgPort.Write(AThread.Name);
                                            MsgPort.Write(" thread\n");
                                        }
                                    }
                                    else
                                    {
                                        ulong ProcessThreadId = ((ulong)ProcessId << 32) | (uint)ThreadId;
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

                            BasicConsole.WriteLine("Debugger > Processing `step` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);
                                int ThreadId = Int32.Parse_DecimalSigned((String)lineParts[2]);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    if (ThreadId == -1)
                                    {
                                        for (int i = 0; i < TheProcess.Threads.Count; i++)
                                        {
                                            Thread AThread = (Thread)TheProcess.Threads[i];

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

                            BasicConsole.WriteLine("Debugger > Processing `single step` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);
                                int ThreadId = Int32.Parse_DecimalSigned((String)lineParts[2]);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    if (ThreadId == -1)
                                    {
                                        for (int i = 0; i < TheProcess.Threads.Count; i++)
                                        {
                                            Thread AThread = (Thread)TheProcess.Threads[i];

                                            if (AThread.Debug_Suspend)
                                            {
                                                // Set trap flag (int1)

                                                uint OldESP = EnableAccessToThreadStack(TheProcess, AThread);
                                                try
                                                {
                                                    AThread.EFLAGSFromInterruptStack |= 0x0100;
                                                }
                                                finally
                                                {
                                                    DisableAccessToThreadStack(TheProcess, AThread, OldESP);
                                                }

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
                                                uint OldESP = EnableAccessToThreadStack(TheProcess, TheThread);
                                                try
                                                {
                                                    TheThread.EFLAGSFromInterruptStack |= 0x0100;
                                                }
                                                finally
                                                {
                                                    DisableAccessToThreadStack(TheProcess, TheThread, OldESP);
                                                }
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

                            BasicConsole.WriteLine("Debugger > Processing `step to address` command.");

                            if (lineParts.Count == 4)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);
                                int ThreadId = Int32.Parse_DecimalSigned((String)lineParts[2]);
                                uint Address = Int32.Parse_HexadecimalUnsigned((String)lineParts[3], 0);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    if (ThreadId == -1)
                                    {
                                        for (int i = 0; i < TheProcess.Threads.Count; i++)
                                        {
                                            Thread AThread = (Thread)TheProcess.Threads[i];

                                            if (AThread.Debug_Suspend)
                                            {
                                                // Set trap flag (int1)
                                                ulong ProcessThreadId = ((ulong)ProcessId << 32) | (uint)ThreadId;
                                                ThreadsToSuspendAtAddresses.Add(ProcessThreadId, Address);
                                                uint OldESP = EnableAccessToThreadStack(TheProcess, AThread);
                                                try
                                                {
                                                    AThread.EFLAGSFromInterruptStack |= 0x0100;
                                                }
                                                finally
                                                {
                                                    DisableAccessToThreadStack(TheProcess, AThread, OldESP);
                                                }
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
                                                ulong ProcessThreadId = ((ulong)ProcessId << 32) | (uint)ThreadId;
                                                ThreadsToSuspendAtAddresses.Add(ProcessThreadId, Address);
                                                uint OldESP = EnableAccessToThreadStack(TheProcess, TheThread);
                                                try
                                                {
                                                    TheThread.EFLAGSFromInterruptStack |= 0x0100;
                                                }
                                                finally
                                                {
                                                    DisableAccessToThreadStack(TheProcess, TheThread, OldESP);
                                                }
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

                            BasicConsole.WriteLine("Debugger > Processing `set breakpoint` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    bool AllSuspended = true;

                                    for (int i = 0; i < TheProcess.Threads.Count; i++)
                                    {
                                        AllSuspended &= ((Thread)TheProcess.Threads[i]).Debug_Suspend;
                                    }

                                    if (AllSuspended)
                                    {
                                        uint Address = Int32.Parse_HexadecimalUnsigned((String)lineParts[2], 0);

                                        MsgPort.Write(" > Breakpoint to be set at ");
                                        MsgPort.Write(Address);
                                        MsgPort.Write("\n");

                                        SetProgramByte(TheProcess, Address, 0xCC);
                                    }
                                    else
                                    {
                                        MsgPort.Write("All threads of the process must be suspended first.\n");
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
                        else if (cmd == "bpc")
                        {
                            #region Clear Breakpoint

                            BasicConsole.WriteLine("Debugger > Processing `clear breakpoint` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    bool AllSuspended = true;
                                    
                                    for (int i = 0; i < TheProcess.Threads.Count; i++)
                                    {
                                        AllSuspended &= ((Thread)TheProcess.Threads[i]).Debug_Suspend;
                                    }

                                    if (AllSuspended)
                                    {
                                        uint Address = Int32.Parse_HexadecimalUnsigned((String)lineParts[2], 0);

                                        MsgPort.Write(" > Breakpoint to be cleared at ");
                                        MsgPort.Write(Address);
                                        MsgPort.Write("\n");

                                        SetProgramByte(TheProcess, Address, 0x90);
                                    }
                                    else
                                    {
                                        MsgPort.Write("All threads of the process must be suspended first.\n");
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
                        else if (cmd == "regs")
                        {
                            #region Registers

                            BasicConsole.WriteLine("Debugger > Processing `registers` command.");

                            if (lineParts.Count == 3)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);
                                uint ThreadId = Int32.Parse_DecimalUnsigned((String)lineParts[2], 0);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    Thread TheThread = ProcessManager.GetThreadById(ThreadId, TheProcess);

                                    if (TheThread != null)
                                    {
                                        if (TheThread.Debug_Suspend)
                                        {
                                            MsgPort.Write("Registers of ");
                                            MsgPort.Write(TheThread.Name);
                                            MsgPort.Write(" : \n");

                                            uint OldESP = EnableAccessToThreadStack(TheProcess, TheThread);
                                            try
                                            {
                                                MsgPort.Write(" > EAX : ");
                                                MsgPort.Write(TheThread.EAXFromInterruptStack);
                                                MsgPort.Write("\n > EBX : ");
                                                MsgPort.Write(TheThread.EBXFromInterruptStack);
                                                MsgPort.Write("\n > ECX : ");
                                                MsgPort.Write(TheThread.ECXFromInterruptStack);
                                                MsgPort.Write("\n > EDX : ");
                                                MsgPort.Write(TheThread.EDXFromInterruptStack);
                                                MsgPort.Write("\n");

                                                MsgPort.Write("\n > ESP : ");
                                                MsgPort.Write(TheThread.ESPFromInterruptStack);
                                                MsgPort.Write("\n > EBP : ");
                                                MsgPort.Write(TheThread.EBPFromInterruptStack);
                                                MsgPort.Write("\n > EIP : ");
                                                MsgPort.Write(TheThread.EIPFromInterruptStack);
                                                MsgPort.Write("\n");
                                            }
                                            finally
                                            {
                                                DisableAccessToThreadStack(TheProcess, TheThread, OldESP);
                                            }
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

                            BasicConsole.WriteLine("Debugger > Processing `memory` command.");

                            if (lineParts.Count == 5)
                            {
                                uint ProcessId = Int32.Parse_DecimalUnsigned((String)lineParts[1], 0);

                                Process TheProcess = ProcessManager.GetProcessById(ProcessId);

                                if (TheProcess != null)
                                {
                                    uint OldVAddr = Int32.Parse_HexadecimalUnsigned((String)lineParts[2], 0);
                                    int FullLength = Int32.Parse_DecimalSigned((String)lineParts[3]);
                                    int units = Int32.Parse_DecimalSigned((String)lineParts[4]);

                                    MsgPort.Write(" Memory from ");
                                    MsgPort.Write(OldVAddr);
                                    MsgPort.Write(" for ");
                                    MsgPort.Write(FullLength);
                                    MsgPort.Write(" units with ");
                                    MsgPort.Write(units);
                                    MsgPort.Write(" bytes/unit:\n");
                                    try
                                    {
                                        // Loop logic makes sure we remap if/when we cross a page boundary
                                        int PartialLength = 0;
                                        while (PartialLength < FullLength)
                                        {
                                            uint OldPAddr = CheckPhysicalAddress(OldVAddr & 0xFFFFF000, 
                                                TheProcess.TheMemoryLayout.GetPhysicalAddress(OldVAddr & 0xFFFFF000));
                                            uint NewVAddr =
                                                (uint)
                                                    ProcessManager.EnableDebuggerAccessToProcessMemory(TheProcess,
                                                        (void*)OldPAddr);
                                            uint PageOffset = OldVAddr & 0x00000FFF;
                                            uint Address = NewVAddr + PageOffset;

                                            int MaxFullLength = FullLength - PartialLength;
                                            int MaxPageLength = (int)(0x1000 - PageOffset);
                                            int length = MaxPageLength > MaxFullLength ? MaxFullLength : MaxPageLength;
                                            PartialLength += length;

                                            if (units == 1)
                                            {
                                                byte* AddrPtr = (byte*)Address;
                                                for (int i = 0; i < length; i++)
                                                {
                                                    MsgPort.Write((String)AddrPtr[i]);
                                                    MsgPort.Write(" ");
                                                }
                                            }
                                            else if (units == 2)
                                            {
                                                ushort* AddrPtr = (ushort*)Address;
                                                for (int i = 0; i < length; i++)
                                                {
                                                    MsgPort.Write(AddrPtr[i]);
                                                    MsgPort.Write(" ");
                                                }
                                            }
                                            else if (units == 4)
                                            {
                                                uint* AddrPtr = (uint*)Address;
                                                for (int i = 0; i < length; i++)
                                                {
                                                    MsgPort.Write(AddrPtr[i]);
                                                    MsgPort.Write(" ");
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        ProcessManager.DisableDebuggerAccessToProcessMemory(TheProcess);
                                    }

                                    MsgPort.Write("\n");
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
                }
                catch
                {
                    BasicConsole.WriteLine("Debugger encountered an error while processing a command! Error was:");
                    if (ExceptionMethods.CurrentException != null)
                    {
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }

                    MsgPort.Write("Error processing command! Error was:\n");
                    if (ExceptionMethods.CurrentException != null)
                    {
                        MsgPort.Write(ExceptionMethods.CurrentException.Message);
                        MsgPort.Write((byte)'\n');
                    }
                }
                finally
                {
                    // Always issue end of command signal, even if something else went wrong
                    //  - Keeps the host in sync.
                    MsgPort.Write("END OF COMMAND\n");
                }
            }
        }

        private static uint EnableAccessToThreadStack(Process TheProcess, Thread AThread)
        {
            uint OldESP = AThread.State->ESP;
            uint OldThreadStackBottomVAddr = (uint)(AThread.State->ThreadStackTop - Thread.ThreadStackTopOffset);
            uint OldThreadStackBottomPAddr = CheckPhysicalAddress(OldThreadStackBottomVAddr, 
                TheProcess.TheMemoryLayout.GetPhysicalAddress(OldThreadStackBottomVAddr));
            uint NewThreadStackBottomVAddr =
                (uint)ProcessManager.EnableDebuggerAccessToProcessMemory(TheProcess, (void*)OldThreadStackBottomPAddr);
            uint NewESP = OldESP - OldThreadStackBottomVAddr + NewThreadStackBottomVAddr;
            AThread.State->ESP = NewESP;
            return OldESP;
        }

        private static void DisableAccessToThreadStack(Process TheProcess, Thread AThread, uint OldESP)
        {
            AThread.State->ESP = OldESP;
            ProcessManager.DisableDebuggerAccessToProcessMemory(TheProcess);
        }

        private static void SetProgramByte(Process TheProcess, uint OldVAddr, byte Value)
        {
            uint OldPAddr = CheckPhysicalAddress(OldVAddr & 0xFFFFF000, TheProcess.TheMemoryLayout.GetPhysicalAddress(OldVAddr & 0xFFFFF000));
            uint NewVAddr = (uint)ProcessManager.EnableDebuggerAccessToProcessMemory(TheProcess, (void*)OldPAddr);
            uint PageOffset = OldVAddr & 0x00000FFF;
            uint Address = NewVAddr + PageOffset;
            *(byte*)Address = Value;
            ProcessManager.DisableDebuggerAccessToProcessMemory(TheProcess);
        }

        private static uint CheckPhysicalAddress(uint OldVAddr, uint PAddr)
        {
            //TODO: Unhack this: "Built in processes" use fixed-addressed kernel memory for their code pages which isn't included in their memory layout
            if (PAddr == 0xFFFFFFFF)
            {
                // Double check that the virtual address wasn't in the process' memory layout because it's a 'kernel fixed page'
                if (VirtualMemory.VirtualMemoryManager.IsWithinKernelFixedMemory(OldVAddr))
                {
                    //WARNING: Enabling this may result in the debugger application hanging as it inteferes with the messaging protocol:
                    //  MsgPort.Write("Address is inside the kernel fixed-address space.\n");
                    return VirtualMemory.VirtualMemoryManager.GetPhysicalAddress(OldVAddr);
                }
            }
            return PAddr;
        }
    }
}