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

/*
    Note: This class is totally out of date and probably won't function properly in the modern system

using Kernel.Devices.Timers;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Multiprocessing;

namespace Kernel.Devices.Tasks
{
    public static class PlayNotesTask
    {

        public static Thread OwnerThread;
        public static bool Awake = true;

        public static bool Terminate = false;

        private static readonly CircularBuffer LiveNoteRequests;
        private static readonly CircularBuffer DeadNoteRequests;

        private static bool Playing;

        static PlayNotesTask()
        {
            DeadNoteRequests = new CircularBuffer(256, false);
            LiveNoteRequests = new CircularBuffer(DeadNoteRequests.Blocks, false);
            for (int i = 0; i < DeadNoteRequests.Blocks; i++)
            {
                DeadNoteRequests.Push(new NoteRequest());
            }
        }

        public static void RequestNote(PIT.MusicalNote note, PIT.MusicalNoteValue duration, uint bpm)
        {
            NoteRequest next = (NoteRequest) DeadNoteRequests.Pop();
            if (next == null)
            {
                BasicConsole.WriteLine(
                    "Cannot set note request because a null object was returned from the circular buffer. Buffer may be full.");
                BasicConsole.DelayOutput(10);
            }
            else
            {
                next.note = note;
                next.duration = duration;
                next.bpm = (int) bpm;

                LiveNoteRequests.Push(next);

                if (OwnerThread != null)
                {
                    OwnerThread._Wake();
                }
            }
        }

        public static void Main()
        {
            OwnerThread = ProcessManager.CurrentThread;

            SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);

            while (!Terminate)
            {
                //Scheduler.Disable();
                Awake = false;

                //BasicConsole.WriteLine("Playing notes...");

                while (LiveNoteRequests.Count > 0)
                {
                    //BasicConsole.WriteLine("Playing note...");

                    NoteRequest theReq = (NoteRequest) LiveNoteRequests.Pop();

                    try
                    {
                        PIT.MusicalNote note = theReq.note;
                        PIT.MusicalNoteValue duration = theReq.duration;
                        int bpm = theReq.bpm;

                        int dur_ms = (int) duration*60*1000/(bpm*16);
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
                        NoteState state = new NoteState
                        {
                            dur_ms = dur_ms
                        };

                        if (note != PIT.MusicalNote.Silent)
                        {
                            PIT.ThePIT.PlaySound((int) note);
                        }

                        Playing = true;

                        state.handlerId =
                            PIT.ThePIT.RegisterHandler(new PITHandler(SysCall_StopNoteHandler, state, 1000000L*do_ms,
                                true));

                        while (Playing)
                        {
                            SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);
                        }
                    }
                    catch
                    {
                        BasicConsole.Write("Error processing note request! ");
                        if (ExceptionMethods.CurrentException != null)
                        {
                            BasicConsole.Write(ExceptionMethods.CurrentException.Message);
                        }
                        BasicConsole.WriteLine();
                        Playing = false;
                        PIT.ThePIT.MuteSound();
                        BasicConsole.DelayOutput(15);
                    }
                    finally
                    {
                        DeadNoteRequests.Push(theReq);
                    }
                }

                //BasicConsole.WriteLine("Finished playing notes.");

                if (!Awake)
                {
                    //BasicConsole.WriteLine("Sleeping non-critical interrupts thread...");

                    //Scheduler.Enable();
                    if (SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread) != SystemCallResults.OK)
                    {
                        BasicConsole.SetTextColour(BasicConsole.error_colour);
                        BasicConsole.WriteLine("Failed to sleep play notes thread!");
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
                    }
                }
            }
        }

        private static void SysCall_StopNoteHandler(IObject objState)
        {
            NoteState state = (NoteState) objState;
            if (state.dur_ms >= 0)
            {
                state.dur_ms -= 2000;
            }
            else
            {
                //BasicConsole.WriteLine("Note muted.");

                Playing = false;

                PIT.ThePIT.MuteSound();
                PIT.ThePIT.UnregisterHandler(state.handlerId);
            }
        }

        public class NoteState : Object
        {
            public int dur_ms;
            public int handlerId;
        }

        public class NoteRequest : Object
        {
            public int bpm;
            public PIT.MusicalNoteValue duration;
            public PIT.MusicalNote note;
        }
    }  
}
*/
