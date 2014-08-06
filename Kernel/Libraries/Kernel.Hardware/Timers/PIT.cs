using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.Timers
{
    /// <summary>
    /// Represents the programmable interval timer.
    /// </summary>
    public class PIT : Devices.Timer
    {
        /// <summary>
        /// The interrupt handler Id returned when the interrupt handler is set.
        /// Use to remove the interrupt handler when disabling.
        /// </summary>
        protected int InterruptHandlerId;
        /// <summary>
        /// The PIT command port.
        /// </summary>
        protected IO.IOPort Command = new IO.IOPort(0x43);
        /// <summary>
        /// The PIT Data0 port used to send data about timer 0.
        /// </summary>
        protected IO.IOPort Data0 = new IO.IOPort(0x40);
        /*<summary>
        /// The PIT Data1 port used to send data about timer 1.
        /// Timer 1 is outdataed and "probably doesn't exist on most modern hardware".
        /// Hence this port is pointless and so commented out.
        /// </summary>
        //protected IO.IOPort Data1 = new IO.IOPort(0x41);*/

        /// <summary>
        /// The PIT Data2 port used to send data about timer 2.
        /// Timer 2 is used to drive the PC speaker e.g. speaker beep.
        /// </summary>
        protected IO.IOPort Data2 = new IO.IOPort(0x42);
        /// <summary>
        /// The PC speaker port used to turn it on/off.
        /// </summary>
        protected IO.IOPort SpeakerPort = new IO.IOPort(0x61);

        /// <summary>
        /// Currently active handlers for when the timer interrupt occurs.
        /// </summary>
        private List ActiveHandlers = new List();
        
        /// <summary>
        /// The reload value for timer 0. Sets the frequency of timer 0.
        /// </summary>
        private ushort _T0Reload = 65535; //2048;     //Produces ~1.72ms delay between interrupts
        /// <summary>
        /// The reload value for timer 2. Sets the frequency of timer 2 hence 
        /// the frequency of the PC speaker beep.
        /// </summary>
        private ushort _T2Reload = 65535;
        /// <summary>
        /// Incremented endlessly to create a unique timer id.
        /// </summary>
        private int TimerIdGenerator = 0;
        /// <summary>
        /// Set to true when the current wait timer elapses.
        /// </summary>
        private bool WaitSignaled = false;
        /// <summary>
        /// The known frequency at which the PIT decrements the timer counters (reload values).
        /// </summary>
        public const uint PITFrequency = 1193180;
        /// <summary>
        /// The known time between PIT decrements of the timer counters (reload values).
        /// </summary>
        public const uint PITDelayNS = 838;
        /// <summary>
        /// Whether timer 0 should be in rate generator mode or not. Set to true to 
        /// have timer 0 reload from the reload value after the count hits 0.
        /// Essentially, set to true to make timer 0 repeat endlessly.
        /// </summary>
        public bool T0RateGen = false;

        /// <summary>
        /// The timer 0 reload value.
        /// </summary>
        public ushort T0Reload
        {
            get
            {
                return _T0Reload;
            }
            set
            {
                _T0Reload = value;

                Command.Write_Byte((byte)(T0RateGen ? 0x34 : 0x30));
                Data0.Write_Byte((byte)(value & 0xFF));
                Data0.Write_Byte((byte)(value >> 8));
            }
        }
        /// <summary>
        /// The timer 0 frequency.
        /// </summary>
        public uint T0Frequency
        {
            get
            {
                return (PITFrequency / ((uint)_T0Reload));
            }
            set
            {
                if (value < 19 || value > 1193180)
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException(
                        "Frequency must be between 19 and 1193180!"));
                }

                T0Reload = (ushort)(PITFrequency / value);
            }
        }
        /// <summary>
        /// The timer 0 delay in nanoseconds.
        /// </summary>
        public uint T0DelyNS
        {
            get
            {
                return (PITDelayNS * _T0Reload);
            }
            set
            {
                if (value > 54918330)
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException(
                        "Delay must be no greater that 54918330"));
                }

                T0Reload = (ushort)(value / PITDelayNS);
            }
        }

        /// <summary>
        /// The timer 2 reload value.
        /// </summary>
        public ushort T2Reload
        {
            get
            {
                return _T2Reload;
            }
            set
            {
                _T2Reload = value;

                Command.Write_Byte(0xB6);
                Data2.Write_Byte((byte)(value & 0xFF));
                Data2.Write_Byte((byte)(value >> 8));
            }
        }
        /// <summary>
        /// The timer 2 frequency.
        /// </summary>
        public uint T2Frequency
        {
            get
            {
                return (PITFrequency / ((uint)_T2Reload));
            }
            set
            {
                if (value < 19 || value > 1193180)
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException(
                        "Frequency must be between 19 and 1193180!"));
                }

                T2Reload = (ushort)(PITFrequency / value);
            }
        }
        /// <summary>
        /// The timer 2 delay in nanoseconds.
        /// </summary>
        public uint T2DelyNS
        {
            get
            {
                return (PITDelayNS * _T2Reload);
            }
            set
            {
                if (value > 54918330)
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException(
                        "Delay must be no greater than 54918330"));
                }

                T2Reload = (ushort)(value / PITDelayNS);
            }
        }

        /// <summary>
        /// Enables the PC speaker sound.
        /// </summary>
        public void EnableSound()
        {
            SpeakerPort.Write_Byte((byte)(SpeakerPort.Read_Byte() | 0x03));
        }
        /// <summary>
        /// Disables the PC speaker sound.
        /// </summary>
        public void DisableSound()
        {
            SpeakerPort.Write_Byte((byte)(SpeakerPort.Read_Byte() & 0xFC));
        }
        /// <summary>
        /// Plays the PC speaker at the specified frequency (tone).
        /// </summary>
        /// <param name="aFreq">The frequency to play.</param>
        public void PlaySound(int aFreq)
        {
            T2Frequency = (uint)aFreq;
            EnableSound();
        }
        /// <summary>
        /// Mutes the PC speaker.
        /// </summary>
        public void MuteSound()
        {
            DisableSound();
        }

        /// <summary>
        /// The internal wait interrupt handler static wrapper.
        /// </summary>
        /// <param name="state">The PIT object state.</param>
        private static void SignalWait(FOS_System.Object state)
        {
            ((PIT)state).SignalWait();
        }
        /// <summary>
        /// The internal wait interrupt handler.
        /// </summary>
        private void SignalWait()
        {
            WaitSignaled = true;
        }

        /// <summary>
        /// Blocks the caller for the specified length of time (in milliseconds).
        /// </summary>
        /// <param name="TimeoutMS">The length of time to wait in milliseconds.</param>
        public override void Wait(uint TimeoutMS)
        {
            //TODO Remove this hack.
            //  - 03/08/2014 : This hack is a solution for 64-64 multiplication and/or subtraction not working.
            while (TimeoutMS >= 2000)
            {
                WaitNS(1000000L * 2000L);
                TimeoutMS -= 2000;
            }
            WaitNS(1000000L * TimeoutMS);
        }
        /// <summary>
        /// Blocks the caller for the specified length of time (in nanoseconds).
        /// </summary>
        /// <param name="TimeoutMS">The length of time to wait in nanoseconds.</param>
        public override void WaitNS(long TimeoutNS)
        {
            WaitSignaled = false;

            RegisterHandler(new PITHandler(PIT.SignalWait, this, TimeoutNS, false));

            while (!WaitSignaled)
            {
                Devices.CPU.Default.Halt();
            }
        }

        /// <summary>
        /// Registers the specified handler for the timer 0 interrupt.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        /// <returns>The Id of the registered handler.</returns>
        public int RegisterHandler(PITHandler handler)
        {
            if (handler.id != -1)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Timer has already been registered!"));
            }

            handler.id = (TimerIdGenerator++);
            ActiveHandlers.Add(handler);

            return handler.id;
        }
        /// <summary>
        /// Unregisters the handler with the specified id.
        /// </summary>
        /// <param name="handlerId">The Id of the handler to unregister.</param>
        public void UnregisterHandler(int handlerId)
        {
            for (int i = 0; i < ActiveHandlers.Count; i++)
            {
                if (((PITHandler)ActiveHandlers[i]).id == handlerId)
                {
                    ((PITHandler)ActiveHandlers[i]).id = -1;
                    ActiveHandlers.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// The internal timer 0 interrupt handler static wrapper.
        /// </summary>
        /// <param name="state">The PIT object state.</param>
        protected static void InterruptHandler(FOS_System.Object state)
        {
            ((PIT)state).InterruptHandler();
        }
        /// <summary>
        /// The internal timer 0 interrupt handler.
        /// </summary>
        protected void InterruptHandler()
        {
            uint T0Delay = T0DelyNS;
            PITHandler hndlr = null;
            for (int i = ActiveHandlers.Count - 1; i >= 0; i--)
            {
                hndlr = (PITHandler)ActiveHandlers[i];

                hndlr.NSRemaining -= T0Delay;
                
                if (hndlr.NSRemaining < T0Delay)
                {
                    if (hndlr.Recurring)
                    {
                        hndlr.NSRemaining = hndlr.NanosecondsTimeout;
                    }
                    else
                    {
                        hndlr.id = -1;
                        ActiveHandlers.RemoveAt(i);
                    }
                    hndlr.HandleTrigger(hndlr.state);
                }
            }
        }

        /// <summary>
        /// Enables the PIT.
        /// </summary>
        public override void Enable()
        {
            if (!enabled)
            {
                InterruptHandlerId = Interrupts.Interrupts.SetIRQHandler(0, InterruptHandler, this);
                DeviceManager.Devices.Add(this);
                enabled = true;
                
                T0RateGen = true;
                T0Reload = _T0Reload;
            }
        }
        /// <summary>
        /// Disables the PIT.
        /// </summary>
        public override void Disable()
        {
            if (enabled)
            {
                DeviceManager.Devices.Remove(this);
                Interrupts.Interrupts.RemoveIRQHandler(0, InterruptHandlerId);
                enabled = false;
            }
        }

        /// <summary>
        /// The (only) PIT device instance.
        /// </summary>
        public static PIT ThePIT;
        /// <summary>
        /// Initialises the (only) PIT device instance.
        /// </summary>
        public static void Init()
        {
            if(ThePIT == null)
            {
                ThePIT = new PIT();
            }
            ThePIT.Enable();
        }
        /// <summary>
        /// Cleans up the (only) PIT device instance.
        /// </summary>
        public static void Clean()
        {
            if(ThePIT != null)
            {
                ThePIT.Disable();
            }
        }
    }
    /// <summary>
    /// Represents a PIT timer 0 handler.
    /// </summary>
    public class PITHandler : FOS_System.Object
    {
        /// <summary>
        /// The remaining time for this handler, in nanoseconds.
        /// </summary>
        internal long NSRemaining;
        /// <summary>
        /// The timeout for the handler in nanoseconds. Used as a reload value if the timer is recursive.
        /// </summary>
        public long NanosecondsTimeout;
        /// <summary>
        /// Whether the handler should recur or not.
        /// </summary>
        public bool Recurring;
        /// <summary>
        /// The Id of the handler.
        /// </summary>
        internal int id = -1;
        /// <summary>
        /// The state object to use when caller the handler callback.
        /// </summary>
        internal FOS_System.Object state;

        /// <summary>
        /// The handler Id.
        /// </summary>
        public int Id
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// Represents a method for a handler to call. The method to call must be static.
        /// </summary>
        /// <param name="state">The state object.</param>
        public delegate void OnTriggerDelegate(FOS_System.Object state);
        /// <summary>
        /// The method to call when the handler timeout expires.
        /// </summary>
        public OnTriggerDelegate HandleTrigger;

        /// <summary>
        /// Initialises a new PIT handler.
        /// </summary>
        /// <param name="HandleOnTrigger">The method to call when the timeout expires.</param>
        /// <param name="aState">The state object to pass to the handler.</param>
        /// <param name="NanosecondsTimeout">The timeout, in nanoseconds.</param>
        /// <param name="Recurring">Whether the handler should repeat or not.</param>
        public PITHandler(OnTriggerDelegate HandleOnTrigger, FOS_System.Object aState, long NanosecondsTimeout, bool Recurring)
        {
            this.HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            this.NSRemaining = this.NanosecondsTimeout;
            this.Recurring = Recurring;
            this.state = aState;
        }
        /// <summary>
        /// Initialises a recurring new PIT handler.
        /// </summary>
        /// <param name="HandleOnTrigger">The method to call when the timeout expires.</param>
        /// <param name="aState">The state object to pass to the handler.</param>
        /// <param name="NanosecondsTimeout">The timeout, in nanoseconds.</param>
        /// <param name="NanosecondsLeft">The intial timeout value, in nanoseconds.</param>
        public PITHandler(OnTriggerDelegate HandleOnTrigger, FOS_System.Object aState, long NanosecondsTimeout, uint NanosecondsLeft)
        {
            this.HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            this.NSRemaining = NanosecondsLeft;
            this.Recurring = true;
            this.state = aState;
        }
    }
}
