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

namespace Kernel.FOS_System
{
    /// <summary>
    /// Replacement class for methods, properties and fields usually found on standard System.String type.
    /// Also contains utility methods for low-level string manipulation.
    /// </summary>
    [Drivers.Compiler.Attributes.StringClass]
    public sealed class String : Object
    {
        /* If you add more fields here, remember to update the compiler and all the ASM files that depend on the string
           class structure ( i.e. do all the hard work! ;) )
         */

        /// <summary>
        /// The size of the fields in an string object that come before the actual string data.
        /// </summary>
        public const uint FieldsBytesSize = 8;

        /// <summary>
        /// The length of the string.
        /// </summary>
        public int length;

        /*   ----------- DO NOT CREATE A CONSTRUCTOR FOR THIS CLASS - IT WILL NEVER BE CALLED IF YOU DO ----------- */

        /// <summary>
        /// Creates a new, blank FOS_System.String of specified length.
        /// IMPORTANT NOTE: You MUST assign the return value of this to a variable / local / arg / 
        /// field etc. You may not use IL or C# that results in an IL Pop op of the return value
        /// of this method as it will screw up the GC RefCount handling.
        /// </summary>
        /// <param name="length">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static unsafe FOS_System.String New(int length)
        {
            if(length < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("Parameter \"length\" cannot be less than 0 in FOS_System.String.New(int length)."));
            }
            FOS_System.String result = (FOS_System.String)Utilities.ObjectUtilities.GetObject(GC.NewString(length));
            if (result == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            return result;
        }

        /// <summary>
        /// Concatenates two strings into one new string.
        /// </summary>
        /// <param name="str1">The first part of the new string.</param>
        /// <param name="str2">The second part of the new string.</param>
        /// <returns>The new string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static unsafe FOS_System.String Concat(FOS_System.String str1, FOS_System.String str2)
        {
            FOS_System.String newStr = New(str1.length + str2.length);
            for (int i = 0; i < str1.length; i++)
            {
                newStr[i] = str1[i];
            }
            for (int i = 0; i < str2.length; i++)
            {
                newStr[i + str1.length] = str2[i];
            }
            return newStr;
        }

        /// <summary>
        /// Gets the character at the specified index.
        /// </summary>
        /// <param name="index">The index of the character to get.</param>
        /// <returns>The character at the specified index.</returns>
        public unsafe char this[int index]
        {
            [Drivers.Compiler.Attributes.NoDebug]
            [Drivers.Compiler.Attributes.NoGC]
            get
            {
                byte* thisPtr = (byte*)Utilities.ObjectUtilities.GetHandle(this);
                thisPtr += 8; /*For fields inc. inherited*/
                return ((char*)thisPtr)[index];
            }
            [Drivers.Compiler.Attributes.NoDebug]
            [Drivers.Compiler.Attributes.NoGC]
            set
            {
                byte* thisPtr = (byte*)Utilities.ObjectUtilities.GetHandle(this);
                thisPtr += 8; /*For fields inc. inherited*/
                ((char*)thisPtr)[index] = value;
            }
        }
        /// <summary>
        /// Gets the character at the specified index.
        /// </summary>
        /// <param name="index">The index of the character to get.</param>
        /// <returns>The character at the specified index.</returns>
        public unsafe char this[uint index]
        {
            [Drivers.Compiler.Attributes.NoDebug]
            [Drivers.Compiler.Attributes.NoGC]
            get
            {
                byte* thisPtr = (byte*)Utilities.ObjectUtilities.GetHandle(this);
                thisPtr += 8; /*For fields inc. inherited*/
                return ((char*)thisPtr)[index];
            }
            [Drivers.Compiler.Attributes.NoDebug]
            [Drivers.Compiler.Attributes.NoGC]
            set
            {
                byte* thisPtr = (byte*)Utilities.ObjectUtilities.GetHandle(this);
                thisPtr += 8; /*For fields inc. inherited*/
                ((char*)thisPtr)[index] = value;
            }
        }
        /// <summary>
        /// Gets a pointer to the first character in the string.
        /// </summary>
        /// <returns>A pointer to the first char (that represents a character) of the specified string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public unsafe char* GetCharPointer()
        {
            return (char*)(((byte*)Utilities.ObjectUtilities.GetHandle(this)) + FieldsBytesSize);
        }

