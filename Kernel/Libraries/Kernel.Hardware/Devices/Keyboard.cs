#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.Devices
{
    public abstract class Keyboard : Device
    {
        public static uint GetCharDelayTimeMS = 10;

        protected UInt32List scancodeBuffer = new UInt32List(64);
        protected bool enabled = false;

        protected List mKeys;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }

        protected bool mEscaped;
        protected bool mShiftState;
        protected bool mCtrlState;
        protected bool mAltState;

        public bool ShiftPressed
        {
            get
            {
                return mShiftState;
            }
        }
        public bool CtrlPressed
        {
            get
            {
                return mCtrlState;
            }
        }
        public bool AltPressed
        {
            get
            {
                return mAltState;
            }
        }

        public Keyboard()
        {
            if (mKeys == null)
            {
                CreateDefaultKeymap();
            }
        }

        public abstract void Enable();
        public abstract void Disable();
        
        protected void CreateDefaultKeymap()
        {
            mKeys = new List(164);

            //TODO: fn key
            //TODO: full numpad?
            //TODO: Other UK keys e.g. backslash

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

            #region digits
            AddKey(0x29, '`', KeyboardKey.NoName);
            AddKey(0x290000, (char)170u, KeyboardKey.NoName);
            AddKey(0x2, '1', KeyboardKey.D1);
            AddKey(0x20000, '!', KeyboardKey.D1);
            AddKey(0x3, '2', KeyboardKey.D2);
            AddKey(0x30000, '"', KeyboardKey.D2);
            AddKey(0x4, '3', KeyboardKey.D3);
            AddKey(0x40000, (char)156u, KeyboardKey.D3);
            AddKey(0x5, '4', KeyboardKey.D4);
            AddKey(0x50000, '$', KeyboardKey.D5);
            AddKey(0x6, '5', KeyboardKey.D5);
            AddKey(0x60000, '%', KeyboardKey.D5);
            AddKey(0x7, '6', KeyboardKey.D6);
            AddKey(0x70000, '^', KeyboardKey.D6);
            AddKey(0x8, '7', KeyboardKey.D7);
            AddKey(0x80000, '&', KeyboardKey.D7);
            AddKey(0x9, '8', KeyboardKey.D8);
            AddKey(0x90000, '*', KeyboardKey.D8);
            AddKey(0xA, '9', KeyboardKey.D9);
            AddKey(0xA0000, '(', KeyboardKey.D9);
            AddKey(0xB, '0', KeyboardKey.D0);
            AddKey(0xB0000, ')', KeyboardKey.D0);

            #endregion

            #region Special
            AddKeyWithShift(0x0E, '\0', KeyboardKey.Backspace);               //Backspace
            AddKeyWithShift(0x0F, '\t', KeyboardKey.Tab);                         //Tabulator
            AddKeyWithShift(0x1C, '\n', KeyboardKey.Enter);                       //Enter
            AddKeyWithShift(0x39, ' ', KeyboardKey.Spacebar);                     //Space
            AddKeyWithShift(0x4b, '\u2190', KeyboardKey.LeftArrow);               //Left arrow
            AddKeyWithShift(0x48, '\u2191', KeyboardKey.UpArrow);                 //Up arrow
            AddKeyWithShift(0x4d, '\u2192', KeyboardKey.RightArrow);              //Right arrow
            AddKeyWithShift(0x50, '\u2193', KeyboardKey.DownArrow);               //Down arrow

            AddKeyWithShift(0x5b, KeyboardKey.LeftWindows);
            AddKeyWithShift(0x5c, KeyboardKey.RightWindows);
            //AddKey(0x5d, KeyboardKey.NoName);                                   //Context Menu  

            AddKeyWithShift(0x52, KeyboardKey.Insert);
            AddKeyWithShift(0x47, KeyboardKey.Home);
            AddKeyWithShift(0x49, KeyboardKey.PageUp);
            AddKeyWithShift(0x53, KeyboardKey.Delete);
            AddKeyWithShift(0x4f, KeyboardKey.End);
            AddKeyWithShift(0x51, KeyboardKey.PageDown);

            AddKeyWithShift(0x37, KeyboardKey.PrintScreen);
            //AddKeyWithShift(0x46, KeyboardKey.NoName);                          //Scroll Lock
            //AddKeyWithShift(0x3a, KeyboardKey.NoName);                          //Caps Lock
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
            AddKey(0x27, ';', KeyboardKey.NoName);
            AddKey(0x270000, ':', KeyboardKey.NoName);
            AddKey(0x28, '\'', KeyboardKey.NoName);
            AddKey(0x280000, '@', KeyboardKey.NoName);
            AddKey(0x2B, '#', KeyboardKey.NoName);
            AddKey(0x2B0000, '~', KeyboardKey.NoName);
            AddKey(0x33, ',', KeyboardKey.OemComma);
            AddKey(0x330000, '<', KeyboardKey.OemComma);
            AddKey(0x34, '.', KeyboardKey.OemPeriod);
            AddKey(0x340000, '>', KeyboardKey.OemPeriod);
            AddKey(0x35, '/', KeyboardKey.Divide);
            AddKey(0x350000, '?', KeyboardKey.Divide);
            //AddKey(0x4A, '-');
            AddKey(0x0C, '-', KeyboardKey.Subtract);
            AddKey(0x0C0000, '_', KeyboardKey.Subtract);
            AddKey(0x0D, '=', KeyboardKey.OemPlus);
            AddKey(0x0D0000, '+', KeyboardKey.OemPlus);
            //AddKey(0x4E, '+');
            AddKey(0x1A, '[', KeyboardKey.NoName);
            AddKey(0x1A0000, '{', KeyboardKey.NoName);
            AddKey(0x1B, ']', KeyboardKey.NoName);
            AddKey(0x1B0000, '}', KeyboardKey.NoName);

            AddKeyWithShift(0x4c, '5', KeyboardKey.NumPad5);

            AddKeyWithShift(0x4a, '-', KeyboardKey.OemMinus);
            AddKeyWithShift(0x4e, '+', KeyboardKey.OemPlus);

            AddKeyWithShift(0x37, '*', KeyboardKey.Multiply);
            #endregion
        }

        protected uint KeyCount = 0;

        protected void AddKey(uint p, char p_2, KeyboardKey p_3)
        {
            mKeys.Add(new KeyMapping(p, p_2, p_3));
            KeyCount++;
        }
        protected void AddKeyWithShift(uint p, char p_2, KeyboardKey p_3)
        {
            AddKey(p, p_2, p_3);
            AddKey(p << 16, p_2, p_3);
        }
        protected void AddKey(uint p, KeyboardKey p_3)
        {
            AddKey(p, '\0', p_3);
        }
        protected void AddKeyWithShift(uint p, KeyboardKey p_3)
        {
            AddKeyWithShift(p, '\0', p_3);
        }

        public void ChangeKeyMap(List aKeys)
        {
            mKeys = aKeys;
        }

        protected void Enqueue(uint scancode)
        {
            scancodeBuffer.Add(scancode);
        }
        public uint Dequeue()
        {
            uint result = scancodeBuffer[0];
            scancodeBuffer.RemoveAt(0);
            return result;
        }

        public bool GetCharValue(uint aScanCode, out char aValue)
        {
            for (int i = 0; i < mKeys.Count; i++)
            {
                if (((KeyMapping)mKeys[i]).Scancode == aScanCode)
                {
                    if (((KeyMapping)mKeys[i]).Value != '\0')
                    {
                        aValue = ((KeyMapping)mKeys[i]).Value;
                        return true;
                    }
                    break;
                }
            }

            aValue = '\0';
            return false;
        }
        public bool GetKeyValue(uint aScanCode, out KeyboardKey aValue)
        {
            for (int i = 0; i < mKeys.Count; i++)
            {
                if (((KeyMapping)mKeys[i]).Scancode == aScanCode)
                {
                    aValue = ((KeyMapping)mKeys[i]).Key;
                    return true;
                }
            }

            aValue = KeyboardKey.NoName;
            return false;
        }
        public bool GetKeyMapping(uint aScanCode, out KeyMapping aValue)
        {
            for (int i = 0; i < mKeys.Count; i++)
            {
                if (((KeyMapping)mKeys[i]).Scancode == aScanCode)
                {
                    aValue = ((KeyMapping)mKeys[i]);
                    return true;
                }
            }

            aValue = null;
            return false;
        }

        public KeyMapping ReadMapping()
        {
            KeyMapping xResult = null;
            while (scancodeBuffer.Count == 0 || !GetKeyMapping(Dequeue(), out xResult))
            {
                Timer.Default.Wait(50);
            }
            return xResult;
        }
        public char ReadChar()
        {
            char xResult = '\0';
            while (scancodeBuffer.Count == 0 || !GetCharValue(Dequeue(), out xResult))
            {
                Timer.Default.Wait(50);
            }
            return xResult;
        }
        public KeyboardKey ReadKey()
        {
            KeyboardKey xResult = KeyboardKey.NoName;
            while (scancodeBuffer.Count == 0 || !GetKeyValue(Dequeue(), out xResult))
            {
                Timer.Default.Wait(50);
            }
            return xResult;
        }
        public uint ReadScancode()
        {
            while (scancodeBuffer.Count == 0)
            {
                Timer.Default.Wait(50);
            }

            return Dequeue();
        }

        public bool GetChar(out char c)
        {
            c = '\0';

            if (scancodeBuffer.Count > 0)
            {
                GetCharValue(Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool GetChar_Blocking(uint timeout, out char c)
        {
            c = '\0';

            timeout /= GetCharDelayTimeMS;
            while(scancodeBuffer.Count == 0 && timeout-- > 0)
            {
                Timer.Default.Wait(GetCharDelayTimeMS);
            }

            if(scancodeBuffer.Count > 0)
            {
                GetCharValue(Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetKey(out KeyboardKey c)
        {
            c = KeyboardKey.NoName;

            if (scancodeBuffer.Count > 0)
            {
                GetKeyValue(Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool GetMapping(out KeyMapping c)
        {
            c = null;

            if (scancodeBuffer.Count > 0)
            {
                GetKeyMapping(Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool GetScancode(out uint c)
        {
            if (scancodeBuffer.Count > 0)
            {
                c = Dequeue();
                return true;
            }
            else
            {
                c = 0;
                return false;
            }
        }

        public static Keyboard Default;
        public static void InitDefault()
        {
            Keyboards.PS2.Init();
            Default = Keyboards.PS2.ThePS2;
        }
        public static void CleanDefault()
        {
            if (Default != null)
            {
                Default.Disable();
            }
        }
    }
    public class KeyMapping : FOS_System.Object
    {
        public uint Scancode;
        public char Value;
        public KeyboardKey Key;

        public KeyMapping(uint aScanCode, char aValue, KeyboardKey aKey)
        {
            Scancode = aScanCode;
            Value = aValue;
            Key = aKey;
        }
        public KeyMapping(uint aScanCode, KeyboardKey aKey)
        {
            Scancode = aScanCode;
            Value = '\0';
            Key = aKey;
        }
    }

    #region Keyboard Keys
    public enum KeyboardKey
    {
        // Summary:
        //     The BACKSPACE key.
        Backspace = 8,
        //
        // Summary:
        //     The TAB key.
        Tab = 9,
        //
        // Summary:
        //     The CLEAR key.
        Clear = 12,
        //
        // Summary:
        //     The ENTER key.
        Enter = 13,
        //
        // Summary:
        //     The PAUSE key.
        Pause = 19,
        //
        // Summary:
        //     The ESC (ESCAPE) key.
        Escape = 27,
        //
        // Summary:
        //     The SPACEBAR key.
        Spacebar = 32,
        //
        // Summary:
        //     The PAGE UP key.
        PageUp = 33,
        //
        // Summary:
        //     The PAGE DOWN key.
        PageDown = 34,
        //
        // Summary:
        //     The END key.
        End = 35,
        //
        // Summary:
        //     The HOME key.
        Home = 36,
        //
        // Summary:
        //     The LEFT ARROW key.
        LeftArrow = 37,
        //
        // Summary:
        //     The UP ARROW key.
        UpArrow = 38,
        //
        // Summary:
        //     The RIGHT ARROW key.
        RightArrow = 39,
        //
        // Summary:
        //     The DOWN ARROW key.
        DownArrow = 40,
        //
        // Summary:
        //     The SELECT key.
        Select = 41,
        //
        // Summary:
        //     The PRINT key.
        Print = 42,
        //
        // Summary:
        //     The EXECUTE key.
        Execute = 43,
        //
        // Summary:
        //     The PRINT SCREEN key.
        PrintScreen = 44,
        //
        // Summary:
        //     The INS (INSERT) key.
        Insert = 45,
        //
        // Summary:
        //     The DEL (DELETE) key.
        Delete = 46,
        //
        // Summary:
        //     The HELP key.
        Help = 47,
        //
        // Summary:
        //     The 0 key.
        D0 = 48,
        //
        // Summary:
        //     The 1 key.
        D1 = 49,
        //
        // Summary:
        //     The 2 key.
        D2 = 50,
        //
        // Summary:
        //     The 3 key.
        D3 = 51,
        //
        // Summary:
        //     The 4 key.
        D4 = 52,
        //
        // Summary:
        //     The 5 key.
        D5 = 53,
        //
        // Summary:
        //     The 6 key.
        D6 = 54,
        //
        // Summary:
        //     The 7 key.
        D7 = 55,
        //
        // Summary:
        //     The 8 key.
        D8 = 56,
        //
        // Summary:
        //     The 9 key.
        D9 = 57,
        //
        // Summary:
        //     The A key.
        A = 65,
        //
        // Summary:
        //     The B key.
        B = 66,
        //
        // Summary:
        //     The C key.
        C = 67,
        //
        // Summary:
        //     The D key.
        D = 68,
        //
        // Summary:
        //     The E key.
        E = 69,
        //
        // Summary:
        //     The F key.
        F = 70,
        //
        // Summary:
        //     The G key.
        G = 71,
        //
        // Summary:
        //     The H key.
        H = 72,
        //
        // Summary:
        //     The I key.
        I = 73,
        //
        // Summary:
        //     The J key.
        J = 74,
        //
        // Summary:
        //     The K key.
        K = 75,
        //
        // Summary:
        //     The L key.
        L = 76,
        //
        // Summary:
        //     The M key.
        M = 77,
        //
        // Summary:
        //     The N key.
        N = 78,
        //
        // Summary:
        //     The O key.
        O = 79,
        //
        // Summary:
        //     The P key.
        P = 80,
        //
        // Summary:
        //     The Q key.
        Q = 81,
        //
        // Summary:
        //     The R key.
        R = 82,
        //
        // Summary:
        //     The S key.
        S = 83,
        //
        // Summary:
        //     The T key.
        T = 84,
        //
        // Summary:
        //     The U key.
        U = 85,
        //
        // Summary:
        //     The V key.
        V = 86,
        //
        // Summary:
        //     The W key.
        W = 87,
        //
        // Summary:
        //     The X key.
        X = 88,
        //
        // Summary:
        //     The Y key.
        Y = 89,
        //
        // Summary:
        //     The Z key.
        Z = 90,
        //
        // Summary:
        //     The left Windows logo key (Microsoft Natural Keyboard).
        LeftWindows = 91,
        //
        // Summary:
        //     The right Windows logo key (Microsoft Natural Keyboard).
        RightWindows = 92,
        //
        // Summary:
        //     The Application key (Microsoft Natural Keyboard).
        Applications = 93,
        //
        // Summary:
        //     The Computer Sleep key.
        Sleep = 95,
        //
        // Summary:
        //     The 0 key on the numeric keypad.
        NumPad0 = 96,
        //
        // Summary:
        //     The 1 key on the numeric keypad.
        NumPad1 = 97,
        //
        // Summary:
        //     The 2 key on the numeric keypad.
        NumPad2 = 98,
        //
        // Summary:
        //     The 3 key on the numeric keypad.
        NumPad3 = 99,
        //
        // Summary:
        //     The 4 key on the numeric keypad.
        NumPad4 = 100,
        //
        // Summary:
        //     The 5 key on the numeric keypad.
        NumPad5 = 101,
        //
        // Summary:
        //     The 6 key on the numeric keypad.
        NumPad6 = 102,
        //
        // Summary:
        //     The 7 key on the numeric keypad.
        NumPad7 = 103,
        //
        // Summary:
        //     The 8 key on the numeric keypad.
        NumPad8 = 104,
        //
        // Summary:
        //     The 9 key on the numeric keypad.
        NumPad9 = 105,
        //
        // Summary:
        //     The Multiply key.
        Multiply = 106,
        //
        // Summary:
        //     The Add key.
        Add = 107,
        //
        // Summary:
        //     The Separator key.
        Separator = 108,
        //
        // Summary:
        //     The Subtract key.
        Subtract = 109,
        //
        // Summary:
        //     The Decimal key.
        Decimal = 110,
        //
        // Summary:
        //     The Divide key.
        Divide = 111,
        //
        // Summary:
        //     The F1 key.
        F1 = 112,
        //
        // Summary:
        //     The F2 key.
        F2 = 113,
        //
        // Summary:
        //     The F3 key.
        F3 = 114,
        //
        // Summary:
        //     The F4 key.
        F4 = 115,
        //
        // Summary:
        //     The F5 key.
        F5 = 116,
        //
        // Summary:
        //     The F6 key.
        F6 = 117,
        //
        // Summary:
        //     The F7 key.
        F7 = 118,
        //
        // Summary:
        //     The F8 key.
        F8 = 119,
        //
        // Summary:
        //     The F9 key.
        F9 = 120,
        //
        // Summary:
        //     The F10 key.
        F10 = 121,
        //
        // Summary:
        //     The F11 key.
        F11 = 122,
        //
        // Summary:
        //     The F12 key.
        F12 = 123,
        //
        // Summary:
        //     The F13 key.
        F13 = 124,
        //
        // Summary:
        //     The F14 key.
        F14 = 125,
        //
        // Summary:
        //     The F15 key.
        F15 = 126,
        //
        // Summary:
        //     The F16 key.
        F16 = 127,
        //
        // Summary:
        //     The F17 key.
        F17 = 128,
        //
        // Summary:
        //     The F18 key.
        F18 = 129,
        //
        // Summary:
        //     The F19 key.
        F19 = 130,
        //
        // Summary:
        //     The F20 key.
        F20 = 131,
        //
        // Summary:
        //     The F21 key.
        F21 = 132,
        //
        // Summary:
        //     The F22 key.
        F22 = 133,
        //
        // Summary:
        //     The F23 key.
        F23 = 134,
        //
        // Summary:
        //     The F24 key.
        F24 = 135,
        //
        // Summary:
        //     The Browser Back key (Windows 2000 or later).
        BrowserBack = 166,
        //
        // Summary:
        //     The Browser Forward key (Windows 2000 or later).
        BrowserForward = 167,
        //
        // Summary:
        //     The Browser Refresh key (Windows 2000 or later).
        BrowserRefresh = 168,
        //
        // Summary:
        //     The Browser Stop key (Windows 2000 or later).
        BrowserStop = 169,
        //
        // Summary:
        //     The Browser Search key (Windows 2000 or later).
        BrowserSearch = 170,
        //
        // Summary:
        //     The Browser Favorites key (Windows 2000 or later).
        BrowserFavorites = 171,
        //
        // Summary:
        //     The Browser Home key (Windows 2000 or later).
        BrowserHome = 172,
        //
        // Summary:
        //     The Volume Mute key (Microsoft Natural Keyboard, Windows 2000 or later).
        VolumeMute = 173,
        //
        // Summary:
        //     The Volume Down key (Microsoft Natural Keyboard, Windows 2000 or later).
        VolumeDown = 174,
        //
        // Summary:
        //     The Volume Up key (Microsoft Natural Keyboard, Windows 2000 or later).
        VolumeUp = 175,
        //
        // Summary:
        //     The Media Next Track key (Windows 2000 or later).
        MediaNext = 176,
        //
        // Summary:
        //     The Media Previous Track key (Windows 2000 or later).
        MediaPrevious = 177,
        //
        // Summary:
        //     The Media Stop key (Windows 2000 or later).
        MediaStop = 178,
        //
        // Summary:
        //     The Media Play/Pause key (Windows 2000 or later).
        MediaPlay = 179,
        //
        // Summary:
        //     The Start Mail key (Microsoft Natural Keyboard, Windows 2000 or later).
        LaunchMail = 180,
        //
        // Summary:
        //     The Select Media key (Microsoft Natural Keyboard, Windows 2000 or later).
        LaunchMediaSelect = 181,
        //
        // Summary:
        //     The Start Application 1 key (Microsoft Natural Keyboard, Windows 2000 or
        //     later).
        LaunchApp1 = 182,
        //
        // Summary:
        //     The Start Application 2 key (Microsoft Natural Keyboard, Windows 2000 or
        //     later).
        LaunchApp2 = 183,
        //
        // Summary:
        //     The OEM 1 key (OEM specific).
        Oem1 = 186,
        //
        // Summary:
        //     The OEM Plus key on any country/region keyboard (Windows 2000 or later).
        OemPlus = 187,
        //
        // Summary:
        //     The OEM Comma key on any country/region keyboard (Windows 2000 or later).
        OemComma = 188,
        //
        // Summary:
        //     The OEM Minus key on any country/region keyboard (Windows 2000 or later).
        OemMinus = 189,
        //
        // Summary:
        //     The OEM Period key on any country/region keyboard (Windows 2000 or later).
        OemPeriod = 190,
        //
        // Summary:
        //     The OEM 2 key (OEM specific).
        Oem2 = 191,
        //
        // Summary:
        //     The OEM 3 key (OEM specific).
        Oem3 = 192,
        //
        // Summary:
        //     The OEM 4 key (OEM specific).
        Oem4 = 219,
        //
        // Summary:
        //     The OEM 5 (OEM specific).
        Oem5 = 220,
        //
        // Summary:
        //     The OEM 6 key (OEM specific).
        Oem6 = 221,
        //
        // Summary:
        //     The OEM 7 key (OEM specific).
        Oem7 = 222,
        //
        // Summary:
        //     The OEM 8 key (OEM specific).
        Oem8 = 223,
        //
        // Summary:
        //     The OEM 102 key (OEM specific).
        Oem102 = 226,
        //
        // Summary:
        //     The IME PROCESS key.
        Process = 229,
        //
        // Summary:
        //     The PACKET key (used to pass Unicode characters with keystrokes).
        Packet = 231,
        //
        // Summary:
        //     The ATTN key.
        Attention = 246,
        //
        // Summary:
        //     The CRSEL (CURSOR SELECT) key.
        CrSel = 247,
        //
        // Summary:
        //     The EXSEL (EXTEND SELECTION) key.
        ExSel = 248,
        //
        // Summary:
        //     The ERASE EOF key.
        EraseEndOfFile = 249,
        //
        // Summary:
        //     The PLAY key.
        Play = 250,
        //
        // Summary:
        //     The ZOOM key.
        Zoom = 251,
        //
        // Summary:
        //     A constant reserved for future use.
        NoName = 252,
        //
        // Summary:
        //     The PA1 key.
        Pa1 = 253,
        //
        // Summary:
        //     The CLEAR key (OEM specific).
        OemClear = 254
    }
#endregion
}
