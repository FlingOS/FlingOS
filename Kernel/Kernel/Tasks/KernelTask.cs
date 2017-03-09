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

// DSC  : Deferred System Calls
// DFSC : Deferred File System Calls
//#define DSC_TRACE
//#define DFSC_TRACE
//#define SYSCALLS_TRACE

using Drivers.Compiler.Attributes;
using Kernel.Consoles;
using Kernel.Devices;
using Kernel.Devices.Keyboards;
using Kernel.Devices.Serial;
using Kernel.Devices.Timers;
using Kernel.FileSystems;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Framework.Processes.Requests.Processes;
using Kernel.Multiprocessing;
using Kernel.Pipes;
using Kernel.Shells;
using Kernel.Tasks.App;
using Kernel.Tasks.Driver;
using Kernel.Utilities;
using Kernel.VGA.Configurations.Graphical;
using Kernel.VGA.Fonts;
using Kernel.VirtualMemory;
using x86Interrupts = Kernel.Interrupts.Interrupts;

namespace Kernel.Tasks
{
    public static unsafe class KernelTask
    {
        public static bool Terminating = false;

        [Group(Name = "IsolatedKernel")] private static Queue DeferredSyscallsInfo_Unqueued;

        [Group(Name = "IsolatedKernel")] private static Queue DeferredSyscallsInfo_Queued;

        [Group(Name = "IsolatedKernel")] private static bool DeferredSyscalls_Ready;

        [Group(Name = "IsolatedKernel")] private static Thread DeferredSyscallsThread;

        [Group(Name = "IsolatedKernel")] private static int DeferredSyscalls_QueuedSemaphoreId;

        [Group(Name = "IsolatedKernel")] private static bool FilePipesReady;

        [Group(Name = "IsolatedKernel")] private static Thread FilePipeInitialisationThread;

        [Group(Name = "IsolatedKernel")] private static Thread FileSystemManagerInitialisationThread;

        [Group(Name = "IsolatedKernel")] private static int FilePipeAvailable_SemaphoreId;

        [Group(Name = "IsolatedKernel")] private static int FileSystemManagerAvailable_SemaphoreId;

        [Group(Name = "IsolatedKernel")] public static uint WindowManagerTask_ProcessId;

        private static VirtualKeyboard keyboard;
        private static VirtualConsole console;

        [Group(Name = "IsolatedKernel")] private static int File_ConnectSemaphoreId;

        [Group(Name = "IsolatedKernel")] private static List FileSystemAccessors;

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
                        BasicConsole.WriteLine("Pair: key=" + (Framework.String)pair.Key + ", value=" + pair.Value);
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
                        BasicConsole.WriteLine("Pair: key=" + (Framework.String)pair.Key + ", value=" + pair.Value);
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
            DeferredSyscalls_QueuedSemaphoreId = ProcessManager.Semaphore_Allocate(-1, ProcessManager.KernelProcess);

            //TODO: These need registering via system calls and message or pipe interfaces created
            //      Really they shouldn't be initialised by Kernel Task nor used by Kernel/Debugger Tasks directly.
            //      But they are needed for from-startup/first-instance/fail-proof debugging.
            //DeviceManager.AddDevice(Serial.COM1);
            //DeviceManager.AddDevice(Serial.COM2);
            //DeviceManager.AddDevice(Serial.COM3);

            try
            {
                BasicConsole.WriteLine("> Initialising kernel ISRs...");
                ProcessManager.CurrentProcess.ISRHandler = HandleISR;
                ProcessManager.CurrentProcess.SwitchProcessForISRs = false;
                ProcessManager.CurrentProcess.ISRsToHandle.Set(48);

                BasicConsole.WriteLine(" > Initialising system calls...");
                ProcessManager.CurrentProcess.SyscallHandler = SyscallHandler;
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterISRHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterISRHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterIRQHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterIRQHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.StartProcess);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetNumProcesses);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetProcessList);
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
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.AbortPipeReadWrite);
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
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterDevice);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterDevice);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetNumDevices);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetDeviceList);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetDeviceInfo);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ClaimDevice);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ReleaseDevice);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterTimerEvent);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterTimerEvent);

                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.StatFS);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.InitFS);

                //ProcessManager.CurrentProcess.OutputMemTrace = true;

                BasicConsole.WriteLine(" > Forcing initial GC cleanup...");
                GC.Cleanup();

                BasicConsole.WriteLine(" > Starting GC Cleanup thread...");
                ProcessManager.CurrentProcess.CreateThread(GCCleanupTask.Main, "GC Cleanup", null);

                BasicConsole.WriteLine(" > Starting deferred syscalls thread...");
                DeferredSyscallsThread = ProcessManager.CurrentProcess.CreateThread(DeferredSyscallsThread_Main,
                    "Deferred Sys Calls", null);

                while (!DeferredSyscalls_Ready)
                {
                    BasicConsole.WriteLine("Waiting on deferred syscalls thread...");
                    SystemCalls.SleepThread(50);
                }
                
                BasicConsole.WriteLine(" > Starting Idle process...");
                Process IdleProcess1 = ProcessManager.CreateProcess(IdleTask.Main, "Idle Task", false, null);
                ProcessManager.RegisterProcess(IdleProcess1, Scheduler.Priority.ZeroTimed);

                BasicConsole.WriteLine(" > Initialising device manager...");
                DeviceManager.Init();
                DeviceManager.InitForProcess();

                BasicConsole.WriteLine(" > Registering PIT device...");
                DeviceManager.RegisterDevice(PIT.ThePIT);

                BasicConsole.WriteLine(" > Registering COM1 device...");
                DeviceManager.RegisterDevice(Serial.COM1);

                BasicConsole.WriteLine(" > Setting up VGA...");
                VGA.VGA TheVGA = VGA.VGA.GetConfiguredInstance(T_80x25.Instance, Jupitor.Instance);
                TheVGA.SetCGAPalette();

                BasicConsole.WriteLine(" > Starting test thread...");
                uint NewThreadId;
                if (SystemCalls.StartThread(ObjectUtilities.GetHandle((TestThreadStartPoint)Test), out NewThreadId, new uint[] { 0xDEADBEEF }) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine(" > Failed to start test thread.");
                }
                
