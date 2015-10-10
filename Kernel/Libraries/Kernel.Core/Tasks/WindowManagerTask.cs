using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private class PipeInfo : FOS_System.Object
        {
            public int Id;
            public uint OutProcessId;
            public Console ScreenOutput = new Consoles.AdvancedConsole();
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
        private static uint ProcessingThreadId;

        private static bool ProcessingThreadAwake = false;

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

            // Start thread for handling background processing
            if (SystemCallMethods.StartThread(Processing, out ProcessingThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Processing thread failed to create!");
            }

            BasicConsole.Write("WM > Processing thread id: ");
            BasicConsole.WriteLine(ProcessingThreadId);

            SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.RegisterPipeOutpoint, SyscallHandler);

            Pipes.ReadPipeRequest* ReadPipeRequestPtr = (Pipes.ReadPipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.ReadPipeRequest), "Window Manager : Alloc ReadPipeRequest");
            try
            {
                bool CanReadPipe = ReadPipeRequestPtr != null;
                if (CanReadPipe)
                {
                    ReadPipeRequestPtr->length = 2000;
                    ReadPipeRequestPtr->outBuffer = (byte*)Heap.AllocZeroed((uint)ReadPipeRequestPtr->length, "Window Manager : Alloc read pipe buffer");
                    if (ReadPipeRequestPtr->outBuffer == null)
                    {
                        CanReadPipe = false;
                        Heap.Free(ReadPipeRequestPtr);
                    }
                }

                // Wait for pipe to be created
                SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);

                while (!Terminating)
                {
                    try
                    {
                        if (CanReadPipe && CurrentPipeIdx > -1)
                        {
                            PipeInfo CurrentPipeInfo = ((PipeInfo)ConnectedPipes[CurrentPipeIdx]);

                            ReadPipeRequestPtr->offset = 0;
                            ReadPipeRequestPtr->PipeId = CurrentPipeInfo.Id;

                            int BytesRead;
                            SystemCallResults SysCallResult = SystemCallMethods.ReadPipe(ReadPipeRequestPtr, out BytesRead);
                            switch (SysCallResult)
                            {
                                case SystemCallResults.Unhandled:
                                    CurrentPipeInfo.ScreenOutput.WriteLine("WM > ReadPipe: Unhandled!");
                                    break;
                                case SystemCallResults.Fail:
                                    CurrentPipeInfo.ScreenOutput.WriteLine("WM > ReadPipe: Failed!");
                                    break;
                                case SystemCallResults.OK:
                                    //ScreenOutput.WriteLine("WM > ReadPipe: Succeeded.");
                                    //ScreenOutput.Write("WM > Bytes read: ");
                                    //ScreenOutput.WriteLine(BytesRead);

                                    if (BytesRead > 0)
                                    {
                                        FOS_System.String message = ByteConverter.GetASCIIStringFromASCII(ReadPipeRequestPtr->outBuffer, 0, (uint)BytesRead);
                                        CurrentPipeInfo.ScreenOutput.Write(message);
                                    }
                                    break;
                                default:
                                    CurrentPipeInfo.ScreenOutput.WriteLine("WM > ReadPipe: Unexpected system call result!");
                                    break;
                            }

                            CurrentPipeIdx++;
                            if (CurrentPipeIdx >= ConnectedPipes.Count)
                            {
                                CurrentPipeIdx = 0;
                            }
                        }
                    }
                    catch
                    {
                        BasicConsole.WriteLine("WM > Exception running window manager.");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }
                }
            }
            finally
            {
                Heap.Free(ReadPipeRequestPtr);
            }
        }

        public static void Processing()
        {
            while (!Terminating)
            {
                if (!ProcessingThreadAwake)
                {
                    SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);
                }
                ProcessingThreadAwake = false;

                BasicConsole.WriteLine("WM > Processing thread runnning...");

                int numOutpoints;
                SystemCallResults SysCallResult;
                GetNumPipeOutpoints(out numOutpoints, out SysCallResult);

                if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                {
                    Pipes.PipeOutpointDescriptor[] OutpointDescriptors = null;
                    GetOutpointDescriptors(numOutpoints, ref SysCallResult, ref OutpointDescriptors);

                    if (SysCallResult == SystemCallResults.OK)
                    {
                        Pipes.CreatePipeRequest* RequestPtr = (Pipes.CreatePipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.CreatePipeRequest), "Device Manager : Alloc CreatePipeRequest");
                        if (RequestPtr != null)
                        {
                            try
                            {
                                RequestPtr->BufferSize = 4000; // 2000 chars * 2 bytes per char
                                RequestPtr->Class = Pipes.PipeClasses.Standard;
                                RequestPtr->Subclass = Pipes.PipeSubclasses.Standard_Out;

                                for (int i = 0; i < OutpointDescriptors.Length; i++)
                                {
                                    try
                                    {
                                        Pipes.PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
                                        bool PipeExists = false;

                                        for (int j = 0; j < ConnectedPipes.Count; j++)
                                        {
                                            PipeInfo ExistingPipeInfo = (PipeInfo)ConnectedPipes[j];
                                            if (ExistingPipeInfo.OutProcessId == Descriptor.ProcessId)
                                            {
                                                PipeExists = true;
                                                break;
                                            }
                                        }

                                        if (!PipeExists)
                                        {
                                            SysCallResult = SystemCallMethods.CreatePipe(Descriptor.ProcessId, RequestPtr);
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
                                                    int CreatedPipeId = RequestPtr->Result.Id;

                                                    BasicConsole.Write("WM > New pipe id: ");
                                                    BasicConsole.WriteLine(CreatedPipeId);

                                                    ConnectedPipes.Add(new PipeInfo()
                                                    {
                                                        Id = CreatedPipeId,
                                                        OutProcessId = Descriptor.ProcessId
                                                    });

                                                    if (CurrentPipeIdx == -1)
                                                    {
                                                        CurrentPipeIdx = 0;
                                                        SystemCallMethods.WakeThread(MainThreadId);
                                                    }
                                                    break;
                                                default:
                                                    BasicConsole.WriteLine("WM > CreatePipe: Unexpected system call result!");
                                                    break;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        BasicConsole.WriteLine("WM > Error occurred while trying to create pipe!");
                                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                                    }
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
                        BasicConsole.WriteLine("WM > Couldn't get outpoint descriptors!");
                    }
                }
                else
                {
                    BasicConsole.WriteLine("WM > Cannot get outpoints!");
                }
            }
        }
        private static void GetNumPipeOutpoints(out int numOutpoints, out SystemCallResults SysCallResult)
        {
            SysCallResult = SystemCallMethods.GetNumPipeOutpoints(Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out, out numOutpoints);
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

                    BasicConsole.Write("WM > Num pipe outpoints of class: Standard, subclass: Standard_Out = ");
                    BasicConsole.WriteLine(numOutpoints);
                    break;
                default:
                    BasicConsole.WriteLine("WM > GetNumPipeOutpoints: Unexpected system call result!");
                    break;
            }
        }
        private static void GetOutpointDescriptors(int numOutpoints, ref SystemCallResults SysCallResult, ref Pipes.PipeOutpointDescriptor[] OutpointDescriptors)
        {
            OutpointDescriptors = new Pipes.PipeOutpointDescriptor[numOutpoints];

            Pipes.PipeOutpointsRequest* RequestPtr = (Pipes.PipeOutpointsRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.PipeOutpointsRequest), "Device Manager : Alloc PipeOutpointsRequest");
            if (RequestPtr != null)
            {
                try
                {
                    RequestPtr->MaxDescriptors = numOutpoints;
                    RequestPtr->Outpoints = (Pipes.PipeOutpointDescriptor*)((byte*)Utilities.ObjectUtilities.GetHandle(OutpointDescriptors) + FOS_System.Array.FieldsBytesSize);
                    if (RequestPtr->Outpoints != null)
                    {
                        SysCallResult = SystemCallMethods.GetPipeOutpoints(Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out, RequestPtr);
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
                                break;
                            default:
                                BasicConsole.WriteLine("WM > GetPipeOutpoints: Unexpected system call result!");
                                break;
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
                        Return2 = ProcessingThreadId;
                        ProcessingThreadAwake = true;
                    }
                    break;
            }

            return (int)result;
        }
    }
}
