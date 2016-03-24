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

namespace Kernel.Tasks
{
    /// <summary>
    /// The Device Manager Task 
    /// </summary>
    public static unsafe class DeviceManagerTask
    {
        /// <summary>
        /// Task terminating
        /// </summary>
        public static bool Terminating = false;

        /// <summary>
        /// The gc thread identifier
        /// </summary>
        private static uint GCThreadId;

        /// <summary>
        /// The standard out
        /// </summary>
        private static Pipes.Standard.StandardOutpoint StdOut;
        //private static Pipes.Standard.StandardInpoint StdIn;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
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
                StdOut = new Pipes.Standard.StandardOutpoint(true);
                int StdOutPipeId = StdOut.WaitForConnect();

                //int numOutpoints;
                //Pipes.BasicOutpoint.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_In);
                //if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                //{
                //    Pipes.PipeOutpointDescriptor[] OutpointDescriptors;
                //    Pipes.BasicOutpoint.GetOutpointDescriptors(numOutpoints, out SysCallResult, out OutpointDescriptors, Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_In);
                //
                //    if (SysCallResult == SystemCallResults.OK)
                //    {
                //        for (int i = 0; i < OutpointDescriptors.Length; i++)
                //        {
                //            Pipes.PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
                //            //TODO: Filter to target
                //            StdIn = new Pipes.Standard.StandardInpoint(Descriptor.ProcessId, false);
                //        }
                //    }
                //}

                uint loops = 0;
                while (!Terminating)
                {
                    try
                    {
                        StdOut.Write(StdOutPipeId, "DM > Hello, world! (" + (FOS_System.String)loops++ + ")\n", true);
                        SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);
                    }
                    catch
                    {
                        BasicConsole.WriteLine("DM > Error writing to StdOut!");
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
    }
}
