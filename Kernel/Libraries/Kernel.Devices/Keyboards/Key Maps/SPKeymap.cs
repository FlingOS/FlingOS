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

using Kernel.Framework.Collections;

// ReSharper disable once CheckNamespace
namespace Kernel.Devices.Keyboards
{
#if SPKEYBOARD
    public abstract partial class Keyboard
    {
        /// <summary>
        ///     Creates the Spanish keyboard mapping
        /// </summary>
        private void CreateSPKeymap()
        {
            //  This creates (most/some of) a Spain keyboard mapping.
            //  You can go look up scancodes / characters etc. for other
            //  keyboards if you like.
            BasicConsole.WriteLine("Using Spanish Keyboard layout.");
            //BasicConsole.DelayOutput(3);
            KeyMappings = new List(164);

            //TODO: Fn key
            //TODO: Full numpad?
            //TODO: Alt special symbols

#region Letters

            AddKey(0x10, 'q', KeyboardKey.Q);
            AddKey(0x100000, 'Q', KeyboardKey.Q);
            AddKey(0x11, 'w', KeyboardKey.W);
            AddKey(0x110000, 'W', KeyboardKey.W);
            AddKey(0x12, 'e', KeyboardKey.E);
            AddKey(0x120000, 'E', KeyboardKey.E);
            AddKey(0x13, 'r', KeyboardKey.R);
            AddKey(0x130000, 'R', KeyboardKey.R);
            AddKey(0x14, 't', KeyboardKey.T);
            AddKey(0x140000, 'T', KeyboardKey.T);
            AddKey(0x15, 'y', KeyboardKey.Y);
            AddKey(0x150000, 'Y', KeyboardKey.Y);
            AddKey(0x16, 'u', KeyboardKey.U);
            AddKey(0x160000, 'U', KeyboardKey.U);
            AddKey(0x17, 'i', KeyboardKey.I);
            AddKey(0x170000, 'I', KeyboardKey.I);
            AddKey(0x18, 'o', KeyboardKey.O);
            AddKey(0x180000, 'O', KeyboardKey.O);
            AddKey(0x19, 'p', KeyboardKey.P);
            AddKey(0x190000, 'P', KeyboardKey.P);

            AddKey(0x1E, 'a', KeyboardKey.A);
            AddKey(0x1E0000, 'A', KeyboardKey.A);
            AddKey(0x1F, 's', KeyboardKey.S);
            AddKey(0x1F0000, 'S', KeyboardKey.S);
            AddKey(0x20, 'd', KeyboardKey.D);
            AddKey(0x200000, 'D', KeyboardKey.D);
            AddKey(0x21, 'f', KeyboardKey.F);
            AddKey(0x210000, 'F', KeyboardKey.F);
            AddKey(0x22, 'g', KeyboardKey.G);
            AddKey(0x220000, 'G', KeyboardKey.G);
            AddKey(0x23, 'h', KeyboardKey.H);
            AddKey(0x230000, 'H', KeyboardKey.H);
            AddKey(0x24, 'j', KeyboardKey.J);
            AddKey(0x240000, 'J', KeyboardKey.J);
            AddKey(0x25, 'k', KeyboardKey.K);
            AddKey(0x250000, 'K', KeyboardKey.K);
            AddKey(0x26, 'l', KeyboardKey.L);
            AddKey(0x260000, 'L', KeyboardKey.L);
            AddKey(0x27, 'ñ', KeyboardKey.NoName);
            AddKey(0x270000, 'Ñ', KeyboardKey.NoName);

            AddKey(0x2C, 'z', KeyboardKey.Z);
            AddKey(0x2C0000, 'Z', KeyboardKey.Z);
            AddKey(0x2D, 'x', KeyboardKey.X);
            AddKey(0x2D0000, 'X', KeyboardKey.X);
            AddKey(0x2E, 'c', KeyboardKey.C);
            AddKey(0x2E0000, 'C', KeyboardKey.C);
            AddKey(0x2F, 'v', KeyboardKey.V);
            AddKey(0x2F0000, 'V', KeyboardKey.V);
            AddKey(0x30, 'b', KeyboardKey.B);
            AddKey(0x300000, 'B', KeyboardKey.B);
            AddKey(0x31, 'n', KeyboardKey.N);
            AddKey(0x310000, 'N', KeyboardKey.N);
            AddKey(0x32, 'm', KeyboardKey.M);
            AddKey(0x320000, 'M', KeyboardKey.M);

#endregion

#region Digits

            AddKey(0x29, 'º', KeyboardKey.NoName);
            AddKey(0x290000, 'ª', KeyboardKey.NoName);
            AddKey(0x2, '1', KeyboardKey.D1);
            AddKey(0x20000, '!', KeyboardKey.D1);
            AddKey(0x3, '2', KeyboardKey.D2);
            AddKey(0x30000, '"', KeyboardKey.D2);
            AddKey(0x4, '3', KeyboardKey.D3);
            AddKey(0x40000, '·', KeyboardKey.D3);
            AddKey(0x5, '4', KeyboardKey.D4);
            AddKey(0x50000, '$', KeyboardKey.D5);
            AddKey(0x6, '5', KeyboardKey.D5);
            AddKey(0x60000, '%', KeyboardKey.D5);
            AddKey(0x7, '6', KeyboardKey.D6);
            AddKey(0x70000, '&', KeyboardKey.D6);
            AddKey(0x8, '7', KeyboardKey.D7);
            AddKey(0x80000, '/', KeyboardKey.D7);
            AddKey(0x9, '8', KeyboardKey.D8);
            AddKey(0x90000, '(', KeyboardKey.D8);
            AddKey(0xA, '9', KeyboardKey.D9);
            AddKey(0xA0000, ')', KeyboardKey.D9);
            AddKey(0xB, '0', KeyboardKey.D0);
            AddKey(0xB0000, '=', KeyboardKey.D0);

#endregion

#region Special

            AddKeyWithAndWithoutShift(0x0E, '\b', KeyboardKey.Backspace); // Backspace
            AddKeyWithAndWithoutShift(0x0F, '\t', KeyboardKey.Tab); // Tabulator
            AddKeyWithAndWithoutShift(0x1C, '\n', KeyboardKey.Enter); // Enter
            AddKeyWithAndWithoutShift(0x39, ' ', KeyboardKey.Spacebar); // Space
            AddKeyWithAndWithoutShift(0x4b, '\u2190', KeyboardKey.LeftArrow); // Left arrow
            AddKeyWithAndWithoutShift(0x48, '\u2191', KeyboardKey.UpArrow); // Up arrow
            AddKeyWithAndWithoutShift(0x4d, '\u2192', KeyboardKey.RightArrow); // Right arrow
            AddKeyWithAndWithoutShift(0x50, '\u2193', KeyboardKey.DownArrow); // Down arrow

            AddKeyWithShift(0x5b, KeyboardKey.LeftWindows);
            AddKeyWithShift(0x5c, KeyboardKey.RightWindows);
            //AddKey(0x5d, KeyboardKey.NoName);                                   // Context Menu  

            AddKeyWithShift(0x52, KeyboardKey.Insert);
            AddKeyWithShift(0x47, KeyboardKey.Home);
            AddKeyWithShift(0x49, KeyboardKey.PageUp);
            AddKeyWithShift(0x53, KeyboardKey.Delete);
            AddKeyWithShift(0x4f, KeyboardKey.End);
            AddKeyWithShift(0x51, KeyboardKey.PageDown);

            AddKeyWithShift(0x37, KeyboardKey.PrintScreen);
            //AddKeyWithShift(0x46, KeyboardKey.NoName);                          // Scroll Lock
            //AddKeyWithShift(0x3a, KeyboardKey.NoName);                          // Caps Lock
            AddKeyWithShift(0x45, KeyboardKey.Pause);

            AddKeyWithShift(0x3b, KeyboardKey.F1);
            AddKeyWithShift(0x3c, KeyboardKey.F2);
            AddKeyWithShift(0x3d, KeyboardKey.F3);
            AddKeyWithShift(0x3e, KeyboardKey.F4);
            AddKeyWithShift(0x3f, KeyboardKey.F5);
            AddKeyWithShift(0x40, KeyboardKey.F6);
            AddKeyWithShift(0x41, KeyboardKey.F7);
            AddKeyWithShift(0x42, KeyboardKey.F8);
            AddKeyWithShift(0x43, KeyboardKey.F9);
            AddKeyWithShift(0x44, KeyboardKey.F10);
            AddKeyWithShift(0x57, KeyboardKey.F11);
            AddKeyWithShift(0x58, KeyboardKey.F12);

            AddKeyWithShift(0x1, KeyboardKey.Escape);

#endregion

#region Punctuation and Signs

            AddKey(0x2B, 'ç', KeyboardKey.NoName);
            AddKey(0x2B0000, 'Ç', KeyboardKey.NoName);
            //AddKey(0x29, '`', KeyboardKey.NoName);
            //AddKey(0x290000, '~', KeyboardKey.NoName);
            AddKey(0x33, ',', KeyboardKey.OemComma);
            AddKey(0x330000, ';', KeyboardKey.OemComma);
            AddKey(0x34, '.', KeyboardKey.OemPeriod);
            AddKey(0x340000, ':', KeyboardKey.OemPeriod);
            AddKey(0x35, '-', KeyboardKey.Divide);
            AddKey(0x350000, '_', KeyboardKey.Divide);
            AddKey(0x0C, '\'', KeyboardKey.Subtract);
            AddKey(0x0C0000, '?', KeyboardKey.Subtract);
            AddKey(0x0D, '¡', KeyboardKey.OemPlus);
            AddKey(0x0D0000, '¿', KeyboardKey.OemPlus);
            AddKey(0x1A, '`', KeyboardKey.NoName);
            AddKey(0x1A0000, '^', KeyboardKey.NoName);
            AddKey(0x1B, '+', KeyboardKey.NoName);
            AddKey(0x1B0000, '*', KeyboardKey.NoName);

            AddKeyWithAndWithoutShift(0x4c, '5', KeyboardKey.NumPad5);

            AddKeyWithAndWithoutShift(0x4a, '-', KeyboardKey.OemMinus);
            AddKeyWithAndWithoutShift(0x4e, '+', KeyboardKey.OemPlus);

            AddKeyWithAndWithoutShift(0x37, '*', KeyboardKey.Multiply);

#endregion
        }
    }
#endif
}