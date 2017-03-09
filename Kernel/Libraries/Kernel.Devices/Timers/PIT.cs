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
using Kernel.Devices.CPUs;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.IO;
using Kernel.Multiprocessing;

namespace Kernel.Devices.Timers
{
    /// <summary>
    ///     Represents the programmable interval timer.
    /// </summary>
    public class PIT : Timer
    {
        public enum MusicalNote
        {
            Silent = 0,
            C0 = 16,
            Cs0 = 17,
            Db0 = 17,
            D0 = 18,
            Ds0 = 19,
            Eb0 = 19,
            E0 = 20,
            F0 = 21,
            Fs0 = 23,
            Gb0 = 23,
            G0 = 24,
            Gs0 = 25,
            Ab0 = 25,
            A0 = 27,
            As0 = 29,
            Bb0 = 29,
            B0 = 30,
            C1 = 32,
            Cs1 = 34,
            Db1 = 34,
            D1 = 36,
            Ds1 = 38,
            Eb1 = 38,
            E1 = 41,
            F1 = 43,
            Fs1 = 46,
            Gb1 = 46,
            G1 = 49,
            Gs1 = 51,
            Ab1 = 51,
            A1 = 55,
            As1 = 58,
            Bb1 = 58,
            B1 = 61,
            C2 = 65,
            Cs2 = 69,
            Db2 = 69,
            D2 = 73,
            Ds2 = 77,
            Eb2 = 77,
            E2 = 82,
            F2 = 87,
            Fs2 = 92,
            Gb2 = 92,
            G2 = 98,
            Gs2 = 103,
            Ab2 = 103,
            A2 = 110,
            As2 = 116,
            Bb2 = 116,
            B2 = 123,
            C3 = 130,
            Cs3 = 138,
            Db3 = 138,
            D3 = 146,
            Ds3 = 155,
            Eb3 = 155,
            E3 = 164,
            F3 = 174,
            Fs3 = 185,
            Gb3 = 185,
            G3 = 196,
            Gs3 = 207,
            Ab3 = 207,
            A3 = 220,
            As3 = 233,
            Bb3 = 233,
            B3 = 246,
            C4 = 261,
            Cs4 = 277,
            Db4 = 277,
            D4 = 293,
            Ds4 = 311,
            Eb4 = 311,
            E4 = 329,
            F4 = 349,
            Fs4 = 369,
            Gb4 = 369,
            G4 = 392,
            Gs4 = 415,
            Ab4 = 415,
            A4 = 440,
            As4 = 466,
            Bb4 = 466,
            B4 = 493,
            C5 = 523,
            Cs5 = 554,
            Db5 = 554,
            D5 = 587,
            Ds5 = 622,
            Eb5 = 622,
            E5 = 659,
            F5 = 698,
            Fs5 = 739,
            Gb5 = 739,
            G5 = 783,
            Gs5 = 830,
            Ab5 = 830,
            A5 = 880,
            As5 = 932,
            Bb5 = 932,
            B5 = 987,
            C6 = 1046,
            Cs6 = 1108,
            Db6 = 1108,
            D6 = 1174,
            Ds6 = 1244,
            Eb6 = 1244,
            E6 = 1318,
            F6 = 1396,
            Fs6 = 1479,
            Gb6 = 1479,
            G6 = 1567,
            Gs6 = 1661,
            Ab6 = 1661,
            A6 = 1760,
            As6 = 1864,
            Bb6 = 1864,
            B6 = 1975,
            C7 = 2093,
            Cs7 = 2217,
            Db7 = 2217,
            D7 = 2349,
            Ds7 = 2489,
            Eb7 = 2489,
            E7 = 2637,
            F7 = 2793,
            Fs7 = 2959,
            Gb7 = 2959,
            G7 = 3135,
            Gs7 = 3322,
            Ab7 = 3322,
            A7 = 3520,
            As7 = 3729,
            Bb7 = 3729,
            B7 = 3951,
            C8 = 4186,
            Cs8 = 4434,
            Db8 = 4434,
            D8 = 4698,
            Ds8 = 4978,
            Eb8 = 4978,
            E8 = 5274,
            F8 = 5587,
            Fs8 = 5919,
            Gb8 = 5919,
            G8 = 6271,
            Gs8 = 6644,
            Ab8 = 6644,
            A8 = 7040,
            As8 = 7458,
            Bb8 = 7458,
            B8 = 7902
        }

