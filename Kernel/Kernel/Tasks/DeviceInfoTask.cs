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
    
using System;
using Kernel.FOS_System;
using Kernel.Hardware;
using Kernel.Hardware.Devices;
using Kernel.FOS_System.Processes;

namespace Kernel.Tasks
{
    public static unsafe class DeviceInfoTask
    {
        public static bool Terminating = false;

        private static uint GCThreadId;
        
        private static Hardware.Keyboards.VirtualKeyboard keyboard;
        private static Consoles.VirtualConsole console;

        private static Hardware.Timers.RTC RTC;
        private static UInt64 StartTime;

        public static void Main()
        {
            Helpers.ProcessInit("Device Info", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("DI > Creating virtual keyboard...");
                keyboard = new Hardware.Keyboards.VirtualKeyboard();

                BasicConsole.WriteLine("DI > Registering for Receive Message syscall...");
                SystemCalls.RegisterSyscallHandler(SystemCallNumbers.ReceiveMessage, SyscallHandler);
                
                BasicConsole.WriteLine("DI > Creating virtual console...");
                console = new Consoles.VirtualConsole();

                BasicConsole.WriteLine("DI > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("DI > Creating device shell...");
                Shells.DeviceShell shell = new Shells.DeviceShell(console, keyboard);

                BasicConsole.WriteLine("DI > Executing.");

                DeviceManager.Init();

                #region Real Time Clock initialisation
                
                //TODO: This is x86 specific
                RTC = new Hardware.Timers.RTC();
                //TODO: DeviceManager.AddDevice(RTC);

                StartTime = RTC.GetUTCTime();
                
                BasicConsole.WriteLine("Start time:");
                BasicConsole.WriteLine(new FOS_System.DateTime(StartTime).ToString());
                
                SystemCalls.RegisterSyscallHandler(SystemCallNumbers.GetTime);
                SystemCalls.RegisterSyscallHandler(SystemCallNumbers.GetUpTime);

                #endregion

                //uint loops = 0;
                while (!Terminating)
                {
                    try
                    {
                        /*uint SharedPages_StartAddress;
                        SystemCallResults RequestPagesResult = SystemCalls.RequestPages(0xE0000000, 1, out SharedPages_StartAddress);
                        if (RequestPagesResult == SystemCallResults.OK)
                        {
                            BasicConsole.WriteLine("DI > Allocated pages for sharing with WM.");

                            char* TextPtr = (char*)SharedPages_StartAddress;
                            TextPtr[0] = '\0';

                            SystemCallResults SharePagesResult = SystemCalls.SharePages(SharedPages_StartAddress, 1, KernelTask.WindowManagerTask_ProcessId);
                            if (SharePagesResult == SystemCallResults.OK)
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    FOS_System.String TestMessage = "Shared paging proof of concept message number ";
                                    TestMessage += j;
                                    TestMessage += "\n";

                                    for (int i = 0; i < TestMessage.length; i++)
                                    {
                                        TextPtr[i + 1] = TestMessage[i];
                                    }
                                    TextPtr[TestMessage.length + 1] = '\0';
                                    TextPtr[0] = '1';

                                    while (TextPtr[0] != '\0')
                                    {
                                        SystemCalls.SleepThread(100);
                                    }
                                }

                                int SemaphoreId;
                                SystemCalls.CreateSemaphore(0, out SemaphoreId);
                                SystemCalls.ShareSemaphore(SemaphoreId, KernelTask.WindowManagerTask_ProcessId);
                                int* IdPtr = (int*)(TextPtr + 1);
                                *IdPtr = SemaphoreId;
                                TextPtr[0] = '1';
                                for (int i = 0; i < 10; i++)
                                {
                                    BasicConsole.WriteLine("Delaying semaphore signal: " + (FOS_System.String)i);
                                    SystemCalls.SleepThread(1000);
                                }
                                SystemCalls.SignalSemaphore(SemaphoreId);

                                SystemCalls.UnmapPages(SharedPages_StartAddress, 1);

                                // This worked last time I tested it 
                                //      [ Ed Nutting - 2016-01-20 18:29 ]
                                //try
                                //{
                                //    // This should now cause a page fault
                                //    TextPtr[0] = '\0';
                                //}
                                //catch
                                //{
                                //    BasicConsole.WriteLine("Expected page fault caught.");
                                //}
                            }
                            else
                            {
                                BasicConsole.WriteLine("DI > Couldn't share pages with WM.");
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("DI > Couldn't allocate pages for sharing with WM.");
                        }*/
                        
                        shell.Execute();
                        //TODO: DeviceManager.UpdateDevices();
                    }
                    catch
                    {
                        BasicConsole.WriteLine("DI > Error executing shell!");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }

                    SystemCalls.SleepThread(1000);
                }
            }
            catch
            {
                BasicConsole.WriteLine("DI > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }
        }
        
        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = HandleSystemCall(syscallNumber,
                param1, param2, param3,
                ref Return2, ref Return3, ref Return4,
                callerProcessId, callerThreadId);

            return (int)result;
        }

        /// <summary>
        /// Special handler method for system calls recognised/handlded by the task.
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
                case SystemCallNumbers.ReceiveMessage:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("DI > Syscall: Receive message");
#endif
                    ReceiveMessage(callerProcessId, param1, param2);
                    break;
                case SystemCallNumbers.GetTime:
                    {
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("DI > Syscall: Get time");
#endif
                        UInt64 CurrentUTCTime = RTC.GetUTCTime();
                        Return2 = (UInt32)(CurrentUTCTime);
                        Return3 = (UInt32)(CurrentUTCTime >> 32);
                        result = SystemCallResults.OK;
                    }
                    break;
                case SystemCallNumbers.GetUpTime:
                    {
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("DI > Syscall: Get time");
#endif
                        UInt64 CurrentUTCTime = RTC.GetUTCTime();
                        UInt64 UpTime = CurrentUTCTime - StartTime;
                        Return2 = (UInt32)(UpTime);
                        Return3 = (UInt32)(UpTime >> 32);
                        result = SystemCallResults.OK;
                    }
                    break;
                default:
                    BasicConsole.WriteLine("System call unrecognised/unhandled by Device Info Task.");
                    break;
            }

            return result;
        }

        public static void ReceiveMessage(uint CallerProcessId, uint Message1, uint Message2)
        {
            if (CallerProcessId == KernelTask.WindowManagerTask_ProcessId)
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
}