#if DEBUG
                //BasicConsole.WriteLine(" > Starting Debugger thread...");
                //Debug.Debugger.MainThread = ProcessManager.CurrentProcess.CreateThread(Debug.Debugger.Main, "Debugger");
#endif

                BasicConsole.PrimaryOutputEnabled = false;
                //BasicConsole.SecondaryOutputEnabled = false;

                BasicConsole.WriteLine(" > Starting Window Manager...");
                Process WindowManagerProcess = ProcessManager.CreateProcess(WindowManagerTask.Main, "Window Manager",
                    false, null);
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

                BasicConsole.WriteLine(" > Starting file pipe initialisation thread...");
                FilePipeInitialisationThread = ProcessManager.CurrentProcess.CreateThread(FilePipeInitialisation_Main,
                    "File Management : File Pipe Initialisation", null);

                while (!FilePipesReady)
                {
                    BasicConsole.WriteLine("Waiting on file pipes to be ready...");
                    SystemCalls.SleepThread(50);
                }

                BasicConsole.WriteLine(" > Starting File Systems driver...");
                Process FileSystemsProcess = ProcessManager.CreateProcess(FileSystemsDriverTask.Main,
                    "File Systems Driver", false, null);
                ProcessManager.RegisterProcess(FileSystemsProcess, Scheduler.Priority.Normal);

                BasicConsole.WriteLine(" > Waiting for File System Driver to be ready...");
                while (!FileSystemsDriverTask.Ready)
                {
                    BasicConsole.WriteLine(" > [Wait pause]");
                    SystemCalls.SleepThread(1000);
                }
                BasicConsole.WriteLine(" > File System Driver reported ready.");

                BasicConsole.WriteLine(" > Starting Device Info task...");
                Process DeviceManagerProcess = ProcessManager.CreateProcess(DeviceInfoTask.Main, "Device Info", false, null);
                ProcessManager.RegisterProcess(DeviceManagerProcess, Scheduler.Priority.Normal);

                BasicConsole.WriteLine(" > Starting System Status App...");
                Process SystemStatusProcess = ProcessManager.CreateProcess(SystemStatusTask.Main, "System Status App",
                    false, null);
                ProcessManager.RegisterProcess(SystemStatusProcess, Scheduler.Priority.Normal);

                BasicConsole.WriteLine("Kernel Started.");

                try
                {
                    BasicConsole.WriteLine("KT > Creating virtual keyboard...");
                    keyboard = new VirtualKeyboard();

                    BasicConsole.WriteLine("KT > Creating virtual console...");
                    console = new VirtualConsole();

                    BasicConsole.WriteLine("KT > Connecting virtual console...");
                    console.Connect();

                    BasicConsole.WriteLine("KT > Creating main shell...");
                    MainShell shell = new MainShell(console, keyboard);

                    BasicConsole.WriteLine("KT > Running...");

                    //uint loops = 0;
                    while (!Terminating)
                    {
                        try
                        {
                            //Framework.String msg = "KT > Hello, world! (" + (Framework.String)loops++ + ")";
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
                DeferredSyscalls_Ready = true;

                ProcessManager.Semaphore_WaitCurrentThread(DeferredSyscalls_QueuedSemaphoreId);

                while (DeferredSyscallsInfo_Queued.Count > 0)
                {
                    // Scheduler must be disabled during pop/push from circular buffer or we can
                    //  end up in an infinite lock. Consider what happens if a process invokes 
                    //  a deferred system call during the pop/push here and at the end of this loop.
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Pausing scheduler...");
#endif
                    Scheduler.Disable( /*"DSC 1"*/);
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
                    Scheduler.Disable( /*"DSC 2"*/);
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

        public static SystemCallResults HandleDeferredSystemCall(
            Process CallerProcess, Thread CallerThread,
            SystemCallNumbers syscallNumber, uint Param1, uint Param2, uint Param3,
            ref uint Return2, ref uint Return3, ref uint Return4)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch (syscallNumber)
            {
                case SystemCallNumbers.StartProcess:

                    #region Start Process

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Start Process");
#endif
                    {
                        ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                        StartProcessRequest* StartRequest = (StartProcessRequest*)Param1;
                        uint* OriginStartArgs = StartRequest->StartArgs;
                        uint[] LocalStartArgs = new uint[StartRequest->StartArgsLength];
                        for (int i = 0; i < StartRequest->StartArgsLength; i++)
                        {
                            LocalStartArgs[i] = OriginStartArgs[i];
                        }
                        ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

                        Process NewProcess = ProcessManager.CreateProcess(Param2 == 1 || CallerProcess.UserMode,
                            CallerProcess, StartRequest, LocalStartArgs);
                        ProcessManager.RegisterProcess(NewProcess, Scheduler.Priority.Normal);

                        Return2 = NewProcess.Id;
                        Return3 = ((Thread)NewProcess.Threads[0]).Id;
                    }
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Start Process - done.");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.GetNumProcesses:

                    #region Get Num Processes

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get Num Processes");
#endif

                    Return2 = (uint)ProcessManager.Processes.Count;
                    result = SystemCallResults.OK;

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get Num Processes - done.");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.GetProcessList:

                    #region Get Process List

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get Num Processes");
#endif
                {
                    // Need access to the request structure
                    ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

                    ProcessDescriptor* ProcessList = (ProcessDescriptor*)Param1;
                    int MaxDescriptors = (int)Param2;

                    for (int i = 0; i < MaxDescriptors && i < ProcessManager.Processes.Count; i++)
                    {
                        Process AProcess = (Process)ProcessManager.Processes[i];
                        ProcessDescriptor* ADescriptor = ProcessList + i;
                        ADescriptor->Id = AProcess.Id;
                        ADescriptor->NumThreads = AProcess.Threads.Count;
                        ADescriptor->Priority = (int)AProcess.Priority;
                        ADescriptor->UserMode = AProcess.UserMode;
                    }

                    result = SystemCallResults.OK;

                    ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);
                }

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get Num Processes - done.");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.StartThread:

                    #region Start Thread

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Start Thread");
#endif
                    {
                        ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                        uint* OriginStartArgs = (uint*)Param2;
                        uint[] LocalStartArgs = new uint[Param3];
                        for (int i = 0; i < Param3; i++)
                        {
                            LocalStartArgs[i] = OriginStartArgs[i];
                        }
                        ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

                        Return2 =
                            CallerProcess.CreateThread((ThreadStartPoint)ObjectUtilities.GetObject((void*)Param1),
                                "[From sys call]", LocalStartArgs).Id;
                    }
#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Start Thread - done.");
#endif
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:

                    #region Register Pipe Outpoint

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Register Pipe Outpoint");
#endif
                    PipeOutpoint outpoint;
                    bool registered = PipeManager.RegisterPipeOutpoint(CallerProcess.Id, (PipeClasses)Param1,
                        (PipeSubclasses)Param2, (int)Param3, out outpoint);
                    if (registered)
                    {
                        if ((PipeClasses)Param1 == PipeClasses.File &&
                            (PipeSubclasses)Param2 == PipeSubclasses.File_Data_Out)
                        {
                            ProcessManager.Semaphore_Signal(FilePipeAvailable_SemaphoreId, ProcessManager.KernelProcess);
                        }

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

                    #endregion

                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:

                    #region Get Num Pipe Outpoints

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints");
#endif
                    int numOutpoints;
                    bool obtained = PipeManager.GetNumPipeOutpoints((PipeClasses)Param1, (PipeSubclasses)Param2,
                        out numOutpoints);
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

                    #endregion

                    break;
                case SystemCallNumbers.GetPipeOutpoints:

                    #region Get Pipe Outpoints

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Get Pipe Outpoints");
#endif

                    bool obtained = PipeManager.GetPipeOutpoints(CallerProcess, (PipeClasses)Param1,
                        (PipeSubclasses)Param2, (PipeOutpointsRequest*)Param3);
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

                    #endregion

                    break;
                case SystemCallNumbers.CreatePipe:

                    #region Create Pipe

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Create Pipe");
#endif

                    bool created = PipeManager.CreatePipe(CallerProcess.Id, Param1, (CreatePipeRequest*)Param2);
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

                    #endregion

                    break;
                case SystemCallNumbers.WaitOnPipeCreate:

                    #region Wait On Pipe Create

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Wait On Pipe Create");
#endif

                    bool waiting = PipeManager.WaitOnPipeCreate(CallerProcess.Id, CallerThread.Id,
                        (WaitOnPipeCreateRequest*)Param1);
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

                    #endregion

                    break;
                case SystemCallNumbers.ReadPipe:

                    #region Read Pipe

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Read Pipe");
#endif

                    // Need access to calling process' memory to be able to read values from request structure
                    ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                    ReadPipeRequest* RequestPtr = (ReadPipeRequest*)Param1;
                    RequestPtr->Aborted = false;
                    int PipeId = RequestPtr->PipeId;
                    bool Blocking = RequestPtr->Blocking;
                    ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

                    PipeManager.RWResults RWResult = PipeManager.ReadPipe(PipeId, Blocking, CallerProcess, CallerThread);

                    if (RWResult == PipeManager.RWResults.Error)
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

                    #endregion

                    break;
                case SystemCallNumbers.WritePipe:

                    #region Write Pipe

                {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Write Pipe");
#endif

                    // Need access to calling process' memory to be able to read values from request structure
                    ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                    ReadPipeRequest* RequestPtr = (ReadPipeRequest*)Param1;
                    RequestPtr->Aborted = false;
                    int PipeId = RequestPtr->PipeId;
                    bool Blocking = RequestPtr->Blocking;
                    ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

                    PipeManager.RWResults RWResult = PipeManager.WritePipe(PipeId, Blocking, CallerProcess, CallerThread);

                    if (RWResult == PipeManager.RWResults.Error)
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

                    #endregion

                    break;
                case SystemCallNumbers.AbortPipeReadWrite:

                    #region Abort Pipe Read/Write

                    {
#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Abort Pipe Read/Write");
#endif

                        int PipeId = (int)Param1;
                        bool Aborted = PipeManager.AbortPipeReadWrite(PipeId, CallerProcess);

                        if (!Aborted)
                        {
                            result = SystemCallResults.Fail;
                        }
                        else
                        {
                            result = SystemCallResults.OK;
                        }

#if DSC_TRACE
                        BasicConsole.WriteLine("DSC: Abort Pipe Read/Write - done");
#endif
                    }

                    #endregion

                    break;
                case SystemCallNumbers.RequestPages:

                    #region Request Pages

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
                                void* unusedPAddr;
                                if (CallerProcess == ProcessManager.KernelProcess)
                                {
                                    ptr = (uint)VirtualMemoryManager.MapFreePagesForKernel(
                                        CallerProcess.UserMode
                                            ? PageFlags.None
                                            : PageFlags.KernelOnly, count, out unusedPAddr);
                                }
                                else
                                {
                                    ptr = (uint)VirtualMemoryManager.MapFreePages(
                                        CallerProcess.UserMode
                                            ? PageFlags.None
                                            : PageFlags.KernelOnly, count, out unusedPAddr);
                                }
                            }
                            else
                            {
                                // Any physical, specific virtual

#if DSC_TRACE
                                BasicConsole.WriteLine("DSC: Request pages : Any physical, Specific virtual");
                                BasicConsole.WriteLine("Request virtual address: " + (Framework.String)Param2);
                                BasicConsole.WriteLine("Request count: " + (Framework.String)count);
#endif
                                if (!VirtualMemoryManager.AreAnyVirtualMapped(Param2, (uint)count))
                                {
#if DSC_TRACE
                                    BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                    void* unusedPAddr;
                                    ptr = (uint)VirtualMemoryManager.MapFreePages(
                                        CallerProcess.UserMode
                                            ? PageFlags.None
                                            : PageFlags.KernelOnly, count, Param2,
                                        out unusedPAddr);
                                }
#if DSC_TRACE
                                else
                                {
                                    BasicConsole.WriteLine("First page mapped physical address: " + (String)VirtualMemoryManager.GetPhysicalAddress(Param2));
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
                                if (!VirtualMemoryManager.AreAnyPhysicalMapped(Param1, (uint)count))
                                {
#if DSC_TRACE
                                    BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                    if (CallerProcess == ProcessManager.KernelProcess)
                                    {
                                        ptr = (uint)VirtualMemoryManager.MapFreePhysicalPagesForKernel(
                                            CallerProcess.UserMode
                                                ? PageFlags.None
                                                : PageFlags.KernelOnly, count, Param1);
                                    }
                                    else
                                    {
                                        ptr = (uint)VirtualMemoryManager.MapFreePhysicalPages(
                                            CallerProcess.UserMode
                                                ? PageFlags.None
                                                : PageFlags.KernelOnly, count, Param1);
                                    }
                                }
                            }
                            else
                            {
                                // Specific physical, specific virtual

#if DSC_TRACE
                                BasicConsole.WriteLine("DSC: Request pages : Specific physical, Specific virtual");
#endif
                                if (!VirtualMemoryManager.AreAnyVirtualMapped(Param2, (uint)count))
                                {
                                    if (!VirtualMemoryManager.AreAnyPhysicalMapped(Param1, (uint)count))
                                    {
#if DSC_TRACE
                                        BasicConsole.WriteLine("DSC: Request pages : Okay to map");
#endif
                                        ptr = (uint)VirtualMemoryManager.MapFreePages(
                                            CallerProcess.UserMode
                                                ? PageFlags.None
                                                : PageFlags.KernelOnly, count, Param2,
                                            Param1);
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
                                pAddrs[i] = VirtualMemoryManager.GetPhysicalAddress(currPtr);
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

                    #endregion

                    break;
                case SystemCallNumbers.UnmapPages:

                    #region Unmap Pages

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Unmap pages");
#endif
                    // Param1: Start Virtual Address
                    // Param2: Count
                    CallerProcess.TheMemoryLayout.RemovePages(Param1, Param2);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.SharePages:

                    #region Share Pages

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

                    #endregion

                    break;
                case SystemCallNumbers.CreateSemaphore:

                    #region Create Semaphore

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

                    #endregion

                    break;
                case SystemCallNumbers.ReleaseSemaphore:

                    #region Release Semaphore

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

                    #endregion

                    break;
                case SystemCallNumbers.WaitSemaphore:

                    #region Wait Semaphore

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Wait semaphore");
#endif
                    // Param1: Id
                {
                    int waitResult = ProcessManager.Semaphore_Wait((int)Param1, CallerProcess, CallerThread);
                    if (waitResult == -1)
                    {
                        BasicConsole.WriteLine("Error! Wait Semaphore result == -1?!");
                    }
                    result = waitResult == -1
                        ? SystemCallResults.Fail
                        : (waitResult == 1 ? SystemCallResults.OK : SystemCallResults.OK_NoWake);
                }

                    #endregion

                    break;
                case SystemCallNumbers.SignalSemaphore:

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Signal semaphore");
#endif
                    // Param1: Id
                    {
                        bool signalResult = ProcessManager.Semaphore_Signal((int)Param1,
                            ProcessManager.GetProcessById(CallerProcess.Id));
                        result = signalResult ? SystemCallResults.OK : SystemCallResults.Fail;
                    }

                    break;
                case SystemCallNumbers.RegisterDevice:

                    #region Register Device

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Register device");
#endif

                {
                    ulong DeviceId;
                    result = DeviceManager.RegisterDevice((DeviceDescriptor*)Param1, out DeviceId, CallerProcess);
                    Return2 = (uint)DeviceId;
                    Return3 = (uint)(DeviceId >> 32);
                }

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Register device - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterDevice:

                    #region Deregister Device

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Deregister device");
#endif

                    result = DeviceManager.DeregisterDevice(Param1 | ((ulong)Param2 << 32), CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Deregister device - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.GetNumDevices:

                    #region Get Num Devices

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get num devices");
#endif

                {
                    int num;
                    result = DeviceManager.GetNumDevices(out num, CallerProcess);
                    Return2 = (uint)num;
                }

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get num devices - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.GetDeviceList:

                    #region Get Device List

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get device list");
#endif

                    result = DeviceManager.GetDeviceList((DeviceDescriptor*)Param1, (int)Param2, CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get device list - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.GetDeviceInfo:

                    #region Get Device Info

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get device info");
#endif

                    result = DeviceManager.GetDeviceInfo(Param1 | ((ulong)Param2 << 32), (DeviceDescriptor*)Param3,
                        CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Get device info - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.ClaimDevice:

                    #region Claim Device

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Claim device");
#endif

                    result = DeviceManager.ClaimDevice(Param1 | ((ulong)Param2 << 32), CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Claim device - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.ReleaseDevice:

                    #region Release Device

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Release device");
#endif

                    result = DeviceManager.ReleaseDevice(Param1 | ((ulong)Param2 << 32), CallerProcess);

#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Release device - end");
#endif

                    #endregion

                    break;
                case SystemCallNumbers.InitFS:

                    #region Init File System

#if DFSC_TRACE
                    BasicConsole.WriteLine("DFSC: Init FS");
#endif
                    ProcessManager.Semaphore_Signal(FileSystemManagerAvailable_SemaphoreId, ProcessManager.KernelProcess);

                    #endregion

                    break;
                case SystemCallNumbers.StatFS:

                    #region Stat File System(s)

#if DFSC_TRACE
                    BasicConsole.WriteLine("DFSC: Stat FS");
#endif
                    // Param 1 : Count
                    // Param 2 : char[][] (Mappings)
                    // Param 3 : uint[]   (Processes)

                    int Count = (int)Param1;
                    if (Count == 0)
                    {
                        for (int i = 0; i < FileSystemAccessors.Count; i++)
                        {
                            FileSystemAccessor fsmi = (FileSystemAccessor)FileSystemAccessors[i];
                            if (fsmi.MappingPrefixes != null)
                            {
                                Count += fsmi.MappingPrefixes.Length;
                            }
                        }
                        Return2 = (uint)Count;
                    }
                    else
                    {
                        char* MappingsPtr = (char*)Param2;
                        uint* ProcessesPtr = (uint*)Param3;

                        ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

                        int ActualCount = 0;
                        for (int i = 0; i < FileSystemAccessors.Count && ActualCount < Count; i++)
                        {
                            FileSystemAccessor fsmi = (FileSystemAccessor)FileSystemAccessors[i];
                            if (fsmi.MappingPrefixes != null)
                            {
                                for (int j = 0;
                                    j < fsmi.MappingPrefixes.Length && ActualCount < Count;
                                    j++, ActualCount++)
                                {
                                    char* MappingResultPtr = MappingsPtr + ActualCount*10;
                                    String mapping = fsmi.MappingPrefixes[j];
                                    for (int k = 0; k < 10; k++)
                                    {
                                        MappingResultPtr[k] = k < mapping.Length ? mapping[k] : '\0';
                                    }

                                    ProcessesPtr[ActualCount] = fsmi.RemoteProcessId;
                                }
                            }
                        }

                        Return2 = (uint)ActualCount;

                        ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);
                    }

                    result = SystemCallResults.OK;

                    #endregion

                    break;

                case SystemCallNumbers.RegisterTimerEvent:

                    #region Register Timer Event

#if DFSC_TRACE
                    BasicConsole.WriteLine("DFSC: Register Timer Event");
#endif
                    Return2 = (uint)PIT.ThePIT.RegisterHandler((TimerHandler)ObjectUtilities.GetObject((void*)Param1), ((long)(Param2 & 0x7FFFFFFF) << 32) | Param3, (Param2 & 0x80000000) == 0x80000000, null, CallerProcess.Id);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterTimerEvent:

                    #region Deregister Timer Event

#if DFSC_TRACE
                    BasicConsole.WriteLine("DFSC: Deregister Timer Event");
#endif
                    PIT.ThePIT.UnregisterHandler((int)Param2);
                    result = SystemCallResults.OK;

                    #endregion

                    break;

                default:
//#if DSC_TRACE
                    BasicConsole.WriteLine("DSC: Unrecognised call number.");
                    BasicConsole.WriteLine((uint)syscallNumber);
//#endif
                    break;
            }

            return result;
        }

        public static void EndDeferredSystemCall(Process CallerProcess, Thread CallerThread, SystemCallResults result,
            uint Return2, uint Return3, uint Return4)
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

        private static void InitFilePipes()
        {
            int numOutpoints;
            SystemCallResults SysCallResult;
            BasicOutpoint.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, PipeClasses.File,
                PipeSubclasses.File_Data_Out);

            if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
            {
                PipeOutpointDescriptor[] OutpointDescriptors;
                BasicOutpoint.GetOutpointDescriptors(numOutpoints, out SysCallResult, out OutpointDescriptors,
                    PipeClasses.File, PipeSubclasses.File_Data_Out);

                if (SysCallResult == SystemCallResults.OK)
                {
                    for (int i = 0; i < OutpointDescriptors.Length; i++)
                    {
                        PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
                        bool PipeExists = false;

                        for (int j = 0; j < FileSystemAccessors.Count; j++)
                        {
                            FileSystemAccessor ExistingAccessor = (FileSystemAccessor)FileSystemAccessors[j];
                            if (ExistingAccessor.RemoteProcessId == Descriptor.ProcessId)
                            {
                                PipeExists = true;
                                break;
                            }
                        }

                        if (!PipeExists)
                        {
                            try
                            {
                                if (SystemCalls.WaitSemaphore(File_ConnectSemaphoreId) == SystemCallResults.OK)
                                {
                                    FileSystemAccessors.Add(new FileSystemAccessor(Descriptor.ProcessId));

                                    SystemCalls.SignalSemaphore(File_ConnectSemaphoreId);
                                }
                            }
                            catch
                            {
                                BasicConsole.WriteLine("KT File Management > Error creating new pipe!");
                                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                            }
                        }
                    }
                }
                else
                {
                    BasicConsole.WriteLine("KT File Management > Couldn't get outpoint descriptors!");
                }
            }
            else
            {
                BasicConsole.WriteLine("KT File Management > Cannot get outpoints!");
            }
        }

        private static void InitFileSystemManagers()
        {
            BasicConsole.WriteLine("KT File Management > Initialising file system managers...");
            for (int i = 0; i < FileSystemAccessors.Count; i++)
            {
                BasicConsole.WriteLine("KT File Management > Initialising file system manager...");

                FileSystemAccessor accessor = (FileSystemAccessor)FileSystemAccessors[i];
                accessor.StatFS();
            }
        }

        public static void FilePipeInitialisation_Main()
        {
            FileSystemAccessors = new List();

            if (SystemCalls.CreateSemaphore(1, out File_ConnectSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Kernel Task > File Access > Failed to create a semaphore! (1)");
            }

            if (SystemCalls.CreateSemaphore(-1, out FilePipeAvailable_SemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Kernel Task > File Access > Failed to create a semaphore! (2)");
            }

            if (SystemCalls.CreateSemaphore(-1, out FileSystemManagerAvailable_SemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Kernel Task > File Access > Failed to create a semaphore! (3)");
            }

            BasicConsole.WriteLine(" > Starting file system helper threads...");
            FileSystemManagerInitialisationThread =
                ProcessManager.CurrentProcess.CreateThread(FileSystemManagerInitialisation_Main,
                    "File Management : File System Initialisation", null);

            FilePipesReady = true;

            while (!Terminating)
            {
                SystemCalls.WaitSemaphore(FilePipeAvailable_SemaphoreId);

                SystemCalls.SleepThread(50);

                try
                {
                    InitFilePipes();
                }
                catch
                {
                    BasicConsole.WriteLine("Error initialising file pipes!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        public static void FileSystemManagerInitialisation_Main()
        {
            while (!Terminating)
            {
                SystemCalls.WaitSemaphore(FileSystemManagerAvailable_SemaphoreId);

                try
                {
                    InitFileSystemManagers();
                }
                catch
                {
                    BasicConsole.WriteLine("Error initialising file system managers!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        public static int HandleISR(uint ISRNum)
        {
            if (ISRNum == 48)
            {
                SystemCallHandlers.InterruptHandler();
                return 0;
            }
            return -1;
        }

        public static int HandleIRQ(uint IRQNum)
        {
            if (IRQNum == 0)
            {
                //if (Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 1");
                //}

                PIT.ThePIT.InterruptHandler();
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
                if (DeferredSyscallsInfo_Unqueued.Count == 0)
                {
                    BasicConsole.WriteLine("Error! Deferring system call is going to fail because there are no deferred syscall info objects left unqueued.");
                }

                //BasicConsole.WriteLine("Deferring syscall...");
                //BasicConsole.WriteLine("Popping unqueued info object...");
                DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Unqueued.Pop();
                //BasicConsole.WriteLine("Setting info...");
                info.ProcessId = callerProcessId;
                info.ThreadId = callerThreadId;

                //BasicConsole.WriteLine("Queuing info object...");
                DeferredSyscallsInfo_Queued.Push(info);

                //BasicConsole.WriteLine("Signaling deferred syscalls thread...");
                ProcessManager.Semaphore_Signal(DeferredSyscalls_QueuedSemaphoreId, ProcessManager.KernelProcess);
                //BasicConsole.WriteLine("Signaled.");
            }

            return (int)result;
        }

        /// <summary>
        ///     Special handler method for system calls recognised/handlded by the kernel task.
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
        ///     Executes within the interrupt handler. Usual restrictions apply.
        /// </remarks>
        public static SystemCallResults HandleSystemCall(uint syscallNumber,
            uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.RegisterISRHandler:

                    #region Register ISR Handler

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

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterISRHandler:

                    #region Deregister ISR Handler

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister ISR Handler");
#endif
                    SysCall_DeregisterISRHandler((int)param1, callerProcessId);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.RegisterIRQHandler:

                    #region Register IRQ Handler

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

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterIRQHandler:

                    #region Deregister IRQ Handler

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister IRQ Handler");
#endif
                    SysCall_DeregisterIRQHandler((int)param1, callerProcessId);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.RegisterSyscallHandler:

                    #region Register Syscall Handler

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

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterSyscallHandler:

                    #region Deregister Syscall Handler

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister Syscall Handler");
#endif
                    SysCall_DeregisterSyscallHandler((int)param1, callerProcessId);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.StartProcess:

                    #region Start Process

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Start Process");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.GetNumProcesses:

                    #region Get Num Processes

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Num Processes");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.GetProcessList:

                    #region Get Process List

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Process List");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.StartThread:

                    #region Start Thread

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Start Thread");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.SleepThread:

                    #region Sleep Thread

#if SYSCALLS_TRACE
    //BasicConsole.WriteLine("System call : Sleep Thread");
#endif
                    SysCall_Sleep((int)param1, callerProcessId, callerThreadId);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.WakeThread:

                    #region Wake Thread

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wake Thread");
#endif
                    SysCall_Wake(callerProcessId, param1);
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:

                    #region Register Pipe Outpoint

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Pipe Outpoint");
#endif
                    result = SystemCallResults.Deferred_PermitActions;

                    #endregion

                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:

                    #region Get Num Pipe Outpoints

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Num Pipe Outpoints");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.GetPipeOutpoints:

                    #region Get Pipe Outpoints

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Pipe Outpoints");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.CreatePipe:

                    #region Create Pipe

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Create Pipe");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.WaitOnPipeCreate:

                    #region Wait On Pipe Create

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wait On Pipe Create");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.ReadPipe:

                    #region Read Pipe

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Read Pipe");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.WritePipe:

                    #region Write Pipe

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Write Pipe");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.AbortPipeReadWrite:

                    #region Abort Pipe Read/Write

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Abort Pipe Read/Write");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.SendMessage:

                    #region Send Message

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Send message");
#endif
                    result = SysCall_SendMessage(callerProcessId, callerThreadId, param1, param2, param3)
                        ? SystemCallResults.OK
                        : SystemCallResults.Fail;

                    #endregion

                    break;
                case SystemCallNumbers.ReceiveMessage:

                    #region Receive Message

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Receive message");
#endif
                    ReceiveMessage(callerProcessId, param1, param2);

                    #endregion

                    break;
                case SystemCallNumbers.RequestPages:

                    #region Request Pages

#if SYSCALLS_TRACE
                    BasicConsole.Write("System call : Request pages for ");
                    BasicConsole.WriteLine(ProcessManager.GetProcessById(callerProcessId).Name);
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.IsPhysicalAddressMapped:

                    #region Is Physical Address Mapped

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Is physical address mapped");
#endif

                    // TODO: This is a bit hacky
                    // If address is in low 1MiB then it is mapped
                    if (param1 < 0x100000)
                    {
                        Return2 = 1u;
                    }
                    else
                    {
                        Return2 =
                            ProcessManager.GetProcessById(callerProcessId)
                                .TheMemoryLayout.ContainsAnyPhysicalAddresses(param1, 1)
                                ? 1u
                                : 0u;
                    }
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.IsVirtualAddressMapped:

                    #region Is Virtual Address Mapped

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Is virtual address mapped");
#endif
                    // TODO: This is a bit hacky
                    // If address is in low 1MiB then it is mapped
                    if (param1 < 0x100000)
                    {
                        Return2 = 1u;
                    }
                    else
                    {
                        Return2 =
                            ProcessManager.GetProcessById(callerProcessId)
                                .TheMemoryLayout.ContainsAnyVirtualAddresses(param1, 1)
                                ? 1u
                                : 0u;
                    }
                    result = SystemCallResults.OK;

                    #endregion

                    break;
                case SystemCallNumbers.GetPhysicalAddress:

                    #region Get Physical Address

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get physical address");
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
                        Return2 =
                            ProcessManager.GetProcessById(callerProcessId)
                                .TheMemoryLayout.GetPhysicalAddress(param1 & 0xFFFFF000);
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

                    #endregion

                    break;
                case SystemCallNumbers.GetVirtualAddress:

                    #region Get Virtual Address

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get virtual address");
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
                        Return2 =
                            ProcessManager.GetProcessById(callerProcessId)
                                .TheMemoryLayout.GetVirtualAddress(param1 & 0xFFFFF000);
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

                    #endregion

                    break;
                case SystemCallNumbers.UnmapPages:

                    #region Unmap Pages

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Unmap pages");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.SharePages:

                    #region Share Pages

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Share pages");
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
                        if (!TargetProcess.TheMemoryLayout.ContainsAnyVirtualAddresses(param1, (int)param2*4096))
                        {
                            if (TargetProcess.SyscallsToHandle.IsSet((int)SystemCallNumbers.AcceptPages) &&
                                TargetProcess.SyscallHandler != null)
                            {
                                uint XReturn2 = 0;
                                uint XReturn3 = 0;
                                uint XReturn4 = 0;
                                ProcessManager.SwitchProcess(param3, -1);
                                SystemCallResults AcceptPagesResult =
                                    (SystemCallResults)
                                        TargetProcess.SyscallHandler((uint)SystemCallNumbers.AcceptPages, param1,
                                            param2, 0, ref XReturn2, ref XReturn3, ref XReturn4, CallerProcess.Id,
                                            0xFFFFFFFF);
                                ProcessManager.SwitchProcess(ProcessManager.KernelProcess.Id,
                                    (int)DeferredSyscallsThread.Id);
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

                    #endregion

                    break;
                case SystemCallNumbers.CreateSemaphore:

                    #region Create Semaphore

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Create semaphore");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.ShareSemaphore:

                    #region Share Semaphore

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Share semaphore");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.ReleaseSemaphore:

                    #region Release Semaphore

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Release semaphore");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.WaitSemaphore:

                    #region Wait Semaphore

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wait semaphore");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.SignalSemaphore:

                    #region Signal Semaphore

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Signal semaphore");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.RegisterDevice:

                    #region Register Device

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register device");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterDevice:

                    #region Deregister Device

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister device");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.GetNumDevices:

                    #region Get Num Devices

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get num devices");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.GetDeviceList:

                    #region Get Device List

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get device list");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.GetDeviceInfo:

                    #region Get Device Info

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get device info");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.ClaimDevice:

                    #region Claim Device

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Claim device");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.ReleaseDevice:

                    #region Release Device

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Release device");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;

                case SystemCallNumbers.InitFS:

                    #region Init File System

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : File System Initialise (InitFS)");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.StatFS:

                    #region Stat File System(s)

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : File System Status (StatFS)");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;

                case SystemCallNumbers.RegisterTimerEvent:

                    #region Register Timer Event

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Timer Event");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

                    break;
                case SystemCallNumbers.DeregisterTimerEvent:

                    #region Deregister Timer Event

#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister Timer Event");
#endif
                    result = SystemCallResults.Deferred;

                    #endregion

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
        ///     Performs Sleep system call processing for the Kernel Task.
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
        ///     Performs Wake system call processing for the Kernel Task.
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
        ///     Performs Register ISR Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ISRNum">The ISR number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterISRHandler(int ISRNum, uint handlerAddr, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering ISR handler...");
#endif

            Process theProcess;

            if (ISRNum == -1)
            {
                if (handlerAddr != 0xFFFFFFFF)
                {
                    theProcess = ProcessManager.GetProcessById(callerProcessId);

                    theProcess.ISRHandler = (ISRHanderDelegate)ObjectUtilities.GetObject((void*)handlerAddr);
                    return true;
                }
                return false;
            }

            if (ISRNum < 49)
            {
                return false;
            }

            theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.ISRHandler = (ISRHanderDelegate)ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.ISRsToHandle.Set(ISRNum);

            return true;
        }

        /// <summary>
        ///     Performs Deregister ISR Handler system call processing for the Kernel Task.
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
        ///     Performs Register IRQ Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="IRQNum">The IRQ number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterIRQHandler(int IRQNum, uint handlerAddr, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering IRQ handler...");
#endif

            Process theProcess;

            if (IRQNum == -1)
            {
                if (handlerAddr != 0xFFFFFFFF)
                {
                    theProcess = ProcessManager.GetProcessById(callerProcessId);

                    theProcess.IRQHandler = (IRQHanderDelegate)ObjectUtilities.GetObject((void*)handlerAddr);
                    return true;
                }
                return false;
            }

            if (IRQNum > 15)
            {
                return false;
            }

            theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.IRQHandler = (IRQHanderDelegate)ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.IRQsToHandle.Set(IRQNum);

            x86Interrupts.EnableIRQ((byte)IRQNum);

            return true;
        }

        /// <summary>
        ///     Performs Deregister IRQ Handler system call processing for the Kernel Task.
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
        ///     Performs Register System Call Handler system call processing for the Kernel Task.
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
            Process theProcess;

            if (syscallNum == -1)
            {
                if (handlerAddr != 0xFFFFFFFF)
                {
                    theProcess = ProcessManager.GetProcessById(callerProcessId);

                    theProcess.SyscallHandler = (SyscallHanderDelegate)ObjectUtilities.GetObject((void*)handlerAddr);
                    return true;
                }
                return false;
            }

            theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.SyscallHandler = (SyscallHanderDelegate)ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.SyscallsToHandle.Set(syscallNum);

            return true;
        }

        /// <summary>
        ///     Performs Deregister System Call Handler system call processing for the Kernel Task.
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
        ///     Performs Send Message system call processing for the Kernel Task.
        /// </summary>
        /// <param name="callerProcessId">The Id of the process sending the message.</param>
        /// <param name="callerThreadId">The Id of the thread sending the message.</param>
        /// <param name="targetProcessId">The Id of the process to send the message to.</param>
        /// <param name="message1">The first value of the message.</param>
        /// <param name="message2">The second value of the message.</param>
        /// <returns>True if the message was accepted.</returns>
        private static bool SysCall_SendMessage(uint callerProcessId, uint callerThreadId, uint targetProcessId,
            uint message1, uint message2)
        {
            bool Result = false;

            Process CallerProcess = ProcessManager.GetProcessById(callerProcessId);
            Process TargetProcess = ProcessManager.GetProcessById(targetProcessId);

            if (TargetProcess.SyscallsToHandle.IsSet((int)SystemCallNumbers.ReceiveMessage) &&
                TargetProcess.SyscallHandler != null)
            {
                uint Return2 = 0;
                uint Return3 = 0;
                uint Return4 = 0;
                ProcessManager.SwitchProcess(targetProcessId, -1);
                TargetProcess.SyscallHandler((uint)SystemCallNumbers.ReceiveMessage, message1, message2, 0, ref Return2,
                    ref Return3, ref Return4, callerProcessId, 0xFFFFFFFF);
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

        public static void ReceiveKey(uint Scancode)
        {
            if (keyboard != null)
            {
                keyboard.HandleScancode(Scancode);
            }
        }

        private class DeferredSyscallInfo : Object
        {
            public uint ProcessId;
            public uint ThreadId;
        }
        


        private delegate void TestThreadStartPoint(uint value, uint cs);

        private static void Test(uint value, uint cs)
        {
            int count = 10;
            if (value == 0xDEADBEEF)
            {
                BasicConsole.WriteLine("Thread Start Arguments Test Passed : " + (String)value);
            }
            else
            {
                BasicConsole.WriteLine("Thread Start Arguments Test Failed : " + (String)value);
            }
        }
    }
}