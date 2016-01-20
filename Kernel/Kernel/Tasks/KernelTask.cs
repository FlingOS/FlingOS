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

//DSC: Deferred System Calls
//#define DSC_TRACE
//#define SYSCALLS_TRACE

using Kernel.FOS_System.Collections;
using Kernel.Processes;
using Kernel.Hardware.Processes;

namespace Kernel.Tasks
{
    public unsafe static class KernelTask
    {
        private class DeferredSyscallInfo : FOS_System.Object
        {
            public uint ProcessId;
            public uint ThreadId;
        }

        public static bool Terminating = false;
        private static Queue DeferredSyscallsInfo_Unqueued;
        private static Queue DeferredSyscallsInfo_Queued;

        private static Thread DeferredSyscallsThread;
        
        public static uint WindowManagerTask_ProcessId;

        private static Hardware.Keyboards.VirtualKeyboard keyboard;
        private static Consoles.VirtualConsole console;

        public static void Main()
        {
            BasicConsole.WriteLine("Kernel task! ");
            BasicConsole.WriteLine(" > Executing normally...");

            #region Dictionary Test
            /*try
            {
                UInt32Dictionary dic = new UInt32Dictionary();

                for (uint i = 0; i < 9; i += 3)
                {
                    BasicConsole.WriteLine("Dictionary test loop");
                    BasicConsole.WriteLine("--------------------");

                    uint key1 = 0xC0DEC0DEu + (0x100u * i);
                    uint key2 = 0xC0DEC0DEu + (0x100u * (i+1));
                    uint key3 = 0xC0DEC0DEu + (0x100u * (i+2));

                    uint value1 = 0xDEADBEE0u + (0x1u * i);
                    uint value2 = 0xDEADBEE0u + (0x1u * (i+1));
                    uint value3 = 0xDEADBEE0u + (0x1u * (i+2));

                    dic.Add(key1, value1);
                    dic.Add(key2, value2);
                    dic.Add(key3, value3);

                    for(uint j = 50 * i; j < (50 * (i+1))-20; j++)
                    {
                        dic.Add(j, j);
                    }

                    if (!dic.Contains(key1))
                    {
                        BasicConsole.WriteLine("Dictionary doesn't contain key 1.");
                    }
                    else if (dic[key1] != value1)
                    {
                        BasicConsole.WriteLine("Dictionary value for key 1 wrong.");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Okay (1)");
                    }
                    if (!dic.Contains(key2))
                    {
                        BasicConsole.WriteLine("Dictionary doesn't contain key1");
                    }
                    else if (dic[key2] != value2)
                    {
                        BasicConsole.WriteLine("Dictionary value for key1 wrong.");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Okay (2)");
                    }
                    if (!dic.Contains(key3))
                    {
                        BasicConsole.WriteLine("Dictionary doesn't contain key1");
                    }
                    else if (dic[key3] != value3)
                    {
                        BasicConsole.WriteLine("Dictionary value for key1 wrong.");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Okay (3)");
                    }

                    dic.Remove(key1);

                    if (dic.Contains(key1))
                    {
                        BasicConsole.WriteLine("Dictionary contains key1!");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Okay (4)");
                    }

                    BasicConsole.WriteLine("Iterating");
                    UInt32Dictionary.Iterator itr = dic.GetIterator();
                    while (itr.HasNext())
                    {
                        UInt32Dictionary.KeyValuePair pair = itr.Next();
                        BasicConsole.WriteLine("Pair: key=" + (FOS_System.String)pair.Key + ", value=" + pair.Value);
                    }

                    dic.Remove(key2);

                    for (uint j = (50 * i)+30; j < (50 * (i + 1)); j++)
                    {
                        dic.Add(j, j);
                    }

                    if (dic.Contains(key2))
                    {
                        BasicConsole.WriteLine("Dictionary contains key2!");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Okay (5)");
                    }


                    dic.Remove(key3);

                    if (dic.Contains(key3))
                    {
                        BasicConsole.WriteLine("Dictionary contains key3!");
                    }
                    else
                    {
                        BasicConsole.WriteLine("Okay (6)");
                    }

                    itr = dic.GetIterator();
                    while (itr.HasNext())
                    {
                        UInt32Dictionary.KeyValuePair pair = itr.Next();
                        BasicConsole.WriteLine("Pair: key=" + (FOS_System.String)pair.Key + ", value=" + pair.Value);
                    }
                }
            }
            catch
            {
                BasicConsole.WriteLine("Error testing UInt32Dictionary.");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }
            BasicConsole.DelayOutput(5);
            */
            #endregion

            DeferredSyscallsInfo_Unqueued = new Queue(256, false);
            DeferredSyscallsInfo_Queued = new Queue(DeferredSyscallsInfo_Unqueued.Capacity, false);
            for (int i = 0; i < DeferredSyscallsInfo_Unqueued.Capacity; i++)
            {
                DeferredSyscallsInfo_Unqueued.Push(new DeferredSyscallInfo());
            }

            try
            {
                BasicConsole.WriteLine(" > Initialising system calls...");
                ProcessManager.CurrentProcess.SyscallHandler = SyscallHandler;
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterISRHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterISRHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterIRQHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterIRQHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.StartThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SleepThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WakeThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterPipeOutpoint);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetNumPipeOutpoints);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetPipeOutpoints);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WaitOnPipeCreate);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.CreatePipe);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ReadPipe);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WritePipe);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SendMessage);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ReceiveMessage);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RequestPages);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.UnmapPages);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SharePages);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.IsPhysicalAddressMapped);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.IsVirtualAddressMapped);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetPhysicalAddress);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetVirtualAddress);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.CreateSemaphore);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ShareSemaphore);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ReleaseSemaphore);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WaitSemaphore);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SignalSemaphore);

                //ProcessManager.CurrentProcess.OutputMemTrace = true;

                BasicConsole.WriteLine(" > Starting Idle process...");
                Process IdleProcess = ProcessManager.CreateProcess(Tasks.IdleTask.Main, "Idle", false, true);
                ProcessManager.RegisterProcess(IdleProcess, Scheduler.Priority.ZeroTimed);
                
                BasicConsole.WriteLine(" > Starting deferred syscalls thread...");
                DeferredSyscallsThread = ProcessManager.CurrentProcess.CreateThread(DeferredSyscallsThread_Main, "Deferred Sys Calls");

