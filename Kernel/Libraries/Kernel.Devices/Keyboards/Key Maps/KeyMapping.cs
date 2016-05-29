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

using Kernel.Framework;

// ReSharper disable once CheckNamespace
namespace Kernel.Devices.Keyboards
{
    /// <summary>
    ///     Represents a key mapping that maps a scancode to a character and a keyboard key.
    /// </summary>
    public class KeyMapping : Object
    {
        /// <summary>
        ///     The keyboard key to represent the scancode.
        /// </summary>
        public KeyboardKey Key;

        /// <summary>
        ///     The scancode to map.
        /// </summary>
        public uint Scancode;

        /// <summary>
        ///     The character to represent the scancode.
        /// </summary>
        public char Value;

        /// <summary>
        ///     Initialises a new key mapping.
        /// </summary>
        /// <param name="Scancode">The scancode to map.</param>
        /// <param name="Value">The character to represent the scancode.</param>
        /// <param name="Key">The character to represent the scancode.</param>
        public KeyMapping(uint Scancode, char Value, KeyboardKey Key)
        {
            this.Scancode = Scancode;
            this.Value = Value;
            this.Key = Key;
        }

        /// <summary>
        ///     Initialises a new key mapping without a character representation.
        /// </summary>
        /// <param name="Scancode">The scancode to map.</param>
        /// <param name="aKey">The character to represent the scancode.</param>
        public KeyMapping(uint Scancode, KeyboardKey aKey)
        {
            this.Scancode = Scancode;
            Value = '\0';
            Key = aKey;
        }
    }

    #region Keyboard Keys

    /// <summary>
    ///     The enumeration of keyboard keys.
    /// </summary>
    public enum KeyboardKey
    {
        /// <summary>
        ///     The BACKSPACE key.
        /// </summary>
        Backspace = 8,

        /// <summary>
        ///     The TAB key.
        /// </summary>
        Tab = 9,

        /// <summary>
        ///     The CLEAR key.
        /// </summary>
        Clear = 12,

        /// <summary>
        ///     The ENTER key.
        /// </summary>
        Enter = 13,

        /// <summary>
        ///     The PAUSE key.
        /// </summary>
        Pause = 19,

        /// <summary>
        ///     The ESC (ESCAPE) key.
        /// </summary>
        Escape = 27,

        /// <summary>
        ///     The SPACEBAR key.
        /// </summary>
        Spacebar = 32,

        /// <summary>
        ///     The PAGE UP key.
        /// </summary>
        PageUp = 33,

        /// <summary>
        ///     The PAGE DOWN key.
        /// </summary>
        PageDown = 34,

        /// <summary>
        ///     The END key.
        /// </summary>
        End = 35,

        /// <summary>
        ///     The HOME key.
        /// </summary>
        Home = 36,

        /// <summary>
        ///     The LEFT ARROW key.
        /// </summary>
        LeftArrow = 37,

        /// <summary>
        ///     The UP ARROW key.
        /// </summary>
        UpArrow = 38,

        /// <summary>
        ///     The RIGHT ARROW key.
        /// </summary>
        RightArrow = 39,

        /// <summary>
        ///     The DOWN ARROW key.
        /// </summary>
        DownArrow = 40,

        /// <summary>
        ///     The SELECT key.
        /// </summary>
        Select = 41,

        /// <summary>
        ///     The PRINT key.
        /// </summary>
        Print = 42,

        /// <summary>
        ///     The EXECUTE key.
        /// </summary>
        Execute = 43,

        /// <summary>
        ///     The PRINT SCREEN key.
        /// </summary>
        PrintScreen = 44,

        /// <summary>
        ///     The INS (INSERT) key.
        /// </summary>
        Insert = 45,

        /// <summary>
        ///     The DEL (DELETE) key.
        /// </summary>
        Delete = 46,

        /// <summary>
        ///     The HELP key.
        /// </summary>
        Help = 47,

        /// <summary>
        ///     The 0 key.
        /// </summary>
        D0 = 48,

        /// <summary>
        ///     The 1 key.
        /// </summary>
        D1 = 49,

        /// <summary>
        ///     The 2 key.
        /// </summary>
        D2 = 50,

