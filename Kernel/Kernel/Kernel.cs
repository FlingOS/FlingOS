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
using Kernel.Devices.Timers;
using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Devices.Serial;
using Kernel.Multiprocessing;
using Kernel.Multiprocessing.Scheduling;
using Kernel.Tasks;
using Kernel.VirtualMemory;
using Kernel.VirtualMemory.Implementations;

namespace Kernel
{
    /// <summary>
    ///     The main class (containing the kernel entry point) for the Fling OS kernel.
    /// </summary>
    public static class Kernel
    {
        #region Text Splash Screen

        private static readonly String TextSplashScreen =
            @"                                                                           TM      oooooooooooo oooo   o8o                           .oooooo.    .oooooo..o        `888'     `8 `888   `''                          d8P'  `Y8b  d8P'    `Y8         888          888  oooo  ooo. .oo.    .oooooooo 888      888 Y88bo.              888oooo8     888  `888  `888P'Y88b  888' `88b  888      888  `'Y8888o.          888    '     888   888   888   888  888   888  888      888      `'Y88b         888          888   888   888   888  `88bod8P'  `88b    d88' oo     .d8P        o888o        o888o o888o o888o o888o `8oooooo.   `Y8bood8P'  8''88888P'                                                   YD                                                                      'Y88888P'                                                                                                                    _____ _                 _              _   _               _      ___  ___     |_   _| |_  ___    ___ __| |_  _ __ __ _| |_(_)___ _ _  __ _| |   / _ \/ __|      | | | ' \/ -_)  / -_) _` | || / _/ _` |  _| / _ \ ' \/ _` | |  | (_) \__ \      |_| |_||_\___|  \___\__,_|\_,_\__\__,_|\__|_\___/_||_\__,_|_|   \___/|___/                                                                                           http://www.flingos.co.uk, Copyright (C) Edward Nutting 2014-15                                      Licensed under GPLv2                                ";

        #endregion

        /// <summary>
        ///     Initialises static stuff within the kernel (such as calling GC.Init and BasicDebug.Init)
        /// </summary>
        [NoDebug]
        static Kernel()
        {
            VirtualMemoryManager.Init(new X86VirtualMemoryImplementation());
        }

