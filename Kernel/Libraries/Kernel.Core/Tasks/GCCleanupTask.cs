#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
    
#define GCTASK_TRACE
#undef GCTASK_TRACE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Tasks
{
    public static class GCCleanupTask
    {
        public static void Main()
        {
            while (true)
            {
                bool reenable = Hardware.Processes.Scheduler.Enabled;
                try
                {
                    if (reenable)
                    {
                        Hardware.Processes.Scheduler.Disable();
                    }

#if GCTASK_TRACE
                    BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    BasicConsole.WriteLine("GC cleaning...");
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
                    
                    FOS_System.GC.Cleanup();

#if GCTASK_TRACE
                    BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    BasicConsole.WriteLine("GC stopped.");
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif

                }
                catch
                {
                }

                if (reenable)
                {
                    Hardware.Processes.Scheduler.Enable();
                }
                Hardware.Processes.Thread.Sleep(1000);
            }
        }
    }
}