        /// <summary>
        ///     The 3 key.
        /// </summary>
        D3 = 51,

        /// <summary>
        ///     The 4 key.
        /// </summary>
        D4 = 52,

        /// <summary>
        ///     The 5 key.
        /// </summary>
        D5 = 53,

        /// <summary>
        ///     The 6 key.
        /// </summary>
        D6 = 54,

        /// <summary>
        ///     The 7 key.
        /// </summary>
        D7 = 55,

        /// <summary>
        ///     The 8 key.
        /// </summary>
        D8 = 56,

        /// <summary>
        ///     The 9 key.
        /// </summary>
        D9 = 57,

        /// <summary>
        ///     The A key.
        /// </summary>
        A = 65,

        /// <summary>
        ///     The B key.
        /// </summary>
        B = 66,

        /// <summary>
        ///     The C key.
        /// </summary>
        C = 67,

        /// <summary>
        ///     The D key.
        /// </summary>
        D = 68,

        /// <summary>
        ///     The E key.
        /// </summary>
        E = 69,

        /// <summary>
        ///     The F key.
        /// </summary>
        F = 70,

        /// <summary>
        ///     The G key.
        /// </summary>
        G = 71,

        /// <summary>
        ///     The H key.
        /// </summary>
        H = 72,

        /// <summary>
        ///     The I key.
        /// </summary>
        I = 73,

        /// <summary>
        ///     The J key.
        /// </summary>
        J = 74,

        /// <summary>
        ///     The K key.
        /// </summary>
        K = 75,

        /// <summary>
        ///     The L key.
        /// </summary>
        L = 76,

        /// <summary>
        ///     The M key.
        /// </summary>
        M = 77,

        /// <summary>
        ///     The N key.
        /// </summary>
        N = 78,

        /// <summary>
        ///     The O key.
        /// </summary>
        O = 79,

        /// <summary>
        ///     The P key.
        /// </summary>
        P = 80,

        /// <summary>
        ///     The Q key.
        /// </summary>
        Q = 81,

        /// <summary>
        ///     The R key.
        /// </summary>
        R = 82,

        /// <summary>
        ///     The S key.
        /// </summary>
        S = 83,

        /// <summary>
        ///     The T key.
        /// </summary>
        T = 84,

        /// <summary>
        ///     The U key.
        /// </summary>
        U = 85,

        /// <summary>
        ///     The V key.
        /// </summary>
        V = 86,

        /// <summary>
        ///     The W key.
        /// </summary>
        W = 87,

        /// <summary>
        ///     The X key.
        /// </summary>
        X = 88,

        /// <summary>
        ///     The Y key.
        /// </summary>
        Y = 89,

        /// <summary>
        ///     The Z key.
        /// </summary>
        Z = 90,

        /// <summary>
        ///     The left Windows logo key (Microsoft Natural Keyboard).
        /// </summary>
        LeftWindows = 91,

        /// <summary>
        ///     The right Windows logo key (Microsoft Natural Keyboard).
        /// </summary>
        RightWindows = 92,

        /// <summary>
        ///     The Application key (Microsoft Natural Keyboard).
        /// </summary>
        Applications = 93,

        /// <summary>
        ///     The Computer Sleep key.
        /// </summary>
        Sleep = 95,

        /// <summary>
        ///     The 0 key on the numeric keypad.
        /// </summary>
        NumPad0 = 96,

        /// <summary>
        ///     The 1 key on the numeric keypad.
        /// </summary>
        NumPad1 = 97,

        /// <summary>
        ///     The 2 key on the numeric keypad.
        /// </summary>
        NumPad2 = 98,

        /// <summary>
        ///     The 3 key on the numeric keypad.
        /// </summary>
        NumPad3 = 99,

        /// <summary>
        ///     The 4 key on the numeric keypad.
        /// </summary>
        NumPad4 = 100,

        /// <summary>
        ///     The 5 key on the numeric keypad.
        /// </summary>
        NumPad5 = 101,

        /// <summary>
        ///     The 6 key on the numeric keypad.
        /// </summary>
        NumPad6 = 102,

        /// <summary>
        ///     The 7 key on the numeric keypad.
        /// </summary>
        NumPad7 = 103,

        /// <summary>
        ///     The 8 key on the numeric keypad.
        /// </summary>
        NumPad8 = 104,

