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
    
#define PROCESS_TRACE
#undef PROCESS_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.VirtMem;

namespace Kernel.Hardware.Processes
{
    [Compiler.PluggedClass]
    [Drivers.Compiler.Attributes.PluggedClass]
    public unsafe class Process : FOS_System.Object
    {
        public List Threads = new List();
        public MemoryLayout TheMemoryLayout = new MemoryLayout();

        public uint Id;
        public FOS_System.String Name;

        public Scheduler.Priority Priority;

        protected uint ThreadIdGenerator = 1;
        
        public readonly bool UserMode;

        public Process(ThreadStartMethod MainMethod, uint AnId, FOS_System.String AName, bool userMode)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Constructing process object...");
#endif
            Id = AnId;
            Name = AName;
            UserMode = userMode;

#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
#endif
            CreateThread(MainMethod);
        }

        public virtual void CreateThread(ThreadStartMethod MainMethod)
        {
#if PROCESS_TRACE
            BasicConsole.WriteLine("Creating thread...");
#endif
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            Thread mainThread = new Thread(MainMethod, ThreadIdGenerator++, UserMode);
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding data page...");
#endif
            // Add the page to the processes memory layout
            TheMemoryLayout.AddDataPage(
                (uint)VirtMemManager.GetPhysicalAddress(mainThread.State->ThreadStackTop - 4092),
                (uint)mainThread.State->ThreadStackTop - 4092);
            
#if PROCESS_TRACE
            BasicConsole.WriteLine("Adding thread...");
#endif

            Threads.Add(mainThread);

            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
        }

        public virtual void LoadMemLayout()
        {
            TheMemoryLayout.Load(UserMode);
        }
        public virtual void UnloadMemLayout()
        {
            TheMemoryLayout.Unload();
        }
    }
}
