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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Drivers.Framework;

namespace KernelABI
{
    public static class TestClass
    {
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        static TestClass()
        {
            //Drivers.Framework.Type x;
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.MainMethod]
        public static unsafe void Test()
        {
            //int x = 0xF;
            //int y = 0xF;
            //int z = x < y ? x : y;

            byte bpm = 180;
            while (true)
            {
                *((ushort*)0xB881E) = (0x1F00 | '3');
                SystemCalls.Sleep(1000);
                *((ushort*)0xB881E) = (0x3F00 | '4');
                SystemCalls.Sleep(1000);

                //bpm += 1;
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
                
            }
        }
        
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.HaltMethod]
        public static void Halt()
        {
        }
    }
}