#if DEBUG
                BasicConsole.WriteLine(" > Starting Debugger thread...");
                Debug.Debugger.MainThread = ProcessManager.CurrentProcess.CreateThread(Debug.Debugger.Main, "Debugger");
#endif

                BasicConsole.WriteLine(" > Starting GC Cleanup thread...");
                ProcessManager.CurrentProcess.CreateThread(Tasks.GCCleanupTask.Main, "GC Cleanup");

                BasicConsole.WriteLine(" > Starting Window Manager...");
                Process WindowManagerProcess = ProcessManager.CreateProcess(WindowManagerTask.Main, "Window Manager", false, true);
                WindowManagerTask_ProcessId = WindowManagerProcess.Id;
                //WindowManagerProcess.OutputMemTrace = true;
                ProcessManager.RegisterProcess(WindowManagerProcess, Scheduler.Priority.Normal);
                
                BasicConsole.WriteLine(" > Waiting for Window Manager to be ready...");
                while (!WindowManagerTask.Ready)
                {
                    BasicConsole.WriteLine(" > [Wait pause]");
                    SystemCalls.SleepThread(1000);
                }
                BasicConsole.WriteLine(" > Window Manager reported ready.");

                BasicConsole.WriteLine(" > Starting Device Manager...");
                Process DeviceManagerProcess = ProcessManager.CreateProcess(DeviceManagerTask.Main, "Device Manager", false, true);
                //DeviceManagerProcess.OutputMemTrace = true;
                ProcessManager.RegisterProcess(DeviceManagerProcess, Scheduler.Priority.Normal);

                BasicConsole.WriteLine("Started.");

                BasicConsole.PrimaryOutputEnabled = false;
                //BasicConsole.SecondaryOutputEnabled = false;

                try
                {
                    BasicConsole.WriteLine("KT > Creating virtual keyboard...");
                    keyboard = new Hardware.Keyboards.VirtualKeyboard();

                    BasicConsole.WriteLine("KT > Creating virtual console...");
                    console = new Consoles.VirtualConsole();

                    BasicConsole.WriteLine("KT > Connecting virtual console...");
                    console.Connect();

                    BasicConsole.WriteLine("KT > Creating main shell...");
                    Shells.MainShell shell = new Shells.MainShell(console, keyboard);
                    
                    BasicConsole.WriteLine("KT > Running...");

                    uint loops = 0;
                    while (!Terminating)
                    {
                        try
                        {
                            //FOS_System.String msg = "KT > Hello, world! (" + (FOS_System.String)loops++ + ")";
                            //console.WriteLine(msg);
                            //BasicConsole.WriteLine(msg);
                            //SystemCalls.SleepThread(1000);
                            shell.Execute();
                        }
                        catch
                        {
                            BasicConsole.WriteLine("KT > Error executing MainShell!");
                            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("KT > Error initialising!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
            catch
            {
                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine();
            BasicConsole.WriteLine("---------------------");
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of kernel task.");
        }

        public static void DeferredSyscallsThread_Main()
        {
            while (!Terminating)
            {
                if (DeferredSyscallsInfo_Queued.Count == 0)
                {
                    SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);
                }

                while (DeferredSyscallsInfo_Queued.Count > 0)
                {
                    // Scheduler must be disabled during pop/push from circular buffer or we can
                    //  end up in an infinite lock. Consider what happens if a process invokes 
                    //  a deferred system call during the pop/push here and at the end of this loop.
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Pausing scheduler...");
#endif
                    Scheduler.Disable(/*"DSC 1"*/);
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Popping queued info object...");
#endif
                    DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Queued.Pop();
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Resuming scheduler...");
#endif
                    Scheduler.Enable();

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Getting process & thread...");
#endif
                    Process CallerProcess = ProcessManager.GetProcessById(info.ProcessId);
                    Thread CallerThread = ProcessManager.GetThreadById(info.ThreadId, CallerProcess);

#if DSC_TRACE
                    BasicConsole.Write("DSC: Process: ");
                    BasicConsole.WriteLine(CallerProcess.Name);
                    BasicConsole.Write("DSC: Thread: ");
                    BasicConsole.WriteLine(CallerThread.Name);
#endif

                    ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Getting data...");
#endif
                    SystemCallNumbers SysCallNumber = (SystemCallNumbers)CallerThread.SysCallNumber;
                    uint Param1 = CallerThread.Param1;
                    uint Param2 = CallerThread.Param2;
                    uint Param3 = CallerThread.Param3;
                    uint Return2 = CallerThread.Return2;
                    uint Return3 = CallerThread.Return3;
                    uint Return4 = CallerThread.Return4;
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Getting data done.");
#endif
                    ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Calling...");
#endif
                    SystemCallResults result = HandleDeferredSystemCall(
                        CallerProcess, CallerThread,
                        SysCallNumber, Param1, Param2, Param3,
                        ref Return2, ref Return3, ref Return4);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Ending call...");
#endif
                    if (result != SystemCallResults.Deferred)
                    {
                        EndDeferredSystemCall(CallerProcess, CallerThread, result, Return2, Return3, Return4);
                    }

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Resetting info object...");
#endif
                    info.ProcessId = 0;
                    info.ThreadId = 0;

                    // See comment at top of loop for why this is necessary
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Pausing scheduler...");
#endif
                    Scheduler.Disable(/*"DSC 2"*/);
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Queuing info object...");
#endif
                    DeferredSyscallsInfo_Unqueued.Push(info);
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Resuming scheduler...");
#endif
                    Scheduler.Enable();
                }
            }
        }
        public static unsafe SystemCallResults HandleDeferredSystemCall(
            Process CallerProcess, Thread CallerThread,
            SystemCallNumbers syscallNumber, uint Param1, uint Param2, uint Param3,
            ref uint Return2, ref uint Return3, ref uint Return4)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch (syscallNumber)
            {
                case SystemCallNumbers.StartThread:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Start Thread");
#endif
                    Return2 = CallerProcess.CreateThread((ThreadStartMethod)Utilities.ObjectUtilities.GetObject((void*)Param1), "[From sys call]").Id;
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Start Thread - done.");
#endif
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Register Pipe Outpoint");
#endif
                        Pipes.PipeOutpoint outpoint;
                        bool registered = Pipes.PipeManager.RegisterPipeOutpoint(CallerProcess.Id, (Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, (int)Param3, out outpoint);
                        if (registered)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Register Pipe Outpoint - done.");
#endif
                    }
                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints");
#endif
                        int numOutpoints;
                        bool obtained = Pipes.PipeManager.GetNumPipeOutpoints((Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, out numOutpoints);
                        if (obtained)
                        {
                            result = SystemCallResults.OK;
                            Return2 = (uint)numOutpoints;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints - done");
#endif
                    }
                    break;
                case SystemCallNumbers.GetPipeOutpoints:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Get Pipe Outpoints");
#endif

                        bool obtained = Pipes.PipeManager.GetPipeOutpoints(CallerProcess, (Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, (Pipes.PipeOutpointsRequest*)Param3);
                        if (obtained)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }

#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Get Pipe Outpoints - done");
#endif
                    }
                    break;
                case SystemCallNumbers.CreatePipe:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Create Pipe");
#endif

                        bool created = Pipes.PipeManager.CreatePipe(CallerProcess.Id, Param1, (Pipes.CreatePipeRequest*)Param2);
                        if (created)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }

#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Create Pipe - done");
#endif
                    }
                    break;
                case SystemCallNumbers.WaitOnPipeCreate:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Wait On Pipe Create");
#endif

                        bool waiting = Pipes.PipeManager.WaitOnPipeCreate(CallerProcess.Id, CallerThread.Id, (Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2);
                        if (waiting)
                        {
                            result = SystemCallResults.Deferred;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }

#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Wait On Pipe Create - done");
#endif
                    }
                    break;
                case SystemCallNumbers.ReadPipe:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Read Pipe");
#endif

                        // Need access to calling process' memory to be able to read values from request structure
                        ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                        Pipes.ReadPipeRequest* RequestPtr = (Pipes.ReadPipeRequest*)Param1;
                        int PipeId = RequestPtr->PipeId;
                        bool Blocking = RequestPtr->Blocking;
                        ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

                        Pipes.PipeManager.RWResults RWResult = Pipes.PipeManager.ReadPipe(PipeId, Blocking, CallerProcess, CallerThread);

                        if (RWResult == Pipes.PipeManager.RWResults.Error)
                        {
                            result = SystemCallResults.Fail;
                        }
                        else
                        {
                            // Returning Deferred state from here will leave the caller thread
                            //  in whatever state ReadPipe decided it should be in.
                            result = SystemCallResults.Deferred;
                        }

#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Read Pipe - done");
#endif
                    }
                    break;
                case SystemCallNumbers.WritePipe:
                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Write Pipe");
#endif

                        // Need access to calling process' memory to be able to read values from request structure
                        ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                        Pipes.ReadPipeRequest* RequestPtr = (Pipes.ReadPipeRequest*)Param1;
                        int PipeId = RequestPtr->PipeId;
                        bool Blocking = RequestPtr->Blocking;
                        ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);
                        
                        Pipes.PipeManager.RWResults RWResult = Pipes.PipeManager.WritePipe(PipeId, Blocking, CallerProcess, CallerThread);

                        if (RWResult == Pipes.PipeManager.RWResults.Error)
                        {
                            result = SystemCallResults.Fail;
                        }
                        else
                        {
                            // Returning Deferred state from here will leave the caller thread
                            //  in whatever state WritePipe decided it should be in.
                            result = SystemCallResults.Deferred;
                        }

