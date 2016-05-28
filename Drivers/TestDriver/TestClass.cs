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

//using Kernel.Shared;

namespace TestDriver
{
    public static class TestClass
    {
        public static int SemaphorHandle = -1;

        /// <summary>
        ///     This serves a purpose. We can't use while(true) on its own in any thread
        ///     methods because the compiler has a bug in the way it handles injection of
        ///     GC cleanup / exception handling.
        /// </summary>
        public static bool Terminating = false;

        /*
        [Drivers.Compiler.Attributes.MainMethod]
        public static unsafe void Test()
        {
            CallStaticConstructors();

            byte bpm = (byte)Drivers.Framework.Math.Add(140, 100);
            
            *((ushort*)0xB881E) = (0x1F00 | '1');
            KernelABI.SystemCalls.Sleep(1000);

            
            SemaphoreResponses response = SemaphoreResponses.INVALID;
            while (!Terminating)
            {
                *((ushort*)0xB881E) = (0x3F00 | '2');
                KernelABI.SystemCalls.Sleep(1000);

                if (SemaphorHandle == -1)
                {
                    *((ushort*)0xB881E) = (0x3F00 | '3');
                    KernelABI.SystemCalls.Sleep(1000);

                    if (response == SemaphoreResponses.INVALID)
                    {
                        *((ushort*)0xB881E) = (0x3F00 | '4');
                        KernelABI.SystemCalls.Sleep(1000);

                        response = KernelABI.SystemCalls.Semaphore_Allocate(1, ref SemaphorHandle);

                        if (response == SemaphoreResponses.Success)
                        {
                            if (KernelABI.SystemCalls.Thread_Create(ParallelTest) == ThreadResponses.Success)
                            {
                                *((ushort*)0xB881E) = (0x1F00 | '5');
                                KernelABI.SystemCalls.Sleep(1000);
                            }
                        }
                    }

                    *((ushort*)0xB881E) = (0x3F00 | '6');
                    KernelABI.SystemCalls.Sleep(1000);
                }

                *((ushort*)0xB881E) = (0x3F00 | '7');
                KernelABI.SystemCalls.Sleep(1000);

                if (response == SemaphoreResponses.Success)
                {
                    *((ushort*)0xB881E) = (0x3F00 | '8');
                    KernelABI.SystemCalls.Sleep(1000);

                    while (KernelABI.SystemCalls.Semaphore_Wait(SemaphorHandle) != SemaphoreResponses.Success)
                    {
                        *((ushort*)0xB881E) = (0x3F00 | '9');
                    }

                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.C4,
                        MusicalNoteValue.Quaver,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.Silent,
                        MusicalNoteValue.Minim,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.E4,
                        MusicalNoteValue.Quaver,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.Silent,
                        MusicalNoteValue.Minim,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.G4,
                        MusicalNoteValue.Quaver,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.Silent,
                        MusicalNoteValue.Minim,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.C5,
                        MusicalNoteValue.Minim,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.Silent,
                        MusicalNoteValue.Minim,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.G4,
                        MusicalNoteValue.Minim,
                        bpm);
                    KernelABI.SystemCalls.PlayNote(
                        MusicalNote.C5,
                        MusicalNoteValue.Minim,
                        bpm);

                    while (KernelABI.SystemCalls.Semaphore_Signal(SemaphorHandle) != SemaphoreResponses.Success)
                    {
                        *((ushort*)0xB881E) = (0x3F00 | 'A');
                    }
                }

                *((ushort*)0xB881E) = (0x3F00 | 'B');
                KernelABI.SystemCalls.Sleep(1000);
            }
        }

        public static unsafe void ParallelTest()
        {
            while (!Terminating)
            {
                *((ushort*)0xB881C) = (0x3F00 | '1');
                
                while (KernelABI.SystemCalls.Semaphore_Wait(SemaphorHandle) != SemaphoreResponses.Success)
                {
                    *((ushort*)0xB881C) = (0x3F00 | '2');
                }
                
                *((ushort*)0xB881C) = (0x3F00 | '3');
                KernelABI.SystemCalls.Sleep(10000);

                while (KernelABI.SystemCalls.Semaphore_Signal(SemaphorHandle) != SemaphoreResponses.Success)
                {
                    *((ushort*)0xB881C) = (0x3F00 | '4');
                }

                *((ushort*)0xB881C) = (0x3F00 | '5');
            }
        }

        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        [Drivers.Compiler.Attributes.HaltMethod]
        public static void Halt()
        {
        }
        */
    }
}