        /// <summary>
        /// Creates a new string and pads the left side of the string with the specified character until the 
        /// whole string is of the specified length or returns the original string if it is longer.
        /// </summary>
        /// <param name="totalLength">The final length of the whole string.</param>
        /// <param name="padChar">The character to pad with.</param>
        /// <returns>The new, padded string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String PadLeft(int totalLength, char padChar)
        {
            FOS_System.String result = New(totalLength);

            if (this.length >= totalLength)
            {
                for (int i = 0; i < result.length; i++)
                {
                    result[i] = this[i];
                }
                return result;
            }

            int offset = totalLength - this.length;
            for (int i = 0; i < this.length; i++)
            {
                result[i + offset] = this[i];
            }
            for (int i = 0; i < offset; i++)
            {
                result[i] = padChar;
            }
            return result;
        }
        /// <summary>
        /// Creates a new string and pads the right side of the string with the specified character until the 
        /// whole string is of the specified length or returns the original string if it is longer.
        /// </summary>
        /// <param name="totalLength">The final length of the whole string.</param>
        /// <param name="padChar">The character to pad with.</param>
        /// <returns>The new, padded string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String PadRight(int totalLength, char padChar)
        {
            FOS_System.String result = New(totalLength);
            for (int i = 0; i < this.length && i < totalLength; i++)
            {
                result[i] = this[i];
            }
            for (int i = this.length; i < totalLength; i++)
            {
                result[i] = padChar;
            }
            return result;
        }
        /// <summary>
        /// Creates a new string and trims all spaces from the beginning and end of the string.
        /// </summary>
        /// <returns>The new, trimmed string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String Trim()
        {
            // All characters in the Zs, Zp and Zl Unicode categories, plus U+0009 CHARACTER TABULATION, U+000A LINE FEED, U+000B LINE TABULATION, U+000C FORM FEED, U+000D CARRIAGE RETURN and U+0085 NEXT LINE
            FOS_System.String TrimChars = "\u0009\u000A\u000B\u000C\u000D\u0020\u0085\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000";

            int removeStart = 0;
            int removeEnd = 0;
            for (int i = 0; i < this.length; removeStart++, i++)
            {
                bool ShouldBreak = true;
                for (int j = 0; j < TrimChars.length; j++)
                {
                    if(this[i] == TrimChars[j])
                    {
                        ShouldBreak = false;
                    }
                }
                if(ShouldBreak)
                {
                    break;
                }
            }
            for (int i = this.length - 1; i > removeStart; removeEnd++, i--)
            {
                bool ShouldBreak = true;
                for (int j = 0; j < TrimChars.length; j++)
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

            FOS_System.String result = New(this.length - removeStart - removeEnd);
            for (int i = removeStart; i < this.length - removeEnd; i++)
            {
                result[i - removeStart] = this[i];
            }
            return result;
        }
        /// <summary>
        /// Creates a new string and trims all spaces from the end of the string.
        /// </summary>
        /// <returns>The new, trimmed string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String TrimEnd()
        {
            // All characters in the Zs, Zp and Zl Unicode categories, plus U+0009 CHARACTER TABULATION, U+000A LINE FEED, U+000B LINE TABULATION, U+000C FORM FEED, U+000D CARRIAGE RETURN and U+0085 NEXT LINE
            FOS_System.String TrimChars = "\u0009\u000A\u000B\u000C\u000D\u0020\u0085\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u2028\u2029\u202F\u205F\u3000";

            int removeEnd = 0;
            for (int i = this.length - 1; i > -1; removeEnd++, i--)
            {
                bool ShouldBreak = true;
                for (int j = 0; j < TrimChars.length; j++)
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

            FOS_System.String result = New(this.length - removeEnd);
            for (int i = 0; i < this.length - removeEnd; i++)
            {
                result[i] = this[i];
            }
            return result;
        }
        /// <summary>
        /// Creates a new string that is a copy of the current string starting at the specified index for specified length.
        /// </summary>
        /// <param name="startIndex">The index to start copying at.</param>
        /// <param name="aLength">The number of characters to copy.</param>
        /// <returns>The new string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String Substring(int startIndex, int aLength)
        {
            if (startIndex >= this.length)
            {
                if (aLength == 0)
                {
                    return New(0);
                }
                ExceptionMethods.Throw(new Exceptions.IndexOutOfRangeException(startIndex, this.length));
            }
            else if (aLength > length - startIndex)
            {
                aLength = length - startIndex;
            }
            
            FOS_System.String result = New(aLength);
            for (int i = startIndex; i < aLength + startIndex; i++)
            {
                result[i - startIndex] = this[i];
            }
            return result;
        }
        /// <summary>
        /// Determines whether the string starts with the specified string.
        /// </summary>
        /// <param name="prefix">The string to test for.</param>
        /// <returns>Whether the string starts with the prefix.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public bool StartsWith(FOS_System.String prefix)
        {
            if (this.length < prefix.length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < prefix.length; i++)
                {
                    if (this[i] != prefix[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// Determines whether the string ends with the specified string.
        /// </summary>
        /// <param name="postfix">The string to test for.</param>
        /// <returns>Whether the string ends with the postfix.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public bool EndsWith(FOS_System.String postfix)
        {
            if (this.length < postfix.length)
            {
                return false;
            }
            else
            {
                int offset = this.length - postfix.length;
                for (int i = this.length - 1; i >= offset; i--)
                {
                    if (this[i] != postfix[i - offset])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// Splits the string at every index where splitChar occurs and adds the splits parts (excluding splitChar)
        /// to a list of strings.
        /// </summary>
        /// <param name="splitChar">The char to split with.</param>
        /// <returns>The list of split parts.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public Collections.List Split(char splitChar)
        {
            Collections.List result = new Collections.List(1);

            int lastSplitIndex = 0;
            for (int i = 0; i < this.length; i++)
            {
                if (this[i] == splitChar)
                {
                    result.Add(this.Substring(lastSplitIndex, i - lastSplitIndex));
                    lastSplitIndex = i + 1;
                }
            }
            if (this.length - lastSplitIndex > 0)
            {
                result.Add(this.Substring(lastSplitIndex, this.length - lastSplitIndex));
            }

            return result;
        }
        /// <summary>
        /// Copies the current string then converts all the alpha-characters to upper-case.
        /// </summary>
        /// <returns>The new, upper-case string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String ToUpper()
        {
            if (this.length == 0)
                return "";

            FOS_System.String result = New(this.length);

            for (int i = 0; i < result.length; i++)
            {
                char cChar = this[i];
                if (cChar >= 'a' && cChar <= 'z')
                {
                    cChar = (char)('A' + (cChar - 'a'));
                }
                result[i] = cChar;
            }

            return result;
        }
        /// <summary>
        /// Copies the current string then converts all the alpha-characters to lower-case.
        /// </summary>
        /// <returns>The new, lower-case string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public FOS_System.String ToLower()
        {
            FOS_System.String result = New(this.length);

            for (int i = 0; i < result.length; i++)
            {
                char cChar = this[i];
                if (cChar >= 'A' && cChar <= 'Z')
                {
                    cChar = (char)('a' + (cChar - 'A'));
                }
                result[i] = cChar;
            }

            return result;
        }

        /// <summary>
        /// Finds the first index of the specified character in the string.
        /// </summary>
        /// <param name="c">The character to find.</param>
        /// <returns>The first instance of the character or -1 if not found.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public int IndexOf(char c)
        {
            int result = -1;
            for (int i = 0; i < length; i++)
            {
                if (this[i] == c)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// Finds the last index of the specified character in the string.
        /// </summary>
        /// <param name="c">The character to find.</param>
        /// <returns>The last instance of the character or -1 if not found.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public int LastIndexOf(char c)
        {
            int result = -1;
            for (int i = length - 1; i > -1; i--)
            {
                if (this[i] == c)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }
        
        /// <summary>
        /// Concatenates two strings using "+" operator.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns>The new contenated string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static FOS_System.String operator +(FOS_System.String x, FOS_System.String y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return null;
                }
                else
                {
                    return y;
                }
            }
            else if (y == null)
            {
                return x;
            }

            return FOS_System.String.Concat(x, y);
        }
        /// <summary>
        /// Tests whether all the characters of two strings are equal.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns>Whether the two strings are identical or not.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe bool operator ==(FOS_System.String x, FOS_System.String y)
        {
            bool equal = true;

            //Prevent recursive calls to this "==" implicit method!
            if (Utilities.ObjectUtilities.GetHandle(x) == null || 
                Utilities.ObjectUtilities.GetHandle(y) == null)
            {
                if (Utilities.ObjectUtilities.GetHandle(x) == null &&
                    Utilities.ObjectUtilities.GetHandle(y) == null)
                {
                    return true;
                }
                return false;
            }

            if (x.length != y.length)
            {
                equal = false;
            }
            else
            {
                for (int i = 0; i < x.length; i++)
                {
                    if (x[i] != y[i])
                    {
                        equal = false;
                        break;
                    }
                }
            }

            return equal;
        }
        /// <summary>
        /// Tests whether any of the characters of two strings are not equal.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns>Whether the two strings mismatch in any place.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static bool operator !=(FOS_System.String x, FOS_System.String y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the specified value to an FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator FOS_System.String(string x)
        {
            return (FOS_System.String)(object)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to an FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(bool x)
        {
            return x ? "True" : "False";
        }
        /// <summary>
        /// Implicitly converts the specified FOS_System.String to a System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The System.String.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static explicit operator string(FOS_System.String x)
        {
            return (string)(object)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(byte x)
        {
            return ConvertToString(New(4), x);
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(UInt16 x)
        {
            return ConvertToString(New(6), x);
        }
        /// <summary>
        /// Implicitly converts the specified value to an FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(char x)
        {
            FOS_System.String result = FOS_System.String.New(1);
            result[0] = x;
            return result;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(Int16 x)
        {
            return (UInt16)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(uint x)
        {
            return ConvertToString(New(10), x);
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(int x)
        {
            return (uint)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(ulong x)
        {
            return ConvertToString(New(18), x);
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex FOS_System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The FOS_System.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static implicit operator FOS_System.String(long x)
        {
            return (ulong)x;
        }

        private static String ConvertToString(FOS_System.String result, ulong x)
        {
            result[0] = '0';
            result[1] = 'x';
            for (int i = 2; i < result.length; i++)
            {
                result[i] = '0';
            }

            int index = 9;
            ulong y = x;
            while (y > 0)
            {
                ulong rem = y & 0xFu;
                switch (rem)
                {
                    case 0:
                        result[index] = '0';
                        break;
                    case 1:
                        result[index] = '1';
                        break;
                    case 2:
                        result[index] = '2';
                        break;
                    case 3:
                        result[index] = '3';
                        break;
                    case 4:
                        result[index] = '4';
                        break;
                    case 5:
                        result[index] = '5';
                        break;
                    case 6:
                        result[index] = '6';
                        break;
                    case 7:
                        result[index] = '7';
                        break;
                    case 8:
                        result[index] = '8';
                        break;
                    case 9:
                        result[index] = '9';
                        break;
                    case 10:
                        result[index] = 'A';
                        break;
                    case 11:
                        result[index] = 'B';
                        break;
                    case 12:
                        result[index] = 'C';
                        break;
                    case 13:
                        result[index] = 'D';
                        break;
                    case 14:
                        result[index] = 'E';
                        break;
                    case 15:
                        result[index] = 'F';
                        break;
                }
                y >>= 4;
                index--;
            }
            return result;
        }
    }
}
