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
    
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using System;
using Kernel.Hardware.Processes;

namespace Kernel
{
    /// <summary>
    /// The main class (containing the kernel entry point) for the Fling OS kernel.
    /// </summary>
    [Compiler.PluggedClass]
    [Drivers.Compiler.Attributes.PluggedClass]
    public static class Kernel
    {
        /// <summary>
        /// Initialises static stuff within the kernel (such as calling GC.Init and BasicDebug.Init)
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        static Kernel()
        {
            BasicConsole.Init();
            BasicConsole.Clear();
            
#if DEBUG
            //Debug.BasicDebug.Init();
#endif

            BasicConsole.WriteLine();
        }

        /// <summary>
        /// Filled-in by the compiler.
        /// </summary>
        [Compiler.CallStaticConstructorsMethod]
        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        /// <summary>
        /// Main kernel entry point
        /// </summary>
        [Compiler.KernelMainMethod]
        [Drivers.Compiler.Attributes.MainMethod]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        static unsafe void Main()
        {
            ExceptionMethods.AddExceptionHandlerInfo(null, null);

            try
            {
                BasicConsole.WriteLine("Fling OS  Copyright (C) 2015  Edward Nutting");
                BasicConsole.WriteLine("This program comes with ABSOLUTELY NO WARRANTY;.");
                BasicConsole.WriteLine("This is free software, and you are welcome to redistribute it");
                BasicConsole.WriteLine("under certain conditions; See GPL V2 for details, a copy of");
                BasicConsole.WriteLine("which should have been provided with the executable.");

                BasicConsole.WriteLine("Fling OS Running...");

                // DO NOT REMOVE THE FOLLOWING LINE -- ednutting

                PreReqs.PageFaultDetection_Initialised = true;
            
                Hardware.VirtMemManager.Init();
                Hardware.Devices.CPU.InitDefault();
                Hardware.Devices.Timer.InitDefault();
                Core.Processes.SystemCalls.Init();

                //uint bpm = 140;
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.C4,
                //    Hardware.Timers.PIT.MusicalNoteValue.Quaver,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.Silent,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.E4,
                //    Hardware.Timers.PIT.MusicalNoteValue.Quaver,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.Silent,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.G4,
                //    Hardware.Timers.PIT.MusicalNoteValue.Quaver,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.Silent,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.C5,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.Silent,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.G4,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                //Hardware.Timers.PIT.ThePIT.PlayNote(
                //    Hardware.Timers.PIT.MusicalNote.C5,
                //    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                //    bpm);
                
                Process ManagedMainProcess = ProcessManager.CreateProcess(ManagedMain, "Managed Main", false);                
                Thread ManagedMain_MainThread = ((Thread)ManagedMainProcess.Threads[0]);
                Hardware.VirtMemManager.Unmap(ManagedMain_MainThread.State->ThreadStackTop - 4092);
                ManagedMainProcess.TheMemoryLayout.RemovePage((uint)ManagedMain_MainThread.State->ThreadStackTop - 4092);
                ManagedMain_MainThread.State->ThreadStackTop = GetKernelStackPtr();
                ManagedMain_MainThread.State->ESP = (uint)ManagedMain_MainThread.State->ThreadStackTop;
                ProcessManager.RegisterProcess(ManagedMainProcess, Scheduler.Priority.Normal);

                Scheduler.Init();

                // Busy wait until the scheduler interrupts us. 
                while (true)
                {
                    ;
                }
                // We will never return to this point since there is no way for the scheduler to point
                //  to it.
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    BasicConsole.WriteLine("Page fault exception unhandled!");
                }
                else
                {
                    BasicConsole.WriteLine("Startup error! " + ExceptionMethods.CurrentException.Message);
                }
                BasicConsole.WriteLine("Fling OS forced to halt!");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("Cleaning up...");
            FOS_System.GC.Cleanup();

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.Write("GC num objs: ");
            BasicConsole.WriteLine(FOS_System.GC.NumObjs);
            BasicConsole.Write("GC num strings: ");
            BasicConsole.WriteLine(FOS_System.GC.NumStrings);
            BasicConsole.Write("Heap memory use: ");
            BasicConsole.Write(Heap.FBlock->used * Heap.FBlock->bsize);
            BasicConsole.Write(" / ");
            BasicConsole.WriteLine(Heap.FBlock->size);
            BasicConsole.SetTextColour(BasicConsole.default_colour);

            BasicConsole.WriteLine("Fling OS Ended.");

            //Necessary - no way of returning from this method since add exception info 
            //            at start cannot be "undone" so stack is "corrupted" if we try
            //            to "ret"
            //So we just halt the CPU for want of a better solution later when ACPI is 
            //implemented.
            ExceptionMethods.HaltReason = "End of Main";
            Halt(0xFFFFFFFF);
            //TODO: Proper shutdown method
        }

