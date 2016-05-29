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

//#define SYSCALLS_TRACE

using Kernel.Framework.Processes;

namespace Kernel.Multiprocessing
{
    /// <summary>
    ///     Contains callers and handlers for system calls.
    /// </summary>
    public static class SystemCallHandlers
    {
        /// <summary>
        ///     Main interrupt handler routine for system calls.
        /// </summary>
        /// <remarks>
        ///     Prevents direct invocation of the Receive Message system call, since that's not allowed.
        /// </remarks>
        public static void InterruptHandler()
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("----- Syscall -----");
            BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
#endif

            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Getting param values...");
#endif

            uint syscallNumber = currThread.SysCallNumber;
            uint param1 = currThread.Param1;
            uint param2 = currThread.Param2;
            uint param3 = currThread.Param3;

            SystemCallResults result = SystemCallResults.Unhandled;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;

            if (syscallNumber != (uint)SystemCallNumbers.ReceiveMessage &&
                syscallNumber != (uint)SystemCallNumbers.AcceptPages)
            {
                Process handlerProcess = null;

#if SYSCALLS_TRACE
                BasicConsole.WriteLine("Enumerating processes...");
#endif

                bool PermitActionResulted = false;
                for (int i = 0; i < ProcessManager.Processes.Count; i++)
                {
                    handlerProcess = (Process)ProcessManager.Processes[i];
                    if (handlerProcess.SyscallsToHandle.IsSet((int)syscallNumber))
                    {
                        ProcessManager.SwitchProcess(handlerProcess.Id, ProcessManager.THREAD_DONT_CARE);
                        switched = true;

#if SYSCALLS_TRACE
                        BasicConsole.WriteLine("Calling handler...");

                        //if (process == null)
                        //{
                        //    BasicConsole.WriteLine(" > process is null?!");
                        //}
                        //else if (process.SyscallHandler == null)
                        //{
                        //    BasicConsole.WriteLine(" > process.SysCallHandler is null?!");
                        //}
#endif

                        uint TempReturn2 = 0;
                        uint TempReturn3 = 0;
                        uint TempReturn4 = 0;
                        SystemCallResults tempResult = (SystemCallResults)handlerProcess.SyscallHandler(syscallNumber,
                            param1, param2, param3,
                            ref TempReturn2, ref TempReturn3, ref TempReturn4,
                            currProcess.Id, currThread.Id);

                        if (tempResult == SystemCallResults.RequestAction_WakeThread)
                        {
#if SYSCALLS_TRACE
                            BasicConsole.WriteLine("System calls : Performing action - wake thread");
#endif
                            ProcessManager.WakeThread(handlerProcess, TempReturn2);
                            tempResult = SystemCallResults.Unhandled;
                        }
                        else if (tempResult == SystemCallResults.RequestAction_SignalSemaphore)
                        {
#if SYSCALLS_TRACE
                            BasicConsole.WriteLine("System calls : Performing action - signal semaphore");
#endif
                            ProcessManager.Semaphore_Signal((int)TempReturn2, handlerProcess);
                            tempResult = SystemCallResults.Unhandled;
                        }

                        if (tempResult != SystemCallResults.Unhandled && !PermitActionResulted)
                        {
#if SYSCALLS_TRACE
                            BasicConsole.WriteLine("Result achieved.");
#endif
                            Return2 = TempReturn2;
                            Return3 = TempReturn3;
                            Return4 = TempReturn4;

                            if (tempResult == SystemCallResults.OK_PermitActions)
                            {
                                result = SystemCallResults.OK;
                                PermitActionResulted = true;
                            }
                            else if (tempResult == SystemCallResults.Deferred_PermitActions)
                            {
                                result = SystemCallResults.Deferred;
                                PermitActionResulted = true;
                            }
                            else
                            {
                                result = tempResult;
                                break;
                            }
                        }
                    }
                }

                if (switched)
                {
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Switching back...");
#endif

                    ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
                }
            }

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Setting result values...");
#endif

            if (result == SystemCallResults.Deferred)
            {
#if SYSCALLS_TRACE
                BasicConsole.WriteLine("Deferring thread (by indefinite sleep)...");
#endif
                currThread._EnterSleep(Thread.IndefiniteSleep);
            }
            else
            {
                currThread.Return1 = (uint)result;
                currThread.Return2 = Return2;
                currThread.Return3 = Return3;
                currThread.Return4 = Return4;
            }

            if (currThread.TimeToSleep != 0)
            {
#if SYSCALLS_TRACE
                BasicConsole.WriteLine("Updating thread state...");
#endif
                Scheduler.UpdateCurrentState();
            }

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Syscall handled.");
            BasicConsole.WriteLine("---------------");
#endif
        }
    }
}