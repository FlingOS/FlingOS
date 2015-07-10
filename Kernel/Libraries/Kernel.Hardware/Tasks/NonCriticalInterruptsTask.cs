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
    public static unsafe class NonCriticalInterruptsTask
    {
        public static Thread OwnerThread = null;
        public static bool Awake = true;

        public static bool Terminate = false;

        public static void Main()
        {
            OwnerThread = ProcessManager.CurrentThread;

            Thread.Sleep_Indefinitely();

            while (!Terminate)
            {
                Awake = false;
                
                //BasicConsole.WriteLine("Handling non-critical interrupts...");

                //BasicConsole.WriteLine("Looping ISRs...");

                for (int ISRNum = 0; ISRNum < Interrupts.Interrupts.Handlers.Length; ISRNum++)
                {
                    //BasicConsole.WriteLine("Getting handlers...");
                    InterruptHandlers handlers = Interrupts.Interrupts.Handlers[ISRNum];

                    //BasicConsole.WriteLine("Checking handlers aren't null...");
                    if (handlers != null)
                    {
                        //BasicConsole.WriteLine("Resetting queued occurrences...");
                        if (handlers.QueuedOccurrences < handlers.QueuedOccurrencesOld)
                        {
                            handlers.QueuedOccurrencesOld = 0;
                        }

                        //BasicConsole.WriteLine("Getting queued occurrences data...");
                        int QueuedOccurrences = handlers.QueuedOccurrences;
                        int Occurrences = QueuedOccurrences - handlers.QueuedOccurrencesOld;
                        handlers.QueuedOccurrencesOld = QueuedOccurrences;

                        // Prevent potential weird cases
                        //BasicConsole.WriteLine("Checking number of occurrences...");
                        if (Occurrences > 0 && Occurrences < 1000)
                        {
                            //BasicConsole.WriteLine("Looping handlers...");
                            for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
                            {
                                //BasicConsole.WriteLine("Getting handler descrip...");
                                HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];

                                //BasicConsole.WriteLine("Checking is not a critical handler...");
                                if (!descrip.CriticalHandler)
                                {
                                    //BasicConsole.WriteLine("Getting func...");
                                    InterruptHandler func = descrip.handler;
                                    
                                    //BasicConsole.WriteLine("Looping occurrences...");
                                    for (int x = 0; x < Occurrences; x++)
                                    {
                                        try
                                        {
                                            //BasicConsole.WriteLine("Calling func...");
                                            func(descrip.data);
                                            //BasicConsole.WriteLine("Call completed.");
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

                //BasicConsole.WriteLine("Finished handling.");

                if (!Awake)
                {
                    //BasicConsole.WriteLine("Sleeping non-critical interrupts thread...");

                    //Thread.Sleep(1000);
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