        public enum MusicalNoteValue : uint
        {
            Semiquaver = 1, //  1/16
            Quaver = 2, //  1/8
            Crotchet = 4, //  1/4
            Minim = 8, //  1/2
            Semibreve = 16, //  1
            Breve = 32, //  2
            Longa = 64, //  4
            Maxima = 128 //  8
        }

        /// <summary>
        ///     The known frequency at which the PIT decrements the timer counters (reload values).
        /// </summary>
        public const uint PITFrequency = 1193180;

        /// <summary>
        ///     The known time between PIT decrements of the timer counters (reload values).
        /// </summary>
        public const uint PITDelayNS = 838;

        /// <summary>
        ///     The (only) PIT device instance.
        /// </summary>
        [Group(Name = "IsolatedKernel_Hardware_Devices")] public static PIT ThePIT;

        /// <summary>
        ///     Currently active handlers for when the timer interrupt occurs.
        /// </summary>
        private readonly List ActiveHandlers = new List();

        /// <summary>
        ///     The reload value for timer 0. Sets the frequency of timer 0.
        /// </summary>
        //private ushort _T0Reload = 32; // Produces ~0.0268ms delay between interrupts
        private ushort _T0Reload = 0x1000; // ~3.43284333ms

        /// <summary>
        ///     The reload value for timer 2. Sets the frequency of timer 2 hence
        ///     the frequency of the PC speaker beep.
        /// </summary>
        private ushort _T2Reload = 1024;

        /// <summary>
        ///     The PIT command port.
        /// </summary>
        protected IOPort Command = new IOPort(0x43);

        /// <summary>
        ///     The PIT Data0 port used to send data about timer 0.
        /// </summary>
        protected IOPort Data0 = new IOPort(0x40);

        /*<summary>
        /// The PIT Data1 port used to send data about timer 1.
        /// Timer 1 is outdated and "probably doesn't exist on most modern hardware".
        /// Hence this port is pointless and so commented out.
        /// </summary>
        //protected IO.IOPort Data1 = new IO.IOPort(0x41);*/

        /// <summary>
        ///     The PIT Data2 port used to send data about timer 2.
        ///     Timer 2 is used to drive the PC speaker e.g. speaker beep.
        /// </summary>
        protected IOPort Data2 = new IOPort(0x42);

        /// <summary>
        ///     The interrupt handler Id returned when the interrupt handler is set.
        ///     Use to remove the interrupt handler when disabling.
        /// </summary>
        protected int InterruptHandlerId;

        /// <summary>
        ///     The PC speaker port used to turn it on/off.
        /// </summary>
        protected IOPort SpeakerPort = new IOPort(0x61);

        /// <summary>
        ///     Whether timer 0 should be in rate generator mode or not. Set to true to
        ///     have timer 0 reload from the reload value after the count hits 0.
        ///     Essentially, set to true to make timer 0 repeat endlessly.
        /// </summary>
        public bool T0RateGen;

        /// <summary>
        ///     Incremented endlessly to create a unique timer id.
        /// </summary>
        private int TimerIdGenerator;

        /// <summary>
        ///     Set to true when the current wait timer elapses.
        /// </summary>
        private bool WaitSignaled;

        /// <summary>
        ///     The timer 0 reload value.
        /// </summary>
        public ushort T0Reload
        {
            [NoGC] get { return _T0Reload; }
            set
            {
                _T0Reload = value;

                Command.Write_Byte((byte)(T0RateGen ? 0x34 : 0x30));
                Data0.Write_Byte((byte)(value & 0xFF));
                Data0.Write_Byte((byte)(value >> 8));
            }
        }

