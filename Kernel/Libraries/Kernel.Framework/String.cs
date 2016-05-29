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

using Drivers.Compiler.Attributes;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Utilities;

namespace Kernel.Framework
{
    /// <summary>
    ///     Replacement class for methods, properties and fields usually found on standard System.String type.
    ///     Also contains utility methods for low-level string manipulation.
    /// </summary>
    [StringClass]
    public sealed class String : Object
    {
        /* If you add more fields here, remember to update the compiler and all the ASM files that depend on the string
           class structure ( i.e. do all the hard work! ;) )
         */

        /// <summary>
        ///     The size of the fields in an string object that come before the actual string data.
        /// </summary>
        private const uint FieldsBytesSize = 8;

        /// <summary>
        ///     The length of the string.
        /// </summary>
        public int Length;

        /// <summary>
        ///     Gets the character at the specified index.
        /// </summary>
        /// <param name="Index">The index of the character to get.</param>
        /// <returns>The character at the specified index.</returns>
        public unsafe char this[int Index]
        {
            [NoDebug]
            [NoGC]
            get
            {
                byte* thisPtr = (byte*)ObjectUtilities.GetHandle(this);
                thisPtr += FieldsBytesSize; /*For fields inc. inherited*/
                return ((char*)thisPtr)[Index];
            }
            [NoDebug]
            [NoGC]
            set
            {
                byte* thisPtr = (byte*)ObjectUtilities.GetHandle(this);
                thisPtr += FieldsBytesSize; /*For fields inc. inherited*/
                ((char*)thisPtr)[Index] = value;
            }
        }

        /// <summary>
        ///     Gets the character at the specified index.
        /// </summary>
        /// <param name="Index">The index of the character to get.</param>
        /// <returns>The character at the specified index.</returns>
        public unsafe char this[uint Index]
        {
            [NoDebug]
            [NoGC]
            get
            {
                byte* thisPtr = (byte*)ObjectUtilities.GetHandle(this);
                thisPtr += FieldsBytesSize; /*For fields inc. inherited*/
                return ((char*)thisPtr)[Index];
            }
            [NoDebug]
            [NoGC]
            set
            {
                byte* thisPtr = (byte*)ObjectUtilities.GetHandle(this);
                thisPtr += FieldsBytesSize; /*For fields inc. inherited*/
                ((char*)thisPtr)[Index] = value;
            }
        }

        /*   ----------- DO NOT CREATE A CONSTRUCTOR FOR THIS CLASS - IT WILL NEVER BE CALLED IF YOU DO ----------- */

        /// <summary>
        ///     Creates a new, blank Framework.String of specified length.
        ///     IMPORTANT NOTE: You MUST assign the return value of this to a variable / local / arg /
        ///     field etc. You may not use IL or C# that results in an IL Pop op of the return value
        ///     of this method as it will screw up the GC RefCount handling.
        /// </summary>
        /// <param name="Length">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        [NoGC]
        [NoDebug]
        public static unsafe String New(int Length)
        {
            if (Length < 0)
            {
                ExceptionMethods.Throw(
                    new ArgumentException(
                        "Parameter \"length\" cannot be less than 0 in Framework.String.New(int Length)."));
            }
            String Result = (String)ObjectUtilities.GetObject(GC.NewString(Length));
            if (Result == null)
            {
                ExceptionMethods.Throw(new NullReferenceException());
            }
            return Result;
        }

        /// <summary>
        ///     Concatenates two strings into one new string.
        /// </summary>
        /// <param name="Left">The first part of the new string.</param>
        /// <param name="Right">The second part of the new string.</param>
        /// <returns>The new string.</returns>
        [NoDebug]
        public static String Concat(String Left, String Right)
        {
            String NewString = New(Left.Length + Right.Length);
            for (int i = 0; i < Left.Length; i++)
            {
                NewString[i] = Left[i];
            }
            for (int i = 0; i < Right.Length; i++)
            {
                NewString[i + Left.Length] = Right[i];
            }
            return NewString;
        }

        /// <summary>
        ///     Gets a pointer to the first character in the string.
        /// </summary>
        /// <returns>A pointer to the first char (that represents a character) of the specified string.</returns>
        [NoDebug]
        [NoGC]
        public unsafe char* GetCharPointer() => (char*)((byte*)ObjectUtilities.GetHandle(this) + FieldsBytesSize);

