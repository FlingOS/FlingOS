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
using Kernel.Processes;
using Kernel.Hardware;
using Kernel.Hardware.Devices;

namespace Kernel.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        private static uint GCThreadId;
        
        private static Hardware.Keyboards.VirtualKeyboard keyboard;
        private static Consoles.VirtualConsole console;

        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            SystemCallResults SysCallResult = SystemCalls.StartThread(GCCleanupTask.Main, out GCThreadId);
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Device Manager: GC thread failed to create!");
            }

            try
            {
                BasicConsole.WriteLine("DM > Creating virtual keyboard...");
                keyboard = new Hardware.Keyboards.VirtualKeyboard();

                BasicConsole.WriteLine("DM > Registering for Receive Message syscall...");
                SystemCalls.RegisterSyscallHandler(SystemCallNumbers.ReceiveMessage, SyscallHandler);

                BasicConsole.WriteLine("DM > Creating virtual console...");
                console = new Consoles.VirtualConsole();

                BasicConsole.WriteLine("DM > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("DM > Creating device shell...");
                Shells.DeviceShell shell = new Shells.DeviceShell(console, keyboard);

                BasicConsole.WriteLine("DM > Executing.");

                uint loops = 0;
                while (!Terminating)
                {
                    try
                    {
                        shell.Execute();
                        //TODO: DeviceManager.UpdateDevices();
                    }
                    catch
                    {
                        BasicConsole.WriteLine("DM > Error executing shell!");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }

                    SystemCalls.SleepThread(1000);
                }
            }
            catch
            {
                BasicConsole.WriteLine("DM > Error initialising!");
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
        /// <param name="callerProcesId">The Id of the process which invoked the system call.</param>
        /// <param name="callerThreadId">The Id of the thread which invoked the system call.</param>
        /// <returns>A system call result indicating what has occurred and what should occur next.</returns>
        /// <remarks>
        /// Executes within the interrupt handler. Usual restrictions apply.
        /// </remarks>
        public static SystemCallResults HandleSystemCall(uint syscallNumber,
            uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.ReceiveMessage:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("DM > Syscall: Receive message");
#endif
                    ReceiveMessage(callerProcesId, param1, param2);
                    break;
                default:
                    BasicConsole.WriteLine("System call unrecognised/unhandled by Device Manager Task.");
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
