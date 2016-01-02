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
using Kernel.Hardware.Processes;
using Kernel.Hardware.Interrupts;

namespace Kernel.Hardware.Tasks
{
    public static class DeviceManagerTask
    {
        public static Thread OwnerThread = null;
        public static bool Awake = true;

        public static bool Terminate = false;

        public static void Main()
        {
            OwnerThread = ProcessManager.CurrentThread;

            Kernel.Processes.SystemCalls.SleepThread(Kernel.Processes.SystemCalls.IndefiniteSleepThread);

            while (!Terminate)
            {
                Awake = false;

                DeviceManager.UpdateDevices();
                
                if (!Awake)
                {
                    if (Kernel.Processes.SystemCalls.SleepThread(Kernel.Processes.SystemCalls.IndefiniteSleepThread) != Kernel.Processes.SystemCallResults.OK)
                    {
                        BasicConsole.SetTextColour(BasicConsole.error_colour);
                        BasicConsole.WriteLine("Failed to sleep device manager thread!");
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
                    }
                }
            }
        }
    }
}
