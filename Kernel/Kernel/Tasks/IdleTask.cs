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

using Drivers.Compiler.Attributes;
using Kernel.Devices.CPUs;
using Kernel.Framework;

namespace Kernel.Tasks
{
    public static unsafe class IdleTask
    {
        private static readonly bool Terminating = false;
        private static uint GCThreadId;

        [NoGC]
        public static void Main()
        {
            Helpers.ProcessInit("Idle Task", out GCThreadId);

            //TODO: Use some kind of factory for creating the correct CPU class
            CPU TheCPU = new CPUx86_32();

            //Note: Do not use Thread.Sleep within this task because this is the idle task. Its purpose
            //      is to be the only thread left awake when all others are slept.

            GC.OutputTrace = false;

            while (!Terminating)
            {
                *(ushort*)0xB809E = 0x1F00 | '1';
                TheCPU.Halt();

                *(ushort*)0xB809E = 0x3F00 | '2';
                TheCPU.Halt();
            }
        }
    }
}