#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Write Pipe - done");
#endif
                    }
                    break;
                case SystemCallNumbers.RequestPages:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Request pages");
#endif
                    result = SystemCallResults.Fail;

                    try
                    {
                        ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

                        uint ptr = 0xFFFFFFFF;
                        uint[] pAddrs = null;

                        int count = (int)Param3;

                        // Param1: Start Phys or 0xFFFFFFFF
                        // Param2: Start Virt or 0xFFFFFFFF
                        // Param3: Count
                        if (Param1 == 0xFFFFFFFF)
                        {
                            if (Param2 == 0xFFFFFFFF)
                            {
                                // Any physical, any virtual
#if DSC_TRACE
                                BasicConsole.WriteLine("DSC: Request pages : Any physical, Any virtual");
                                BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                ptr = (uint)Hardware.VirtMemManager.MapFreePages(
                                                    CallerProcess.UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None :
                                                    Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly, count);
                            }
                            else
                            {
                                // Any physical, specific virtual

#if DSC_TRACE
                                BasicConsole.WriteLine("DSC: Request pages : Any physical, Specific virtual");
                                BasicConsole.WriteLine("Request virtual address: " + (FOS_System.String)Param2);
                                BasicConsole.WriteLine("Request count: " + (FOS_System.String)count);
#endif
                                if (!Hardware.VirtMemManager.AreAnyVirtualMapped(Param2, (uint)count))
                                {
#if DSC_TRACE
                                    BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                    ptr = (uint)Hardware.VirtMemManager.MapFreePages(
                                                    CallerProcess.UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None :
                                                    Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly, count, Param2);
                                }
#if DSC_TRACE
                                else
                                {
                                    BasicConsole.WriteLine("First page mapped physical address: " + (FOS_System.String)Hardware.VirtMemManager.GetPhysicalAddress(Param2));
                                }
#endif
                            }
                        }
                        else
                        {
                            if (Param2 == 0xFFFFFFFF)
                            {
                                // Specific physical, any virtual

#if DSC_TRACE
                                BasicConsole.WriteLine("DSC: Request pages : Specific physical, Any virtual");
#endif
                                if (!Hardware.VirtMemManager.AreAnyPhysicalMapped(Param1, (uint)count))
                                {
#if DSC_TRACE
                                    BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                    ptr = (uint)Hardware.VirtMemManager.MapFreePhysicalPages(
                                                    CallerProcess.UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None :
                                                    Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly, count, Param1);
                                }
                            }
                            else
                            {
                                // Specific physical, specific virtual

#if DSC_TRACE
                                BasicConsole.WriteLine("DSC: Request pages : Specific physical, Specific virtual");
#endif
                                if (!Hardware.VirtMemManager.AreAnyVirtualMapped(Param2, (uint)count))
                                {
                                    if (!Hardware.VirtMemManager.AreAnyPhysicalMapped(Param1, (uint)count))
                                    {
#if DSC_TRACE
                                        BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                        ptr = (uint)Hardware.VirtMemManager.MapFreePages(
                                                        CallerProcess.UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None :
                                                        Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly, count, Param2, Param1);
                                    }
                                }
                            }
                        }

                        if (ptr != 0xFFFFFFFF && ptr != 0xDEADBEEF)
                        {
#if DSC_TRACE
                            BasicConsole.WriteLine("DSC: Request pages : Map successful.");
#endif

                            pAddrs = new uint[count];
                            for (uint currPtr = ptr, i = 0; i < count; currPtr += 4096, i++)
                            {
                                pAddrs[i] = Hardware.VirtMemManager.GetPhysicalAddress(currPtr);
                            }

                            // Add allocated new process's memory to layout
                            CallerProcess.TheMemoryLayout.AddDataPages(ptr, pAddrs);

                            result = SystemCallResults.OK;

                            Return2 = ptr;
                            Return3 = 0;
                            Return4 = 0;

#if DSC_TRACE
                            BasicConsole.WriteLine("DSC: Request pages - OK.");
#endif
                        }
                    }
                    catch
                    {
                        BasicConsole.WriteLine("Error during Request Pages system call!");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }
                    finally
                    {
                        ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);
                    }
                    break;
                case SystemCallNumbers.UnmapPages:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Unmap pages");