        /// <summary>
        ///     Creates a new string and pads the left side of the string with the specified character until the
        ///     whole string is of the specified length or returns the original string if it is longer.
        /// </summary>
        /// <param name="TotalLength">The final length of the whole string.</param>
        /// <param name="PadChar">The character to pad with.</param>
        /// <returns>The new, padded string.</returns>
        [NoDebug]
        public String PadLeft(int TotalLength, char PadChar)
        {
            String Result = New(TotalLength);

            if (Length >= TotalLength)
            {
                for (int i = 0; i < Result.Length; i++)
                {
                    Result[i] = this[i];
                }
                return Result;
            }

            int Offset = TotalLength - Length;
            for (int i = 0; i < Length; i++)
            {
                Result[i + Offset] = this[i];
            }
            for (int i = 0; i < Offset; i++)
            {
                Result[i] = PadChar;
            }
            return Result;
        }

        /// <summary>
        ///     Creates a new string and pads the right side of the string with the specified character until the
        ///     whole string is of the specified length or returns the original string if it is longer.
        /// </summary>
        /// <param name="TotalLength">The final length of the whole string.</param>
        /// <param name="PadChar">The character to pad with.</param>
        /// <returns>The new, padded string.</returns>
        [NoDebug]
        public String PadRight(int TotalLength, char PadChar)
        {
            String Result = New(TotalLength);
            for (int i = 0; i < Length && i < TotalLength; i++)
            {
                Result[i] = this[i];
            }
            for (int i = Length; i < TotalLength; i++)
            {
                Result[i] = PadChar;
            }
            return Result;
        }

        /// <summary>
        ///     Creates a new string and trims all spaces from the beginning and end of the string.
        /// </summary>
        /// <returns>The new, trimmed string.</returns>
        [NoDebug]
        public String Trim()
        {
            // All characters in the Zs, Zp and Zl Unicode categories, plus U+0009 CHARACTER TABULATION, U+000A LINE FEED, U+000B LINE TABULATION, U+000C FORM FEED, U+000D CARRIAGE RETURN and U+0085 NEXT LINE
            String TrimChars =
                "\u0009\u000A\u000B\u000C\u000D\u0020\u0085\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000";

            int RemoveStart = 0;
            int RemoveEnd = 0;
            for (int i = 0; i < Length; RemoveStart++, i++)
            {
                bool ShouldBreak = true;
                for (int j = 0; j < TrimChars.Length; j++)
                {
                    if (this[i] == TrimChars[j])
                    {
                        ShouldBreak = false;
                    }
                }
                if (ShouldBreak)
                {
                    break;
                }
            }
            for (int i = Length - 1; i > RemoveStart; RemoveEnd++, i--)
            {
                bool ShouldBreak = true;
                for (int j = 0; j < TrimChars.Length; j++)
                {
                    if (this[i] == TrimChars[j])
                    {
                        ShouldBreak = false;
                    }
                }
                if (ShouldBreak)
                {
                    break;
                }
            }

            String Result = New(Length - RemoveStart - RemoveEnd);
            for (int i = RemoveStart; i < Length - RemoveEnd; i++)
            {
                Result[i - RemoveStart] = this[i];
            }
            return Result;
        }

        /// <summary>
        ///     Creates a new string and trims all spaces from the end of the string.
        /// </summary>
        /// <returns>The new, trimmed string.</returns>
        [NoDebug]
        public String TrimEnd()
        {
            // All characters in the Zs, Zp and Zl Unicode categories, plus U+0009 CHARACTER TABULATION, U+000A LINE FEED, U+000B LINE TABULATION, U+000C FORM FEED, U+000D CARRIAGE RETURN and U+0085 NEXT LINE
            String TrimChars =
                "\u0009\u000A\u000B\u000C\u000D\u0020\u0085\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000";

            int RemoveEnd = 0;
            for (int i = Length - 1; i > -1; RemoveEnd++, i--)
            {
                bool ShouldBreak = true;
                for (int j = 0; j < TrimChars.Length; j++)
                {
                    if (this[i] == TrimChars[j])
                    {
                        ShouldBreak = false;
                    }
                }
                if (ShouldBreak)
                {
                    break;
                }
            }

            String Result = New(Length - RemoveEnd);
            for (int i = 0; i < Length - RemoveEnd; i++)
            {
                Result[i] = this[i];
            }
            return Result;
        }

