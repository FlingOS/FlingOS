using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            SystemCallResults SysCallResult = SystemCallMethods.StartThread(GCCleanupTask.Main);
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Device Manager: GC thread failed to create!");
            }

            SysCallResult = SystemCallMethods.RegisterPipeOutpoint(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, 1);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Unexpected system call result!");
                    break;
            }

            int ThePipeId;
            SysCallResult = SystemCallMethods.WaitOnPipeCreate(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, out ThePipeId);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("DM > WaitOnPipeCreate: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("DM > WaitOnPipeCreate: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("DM > WaitOnPipeCreate: Succeeded.");
                    BasicConsole.Write("DM > New pipe id: ");
                    BasicConsole.WriteLine(ThePipeId);
                    break;
                default:
                    BasicConsole.WriteLine("DM > WaitOnPipeCreate: Unexpected system call result!");
                    break;
            }
            
            Pipes.WritePipeRequest* WritePipeRequestPtr = (Pipes.WritePipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.WritePipeRequest), "Window Manager : Alloc WritePipeRequest");
            try
            {
                bool CanWritePipe = WritePipeRequestPtr != null;
                if (CanWritePipe)
                {
                    WritePipeRequestPtr->offset = 0;
                    WritePipeRequestPtr->PipeId = ThePipeId;
                }

                uint loops = 0;
                while (!Terminating)
                {
                    byte[] messageBytes = ByteConverter.GetASCIIBytes("Hello, world! (" + (FOS_System.String)loops++ + ")");
                    WritePipeRequestPtr->length = messageBytes.Length;
                    WritePipeRequestPtr->inBuffer = (byte*)Utilities.ObjectUtilities.GetHandle(messageBytes) + FOS_System.Array.FieldsBytesSize;
                    SysCallResult = SystemCallMethods.WritePipe(WritePipeRequestPtr);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            BasicConsole.WriteLine("DM > WritePipe: Unhandled!");
                            break;
                        case SystemCallResults.Fail:
                            BasicConsole.WriteLine("DM > WritePipe: Failed!");
                            break;
                        case SystemCallResults.OK:
                            BasicConsole.WriteLine("DM > WritePipe: Succeeded.");
                            break;
                        default:
                            BasicConsole.WriteLine("DM > WritePipe: Unexpected system call result!");
                            break;
                    }

                    SystemCallMethods.SleepThread(1000);
                }
            }
            finally
            {

            }
        }
    }

    class testclass : FOS_System.Object
    {
    }
}
