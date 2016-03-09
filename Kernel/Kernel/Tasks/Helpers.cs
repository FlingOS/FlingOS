//#define TASKHELPERS_TRACE

using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Processes;
using Kernel.Hardware;

namespace Kernel.Tasks
{
    public static class Helpers
    {
        public static unsafe bool StartBuiltInProcess(FOS_System.String CallerName, FOS_System.String NewProcName, ThreadStartPoint MainMethod)
        {
            bool result = false;

            StartProcessRequest* StartRequest = (StartProcessRequest*)FOS_System.Heap.AllocZeroed((uint)sizeof(StartProcessRequest), "DeviceShell.Execute");
            StartRequest->Name = NewProcName.GetCharPointer();
            StartRequest->NameLength = NewProcName.length;
            StartRequest->CodePagesCount = 0;
            uint[] DataPages = VirtMemManager.GetBuiltInProcessVAddrs();
            StartRequest->DataPages = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(DataPages) + FOS_System.Array.FieldsBytesSize);
            StartRequest->DataPagesCount = (uint)DataPages.Length;
            StartRequest->MainMethod = (void*)Utilities.ObjectUtilities.GetHandle(MainMethod);
            
            uint ATADriverProcessId;
            uint ATADriverThreadId;
            SystemCallResults SysCallResult = SystemCalls.StartProcess(StartRequest, false, out ATADriverProcessId, out ATADriverThreadId);
            result = SysCallResult == SystemCallResults.OK;

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
