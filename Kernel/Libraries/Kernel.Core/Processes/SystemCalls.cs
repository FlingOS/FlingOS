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

namespace Kernel.Core.Processes
{
    public enum SystemCall : uint
    {
        INVALID = 0,
        Sleep = 1,
        PlayNote = 2
    }

    public static class SystemCalls
    {
        private static UInt32 sysCallNum = 0;
        private static UInt32 param1 = 0;
        private static UInt32 param2 = 0;
        private static UInt32 param3 = 0;
        public static UInt32 SysCallNumber
        {
            get
            {
                return sysCallNum;
            }
        }
        public static UInt32 Param1
        {
            get
            {
                return param1;
            }
        }
        public static UInt32 Param2
        {
            get
            {
                return param2;
            }
        }
        public static UInt32 Param3
        {
            get
            {
                return param3;
            }
        }
        public static UInt32 Return1
        {
            get
            {
                return ProcessManager.CurrentThread.EAXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.EAXFromInterruptStack = value;
            }
        }
        public static UInt32 Return2
        {
            get
            {
                return ProcessManager.CurrentThread.EBXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.EBXFromInterruptStack = value;
            }
        }
        public static UInt32 Return3
        {
            get
            {
                return ProcessManager.CurrentThread.ECXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.ECXFromInterruptStack = value;
            }
        }
        public static UInt32 Return4
        {
            get
            {
                return ProcessManager.CurrentThread.EDXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.EDXFromInterruptStack = value;
            }
        }


        private static int Int48HandlerId = 0;
        public static void Init()
        {
            if (Int48HandlerId == 0)
            {
                // We want to ignore process state so that we handle the interrupt in the context of
                //  the calling process.
                Int48HandlerId = Hardware.Interrupts.Interrupts.AddISRHandler(48, Int48, null, true);
            }
        }

        private static void Int48(FOS_System.Object state)
        {
            //Temp store because return value 1 is put in EAX
            sysCallNum = ProcessManager.CurrentThread.EAXFromInterruptStack;
            param1 = ProcessManager.CurrentThread.EBXFromInterruptStack;
            param2 = ProcessManager.CurrentThread.ECXFromInterruptStack;
            param3 = ProcessManager.CurrentThread.EDXFromInterruptStack;

            switch ((SystemCall)sysCallNum)
            {
                case SystemCall.INVALID:
                    Console.Default.WriteLine("Error! INVALID System Call made.");
                    break;
                case SystemCall.Sleep:
                    SysCall_Sleep((int)param1);
                    break;
                case SystemCall.PlayNote:
                    SysCall_PlayNote((Hardware.Timers.PIT.MusicalNote)param1, (Hardware.Timers.PIT.MusicalNoteValue)param2, param3);
                    break;
                default:
                    Console.Default.Write("Sys call ");
                    Console.Default.Write_AsDecimal(sysCallNum);
                    Console.Default.Write(" : ");
                    Console.Default.WriteLine(ProcessManager.CurrentProcess.Name);
                    Console.Default.WriteLine(((FOS_System.String)" > Param1: ") + Param1);
                    Console.Default.WriteLine(((FOS_System.String)" > Param2: ") + Param2);
                    Console.Default.WriteLine(((FOS_System.String)" > Param3: ") + Param3);
                    Console.Default.WriteLine(((FOS_System.String)" > Return: ") + ProcessManager.CurrentThread.EAXFromInterruptStack);
                    break;
            }

            // Prevent cross-contamination / this would count as a security consideration
            sysCallNum = 0;
            param1 = 0;
            param2 = 0;
            param3 = 0;
        }

        private static void SysCall_Sleep(int ms)
        {
            Thread.EnterSleep(ms);
            Scheduler.UpdateCurrentState();
        }

        public class NoteState : FOS_System.Object
        {
            public uint dur_ms;
            public int handlerId;
        }
        private static void SysCall_PlayNote(Hardware.Timers.PIT.MusicalNote note, Hardware.Timers.PIT.MusicalNoteValue duration, uint bpm)
        {
            Hardware.Timers.PIT.ThePIT.PlaySound((int)note);

            uint dur_ms = (uint)duration * 60 * 1000 / (bpm * 16);
            long do_ms = dur_ms;
            if (dur_ms >= 2000)
            {
                dur_ms -= 2000;
                do_ms = 2000;
            }
            else
            {
                dur_ms = 0;
            }
            NoteState state = new NoteState()
            {
                dur_ms = dur_ms
            };
            state.handlerId = Hardware.Timers.PIT.ThePIT.RegisterHandler(new Hardware.Timers.PITHandler(SysCall_StopNoteHandler, state, 1000000L * do_ms, true)); 
        }
        private static void SysCall_StopNoteHandler(FOS_System.Object objState)
        {
            NoteState state = (NoteState)objState;
            if (state.dur_ms >= 0)
            {
                state.dur_ms -= 2000;
            }
            else
            {
                Hardware.Timers.PIT.ThePIT.MuteSound();
                Hardware.Timers.PIT.ThePIT.UnregisterHandler(state.handlerId);
            }
        }
    }
}
