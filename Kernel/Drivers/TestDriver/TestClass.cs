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
    
using System;
using Drivers.Framework;
using KernelABI;

namespace TestDriver
{
    public static class TestClass
    {
        [Drivers.Compiler.Attributes.MainMethod]
        public static unsafe void Test()
        {
            byte bpm = (byte)Drivers.Framework.Math.Add(140, 100);

            int semaphorHandle = -1;
            SystemCalls.SemaphoreResponses response = SystemCalls.SemaphoreResponses.INVALID;
            while (true)
            {
                *((ushort*)0xB881E) = (0x1F00 | '3');
                SystemCalls.Sleep(1000);
                *((ushort*)0xB881E) = (0x3F00 | '4');
                SystemCalls.Sleep(1000);

                if (semaphorHandle == -1)
                {
                    *((ushort*)0xB881E) = (0x3F00 | '5');
                    SystemCalls.Sleep(1000);

                    if (response == SystemCalls.SemaphoreResponses.INVALID)
                    {
                        *((ushort*)0xB881E) = (0x3F00 | '6');
                        SystemCalls.Sleep(1000);
                        
                        response = SystemCalls.AllocateSemaphore(5, ref semaphorHandle);
                    }

                    *((ushort*)0xB881E) = (0x3F00 | '7');
                    SystemCalls.Sleep(1000);
                }

                *((ushort*)0xB881E) = (0x3F00 | '8');
                SystemCalls.Sleep(1000);

                if (response == SystemCalls.SemaphoreResponses.Success)
                {
                    *((ushort*)0xB881E) = (0x3F00 | '9');
                    SystemCalls.Sleep(1000);

                    //bpm += 1;z`
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.C4,
                        SystemCalls.MusicalNoteValue.Quaver,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.Silent,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.E4,
                        SystemCalls.MusicalNoteValue.Quaver,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.Silent,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.G4,
                        SystemCalls.MusicalNoteValue.Quaver,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.Silent,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.C5,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.Silent,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.G4,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);
                    SystemCalls.PlayNote(
                        SystemCalls.MusicalNote.C5,
                        SystemCalls.MusicalNoteValue.Minim,
                        bpm);

                    *((ushort*)0xB881E) = (0x3F00 | 'A');
                    SystemCalls.Sleep(1000);
                }

                *((ushort*)0xB881E) = (0x3F00 | 'B');
                SystemCalls.Sleep(1000);
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
    }
}
