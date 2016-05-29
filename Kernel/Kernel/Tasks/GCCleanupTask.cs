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

//#define GCTASK_TRACE

using Kernel.Framework;
using Kernel.Framework.Processes;

namespace Kernel.Tasks
{
    public static class GCCleanupTask
    {
        public static bool Terminating = false;

        public static void Main()
        {
            while (!Terminating)
            {
                //bool reenable = Hardware.Processes.Scheduler.Enabled;
                try
                {
                    //if (reenable)
                    //{
                    //    Hardware.Processes.Scheduler.Disable();
                    //}

#if GCTASK_TRACE
                    BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    BasicConsole.Write("GC cleaning: ");
                    BasicConsole.WriteLine(Hardware.Processes.ProcessManager.CurrentProcess.Name);
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif

                    GC.Cleanup();

#if GCTASK_TRACE
                    BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    BasicConsole.WriteLine("GC stopped (1).");
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
                }
                catch
                {
#if GCTASK_TRACE
                    BasicConsole.SetTextColour(BasicConsole.error_colour);
                    BasicConsole.WriteLine("GC error.");
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
                }

#if GCTASK_TRACE
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine("GC stopped (2).");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
                //if (reenable)
                //{
                //    Hardware.Processes.Scheduler.Enable();
                //}
                SystemCalls.SleepThread(1000);
            }
        }
    }
}