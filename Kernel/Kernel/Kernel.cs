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
    public static class Kernel
    {
        /// <summary>
        /// Initialises static stuff within the kernel (such as calling GC.Init and BasicDebug.Init)
        /// </summary>
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
        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        /// <summary>
        /// Main kernel entry point
        /// </summary>
        [Drivers.Compiler.Attributes.MainMethod]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        static unsafe void Main()
        {
            ExceptionMethods.AddExceptionHandlerInfo(null, null);
      

            try
            {                
                //FOS_System.GC.Cleanup();
                
                //BasicConsole.WriteLine("Virtual memory initialised.");
                //BasicConsole.Write("Heap memory use: ");
                //BasicConsole.Write(Heap.GetTotalUsedMem());
                //BasicConsole.Write(" / ");
                //BasicConsole.WriteLine(Heap.GetTotalMem());
                //BasicConsole.DelayOutput(5);
                
                Hardware.IO.Serial.Serial.InitCOM1();
                Hardware.IO.Serial.Serial.InitCOM2();

                BasicConsole.SecondaryOutput = BasicConsole_SecondaryOutput;
                BasicConsole.SecondaryOutputEnabled = true;

                // DO NOT REMOVE THE FOLLOWING LINE -- ednutting
                PreReqs.PageFaultDetection_Initialised = true;

                Hardware.Devices.CPU.InitDefault();
                Hardware.Devices.Timer.InitDefault();

                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SecondaryOutputEnabled = false;
                BasicConsole.WriteLine();
                BasicConsole.WriteLine();
                BasicConsole.WriteLine(TextSplashScreen);
                
                BasicConsole.SecondaryOutputEnabled = true;
                BasicConsole.PrimaryOutputEnabled = false;
                
                for(int i = 0; i < TextSplashScreen.length; i += 80)
                {
                    TextSplashScreen[i] = '\n';
                }
                BasicConsole.WriteLine(TextSplashScreen);

                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SecondaryOutputEnabled = true;

                for (int i = 0; i < 37; i++)
                {
                    BasicConsole.Write(' ');
                }
                char num = '1';
                for (int i = 0; i < 3; i++)
                {
                    BasicConsole.Write(num++);
                    Hardware.Devices.Timer.Default.Wait(1000);
                    BasicConsole.Write(' ');
                }
                BasicConsole.WriteLine();

                BasicConsole.WriteLine("FlingOS(TM)");
                BasicConsole.WriteLine("Copyright (C) 2014-15 Edward Nutting");
                BasicConsole.WriteLine("This program comes with ABSOLUTELY NO WARRANTY;.");
                BasicConsole.WriteLine("This is free software, and you are welcome to redistribute it");
                BasicConsole.WriteLine("under certain conditions; See GPL V2 for details, a copy of");
                BasicConsole.WriteLine("which should have been provided with the executable.");

                BasicConsole.WriteLine("Fling OS Running...");
                BasicConsole.WriteLine();

#if MIPS
            BasicConsole.WriteLine("MIPS Kernel");
#elif x86 || AnyCPU
                BasicConsole.WriteLine("x86 Kernel");
#endif
                BasicConsole.WriteLine();

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

                BasicConsole.WriteLine("Creating kernel process...");
                Process KernelProcess = ProcessManager.CreateProcess(Core.Tasks.KernelTask.Main, "Kernel Task", false);
                ProcessManager.KernelProcess = KernelProcess;

                BasicConsole.WriteLine("Creating kernel thread...");
                Thread KernelThread = ((Thread)KernelProcess.Threads[0]);

                BasicConsole.WriteLine("Initialising kernel thread stack...");
                Hardware.VirtMemManager.Unmap(KernelThread.State->ThreadStackTop - 4092);
                KernelProcess.TheMemoryLayout.RemovePage((uint)KernelThread.State->ThreadStackTop - 4092);
                KernelThread.State->ThreadStackTop = GetKernelStackPtr();
                KernelThread.State->ESP = (uint)KernelThread.State->ThreadStackTop;

                BasicConsole.WriteLine("Initialising kernel process heap...");
                KernelProcess.HeapLock = Heap.AccessLock;
                KernelProcess.HeapPtr = Heap.FBlock;

                BasicConsole.WriteLine("Initialising kernel process GC...");
                KernelProcess.TheGCState = FOS_System.GC.State;
                
                BasicConsole.WriteLine("Registering kernel process...");
                ProcessManager.RegisterProcess(KernelProcess, Scheduler.Priority.Normal);

                BasicConsole.WriteLine("Initialising scheduler...");
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
                BasicConsole.WriteLine("FlingOS forced to halt!");
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
        [Drivers.Compiler.Attributes.HaltMethod]
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
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static uint GetStackValue(uint offset)
        {
            return 0;
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static uint GetESP()
        {
            return 0;
        }

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Kernel")]
        private static unsafe byte* GetKernelStackPtr()
        {
            return null;
        }

        #region Text Splash Screen
        
        private static FOS_System.String TextSplashScreen = @"                                                                           TM      oooooooooooo oooo   o8o                           .oooooo.    .oooooo..o        `888'     `8 `888   `''                          d8P'  `Y8b  d8P'    `Y8         888          888  oooo  ooo. .oo.    .oooooooo 888      888 Y88bo.              888oooo8     888  `888  `888P'Y88b  888' `88b  888      888  `'Y8888o.          888    '     888   888   888   888  888   888  888      888      `'Y88b         888          888   888   888   888  `88bod8P'  `88b    d88' oo     .d8P        o888o        o888o o888o o888o o888o `8oooooo.   `Y8bood8P'  8''88888P'                                                   YD                                                                      'Y88888P'                                                                                                                    _____ _                 _              _   _               _      ___  ___     |_   _| |_  ___    ___ __| |_  _ __ __ _| |_(_)___ _ _  __ _| |   / _ \/ __|      | | | ' \/ -_)  / -_) _` | || / _/ _` |  _| / _ \ ' \/ _` | |  | (_) \__ \      |_| |_||_\___|  \___\__,_|\_,_\__\__,_|\__|_\___/_||_\__,_|_|   \___/|___/                                                                                           http://www.flingos.co.uk, Copyright (C) Edward Nutting 2014-15                                      Licensed under GPLv2                                ";
        
        #endregion
    }
}