        /// <summary>
        ///     The timer 0 frequency.
        /// </summary>
        public uint T0Frequency
        {
            [NoGC] get { return PITFrequency/_T0Reload; }
            set
            {
                if (value < 19 || value > 1193180)
                {
                    ExceptionMethods.Throw(new ArgumentException(
                        "Frequency must be between 19 and 1193180!"));
                }

                T0Reload = (ushort)(PITFrequency/value);
            }
        }

        /// <summary>
        ///     The timer 0 delay in nanoseconds.
        /// </summary>
        public uint T0DelyNS
        {
            [NoGC] get { return PITDelayNS*_T0Reload; }
            set
            {
                if (value > 54918330)
                {
                    ExceptionMethods.Throw(new ArgumentException(
                        "Delay must be no greater that 54918330"));
                }

                T0Reload = (ushort)(value/PITDelayNS);
            }
        }

        /// <summary>
        ///     The timer 2 reload value.
        /// </summary>
        public ushort T2Reload
        {
            [NoGC] get { return _T2Reload; }
            set
            {
                _T2Reload = value;

                Command.Write_Byte(0xB6);
                Data2.Write_Byte((byte)(value & 0xFF));
                Data2.Write_Byte((byte)(value >> 8));
            }
        }

        /// <summary>
        ///     The timer 2 frequency.
        /// </summary>
        public uint T2Frequency
        {
            [NoGC] get { return PITFrequency/_T2Reload; }
            set
            {
                if (value < 19 || value > 1193180)
                {
                    ExceptionMethods.Throw(new ArgumentException(
                        "Frequency must be between 19 and 1193180!"));
                }

                T2Reload = (ushort)(PITFrequency/value);
            }
        }

        /// <summary>
        ///     The timer 2 delay in nanoseconds.
        /// </summary>
        public uint T2DelyNS
        {
            [NoGC] get { return PITDelayNS*_T2Reload; }
            set
            {
                if (value > 54918330)
                {
                    ExceptionMethods.Throw(new ArgumentException(
                        "Delay must be no greater than 54918330"));
                }

                T2Reload = (ushort)(value/PITDelayNS);
            }
        }

        public PIT()
            : base(DeviceGroup.System, DeviceSubClass.Timer, "Programmable Interval Timer", new uint[0], true)
        {
        }

        /// <summary>
        ///     Enables the PC speaker sound.
        /// </summary>
        public void EnableSound()
        {
            //OR with 0x03 to enable sound
            SpeakerPort.Write_Byte((byte)(SpeakerPort.Read_Byte() | 0x03));
        }

        /// <summary>
        ///     Disables the PC speaker sound.
        /// </summary>
        public void DisableSound()
        {
            //AND with 0xFC to disable sound
            SpeakerPort.Write_Byte((byte)(SpeakerPort.Read_Byte() & 0xFC));
        }

        /// <summary>
        ///     Plays the PC speaker at the specified frequency (tone).
        /// </summary>
        /// <param name="aFreq">The frequency to play.</param>
        public void PlaySound(int aFreq)
        {
            T2Frequency = (uint)aFreq;
            EnableSound();
        }

        /// <summary>
        ///     Mutes the PC speaker.
        /// </summary>
        public void MuteSound()
        {
            DisableSound();
        }

        /// <summary>
        ///     The internal wait interrupt handler static wrapper.
        /// </summary>
        /// <param name="state">The PIT object state.</param>
        private static void SignalWait(IObject state)
        {
            ((PIT)state).SignalWait();
        }

        /// <summary>
        ///     The internal wait interrupt handler.
        /// </summary>
        private void SignalWait()
        {
            WaitSignaled = true;
        }

        /// <summary>
        ///     Blocks the caller for the specified length of time (in milliseconds).
        /// </summary>
        /// <param name="TimeoutMS">The length of time to wait in milliseconds.</param>
        public override void Wait(uint TimeoutMS)
        {
            WaitNS(1000000L*TimeoutMS);
        }

