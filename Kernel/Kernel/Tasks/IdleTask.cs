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
    
using Kernel.FOS_System.Processes;

namespace Kernel.Tasks
{
    public static unsafe class IdleTask
    {
        private static bool Terminating = false;
        private static uint GCThreadId;

        [Drivers.Compiler.Attributes.NoGC]
        public static void Main()
        {
            // Initialise heap & GC
            BasicConsole.WriteLine("IDLE > Creating heap...");
            FOS_System.Heap.InitForProcess();

            BasicConsole.WriteLine("IDLE > Starting GC thread...");
            // Start thread for calling GC Cleanup method
            if (SystemCalls.StartThread(GCCleanupTask.Main, out GCThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Idle Task: GC thread failed to create!");
            }
            
            //TODO: Use some kind of factory for creating the correct CPU class
            Hardware.Devices.CPU TheCPU = new Hardware.CPUs.CPUx86_32();

            //Note: Do not use Thread.Sleep within this task because this is the idle task. Its purpose
            //      is to be the only thread left awake when all others are slept.

            FOS_System.GC.OutputTrace = false;

            int i = 0;
            while (!Terminating)
            {
                *((ushort*)0xB809E) = (0x1F00 | '1');
                TheCPU.Halt();

                *((ushort*)0xB809E) = (0x3F00 | '2');
                TheCPU.Halt();

                if (i < 10000)
                {
                    i++;
                }
                else if (i == 10000)
                {
                    i = 0;
                    BasicConsole.WriteLine(GCThreadId);
                    SystemCalls.SleepThread(1000);
                }
            }
        }
    }
}