        /// <summary>
        /// Halts the kernel and halts the CPU.
        /// </summary>
        /// <param name="lastAddress">The address of the last line of code which ran or 0xFFFFFFFF.</param>
        [Compiler.HaltMethod]
        [Drivers.Compiler.Attributes.HaltMethod]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Halt(uint lastAddress)
        {
            BasicConsole.PrimaryOutputEnabled = true;

            try
            {
                Hardware.Devices.Keyboard.CleanDefault();
                Hardware.Devices.Timer.CleanDefault();
            }
            catch
            {
            }

            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.Write("Halt Reason: ");
            BasicConsole.WriteLine(ExceptionMethods.HaltReason);

            FOS_System.String LastAddressStr = "Last address: 0x        ";
            uint y = lastAddress;
            int offset = 23;
            #region Address
            while (offset > 15)
            {
                uint rem = y & 0xFu;
                switch (rem)
                {
                    case 0:
                        LastAddressStr[offset] = '0';
                        break;
                    case 1:
                        LastAddressStr[offset] = '1';
                        break;
                    case 2:
                        LastAddressStr[offset] = '2';
                        break;
                    case 3:
                        LastAddressStr[offset] = '3';
                        break;
                    case 4:
                        LastAddressStr[offset] = '4';
                        break;
                    case 5:
                        LastAddressStr[offset] = '5';
                        break;
                    case 6:
                        LastAddressStr[offset] = '6';
                        break;
                    case 7:
                        LastAddressStr[offset] = '7';
                        break;
                    case 8:
                        LastAddressStr[offset] = '8';
                        break;
                    case 9:
                        LastAddressStr[offset] = '9';
                        break;
                    case 10:
                        LastAddressStr[offset] = 'A';
                        break;
                    case 11:
                        LastAddressStr[offset] = 'B';
                        break;
                    case 12:
                        LastAddressStr[offset] = 'C';
                        break;
                    case 13:
                        LastAddressStr[offset] = 'D';
                        break;
                    case 14:
                        LastAddressStr[offset] = 'E';
                        break;
                    case 15:
                        LastAddressStr[offset] = 'F';
                        break;
                }
                y >>= 4;
                offset--;
            }

            #endregion
            BasicConsole.WriteLine(LastAddressStr);

            BasicConsole.SetTextColour(BasicConsole.default_colour);

            if (ExceptionMethods.CurrentException != null)
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    BasicConsole.Write("Address: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).address);
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).errorCode);
                }
                else if (ExceptionMethods.CurrentException is FOS_System.Exceptions.DoubleFaultException)
                {
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.DoubleFaultException)ExceptionMethods.CurrentException).ErrorCode);
                }
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Kernel halting!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            PreReqs.Reset();
        }

        /// <summary>
        /// The actual main method for the kernel - by this point, all memory management, exception handling 
        /// etc has been set up properly.
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        private static unsafe void ManagedMain()
        {
            BasicConsole.WriteLine(" Managed Main! ");
            BasicConsole.WriteLine(" > Executing normally...");

            try
            {
                BasicConsole.WriteLine(" > Starting GC Cleanup task...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.GCCleanupTask.Main);

                BasicConsole.WriteLine(" > Starting Idle task...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.IdleTask.Main);

                BasicConsole.WriteLine(" > Starting Non-critical interrupts task...");
                ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.NonCriticalInterruptsTask.Main);

                BasicConsole.WriteLine(" > Starting System Status task...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.SystemStatusTask.Main);

                BasicConsole.WriteLine(" > Starting Device Manager task...");
                ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.DeviceManagerTask.Main);
                
                BasicConsole.WriteLine(" > Starting Play Notes task...");
                ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.PlayNotesTask.Main);
                
                Hardware.Devices.Keyboard.InitDefault();
                Core.Console.InitDefault();
                Core.Shell.InitDefault();

                Hardware.IO.Serial.Serial.InitCOM1();
                Hardware.IO.Serial.Serial.InitCOM2();
                BasicConsole.SecondaryOutput = BasicConsole_SecondaryOutput;
                BasicConsole.PrimaryOutputEnabled = false;
                Core.Shell.Default.Execute();
                BasicConsole.PrimaryOutputEnabled = true;

                if (!Core.Shell.Default.Terminating)
                {
                    Core.Console.Default.WarningColour();
                    Core.Console.Default.WriteLine("Abnormal shell shutdown!");
                    Core.Console.Default.DefaultColour();
                }
                else
                {
                    Core.Console.Default.Clear();
                }
            }
            catch
            {
                BasicConsole.PrimaryOutputEnabled = true;
                OutputCurrentExceptionInfo();
            }
            
            BasicConsole.WriteLine();
            OutputDivider();
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of managed main.");

            ExceptionMethods.HaltReason = "Managed main thread ended.";
            Halt(0);
        }

        /// <summary>
        /// Outputs the current exception information.
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        private static void OutputCurrentExceptionInfo()
        {
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
        }

        /// <summary>
        /// Outputs a divider line.
        /// </summary>
        private static void OutputDivider()
        {
            BasicConsole.WriteLine("---------------------");
        }

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        private static void BasicConsole_SecondaryOutput(FOS_System.String str)
        {
            Hardware.IO.Serial.Serial.COM1.Write(str);
        }

        public static void OutputAddressDetectedMethod(uint EIP, uint OpNum)
        {
            BasicConsole.WriteLine(((FOS_System.String)"Test address detected! EIP: ") + EIP + ", Op Num: " + OpNum);
            BasicConsole.DelayOutput(5);
            BasicConsole.WriteLine(((FOS_System.String)"Stack values: ESP = ") + GetESP() + " (Offset from ESP : Value)");
            for (uint i = 0x1C; i < 120 /*0x1C + (23 * 4)*/; i += 4)
            {
                BasicConsole.WriteLine(((FOS_System.String)i) + " : " + GetStackValue(i));
            }
            BasicConsole.DelayOutput(1000);
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static uint GetStackValue(uint offset)
        {
            return 0;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static uint GetESP()
        {
            return 0;
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Kernel")]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=@"ASM\Kernel")]
        private static unsafe void* GetManagedMainMethodPtr()
        {
            return null;
        }
        [Compiler.PluggedMethod(ASMFilePath=null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=null)]
        private static unsafe byte* GetKernelStackPtr()
        {
            return null;
        }
    }
}