        /// <summary>
        ///     Blocks the caller for the specified length of time (in nanoseconds).
        /// </summary>
        /// <param name="TimeoutNS">The length of time to wait in nanoseconds.</param>
        public override void WaitNS(long TimeoutNS)
        {
            WaitSignaled = false;

            //TODO: Do we need to keep the handler Id?
            RegisterHandler(new PITHandler(SignalWait, this, TimeoutNS, false));

            while (!WaitSignaled)
            {
                CPU.Default.Halt();
            }
        }

        [NoDebug]
        public override int RegisterHandler(TimerHandler Handler, long TimeoutNS, bool Recurring, IObject State)
        {
            return RegisterHandler(new PITHandler(Handler, State, TimeoutNS, Recurring));
        }
        [NoDebug]
        public int RegisterHandler(TimerHandler Handler, long TimeoutNS, bool Recurring, IObject State, uint TargetProcessId)
        {
            return RegisterHandler(new PITHandler(Handler, State, TimeoutNS, Recurring, true, TargetProcessId));
        }

        /// <summary>
        ///     Registers the specified handler for the timer 0 interrupt.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        /// <returns>The Id of the registered handler.</returns>
        [NoDebug]
        public int RegisterHandler(PITHandler handler)
        {
            if (handler.id != -1)
            {
                ExceptionMethods.Throw(new Exception("Timer has already been registered!"));
            }

            handler.id = TimerIdGenerator++;
            ActiveHandlers.Add(handler);

            return handler.id;
        }

