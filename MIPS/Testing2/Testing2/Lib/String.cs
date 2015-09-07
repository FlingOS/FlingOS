using System;

namespace Testing2
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
        /// Creates a new, blank Testing2.String of specified length.
        /// IMPORTANT NOTE: You MUST assign the return value of this to a variable / local / arg / 
        /// field etc. You may not use IL or C# that results in an IL Pop op of the return value
        /// of this method as it will screw up the GC RefCount handling.
        /// </summary>
        /// <param name="length">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static unsafe Testing2.String New(int length)
        {
            if (length < 0)
            {
                UART.Write("ArgumentException! String.New, length less than zero.");
                //ExceptionMethods.Throw(new Exceptions.ArgumentException("Parameter \"length\" cannot be less than 0 in Testing2.String.New(int length)."));
            }
            Testing2.String result = (Testing2.String)Utilities.ObjectUtilities.GetObject(GC.NewString(length));
            if (result == null)
            {
                UART.Write("NullReferenceException! String.New, result is null.");
                //ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            return result;
        }

        /// <summary>
        /// Concatenates two strings into one new string.
        /// </summary>
        /// <param name="str1">The first part of the new string.</param>
        /// <param name="str2">The second part of the new string.</param>
        /// <returns>The new string.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static unsafe Testing2.String Concat(Testing2.String str1, Testing2.String str2)
        {
            Testing2.String newStr = New(str1.length + str2.length);

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
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String PadLeft(int totalLength, char padChar)
        {
            Testing2.String result = New(totalLength);

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
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String PadRight(int totalLength, char padChar)
        {
            Testing2.String result = New(totalLength);
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
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String Trim()
        {
            Testing2.String TrimChars = "\n\r ";

            int removeStart = 0;
            int removeEnd = 0;
            for (int i = 0; i < this.length; removeStart++, i++)
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

            Testing2.String result = New(this.length - removeStart - removeEnd);
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
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String TrimEnd()
        {
            int removeEnd = 0;
            for (int i = this.length - 1; i > -1; removeEnd++, i--)
            {
                if (this[i] != ' ')
                {
                    break;
                }
            }

            Testing2.String result = New(this.length - removeEnd);
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
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String Substring(int startIndex, int aLength)
        {
            if (startIndex >= this.length)
            {
                if (aLength == 0)
                {
                    return New(0);
                }
                UART.Write("IndexOutOfRangeException! Substring.");
                //ExceptionMethods.Throw(new Exceptions.IndexOutOfRangeException(startIndex, this.length));
            }
            else if (aLength > length - startIndex)
            {
                aLength = length - startIndex;
            }

            Testing2.String result = New(aLength);
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
        [Drivers.Compiler.Attributes.NoGC]
        public bool StartsWith(Testing2.String prefix)
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
        [Drivers.Compiler.Attributes.NoGC]
        public bool EndsWith(Testing2.String postfix)
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
        /// Copies the current string then converts all the alpha-characters to upper-case.
        /// </summary>
        /// <returns>The new, upper-case string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String ToUpper()
        {
            if (this.length == 0)
                return "";

            Testing2.String result = New(this.length);

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
        [Drivers.Compiler.Attributes.NoGC]
        public Testing2.String ToLower()
        {
            Testing2.String result = New(this.length);

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
        [Drivers.Compiler.Attributes.NoGC]
        public static Testing2.String operator +(Testing2.String x, Testing2.String y)
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

            return Testing2.String.Concat(x, y);
        }
        /// <summary>
        /// Tests whether all the characters of two strings are equal.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        /// <returns>Whether the two strings are identical or not.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe bool operator ==(Testing2.String x, Testing2.String y)
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
        public static bool operator !=(Testing2.String x, Testing2.String y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the specified value to an Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(bool x)
        {
            return x ? "True" : "False";
        }
        /// <summary>
        /// Implicitly converts the specified value to an Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(string x)
        {
            return (Testing2.String)(object)x;
        }
        /// <summary>
        /// Implicitly converts the specified Testing2.String to a System.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The System.String.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static explicit operator string(Testing2.String x)
        {
            return (string)(object)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(byte x)
        {
            Testing2.String result = "";
            uint y = x;
            while (y > 0)
            {
                uint rem = y % 16u;
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
                    case 10:
                        result = "A" + result;
                        break;
                    case 11:
                        result = "B" + result;
                        break;
                    case 12:
                        result = "C" + result;
                        break;
                    case 13:
                        result = "D" + result;
                        break;
                    case 14:
                        result = "E" + result;
                        break;
                    case 15:
                        result = "F" + result;
                        break;
                }
                y = y / 16u;
            }
            result = "0x" + result.PadLeft(2, '0');
            return result;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(UInt16 x)
        {
            Testing2.String result = "";
            uint y = x;
            while (y > 0)
            {
                uint rem = y & 0xFu;
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
                    case 10:
                        result = "A" + result;
                        break;
                    case 11:
                        result = "B" + result;
                        break;
                    case 12:
                        result = "C" + result;
                        break;
                    case 13:
                        result = "D" + result;
                        break;
                    case 14:
                        result = "E" + result;
                        break;
                    case 15:
                        result = "F" + result;
                        break;
                }
                y >>= 4;
            }
            return "0x" + result.PadLeft(4, '0');
        }
        /// <summary>
        /// Implicitly converts the specified value to an Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(char x)
        {
            Testing2.String result = Testing2.String.New(1);
            result[0] = x;
            return result;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(Int16 x)
        {
            return (UInt16)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(uint x)
        {
            Testing2.String result = "";
            uint y = x;
            while (y > 0)
            {
                uint rem = y & 0xFu;
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
                    case 10:
                        result = "A" + result;
                        break;
                    case 11:
                        result = "B" + result;
                        break;
                    case 12:
                        result = "C" + result;
                        break;
                    case 13:
                        result = "D" + result;
                        break;
                    case 14:
                        result = "E" + result;
                        break;
                    case 15:
                        result = "F" + result;
                        break;
                }
                y >>= 4;
            }
            return "0x" + result.PadLeft(8, '0');
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(int x)
        {
            return (uint)x;
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(ulong x)
        {
            uint part1 = (uint)x;
            uint part2 = (uint)(x >> 16 >> 16);
            return ((Testing2.String)part2) + " " + ((Testing2.String)part1);
        }
        /// <summary>
        /// Implicitly converts the specified value to a hex Testing2.String.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The Testing2.String value.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static implicit operator Testing2.String(long x)
        {
            return (ulong)x;
        }
    }
}