        /// <summary>
        ///     Creates a new string that is a copy of the current string starting at the specified index for specified length.
        /// </summary>
        /// <param name="StartIndex">The index to start copying at.</param>
        /// <param name="MaxLength">The number of characters to copy.</param>
        /// <returns>The new string.</returns>
        [NoDebug]
        public String Substring(int StartIndex, int MaxLength)
        {
            if (StartIndex >= Length)
            {
                if (MaxLength == 0)
                {
                    return New(0);
                }
                ExceptionMethods.Throw(new IndexOutOfRangeException(StartIndex, Length));
            }
            else if (MaxLength > Length - StartIndex)
            {
                MaxLength = Length - StartIndex;
            }

            String Result = New(MaxLength);
            for (int i = StartIndex; i < MaxLength + StartIndex; i++)
            {
                Result[i - StartIndex] = this[i];
            }
            return Result;
        }

        /// <summary>
        ///     Determines whether the string starts with the specified string.
        /// </summary>
        /// <param name="Prefix">The string to test for.</param>
        /// <returns>Whether the string starts with the prefix.</returns>
        [NoDebug]
        public bool StartsWith(String Prefix)
        {
            if (Length < Prefix.Length)
            {
                return false;
            }
            for (int i = 0; i < Prefix.Length; i++)
            {
                if (this[i] != Prefix[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Determines whether the string ends with the specified string.
        /// </summary>
        /// <param name="Postfix">The string to test for.</param>
        /// <returns>Whether the string ends with the postfix.</returns>
        [NoDebug]
        public bool EndsWith(String Postfix)
        {
            if (Length < Postfix.Length)
            {
                return false;
            }
            int Offset = Length - Postfix.Length;
            for (int i = Length - 1; i >= Offset; i--)
            {
                if (this[i] != Postfix[i - Offset])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Splits the string at every index where SplitChar occurs and adds the splits parts (excluding SplitChar)
        ///     to a list of strings.
        /// </summary>
        /// <param name="SplitChar">The char to split with.</param>
        /// <returns>The list of split parts.</returns>
        [NoDebug]
        public List Split(char SplitChar)
        {
            List Result = new List(1);

            int LastSplitIndex = 0;
            for (int i = 0; i < Length; i++)
            {
                if (this[i] == SplitChar)
                {
                    Result.Add(Substring(LastSplitIndex, i - LastSplitIndex));
                    LastSplitIndex = i + 1;
                }
            }
            if (Length - LastSplitIndex > 0)
            {
                Result.Add(Substring(LastSplitIndex, Length - LastSplitIndex));
            }

            return Result;
        }

        /// <summary>
        ///     Copies the current string then converts all the alpha-characters to upper-case.
        /// </summary>
        /// <returns>The new, upper-case string.</returns>
        [NoDebug]
        public String ToUpper()
        {
            if (Length == 0)
                return "";

            String Result = New(Length);

            for (int i = 0; i < Result.Length; i++)
            {
                char CurrentChar = this[i];
                if (CurrentChar >= 'a' && CurrentChar <= 'z')
                {
                    CurrentChar = (char)('A' + (CurrentChar - 'a'));
                }
                Result[i] = CurrentChar;
            }

            return Result;
        }

        /// <summary>
        ///     Copies the current string then converts all the alpha-characters to lower-case.
        /// </summary>
        /// <returns>The new, lower-case string.</returns>
        [NoDebug]
        public String ToLower()
        {
            String Result = New(Length);

            for (int i = 0; i < Result.Length; i++)
            {
                char CurrentChar = this[i];
                if (CurrentChar >= 'A' && CurrentChar <= 'Z')
                {
                    CurrentChar = (char)('a' + (CurrentChar - 'A'));
                }
                Result[i] = CurrentChar;
            }

            return Result;
        }

        /// <summary>
        ///     Finds the first index of the specified character in the string.
        /// </summary>
        /// <param name="c">The character to find.</param>
        /// <returns>The first instance of the character or -1 if not found.</returns>
        [NoDebug]
        [NoGC]
        public int IndexOf(char c)
        {
            int Result = -1;
            for (int i = 0; i < Length; i++)
            {
                if (this[i] == c)
                {
                    Result = i;
                    break;
                }
            }
            return Result;
        }

        /// <summary>
        ///     Finds the last index of the specified character in the string.
        /// </summary>
        /// <param name="c">The character to find.</param>
        /// <returns>The last instance of the character or -1 if not found.</returns>
        [NoDebug]
        [NoGC]
        public int LastIndexOf(char c)
        {
            int Result = -1;
            for (int i = Length - 1; i > -1; i--)
            {
                if (this[i] == c)
                {
                    Result = i;
                    break;
                }
            }
            return Result;
        }

        /// <summary>
        ///     Concatenates two FlingOS strings.
        /// </summary>
        /// <param name="Left">The first string.</param>
        /// <param name="Right">The second string.</param>
        /// <returns>The new contenated string.</returns>
        [NoDebug]
        public static String operator +(String Left, String Right)
        {
            if (Left == null)
            {
                // If Right is null, then the best we could do is return null anyway!
                return Right;
            }
            // Likewise, if Left is null, then the best we could do is return null anyway!
            return Right == null ? Left : Concat(Left, Right);
        }

        /// <summary>
        ///     Tests whether all the characters of two FlingOS strings are equal.
        /// </summary>
        /// <param name="Left">The first string.</param>
        /// <param name="Right">The second string.</param>
        /// <returns>Whether the two strings are identical or not.</returns>
        [NoDebug]
        [NoGC]
        public static unsafe bool operator ==(String Left, String Right)
        {
            bool AreEqual = true;

            //Prevent recursive calls to this "==" implicit method!
            if (ObjectUtilities.GetHandle(Left) == null ||
                ObjectUtilities.GetHandle(Right) == null)
            {
                if (ObjectUtilities.GetHandle(Left) == null &&
                    ObjectUtilities.GetHandle(Right) == null)
                {
                    return true;
                }
                return false;
            }

            if (Left.Length != Right.Length)
            {
                AreEqual = false;
            }
            else
            {
                for (int i = 0; i < Left.Length; i++)
                {
                    if (Left[i] != Right[i])
                    {
                        AreEqual = false;
                        break;
                    }
                }
            }

            return AreEqual;
        }

        /// <summary>
        ///     Tests whether any of the characters of two FlingOS strings are not equal.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns>Whether the two strings mismatch in any place.</returns>
        [NoDebug]
        [NoGC]
        public static bool operator !=(String x, String y) => !(x == y);

        /// <summary>
        ///     Implicitly converts the specified value to a Framework.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        [NoGC]
        public static implicit operator String(string x) => (object)x as String;

        /// <summary>
        ///     Implicitly converts the specified value to a Framework.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(bool x) => x ? "True" : "False";

        /// <summary>
        ///     Implicitly converts the specified Framework.String to a System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The System.String.</returns>
        [NoDebug]
        public static explicit operator string(String x) => (object)x as string;

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(byte Value) => ConvertToString(New(4), Value);

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(ushort Value) => ConvertToString(New(6), Value);

        /// <summary>
        ///     Implicitly converts the specified value to an Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(char Value)
        {
            String Result = New(1);
            Result[0] = Value;
            return Result;
        }

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(short Value) => (ushort)Value;

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(uint Value) => ConvertToString(New(10), Value);

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(int Value) => (uint)Value;

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(ulong Value) => ConvertToString(New(18), Value);

        /// <summary>
        ///     Implicitly converts the specified value to a hex Framework.String.
        /// </summary>
        /// <param name="Value">The value to convert.</param>
        /// <returns>The Framework.String value.</returns>
        [NoDebug]
        public static implicit operator String(long Value) => (ulong)Value;

        private static String ConvertToString(String Result, ulong Value)
        {
            Result[0] = '0';
            Result[1] = 'x';
            for (int i = 2; i < Result.Length; i++)
            {
                Result[i] = '0';
            }

            int Index = Result.Length - 1;
            ulong y = Value;
            while (y > 0)
            {
                uint Rem = (uint)(y & 0xFu);
                switch (Rem)
                {
                    case 0:
                        Result[Index] = '0';
                        break;
                    case 1:
                        Result[Index] = '1';
                        break;
                    case 2:
                        Result[Index] = '2';
                        break;
                    case 3:
                        Result[Index] = '3';
                        break;
                    case 4:
                        Result[Index] = '4';
                        break;
                    case 5:
                        Result[Index] = '5';
                        break;
                    case 6:
                        Result[Index] = '6';
                        break;
                    case 7:
                        Result[Index] = '7';
                        break;
                    case 8:
                        Result[Index] = '8';
                        break;
                    case 9:
                        Result[Index] = '9';
                        break;
                    case 10:
                        Result[Index] = 'A';
                        break;
                    case 11:
                        Result[Index] = 'B';
                        break;
                    case 12:
                        Result[Index] = 'C';
                        break;
                    case 13:
                        Result[Index] = 'D';
                        break;
                    case 14:
                        Result[Index] = 'E';
                        break;
                    case 15:
                        Result[Index] = 'F';
                        break;
                }
                y >>= 4;
                Index--;
            }
            return Result;
        }
    }
}