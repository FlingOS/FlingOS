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

using Drivers.Framework;

namespace KernelABI
{
    public static class SystemCalls
    {
        public enum Calls : uint
        {
            INVALID = 0,
            Sleep = 1,
            PlayNote = 2
        }

        #region Play Note

        public enum MusicalNote : int
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
            Semiquaver = 1,     //  1/16
            Quaver = 2,         //  1/8
            Crotchet = 4,       //  1/4
            Minim = 8,          //  1/2
            Semibreve = 16,     //  1
            Breve = 32,         //  2
            Longa = 64,         //  4
            Maxima = 128        //  8
        }

        #endregion

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=@"ASM\SystemCalls")]
        public static uint Call(Calls callNumber,
            uint Param1,
            uint Param2,
            uint Param3)
        {
            return 0;
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Sleep(uint ms)
        {
            Call(Calls.Sleep, ms, 0, 0);
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void PlayNote(MusicalNote note, MusicalNoteValue duration, uint bpm)
        {
            Call(Calls.PlayNote, (uint)note, (uint)duration, bpm);
        }
    }
}