        /// <summary>
        ///     Unregisters the handler with the specified id.
        /// </summary>
        /// <param name="HandlerId">The Id of the handler to unregister.</param>
        public override void UnregisterHandler(int HandlerId)
        {
            for (int i = 0; i < ActiveHandlers.Count; i++)
            {
                if (((PITHandler)ActiveHandlers[i]).id == HandlerId)
                {
                    ((PITHandler)ActiveHandlers[i]).id = -1;
                    ActiveHandlers.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        ///     The internal timer 0 interrupt handler.
        /// </summary>
        [NoDebug]
        [NoGC]
        public void InterruptHandler()
        {
            //if (Processes.ProcessManager.Processes.Count > 1)
            //    BasicConsole.WriteLine("PIT: 1");

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 2");
            //}

            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;
            
            uint T0Delay = T0DelyNS;
            PITHandler hndlr = null;
            for (int i = ActiveHandlers.Count - 1; i >= 0; i--)
            {
                //if (Processes.ProcessManager.Processes.Count > 1)
                //    BasicConsole.WriteLine("PIT: 2");

                hndlr = (PITHandler)ActiveHandlers[i];

                //if (Processes.ProcessManager.Processes.Count > 1)
                //    BasicConsole.WriteLine("PIT: 3");

                hndlr.NSRemaining -= T0Delay;

                //if (Processes.ProcessManager.Processes.Count > 1)
                //    BasicConsole.WriteLine("PIT: 4");

                if (hndlr.NSRemaining < T0Delay)
                {
                    //if (Processes.ProcessManager.Processes.Count > 1)
                    //    BasicConsole.WriteLine("PIT: 5");

                    if (hndlr.Recurring)
                    {
                        hndlr.NSRemaining = hndlr.NanosecondsTimeout;
                    }
                    else
                    {
                        hndlr.id = -1;

                        //if (Processes.ProcessManager.Processes.Count > 1)
                        //    BasicConsole.WriteLine("PIT: 6");

                        ActiveHandlers.RemoveAt(i);
                    }

                    //if (Processes.ProcessManager.Processes.Count > 1)
                    //    BasicConsole.WriteLine("PIT: 7");

                    if (hndlr.SwitchProcess)
                    {
                        ProcessManager.SwitchProcess(hndlr.ProcessId, ProcessManager.THREAD_DONT_CARE);
                        switched = true;
                    }
                    else if (switched)
                    {
                        ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
                        switched = false;
                    }

                    hndlr.HandleTrigger(hndlr.state);

                }
            }

            if (switched)
            {
                ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
            }
        }

        /// <summary>
        ///     Enables the PIT.
        /// </summary>
        public override void Enable()
        {
            if (!enabled)
            {
                //Ignore the process state for timer interrupts. Timer interrupts occur so frequently
                //  that to continually switch state would be massively inefficient. Also, switching
                //  state isn't necessary for the handlers queued in the timer.

                enabled = true;

                T0RateGen = true;
                T0Reload = _T0Reload;

                Interrupts.Interrupts.EnableIRQ(0);
            }
        }

        /// <summary>
        ///     Disables the PIT.
        /// </summary>
        public override void Disable()
        {
            if (enabled)
            {
                //TODO: This should be done through a DeviceManager.Deregister system call.
                //TODO: This needs un-commenting and fixing
                //DeviceManager.Devices.Remove(this);
                //As per requirements, set temp sote store of id to 0 to prevent
                //  accidental multiple removal.
                InterruptHandlerId = 0;
                enabled = false;

                Interrupts.Interrupts.DisableIRQ(0);
            }
        }

        /// <summary>
        ///     Initialises the (only) PIT device instance.
        /// </summary>
        public static void Init()
        {
            if (ThePIT == null)
            {
                ThePIT = new PIT();
            }
            ThePIT.Enable();
        }

        /// <summary>
        ///     Cleans up the (only) PIT device instance.
        /// </summary>
        public static void Clean()
        {
            if (ThePIT != null)
            {
                ThePIT.Disable();
            }
        }

        public void PlayNote(MusicalNote note, MusicalNoteValue duration, uint bpm)
        {
            uint dur_ms = (uint)duration*60*1000/(bpm*16);

            if (note == MusicalNote.Silent)
            {
                Wait(dur_ms);
            }
            else
            {
                PlaySound((int)note);
                Wait(dur_ms);
                MuteSound();
            }
        }
    }

    /// <summary>
    ///     Represents a PIT timer 0 handler.
    /// </summary>
    public class PITHandler : Object
    {
        /// <summary>
        ///     The method to call when the handler timeout expires.
        /// </summary>
        public TimerHandler HandleTrigger;

        /// <summary>
        ///     The Id of the handler.
        /// </summary>
        internal int id = -1;

        /// <summary>
        ///     The timeout for the handler in nanoseconds. Used as a reload value if the timer is recursive.
        /// </summary>
        public long NanosecondsTimeout;

        /// <summary>
        ///     The remaining time for this handler, in nanoseconds.
        /// </summary>
        internal long NSRemaining;

        /// <summary>
        ///     Whether the handler should recur or not.
        /// </summary>
        public bool Recurring;

        /// <summary>
        ///     The state object to use when caller the handler callback.
        /// </summary>
        internal IObject state;

        internal bool SwitchProcess;
        internal uint ProcessId;

        /// <summary>
        ///     The handler Id.
        /// </summary>
        public int Id
        {
            get { return id; }
        }

        /// <summary>
        ///     Initialises a new PIT handler.
        /// </summary>
        /// <param name="HandleOnTrigger">The method to call when the timeout expires.</param>
        /// <param name="aState">The state object to pass to the handler.</param>
        /// <param name="NanosecondsTimeout">The timeout, in nanoseconds.</param>
        /// <param name="Recurring">Whether the handler should repeat or not.</param>
        public PITHandler(TimerHandler HandleOnTrigger, IObject aState, long NanosecondsTimeout, bool Recurring, bool switchProcess = false, uint processId = 0)
        {
            HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            NSRemaining = this.NanosecondsTimeout;
            this.Recurring = Recurring;
            state = aState;
            SwitchProcess = switchProcess;
            ProcessId = processId;
        }

        /// <summary>
        ///     Initialises a recurring new PIT handler.
        /// </summary>
        /// <param name="HandleOnTrigger">The method to call when the timeout expires.</param>
        /// <param name="aState">The state object to pass to the handler.</param>
        /// <param name="NanosecondsTimeout">The timeout, in nanoseconds.</param>
        /// <param name="NanosecondsLeft">The intial timeout value, in nanoseconds.</param>
        public PITHandler(TimerHandler HandleOnTrigger, IObject aState, long NanosecondsTimeout, uint NanosecondsLeft)
        {
            HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            NSRemaining = NanosecondsLeft;
            Recurring = true;
            state = aState;
        }
    }
}