        /// <summary>
        ///     The 9 key on the numeric keypad.
        /// </summary>
        NumPad9 = 105,

        /// <summary>
        ///     The Multiply key.
        /// </summary>
        Multiply = 106,

        /// <summary>
        ///     The Add key.
        /// </summary>
        Add = 107,

        /// <summary>
        ///     The Separator key.
        /// </summary>
        Separator = 108,

        /// <summary>
        ///     The Subtract key.
        /// </summary>
        Subtract = 109,

        /// <summary>
        ///     The Decimal key.
        /// </summary>
        Decimal = 110,

        /// <summary>
        ///     The Divide key.
        /// </summary>
        Divide = 111,

        /// <summary>
        ///     The F1 key.
        /// </summary>
        F1 = 112,

        /// <summary>
        ///     The F2 key.
        /// </summary>
        F2 = 113,

        /// <summary>
        ///     The F3 key.
        /// </summary>
        F3 = 114,

        /// <summary>
        ///     The F4 key.
        /// </summary>
        F4 = 115,

        /// <summary>
        ///     The F5 key.
        /// </summary>
        F5 = 116,

        /// <summary>
        ///     The F6 key.
        /// </summary>
        F6 = 117,

        /// <summary>
        ///     The F7 key.
        /// </summary>
        F7 = 118,

        /// <summary>
        ///     The F8 key.
        /// </summary>
        F8 = 119,

        /// <summary>
        ///     The F9 key.
        /// </summary>
        F9 = 120,

        /// <summary>
        ///     The F10 key.
        /// </summary>
        F10 = 121,

        /// <summary>
        ///     The F11 key.
        /// </summary>
        F11 = 122,

        /// <summary>
        ///     The F12 key.
        /// </summary>
        F12 = 123,

        /// <summary>
        ///     The F13 key.
        /// </summary>
        F13 = 124,

        /// <summary>
        ///     The F14 key.
        /// </summary>
        F14 = 125,

        /// <summary>
        ///     The F15 key.
        /// </summary>
        F15 = 126,

        /// <summary>
        ///     The F16 key.
        /// </summary>
        F16 = 127,

        /// <summary>
        ///     The F17 key.
        /// </summary>
        F17 = 128,

        /// <summary>
        ///     The F18 key.
        /// </summary>
        F18 = 129,

        /// <summary>
        ///     The F19 key.
        /// </summary>
        F19 = 130,

        /// <summary>
        ///     The F20 key.
        /// </summary>
        F20 = 131,

        /// <summary>
        ///     The F21 key.
        /// </summary>
        F21 = 132,

        /// <summary>
        ///     The F22 key.
        /// </summary>
        F22 = 133,

        /// <summary>
        ///     The F23 key.
        /// </summary>
        F23 = 134,

        /// <summary>
        ///     The F24 key.
        /// </summary>
        F24 = 135,

        /// <summary>
        ///     The Browser Back key (Windows 2000 or later).
        /// </summary>
        BrowserBack = 166,

        /// <summary>
        ///     The Browser Forward key (Windows 2000 or later).
        /// </summary>
        BrowserForward = 167,

        /// <summary>
        ///     The Browser Refresh key (Windows 2000 or later).
        /// </summary>
        BrowserRefresh = 168,

        /// <summary>
        ///     The Browser Stop key (Windows 2000 or later).
        /// </summary>
        BrowserStop = 169,

        /// <summary>
        ///     The Browser Search key (Windows 2000 or later).
        /// </summary>
        BrowserSearch = 170,

        /// <summary>
        ///     The Browser Favorites key (Windows 2000 or later).
        /// </summary>
        BrowserFavorites = 171,

        /// <summary>
        ///     The Browser Home key (Windows 2000 or later).
        /// </summary>
        BrowserHome = 172,

        /// <summary>
        ///     The Volume Mute key (Microsoft Natural Keyboard, Windows 2000 or later).
        /// </summary>
        VolumeMute = 173,

        /// <summary>
        ///     The Volume Down key (Microsoft Natural Keyboard, Windows 2000 or later).
        /// </summary>
        VolumeDown = 174,

