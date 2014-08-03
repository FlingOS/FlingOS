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

namespace Kernel.Core
{
    public abstract class Console : FOS_System.Object
    {
        // Features:
        //  - Writing/reading
        //  - Scrolling
        //  - Foreground/Background Colour
        //  - Beep
        //  - Cursor


        protected List Buffer = new List(100);
        protected Int32 CurrentLine = 0;
        protected Int32 CurrentChar = 0;

        protected Int32 LineLength = 80;
        protected Int32 MaxBufferSize = 200;
        protected ushort CurrentAttr = 0;

        protected Hardware.IO.IOPort CursorCmdPort = new Hardware.IO.IOPort(0x3D4);
        protected Hardware.IO.IOPort CursorDataPort = new Hardware.IO.IOPort(0x3D5);

        public Console()
        {
            Buffer.Add(CreateBlankLine());
            DefaultColour();
        }

        protected FOS_System.String CreateBlankLine()
        {
            return FOS_System.String.New(LineLength);
        }

        protected abstract void Update();
        protected abstract int GetDisplayOffset_Line();
        protected abstract int GetDisplayOffset_Char();

        public virtual void Write(FOS_System.String str)
        {
            for (int i = 0; i < str.length; i++)
            {
                if (CurrentChar == LineLength)
                {
                    WriteLine();
                }
                if (str[i] == '\n')
                {
                    WriteLine();
                }
                else
                {
                    ((FOS_System.String)Buffer[CurrentLine])[CurrentChar++] = (char)(str[i] | CurrentAttr);
                }
            }
            Update();
            SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                              (ushort)(CurrentLine - GetDisplayOffset_Line()));
        }
        public virtual void Write_AsDecimal(UInt32 num)
        {
            FOS_System.String result = "";
            while (num > 0)
            {
                uint rem = num % 10;
                switch (rem)
                {
                    case 0:
                        result = "0" + result;
                        break;
                    case 1:
                        result = "1" + result;
                        break;
                    case 2:
                        result = "2" + result;
                        break;
                    case 3:
                        result = "3" + result;
                        break;
                    case 4:
                        result = "4" + result;
                        break;
                    case 5:
                        result = "5" + result;
                        break;
                    case 6:
                        result = "6" + result;
                        break;
                    case 7:
                        result = "7" + result;
                        break;
                    case 8:
                        result = "8" + result;
                        break;
                    case 9:
                        result = "9" + result;
                        break;
                }
                num /= 10;
            }
            Write(result);
        }
        public virtual void Write_AsDecimal(Int32 num)
        {
            FOS_System.String result = "";
            while (num > 0)
            {
                int rem = num % 10;
                switch (rem)
                {
                    case 0:
                        result = "0" + result;
                        break;
                    case 1:
                        result = "1" + result;
                        break;
                    case 2:
                        result = "2" + result;
                        break;
                    case 3:
                        result = "3" + result;
                        break;
                    case 4:
                        result = "4" + result;
                        break;
                    case 5:
                        result = "5" + result;
                        break;
                    case 6:
                        result = "6" + result;
                        break;
                    case 7:
                        result = "7" + result;
                        break;
                    case 8:
                        result = "8" + result;
                        break;
                    case 9:
                        result = "9" + result;
                        break;
                }
                num /= 10;
            }

            if(num < 0)
            {
                result = "-" + result;
            }
            Write(result);
        }

