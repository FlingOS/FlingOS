using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private static Console ScreenOutput;
        private static int Pings = 0;
        private static int TestThread_Loops = 0;
        private static int TestThread2_Loops = 0;
        private static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Window Manager: Started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            if (SystemCallMethods.StartThread(GCCleanupTask.Main) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: GC thread failed to create!");
            }

            int numOutpoints;
            SystemCallResults SysCallResult = SystemCallMethods.GetNumPipeOutpoints(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, out numOutpoints);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("WM > GetNumPipeOutpoints: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("WM > GetNumPipeOutpoints: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("WM > GetNumPipeOutpoints: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("WM > GetNumPipeOutpoints: Unexpected system call result!");
                    break;
            }
            BasicConsole.Write("WM > Num pipe outpoints of class: Display, subclass: Display_Text_ASCII = ");
            BasicConsole.WriteLine(numOutpoints);

            uint WantedOutpointProcessId = 0;
            if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
            {
                Pipes.PipeOutpointsRequest* RequestPtr = (Pipes.PipeOutpointsRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.PipeOutpointsRequest), "Device Manager : Alloc PipeOutpointsRequest");
                if (RequestPtr != null)
                {
                    try
                    {
                        RequestPtr->MaxDescriptors = numOutpoints;
                        RequestPtr->Outpoints = (Pipes.PipeOutpointDescriptor*)Heap.AllocZeroed((uint)numOutpoints * (uint)sizeof(Pipes.PipeOutpointDescriptor), "Device Manager : Alloc PipeOutpointDescriptor(s)");
                        if (RequestPtr->Outpoints != null)
                        {
                            try
                            {
                                SysCallResult = SystemCallMethods.GetPipeOutpoints(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, RequestPtr);
                                switch (SysCallResult)
                                {
                                    case SystemCallResults.Unhandled:
                                        BasicConsole.WriteLine("WM > GetPipeOutpoints: Unhandled!");
                                        break;
                                    case SystemCallResults.Fail:
                                        BasicConsole.WriteLine("WM > GetPipeOutpoints: Failed!");
                                        break;
                                    case SystemCallResults.OK:
                                        BasicConsole.WriteLine("WM > GetPipeOutpoints: Succeeded.");
                                        WantedOutpointProcessId = RequestPtr->Outpoints[0].ProcessId;

                                        BasicConsole.Write("WM > Outpoint[0].ProcessId: ");
                                        BasicConsole.WriteLine(WantedOutpointProcessId);
                                        break;
                                    default:
                                        BasicConsole.WriteLine("WM > GetPipeOutpoints: Unexpected system call result!");
                                        break;
                                }
                            }
                            finally
                            {
                                Heap.Free(RequestPtr->Outpoints);
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("WM > RequestPtr->Outpoints null! No memory allocated.");
                        }
                    }
                    finally
                    {
                        Heap.Free(RequestPtr);
                    }
                }
                else
                {
                    BasicConsole.WriteLine("WM > RequestPtr null! No memory allocated.");
                }
            }
            else
            {
                BasicConsole.WriteLine("WM > Cannot get outpoints!");
            }

            int CreatedPipeId = 0;
            if (SysCallResult == SystemCallResults.OK)
            {
                Pipes.CreatePipeRequest* RequestPtr = (Pipes.CreatePipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.CreatePipeRequest), "Device Manager : Alloc CreatePipeRequest");
                if (RequestPtr != null)
                {
                    try
                    {
                        RequestPtr->BufferSize = 4096;
                        RequestPtr->Class = Pipes.PipeClasses.Display;
                        RequestPtr->Subclass = Pipes.PipeSubclasses.Display_Text_ASCII;

                        SysCallResult = SystemCallMethods.CreatePipe(WantedOutpointProcessId, RequestPtr);
                        switch (SysCallResult)
                        {
                            case SystemCallResults.Unhandled:
                                BasicConsole.WriteLine("WM > CreatePipe: Unhandled!");
                                break;
                            case SystemCallResults.Fail:
                                BasicConsole.WriteLine("WM > CreatePipe: Failed!");
                                break;
                            case SystemCallResults.OK:
                                BasicConsole.WriteLine("WM > CreatePipe: Succeeded.");
                                CreatedPipeId = RequestPtr->Result.Id;

                                BasicConsole.Write("WM > New pipe id: ");
                                BasicConsole.WriteLine(CreatedPipeId);
                                break;
                            default:
                                BasicConsole.WriteLine("WM > CreatePipe: Unexpected system call result!");
                                break;
                        }
                    }
                    finally
                    {
                        Heap.Free(RequestPtr);
                    }
                }
                else
                {
                    BasicConsole.WriteLine("WM > RequestPtr null! No memory allocated.");
                }
            }
            else
            {
                BasicConsole.WriteLine("WM > Cannot create pipe!");
            }

            int loops = 0;
            ScreenOutput = new Consoles.AdvancedConsole();

            //SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.Semaphore, SyscallHandler);

            if (SystemCallMethods.StartThread(TestThread) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Test thread failed to create!");
            }
            if (SystemCallMethods.StartThread(TestThread2) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Test thread 2 failed to create!");
            }

            Pipes.ReadPipeRequest* ReadPipeRequestPtr = (Pipes.ReadPipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.ReadPipeRequest), "Window Manager : Alloc ReadPipeRequest");
            try
            {
                bool CanReadPipe = ReadPipeRequestPtr != null;
                if (CanReadPipe)
                {
                    ReadPipeRequestPtr->length = 256;
                    ReadPipeRequestPtr->offset = 0;
                    ReadPipeRequestPtr->PipeId = CreatedPipeId;
                    ReadPipeRequestPtr->outBuffer = (byte*)Heap.AllocZeroed((uint)ReadPipeRequestPtr->length, "Window Manager : Alloc read pipe buffer");
                    if (ReadPipeRequestPtr->outBuffer == null)
                    {
                        CanReadPipe = false;
                        Heap.Free(ReadPipeRequestPtr);
                    }
                }

                while (!Terminating)
                {
                    try
                    {
                        ScreenOutput.Clear();
                        ScreenOutput.Write("Window Manager Task (");
                        ScreenOutput.Write_AsDecimal(loops);
                        ScreenOutput.WriteLine(")");

                        ScreenOutput.Write("WM > Pings : ");
                        ScreenOutput.WriteLine_AsDecimal(Pings);

                        ScreenOutput.Write("WM > Test Thread loops : ");
                        ScreenOutput.WriteLine_AsDecimal(Pings);

                        ScreenOutput.Write("WM > Test Thread 2 loops : ");
                        ScreenOutput.WriteLine_AsDecimal(Pings);

                        ScreenOutput.WriteLine();

                        ScreenOutput.Write("WM > Heap: ");
                        uint totalMem = Heap.GetTotalMem();
                        ScreenOutput.Write_AsDecimal(Heap.GetTotalUsedMem() / (totalMem / 100));
                        ScreenOutput.Write("% / ");
                        ScreenOutput.Write_AsDecimal(totalMem / 1024);
                        ScreenOutput.WriteLine(" KiB");

                        ScreenOutput.WriteLine();

                        ScreenOutput.Write("WM > Number of objects: ");
                        ScreenOutput.WriteLine_AsDecimal(FOS_System.GC.NumObjs);
                        ScreenOutput.Write("WM > Number of strings: ");
                        ScreenOutput.WriteLine_AsDecimal(FOS_System.GC.NumStrings);

                        if (CanReadPipe)
                        {
                            int BytesRead;
                            SysCallResult = SystemCallMethods.ReadPipe(ReadPipeRequestPtr, out BytesRead);
                            switch (SysCallResult)
                            {
                                case SystemCallResults.Unhandled:
                                    ScreenOutput.WriteLine("WM > ReadPipe: Unhandled!");
                                    break;
                                case SystemCallResults.Fail:
                                    ScreenOutput.WriteLine("WM > ReadPipe: Failed!");
                                    break;
                                case SystemCallResults.OK:
                                    ScreenOutput.WriteLine("WM > ReadPipe: Succeeded.");
                                    ScreenOutput.Write("WM > Bytes read: ");
                                    ScreenOutput.WriteLine(BytesRead);

                                    ScreenOutput.Write("WM > Message: ");
                                    if (BytesRead > 0)
                                    {
                                        FOS_System.String message = ByteConverter.GetASCIIStringFromASCII(ReadPipeRequestPtr->outBuffer, 0, (uint)BytesRead);
                                        ScreenOutput.WriteLine(message);
                                    }
                                    else
                                    {
                                        ScreenOutput.WriteLine("[NO MESSAGE]");
                                    }
                                    break;
                                default:
                                    BasicConsole.WriteLine("WM > ReadPipe: Unexpected system call result!");
                                    break;
                            }
                        }

                        SystemCallMethods.SleepThread(500);

                        loops++;
                    }
                    catch
                    {
                        BasicConsole.WriteLine("Exception running window manager.");
                    }
                }
            }
            finally
            {
                Heap.Free(ReadPipeRequestPtr);
            }
        }

        public static void TestThread()
        {
            while (!Terminating)
            {
                try
                {
                    //BasicConsole.WriteLine("WM > Test Thread");
                    TestThread_Loops++;
                    SystemCallMethods.SleepThread(100);
                }
                catch
                {
                    BasicConsole.WriteLine("Error in test thread!");
                }
            }
        }

        public static void TestThread2()
        {
            while (!Terminating)
            {
                try
                {
                    //BasicConsole.WriteLine("WM > Test Thread 2");
                    TestThread2_Loops++;
                    SystemCallMethods.SleepThread(100);
                }
                catch
                {
                    BasicConsole.WriteLine("Error in test thread 2!");
                }
            }
        }
        
        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch((SystemCallNumbers)syscallNumber)
            {
                //case SystemCallNumbers.Semaphore:
                //    Pings++;
                //    result = SystemCallResults.OK;
                //    break;
            }

            return (int)result;
        }
    }
}