#endif
                    // Param1: Start Virtual Address
                    // Param2: Count
                    CallerProcess.TheMemoryLayout.RemovePages(Param1, Param2);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.SharePages:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Share pages");
#endif
                    // Param1: Start Virtual Address
                    // Param2: Count
                    // Param3: Target Process Id

                    // 2nd stage of Share Pages - deferred, pages already accepted

                    Process TargetProcess = ProcessManager.GetProcessById(Param3);
                    uint[] PhysicalAddresses = CallerProcess.TheMemoryLayout.GetPhysicalAddresses(Param1, Param2);
                    TargetProcess.TheMemoryLayout.AddDataPages(Param1, PhysicalAddresses);
                    TargetProcess.ResumeThreads();

                    result = SystemCallResults.OK;

                    break;
                case SystemCallNumbers.CreateSemaphore:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Create semaphore");
#endif
                    // Param1: Limit
                    Return2 = (uint)ProcessManager.Semaphore_Allocate((int)Param1, CallerProcess);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.ShareSemaphore:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Share semaphore");
#endif
                    // Param1: Id
                    // Param2: New owner Id
                    {
                        bool added = ProcessManager.Semaphore_AddOwner((int)Param1, Param2, CallerProcess);
                        result = added ? SystemCallResults.OK : SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.ReleaseSemaphore:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Release semaphore");
#endif
                    // Param1: Id
                    {
                        bool removed = ProcessManager.Semaphore_RemoveOwner((int)Param1, CallerProcess.Id, CallerProcess);
                        if (removed)
                        {
                            ProcessManager.Semaphore_CheckForDeallocate((int)Param1);
                        }
                        result = removed ? SystemCallResults.OK : SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.WaitSemaphore:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Wait semaphore");
#endif
                    // Param1: Id
                    {
                        int waitResult = ProcessManager.Semaphore_Wait((int)Param1, CallerProcess, CallerThread);
                        result = waitResult == -1 ? SystemCallResults.Fail : (waitResult == 1 ? SystemCallResults.OK : SystemCallResults.OK_NoWake);
                    }
                    break;
                case SystemCallNumbers.SignalSemaphore:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Signal semaphore");
#endif
                    // Param1: Id
                    {
                        bool signalResult = ProcessManager.Semaphore_Signal((int)Param1, CallerProcess);
                        result = signalResult ? SystemCallResults.OK : SystemCallResults.Fail;
                    }
                    break;
                default:
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Unrecognised call number.");
                    BasicConsole.WriteLine((uint)syscallNumber);
#endif
                    break;
            }

            return result;
        }
        public static void EndDeferredSystemCall(Process CallerProcess, Thread CallerThread, SystemCallResults result, uint Return2, uint Return3, uint Return4)
        {
            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
            CallerThread.Return1 = (uint)(result == SystemCallResults.OK_NoWake ? SystemCallResults.OK : result);
            CallerThread.Return2 = Return2;
            CallerThread.Return3 = Return3;
            CallerThread.Return4 = Return4;
            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            if (result != SystemCallResults.OK_NoWake)
            {
                CallerThread._Wake();
            }
        }

        public static int HandleISR(uint ISRNum)
        {
            if (ISRNum == 48)
            {
                SystemCalls.InterruptHandler();
                return 0;
            }
            return -1;
        }
        public static int HandleIRQ(uint IRQNum)
        {
            if (IRQNum == 0)
            {
                Hardware.Timers.PIT.ThePIT.InterruptHandler();
                return 0;
            }
            return -1;
        }

        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3, 
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = HandleSystemCall(syscallNumber, 
                param1, param2, param3, 
                ref Return2, ref Return3, ref Return4,
                callerProcessId, callerThreadId);

            if (result == SystemCallResults.Deferred || result == SystemCallResults.Deferred_PermitActions)
            {
                //BasicConsole.WriteLine("Deferring syscall...");
                //BasicConsole.WriteLine("Popping unqueued info object...");
                DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Unqueued.Pop();
                //BasicConsole.WriteLine("Setting info...");
                info.ProcessId = callerProcessId;
                info.ThreadId = callerThreadId;

                //BasicConsole.WriteLine("Queuing info object...");
                DeferredSyscallsInfo_Queued.Push(info);

                //BasicConsole.WriteLine("Waking deferred syscalls thread...");
                DeferredSyscallsThread._Wake();
            }

            return (int)result;
        }
        /// <summary>
        /// Special handler method for system calls recognised/handlded by the kernel task.
        /// </summary>
        /// <param name="syscallNumber">The system call number that has been invoked.</param>
        /// <param name="param1">The value of the first parameter.</param>
        /// <param name="param2">The value of the second parameter.</param>
        /// <param name="param3">The value of the third parameter.</param>
        /// <param name="Return2">Reference to the second return value.</param>
        /// <param name="Return3">Reference to the third return value.</param>
        /// <param name="Return4">Reference to the fourth return value.</param>
        /// <param name="callerProcessId">The Id of the process which invoked the system call.</param>
        /// <param name="callerThreadId">The Id of the thread which invoked the system call.</param>
        /// <returns>A system call result indicating what has occurred and what should occur next.</returns>
        /// <remarks>
        /// Executes within the interrupt handler. Usual restrictions apply.
        /// </remarks>
        public static SystemCallResults HandleSystemCall(uint syscallNumber,
            uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.SleepThread:
#if SYSCALLS_TRACE
                    //BasicConsole.WriteLine("System call : Sleep Thread");
#endif
                    SysCall_Sleep((int)param1, callerProcessId, callerThreadId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.WakeThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wake Thread");
#endif
                    SysCall_Wake(callerProcessId, param1);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterISRHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register ISR Handler");
#endif
                    if (SysCall_RegisterISRHandler((int)param1, param2, callerProcessId))
                    {
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        result = SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.DeregisterISRHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister ISR Handler");
#endif
                    SysCall_DeregisterISRHandler((int)param1, callerProcessId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterIRQHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register IRQ Handler");
#endif
                    if (SysCall_RegisterIRQHandler((int)param1, param2, callerProcessId))
                    {
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        result = SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.DeregisterIRQHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister IRQ Handler");
#endif
                    SysCall_DeregisterIRQHandler((int)param1, callerProcessId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterSyscallHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Syscall Handler");
#endif
                    if (SysCall_RegisterSyscallHandler((int)param1, param2, callerProcessId))
                    {
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        result = SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.DeregisterSyscallHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister Syscall Handler");
#endif
                    SysCall_DeregisterSyscallHandler((int)param1, callerProcessId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.StartThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Start Thread");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Pipe Outpoint");
#endif
                    result = SystemCallResults.Deferred_PermitActions;
                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Num Pipe Outpoints");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.GetPipeOutpoints:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Pipe Outpoints");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.CreatePipe:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Create Pipe");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.WaitOnPipeCreate:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wait On Pipe Create");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.ReadPipe:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Read Pipe");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.WritePipe:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Write Pipe");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.SendMessage:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Send message");
#endif
                    result = SysCall_SendMessage(callerProcessId, callerThreadId, param1, param2, param3) ? SystemCallResults.OK : SystemCallResults.Fail;
                    break;
                case SystemCallNumbers.ReceiveMessage:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Receive message");
#endif
                    ReceiveMessage(callerProcessId, param1, param2);
                    break;
                case SystemCallNumbers.RequestPages:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Request pages");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.IsPhysicalAddressMapped:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Is physical address mapped");
#endif
                    
                    // TODO: This is a bit hacky
                    // If address is in low 1MiB then it is mapped
                    if (param1 < 0x100000)
                    {
                        Return2 = 1u;
                    }
                    else
                    {
                        Return2 = ProcessManager.GetProcessById(callerProcessId).TheMemoryLayout.ContainsAnyPhysicalAddresses(param1, 1) ? 1u : 0u;
                    }
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.IsVirtualAddressMapped:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Is virtual address mapped");
#endif
                    // TODO: This is a bit hacky
                    // If address is in low 1MiB then it is mapped
                    if (param1 < 0x100000)
                    {
                        Return2 = 1u;
                    }
                    else
                    {
                        Return2 = ProcessManager.GetProcessById(callerProcessId).TheMemoryLayout.ContainsAnyVirtualAddresses(param1, 1) ? 1u : 0u;
                    }
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.GetPhysicalAddress:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Get physical address");
#endif
                    // TODO: This is a bit hacky
                    // If address is in low 1MiB then it is mapped
                    if (param1 < 0x100000)
                    {
                        Return2 = param1;
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        Return2 = ProcessManager.GetProcessById(callerProcessId).TheMemoryLayout.GetPhysicalAddress(param1 & 0xFFFFF000);
                        if (Return2 != 0xFFFFFFFF)
                        {
                            Return2 = Return2 | (param1 & 0x00000FFF);
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            Return2 = 0;
                            result = SystemCallResults.Fail;
                        }
                    }
                    break;
                case SystemCallNumbers.GetVirtualAddress:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Get virtual address");
#endif
                    // TODO: This is a bit hacky
                    // If address is in low 1MiB then it is mapped
                    if (param1 < 0x100000)
                    {
                        Return2 = param1;
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        Return2 = ProcessManager.GetProcessById(callerProcessId).TheMemoryLayout.GetVirtualAddress(param1 & 0xFFFFF000);
                        if (Return2 != 0xFFFFFFFF)
                        {
                            Return2 = Return2 | (param1 & 0x00000FFF);
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            Return2 = 0;
                            result = SystemCallResults.Fail;
                        }
                    }
                    break;
                case SystemCallNumbers.UnmapPages:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Unmap pages");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.SharePages:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Share pages");
#endif
                    // Param1: Start Virtual Address
                    // Param2: Count
                    // Param3: Target Process Id

                    // 1st stage of Share Pages - propose, accept/reject, defer for adding/return
                    {
                        // Assume failure
                        result = SystemCallResults.Fail;

                        Process CallerProcess = ProcessManager.GetProcessById(callerProcessId);
                        if (CallerProcess.TheMemoryLayout.ContainsAllVirtualAddresses(param1, param2, 4096u))
                        {
                            Process TargetProcess = ProcessManager.GetProcessById(param3);
                            if (!TargetProcess.TheMemoryLayout.ContainsAnyVirtualAddresses(param1, (int)param2 * 4096))
                            {
                                if (TargetProcess.SyscallsToHandle.IsSet((int)SystemCallNumbers.AcceptPages) && TargetProcess.SyscallHandler != null)
                                {
                                    uint XReturn2 = 0;
                                    uint XReturn3 = 0;
                                    uint XReturn4 = 0;
                                    ProcessManager.SwitchProcess(param3, -1);
                                    SystemCallResults AcceptPagesResult = (SystemCallResults)TargetProcess.SyscallHandler((uint)SystemCallNumbers.AcceptPages, param1, param2, 0, ref XReturn2, ref XReturn3, ref XReturn4, CallerProcess.Id, 0xFFFFFFFF);
                                    ProcessManager.SwitchProcess(ProcessManager.KernelProcess.Id, (int)DeferredSyscallsThread.Id);
                                    if (AcceptPagesResult == SystemCallResults.OK)
                                    {
                                        //Suspend the target process until the memory has actually been added to its layout
                                        //  Adding the pages to the layout has to be deferred as it may require more memory for the underlying dictionaries.
                                        TargetProcess.SuspendThreads();
                                        result = SystemCallResults.Deferred;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SystemCallNumbers.CreateSemaphore:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Create semaphore");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.ShareSemaphore:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Share semaphore");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.ReleaseSemaphore:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Release semaphore");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.WaitSemaphore:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Wait semaphore");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.SignalSemaphore:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Signal semaphore");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                    
                //TODO: Implement handlers for remaining sys calls
                //#if SYSCALLS_TRACE
                default:
                    BasicConsole.WriteLine("System call unrecognised/unhandled by Kernel Task.");
                    break;
                //#endif
            }

            return result;
        }

        /// <summary>
        /// Performs Sleep system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ms">The number of milliseconds to sleep for.</param>
        /// <param name="callerProcessId">The Id of the process to sleep.</param>
        /// <param name="callerThreadId">The Id of the thread to sleep.</param>
        private static void SysCall_Sleep(int ms, uint callerProcessId, uint callerThreadId)
        {
#if SYSCALLS_TRACE
            //BasicConsole.WriteLine("Sleeping thread...");
#endif
            ProcessManager.GetThreadById(callerThreadId, ProcessManager.GetProcessById(callerProcessId))._EnterSleep(ms);
        }
        /// <summary>
        /// Performs Wake system call processing for the Kernel Task.
        /// </summary>
        /// <param name="callerProcessId">The Id of the process to wake.</param>
        /// <param name="threadToWakeId">The Id of the thread to wake.</param>
        private static void SysCall_Wake(uint callerProcessId, uint threadToWakeId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Waking thread...");
#endif
            ProcessManager.GetThreadById(threadToWakeId, ProcessManager.GetProcessById(callerProcessId))._Wake();
        }
        /// <summary>
        /// Performs Register ISR Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ISRNum">The ISR number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterISRHandler(int ISRNum, uint handlerAddr, uint callerProcessId)
        {
            if (ISRNum < 49)
            {
                return false;
            }

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering ISR handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.ISRHandler = (Hardware.Processes.ISRHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.ISRsToHandle.Set(ISRNum);

            return true;
        }
        /// <summary>
        /// Performs Deregister ISR Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ISRNum">The ISR number to deregister.</param>
        /// <param name="callerProcessId">The Id of the process to deregister the handler of.</param>
        private static void SysCall_DeregisterISRHandler(int ISRNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering ISR handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).ISRsToHandle.Clear(ISRNum);
        }
        /// <summary>
        /// Performs Register IRQ Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="IRQNum">The IRQ number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterIRQHandler(int IRQNum, uint handlerAddr, uint callerProcessId)
        {
            if (IRQNum > 15)
            {
                return false;
            }

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering IRQ handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.IRQHandler = (Hardware.Processes.IRQHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.IRQsToHandle.Set(IRQNum);

            Hardware.Interrupts.Interrupts.EnableIRQ((byte)IRQNum);

            return true;
        }
        /// <summary>
        /// Performs Deregister IRQ Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="IRQNum">The IRQ number to deregister.</param>
        /// <param name="callerProcessId">The Id of the process to deregister the handler of.</param>
        private static void SysCall_DeregisterIRQHandler(int IRQNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering IRQ handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).IRQsToHandle.Clear(IRQNum);
        }
        /// <summary>
        /// Performs Register System Call Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="syscallNum">The system call number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterSyscallHandler(int syscallNum, uint handlerAddr, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering syscall handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.SyscallHandler = (Hardware.Processes.SyscallHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.SyscallsToHandle.Set(syscallNum);

            return true;
        }
        /// <summary>
        /// Performs Deregister System Call Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="syscallNum">The system call number to deregister.</param>
        /// <param name="callerProcessId">The Id of the process to deregister the handler of.</param>
        private static void SysCall_DeregisterSyscallHandler(int syscallNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering syscall handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).SyscallsToHandle.Clear(syscallNum);
        }
        /// <summary>
        /// Performs Send Message system call processing for the Kernel Task.
        /// </summary>
        /// <param name="callerProcessId">The Id of the process sending the message.</param>
        /// <param name="callerThreadId">The Id of the thread sending the message.</param>
        /// <param name="targetProcessId">The Id of the process to send the message to.</param>
        /// <param name="message1">The first value of the message.</param>
        /// <param name="message2">The second value of the message.</param>
        /// <returns>True if the message was accepted.</returns>
        private static bool SysCall_SendMessage(uint callerProcessId, uint callerThreadId, uint targetProcessId, uint message1, uint message2)
        {
            bool Result = false;

            Process CallerProcess = ProcessManager.GetProcessById(callerProcessId);
            Process TargetProcess = ProcessManager.GetProcessById(targetProcessId);

            if (TargetProcess.SyscallsToHandle.IsSet((int)SystemCallNumbers.ReceiveMessage) && TargetProcess.SyscallHandler != null)
            {
                uint Return2 = 0;
                uint Return3 = 0;
                uint Return4 = 0;
                ProcessManager.SwitchProcess(targetProcessId, -1);
                TargetProcess.SyscallHandler((uint)SystemCallNumbers.ReceiveMessage, message1, message2, 0, ref Return2, ref Return3, ref Return4, callerProcessId, 0xFFFFFFFF);
                ProcessManager.SwitchProcess(callerProcessId, (int)callerThreadId);

                Result = true;
            }

            return Result;
        }


        public static void ReceiveMessage(uint CallerProcessId, uint Message1, uint Message2)
        {
            if (CallerProcessId == WindowManagerTask_ProcessId)
            {
                ReceiveKey(Message1);
            }
        }
        public static void ReceiveKey(uint ScanCode)
        {
            if (keyboard != null)
            {
                keyboard.HandleScancode(ScanCode);
            }
        }
    }

    public class TestComparable : FOS_System.Collections.Comparable
    {
        public TestComparable()
        {
        }
        public TestComparable(int key)
             : base(key)
        {
        }
    }
}