        /// <summary>
        ///     The Volume Up key (Microsoft Natural Keyboard, Windows 2000 or later).
        /// </summary>
        VolumeUp = 175,

        /// <summary>
        ///     The Media Next Track key (Windows 2000 or later).
        /// </summary>
        MediaNext = 176,

        /// <summary>
        ///     The Media Previous Track key (Windows 2000 or later).
        /// </summary>
        MediaPrevious = 177,

        /// <summary>
        ///     The Media Stop key (Windows 2000 or later).
        /// </summary>
        MediaStop = 178,

        /// <summary>
        ///     The Media Play/Pause key (Windows 2000 or later).
        /// </summary>
        MediaPlay = 179,

        /// <summary>
        ///     The Start Mail key (Microsoft Natural Keyboard, Windows 2000 or later).
        /// </summary>
        LaunchMail = 180,

        /// <summary>
        ///     The Select Media key (Microsoft Natural Keyboard, Windows 2000 or later).
        /// </summary>
        LaunchMediaSelect = 181,

        /// <summary>
        ///     The Start Application 1 key (Microsoft Natural Keyboard, Windows 2000 or
        /// </summary>
        //     later).
        LaunchApp1 = 182,

        /// <summary>
        ///     The Start Application 2 key (Microsoft Natural Keyboard, Windows 2000 or
        /// </summary>
        //     later).
        LaunchApp2 = 183,

        /// <summary>
        ///     The OEM 1 key (OEM specific).
        /// </summary>
        Oem1 = 186,

        /// <summary>
        ///     The OEM Plus key on any country/region keyboard (Windows 2000 or later).
        /// </summary>
        OemPlus = 187,

        /// <summary>
        ///     The OEM Comma key on any country/region keyboard (Windows 2000 or later).
        /// </summary>
        OemComma = 188,

        /// <summary>
        ///     The OEM Minus key on any country/region keyboard (Windows 2000 or later).
        /// </summary>
        OemMinus = 189,

        /// <summary>
        ///     The OEM Period key on any country/region keyboard (Windows 2000 or later).
        /// </summary>
        OemPeriod = 190,

        /// <summary>
        ///     The OEM 2 key (OEM specific).
        /// </summary>
        Oem2 = 191,

        /// <summary>
        ///     The OEM 3 key (OEM specific).
        /// </summary>
        Oem3 = 192,

        /// <summary>
        ///     The OEM 4 key (OEM specific).
        /// </summary>
        Oem4 = 219,

        /// <summary>
        ///     The OEM 5 (OEM specific).
        /// </summary>
        Oem5 = 220,

        /// <summary>
        ///     The OEM 6 key (OEM specific).
        /// </summary>
        Oem6 = 221,

        /// <summary>
        ///     The OEM 7 key (OEM specific).
        /// </summary>
        Oem7 = 222,

        /// <summary>
        ///     The OEM 8 key (OEM specific).
        /// </summary>
        Oem8 = 223,

        /// <summary>
        ///     The OEM 102 key (OEM specific).
        /// </summary>
        Oem102 = 226,

        /// <summary>
        ///     The IME PROCESS key.
        /// </summary>
        Process = 229,

        /// <summary>
        ///     The PACKET key (used to pass Unicode characters with keystrokes).
        /// </summary>
        Packet = 231,

        /// <summary>
        ///     The ATTN key.
        /// </summary>
        Attention = 246,

        /// <summary>
        ///     The CRSEL (CURSOR SELECT) key.
        /// </summary>
        CrSel = 247,

        /// <summary>
        ///     The EXSEL (EXTEND SELECTION) key.
        /// </summary>
        ExSel = 248,

        /// <summary>
        ///     The ERASE EOF key.
        /// </summary>
        EraseEndOfFile = 249,

        /// <summary>
        ///     The PLAY key.
        /// </summary>
        Play = 250,

        /// <summary>
        ///     The ZOOM key.
        /// </summary>
        Zoom = 251,

        /// <summary>
        ///     A constant reserved for future use.
        /// </summary>
        NoName = 252,

        /// <summary>
        ///     The PA1 key.
        /// </summary>
        Pa1 = 253,

        /// <summary>
        ///     The CLEAR key (OEM specific).
        /// </summary>
        OemClear = 254
    }

    #endregion
}