//#define TASKHELPERS_TRACE

using Kernel.Framework;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Processes;
using Kernel.Utilities;
using Kernel.VirtualMemory;

namespace Kernel.Tasks
{
    public static class Helpers
    {
        public static void ProcessInit(String ProcessName, out uint GCThreadId)
        {
            BasicConsole.WriteLine(ProcessName + " started.");

            BasicConsole.WriteLine(ProcessName + " > Creating heap...");
            Heap.InitForProcess();

            BasicConsole.WriteLine(ProcessName + " > Starting GC thread...");
            SystemCallResults SysCallResult = SystemCalls.StartThread(GCCleanupTask.Main, out GCThreadId);
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine(ProcessName + ": GC thread failed to create!");
            }

            Heap.name = ProcessName;
        }

        public static unsafe bool StartBuiltInProcess(String CallerName, String NewProcName, ThreadStartPoint MainMethod,
            bool UserMode)
        {
            bool result = false;

            StartProcessRequest* StartRequest =
                (StartProcessRequest*)Heap.AllocZeroed((uint)sizeof(StartProcessRequest), "DeviceShell.Execute");
            try
            {
                StartRequest->Name = NewProcName.GetCharPointer();
                StartRequest->NameLength = NewProcName.Length;
                StartRequest->CodePagesCount = 0;
                //TODO: Direct access to VirtualMemoryManager from within the DeviceInfoTask is wrong...
                uint[] DataPages = VirtualMemoryManager.GetBuiltInProcessVAddrs();
                StartRequest->DataPages = (uint*)((byte*)ObjectUtilities.GetHandle(DataPages) + Array.FieldsBytesSize);
                StartRequest->DataPagesCount = (uint)DataPages.Length;
                StartRequest->MainMethod = ObjectUtilities.GetHandle(MainMethod);

                uint ATADriverProcessId;
                uint ATADriverThreadId;
                SystemCallResults SysCallResult = SystemCalls.StartProcess(StartRequest, UserMode,
                    out ATADriverProcessId, out ATADriverThreadId);
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
                BasicConsole.WriteLine("Process Id is " + Framework.Stubs.UInt32.ToDecimalString(ATADriverProcessId));
                BasicConsole.WriteLine("Thread Id is " + Framework.Stubs.UInt32.ToDecimalString(ATADriverThreadId));
            }
#endif

            return result;
        }
    }
}