        /// <summary>
        ///     Filled-in by the compiler.
        /// </summary>
        [CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        /// <summary>
        ///     Main kernel entry point
        /// </summary>
        [MainMethod]
        [NoGC]
        [NoDebug]
        private static unsafe void Main()
        {
            ExceptionMethods.AddExceptionHandlerInfo(null, null);


            try
            {
                BasicConsole.WriteLine("Initialising scheduler...");
                Scheduler.Init();

                Serial.InitCOM1();
                
                BasicConsole.SecondaryOutput = BasicConsole_SecondaryOutput;
                BasicConsole.SecondaryOutputEnabled = true;

                BasicConsole.WriteLine("Creating kernel process...");
                Process KernelProcess = new Process(KernelTask.Main, ProcessManager.ProcessIdGenerator++, "Kernel Task",
                    false, null);
                //TODO: Kernel Process should have kernel's (heap, stack, static and some code) memory isolated 
                //          by adding it to Kernel Process' memory layout.
                ProcessManager.KernelProcess = KernelProcess;

                BasicConsole.WriteLine("Getting kernel thread...");
                Thread KernelThread = (Thread)KernelProcess.Threads[0];

                BasicConsole.WriteLine("Initialising kernel thread stack...");
                VirtualMemoryManager.Unmap(KernelThread.State->ThreadStackTop - Thread.ThreadStackTopOffset);
                KernelProcess.TheMemoryLayout.RemovePage((uint)KernelThread.State->ThreadStackTop -
                                                         Thread.ThreadStackTopOffset);
                VirtualMemoryManager.MapKernelProcessToMemoryLayout(KernelProcess.TheMemoryLayout);
                KernelThread.State->ThreadStackTop = GetKernelStackPtr() - (4096 - Thread.ThreadStackTopOffset);
                KernelThread.State->ESP = (uint)KernelThread.State->ThreadStackTop;
                KernelThread.State->ExState = (ExceptionState*)(KernelThread.State->ThreadStackTop + 4);
                byte* ExStateBytePtr = (byte*)KernelThread.State->ExState;
                for (int i = 0; i < sizeof(ExceptionState); i++)
                {
                    *ExStateBytePtr++ = 0;
                }

                BasicConsole.WriteLine("Registering kernel process...");
                ProcessManager.RegisterProcess(KernelProcess, Scheduler.Priority.High);

                BasicConsole.WriteLine("Initialising kernel IRQs...");
                KernelProcess.IRQHandler = KernelTask.HandleIRQ;
                KernelProcess.SwitchProcessForIRQs = false;
                KernelProcess.IRQsToHandle.Set(0);

                BasicConsole.WriteLine("Initialising default timer...");
                PIT.Init();
                Timer.Default = PIT.ThePIT;


                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SecondaryOutputEnabled = false;
                BasicConsole.WriteLine();
                BasicConsole.WriteLine();
                BasicConsole.WriteLine(TextSplashScreen);

                BasicConsole.SecondaryOutputEnabled = true;
                BasicConsole.PrimaryOutputEnabled = false;

                for (int i = 0; i < TextSplashScreen.Length; i += 80)
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
                char Num = '1';
                for (int i = 0; i < 3; i++)
                {
                    BasicConsole.Write(Num++);
                    Timer.Default.Wait(1000);
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

                BasicConsole.WriteLine("Starting scheduler...");
                PreemptionHandler PreempHandler = Scheduler.Start();
                //TODO: Should store the handler Id for future reference
                // ReSharper disable once PossibleInvalidCastException
                Timer.Default.RegisterHandler((TimerHandler)(object)PreempHandler, Scheduler.PreemptionPeriod, true,
                    Scheduler.PreemptionState);
                Scheduler.Enable();

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
                if (ExceptionMethods.CurrentException is PageFaultException)
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
        ///     Halts the kernel and halts the CPU.
        /// </summary>
        /// <param name="lastAddress">The address of the last line of code which ran or 0xFFFFFFFF.</param>
        [HaltMethod]
        [NoGC]
        public static void Halt(uint lastAddress)
        {
            BasicConsole.PrimaryOutputEnabled = true;

            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.Write("Halt Reason: ");
            BasicConsole.WriteLine(ExceptionMethods.HaltReason);

            String LastAddressStr = "Last address: 0x        ";
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
                if (ExceptionMethods.CurrentException is PageFaultException)
                {
                    BasicConsole.Write("Address: ");
                    BasicConsole.WriteLine(((PageFaultException)ExceptionMethods.CurrentException).address);
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((PageFaultException)ExceptionMethods.CurrentException).errorCode);
                }
                else if (ExceptionMethods.CurrentException is DoubleFaultException)
                {
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((DoubleFaultException)ExceptionMethods.CurrentException).ErrorCode);
                }
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Kernel halting!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Boot.Reset();
        }

        /// <summary>
        ///     Function used for BasicConsole's Secondary Output capability to write BasicConsole output to the first serial port.
        /// </summary>
        /// <param name="str">The string to write to the serial port.</param>
        [NoGC]
        [NoDebug]
        private static void BasicConsole_SecondaryOutput(String str)
        {
            Serial.COM1.Write(str);
        }

        /// <summary>
        ///     Gets a pointer to the top of the pre-allocated kernel stack.
        /// </summary>
        /// <remarks>
        ///     The pre-allocated stack is the stack which is built into the kernel's machine code using assembly instructions.
        /// </remarks>
        /// <returns>A pointer to the top of the stack.</returns>
        [PluggedMethod(ASMFilePath = @"ASM\Kernel")]
        private static unsafe byte* GetKernelStackPtr()
        {
            return null;
        }
    }
}