        public virtual void WriteLine()
        {
            if (Buffer.Count == MaxBufferSize)
            {
                Buffer.RemoveAt(0);
                Buffer.Add(CreateBlankLine());
            }
            else
            {
                Buffer.Add(CreateBlankLine());
            }
            CurrentLine = Buffer.Count - 1;
            CurrentChar = 0;
            Update();
            SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                              (ushort)(CurrentLine - GetDisplayOffset_Line()));
        }
        public virtual void WriteLine(FOS_System.String str)
        {
            Write(str);
            WriteLine();
        }
        public virtual void WriteLine_AsDecimal(UInt32 num)
        {
            Write_AsDecimal(num);
            WriteLine();
        }
        public virtual void WriteLine_AsDecimal(Int32 num)
        {
            Write_AsDecimal(num);
            WriteLine();
        }

        public virtual Hardware.Devices.KeyboardKey ReadKey()
        {
            return ReadKey(true);
        }
        public virtual Hardware.Devices.KeyboardKey ReadKey(bool output)
        {
            Hardware.Devices.KeyMapping mapping = Hardware.Devices.Keyboard.Default.ReadMapping();
            if (mapping.Value != '\0' && output)
            {
                Write(mapping.Value);
            }
            return mapping.Key;
        }

        public virtual char Read()
        {
            char result = Hardware.Devices.Keyboard.Default.ReadChar();
            Write(result);        
            return result;
        }
        public virtual FOS_System.String ReadLine()
        {
            FOS_System.String result = "";
            Hardware.Devices.KeyMapping c;
            Int32 StartLine = CurrentLine;
            Int32 StartChar = CurrentChar;
            while((c = Hardware.Devices.Keyboard.Default.ReadMapping()).Key != Hardware.Devices.KeyboardKey.Enter)
            {
                if (c.Key == Hardware.Devices.KeyboardKey.Backspace)
                {
                    if (result.length > 0)
                    {
                        result = result.Substring(0, result.length - 1);
                        CurrentChar--;
                        if (CurrentChar < 0)
                        {
                            Buffer.RemoveAt(CurrentLine);

                            CurrentChar = LineLength - 1;
                            CurrentLine--;
                        }
                        ((FOS_System.String)Buffer[CurrentLine])[CurrentChar] = ' ';
                        Update();
                    }
                }
                else if(c.Key == Hardware.Devices.KeyboardKey.Escape)
                {
                    result = "";

                    //Reset to StartLine, StartChar
                    for(int i = CurrentLine; i >= StartLine; i--)
                    {
                        if (i == StartLine)
                        {
                            FOS_System.String line = ((FOS_System.String)Buffer[i]);
                            for (int j = StartChar; j < LineLength; j++)
                            {
                                line[j] = ' ';
                            }
                        }
                        else
                        {
                            Buffer.RemoveAt(i);
                        }
                    }

                    CurrentLine = StartLine;
                    CurrentChar = StartChar;

                    Update();
                }
                else if(c.Value != '\0')
                {
                    result += c.Value;
                    Write(c.Value);
                }
            }
            return result;
        }

        public virtual void Clear()
        {
            Buffer.Empty();
            WriteLine();
        }

        public virtual void Beep()
        {
            Beep(245, 500);
        }
        public virtual void Beep(Int32 freq, UInt32 duration)
        {
            Hardware.Timers.PIT.ThePIT.PlaySound(freq);
            Hardware.Timers.PIT.ThePIT.Wait(duration);
            Hardware.Timers.PIT.ThePIT.MuteSound();
        }

        public abstract void SetCursorPosition(ushort character, ushort line);

        public virtual void Scroll(Int32 dist)
        {
            CurrentLine += dist;
            if (CurrentLine < 0)
            {
                CurrentLine = 0;
            }
            else if (CurrentLine >= Buffer.Count)
            {
                CurrentLine = Buffer.Count - 1;
            }
            Update();
        }

        public virtual void Colour(byte col)
        {
            CurrentAttr = (ushort)((CurrentAttr & 0x00FF) | (col << 8));
        }
        public virtual void DefaultColour()
        {
            Colour(0x0F);
        }
        public virtual void WarningColour()
        {
            Colour(0x0E);
        }
        public virtual void ErrorColour()
        {
            Colour(0x04);
        }

        public static Console Default;
        public static void InitDefault()
        {
            Default = new Consoles.AdvancedConsole();
        }
        public static void CleanDefault()
        {
            Default = null;
        }
    }
}
