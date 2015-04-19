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

namespace Kernel.Core.Tasks
{
    public static unsafe class PlayNotesTask
    {
        public class NoteState : FOS_System.Object
        {
            public int dur_ms;
            public int handlerId;
        }
        public class NoteRequest : FOS_System.Object
        {
            public bool Handled = false;
            public Hardware.Timers.PIT.MusicalNote note;
            public Hardware.Timers.PIT.MusicalNoteValue duration;
            public int bpm;
        }

        public static Thread OwnerThread = null;
        public static bool Awake = true;

        public static bool Terminate = false;

        private static NoteRequest[] NoteRequests;
        static PlayNotesTask()
        {
            NoteRequests = new NoteRequest[256];
            for (int i = 0; i < NoteRequests.Length; i++)
            {
                NoteRequests[i] = new NoteRequest();
            }
        }

        public static void RequestNote(Hardware.Timers.PIT.MusicalNote note, Hardware.Timers.PIT.MusicalNoteValue duration, uint bpm)
        {
            //TODO

            if (OwnerThread != null)
            {
                OwnerThread._Wake();
            }
        }

        public static void Main()
        {
            OwnerThread = ProcessManager.CurrentThread;

            Thread.Sleep_Indefinitely();

            while (!Terminate)
            {
                //Scheduler.Disable();
                Awake = false;

                BasicConsole.WriteLine("Playing notes...");

                Hardware.Timers.PIT.MusicalNote note = Hardware.Timers.PIT.MusicalNote.C4;
                Hardware.Timers.PIT.MusicalNoteValue duration = Hardware.Timers.PIT.MusicalNoteValue.Minim;
                int bpm = 120;

                Hardware.Timers.PIT.ThePIT.PlaySound((int)note);

                int dur_ms = (int)duration * 60 * 1000 / (bpm * 16);
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

                BasicConsole.WriteLine("Finished playing notes.");

                if (!Awake)
                {
                    //BasicConsole.WriteLine("Sleeping non-critical interrupts thread...");

                    //Scheduler.Enable();
                    if (!Thread.Sleep_Indefinitely())
                    {
                        BasicConsole.SetTextColour(BasicConsole.error_colour);
                        BasicConsole.WriteLine("Failed to sleep play notes thread!");
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
                    }
                }
            }
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
                BasicConsole.WriteLine("Note muted.");
                Hardware.Timers.PIT.ThePIT.MuteSound();
                Hardware.Timers.PIT.ThePIT.UnregisterHandler(state.handlerId);
            }
        }
    }
}
