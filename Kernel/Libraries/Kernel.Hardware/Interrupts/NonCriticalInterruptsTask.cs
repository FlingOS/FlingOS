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
    
using System;
using Kernel.Hardware.Processes;

namespace Kernel.Hardware.Interrupts
{
    public static unsafe class NonCriticalInterruptsTask
    {
        public static Thread OwnerThread = null;
        public static bool Awake = true;

        public static void Main()
        {
            OwnerThread = ProcessManager.CurrentThread;

            Thread.Sleep_Indefinitely();

            while (true)
            {
                //Scheduler.Disable();
                Awake = false;
                
                //BasicConsole.WriteLine("Handling non-critical interrupts...");

                uint currProcessId = 0;
                uint currThreadId = 0;
                if (ProcessManager.CurrentProcess != null)
                {
                    currProcessId = ProcessManager.CurrentProcess.Id;
                    currThreadId = ProcessManager.CurrentThread.Id;
                }
                bool switched = false;

                for (int ISRNum = 0; ISRNum < Interrupts.Handlers.Length; ISRNum++)
                {
                    InterruptHandlers handlers = Interrupts.Handlers[ISRNum];
                    if (handlers != null)
                    {
                        if (handlers.QueuedOccurrences < handlers.QueuedOccurrencesOld)
                        {
                            handlers.QueuedOccurrencesOld = 0;
                        }

                        int QueuedOccurrences = handlers.QueuedOccurrences;
                        int Occurrences = QueuedOccurrences - handlers.QueuedOccurrencesOld;
                        handlers.QueuedOccurrencesOld = QueuedOccurrences;

                        // Prevent potential weird cases
                        if (Occurrences > 0 && Occurrences < 1000)
                        {
                            for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
                            {
                                HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];

                                if (!descrip.CriticalHandler)
                                {
                                    InterruptHandler func = descrip.handler;

                                    if (!descrip.IgnoreProcessId)
                                    {
                                        ProcessManager.SwitchProcess(descrip.ProcessId, -1);
                                        switched = true;
                                    }

                                    for (int x = 0; x < Occurrences; x++)
                                    {
                                        try
                                        {
                                            func(descrip.data);
                                        }
                                        catch
                                        {
                                            BasicConsole.SetTextColour(BasicConsole.warning_colour);
                                            BasicConsole.WriteLine("Error while processing non-critical interrupt.");
                                            BasicConsole.SetTextColour(BasicConsole.default_colour);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (switched)
                {
                    ProcessManager.SwitchProcess(currProcessId, (int)currThreadId);
                }

                //BasicConsole.WriteLine("Finished handling...");

                if (!Awake)
                {
                    //BasicConsole.WriteLine("Sleeping non-critical interrupts thread...");

                    //Scheduler.Enable();
                    if (!Thread.Sleep_Indefinitely())
                    {
                        BasicConsole.SetTextColour(BasicConsole.error_colour);
                        BasicConsole.WriteLine("Failed to sleep non-critical interrupts thread!");
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
                    }
                }
            }
        }
    }
}
