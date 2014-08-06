using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.Timers
{
    public class PIT : Devices.Timer
    {
        protected int InterruptHandlerId;
        protected IO.IOPort Command = new IO.IOPort(0x43);
        protected IO.IOPort Data0 = new IO.IOPort(0x40);
        protected IO.IOPort Data1 = new IO.IOPort(0x41);
        protected IO.IOPort Data2 = new IO.IOPort(0x42);
        protected IO.IOPort SpeakerPort = new IO.IOPort(0x61);

        private List ActiveHandlers = new List();
        
        public PIT()
            : base()
        {
        }
        
        private ushort _T0Reload = 65535; //2048;     //Produces ~1.72ms delay between interrupts
        private ushort _T2Reload = 65535;
        private int TimerCounter = 0;
        private bool WaitSignaled = false;
        public const uint PITFrequency = 1193180;
        public const uint PITDelayNS = 838;
        public bool T0RateGen = false;

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

        public void EnableSound()
        {
            SpeakerPort.Write_Byte((byte)(SpeakerPort.Read_Byte() | 0x03));
        }
        public void DisableSound()
        {
            SpeakerPort.Write_Byte((byte)(SpeakerPort.Read_Byte() & 0xFC));
        }
        public void PlaySound(int aFreq)
        {
            T2Frequency = (uint)aFreq;
            EnableSound();
        }
        public void MuteSound()
        {
            DisableSound();
        }

        private static void SignalWait(FOS_System.Object state)
        {
            ((PIT)state).SignalWait();
        }
        private void SignalWait()
        {
            WaitSignaled = true;
        }

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
        public override void WaitNS(long TimeoutNS)
        {
            WaitSignaled = false;

            RegisterHandler(new PITHandler(PIT.SignalWait, this, TimeoutNS, false));

            while (!WaitSignaled)
            {
                Devices.CPU.Default.Halt();
            }
        }

        public int RegisterHandler(PITHandler timer)
        {
            if (timer.id != -1)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Timer has already been registered!"));
            }

            timer.id = (TimerCounter++);
            ActiveHandlers.Add(timer);

            return timer.id;
        }
        public void UnregisterHandler(int timerid)
        {
            for (int i = 0; i < ActiveHandlers.Count; i++)
            {
                if (((PITHandler)ActiveHandlers[i]).id == timerid)
                {
                    ((PITHandler)ActiveHandlers[i]).id = -1;
                    ActiveHandlers.RemoveAt(i);
                    return;
                }
            }
        }

        protected static void InterruptHandler(FOS_System.Object state)
        {
            ((PIT)state).InterruptHandler();
        }
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
        public override void Disable()
        {
            if (enabled)
            {
                DeviceManager.Devices.Remove(this);
                Interrupts.Interrupts.RemoveIRQHandler(0, InterruptHandlerId);
                enabled = false;
            }
        }

        public static PIT ThePIT;
        public static void Init()
        {
            if(ThePIT == null)
            {
                ThePIT = new PIT();
            }
            ThePIT.Enable();
        }
        public static void Clean()
        {
            if(ThePIT != null)
            {
                ThePIT.Disable();
            }
        }
    }
    public class PITHandler : FOS_System.Object
    {
        internal long NSRemaining;
        public long NanosecondsTimeout;
        public bool Recurring;
        internal int id = -1;
        internal FOS_System.Object state;

        public int ID
        {
            get
            {
                return id;
            }
        }

        public delegate void dOnTrigger(FOS_System.Object state);
        public dOnTrigger HandleTrigger;

        public PITHandler(dOnTrigger HandleOnTrigger, FOS_System.Object aState, long NanosecondsTimeout, bool Recurring)
        {
            this.HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            this.NSRemaining = this.NanosecondsTimeout;
            this.Recurring = Recurring;
            this.state = aState;
        }
        public PITHandler(dOnTrigger HandleOnTrigger, FOS_System.Object aState, long NanosecondsTimeout, uint NanosecondsLeft)
        {
            this.HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            this.NSRemaining = NanosecondsLeft;
            this.Recurring = true;
            this.state = aState;
        }
    }
}
