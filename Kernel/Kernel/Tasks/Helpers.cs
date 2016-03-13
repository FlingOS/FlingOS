//#define TASKHELPERS_TRACE

using Kernel.FOS_System;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Processes;
using Kernel.Hardware;

namespace Kernel.Tasks
{
    public static class Helpers
    {
        public static void ProcessInit(FOS_System.String ProcessName, out uint GCThreadId)
        {
            BasicConsole.WriteLine(ProcessName + " started.");

            BasicConsole.WriteLine(ProcessName + " > Creating heap...");
            FOS_System.Heap.InitForProcess();

            BasicConsole.WriteLine(ProcessName + " > Starting GC thread...");
            SystemCallResults SysCallResult = SystemCalls.StartThread(GCCleanupTask.Main, out GCThreadId);
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine(ProcessName + ": GC thread failed to create!");
            }

            FOS_System.Heap.name = ProcessName;
        }

        public static unsafe bool StartBuiltInProcess(String CallerName, String NewProcName, ThreadStartPoint MainMethod, bool UserMode)
        {
            bool result = false;

            StartProcessRequest* StartRequest = (StartProcessRequest*)Heap.AllocZeroed((uint)sizeof(StartProcessRequest), "DeviceShell.Execute");
            try
            {
                StartRequest->Name = NewProcName.GetCharPointer();
                StartRequest->NameLength = NewProcName.length;
                StartRequest->CodePagesCount = 0;
                uint[] DataPages = VirtMemManager.GetBuiltInProcessVAddrs();
                StartRequest->DataPages = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(DataPages) + FOS_System.Array.FieldsBytesSize);
                StartRequest->DataPagesCount = (uint)DataPages.Length;
                StartRequest->MainMethod = (void*)Utilities.ObjectUtilities.GetHandle(MainMethod);

                uint ATADriverProcessId;
                uint ATADriverThreadId;
                SystemCallResults SysCallResult = SystemCalls.StartProcess(StartRequest, UserMode, out ATADriverProcessId, out ATADriverThreadId);
                result = SysCallResult == SystemCallResults.OK;
            }
            finally
            {
                if (StartRequest != null)
                {
                    Heap.Free(StartRequest);
                }
            }

#if TASKHELPERS_TRACE
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine(CallerName + " > " + NewProcName + " failed to start!");
            }
            else
            {
                BasicConsole.WriteLine(CallerName + " > " + NewProcName + " started.");
                BasicConsole.WriteLine("Process Id is " + FOS_System.Stubs.UInt32.ToDecimalString(ATADriverProcessId));
                BasicConsole.WriteLine("Thread Id is " + FOS_System.Stubs.UInt32.ToDecimalString(ATADriverThreadId));
            }
#endif 

            return result;
        }
    }
}
