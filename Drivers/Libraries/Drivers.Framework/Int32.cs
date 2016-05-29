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

namespace Drivers.Framework
{
    /// <summary>
    ///     Replacement class for methods, properties and fields usually found on standard System.Int32 type.
    /// </summary>
    public static class Int32
    {
        /// <summary>
        ///     Returns the maximum value of an Int32.
        /// </summary>
        public static int MaxValue
        {
            get { return 2147483647; }
        }

        /// <summary>
        ///     Parses a string as an unsigned decimal integer.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="offset">The offset into the string at which to start parsing.</param>
        /// <returns>The parsed uint.</returns>
        public static uint Parse_DecimalUnsigned(String str, int offset)
        {
            uint result = 0;
            for (int i = offset; i < str.length; i++)
            {
                char c = str[i];
                if (c < '0' || c > '9')
                {
                    break;
                }
                result *= 10;
                result += (uint)(c - '0');
            }
            return result;
        }

        /// <summary>
        ///     Parses a string as an signed decimal integer.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>The parsed int.</returns>
        public static int Parse_DecimalSigned(String str)
        {
            bool neg = str.StartsWith("-");
            int result = (int)Parse_DecimalUnsigned(str, neg ? 1 : 0);
            if (neg)
            {
                result *= -1;
            }
            return result;
        }

        /// <summary>
        ///     Parses a string as an unsigned hexadecimal integer.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="offset">The offset into the string at which to start parsing.</param>
        /// <returns>The parsed uint.</returns>
        public static uint Parse_HexadecimalUnsigned(String str, int offset)
        {
            str = str.ToLower();

            if (str.length - offset >= 2)
            {
                if (str[offset] == '0' && str[offset + 1] == 'x')
                {
                    offset += 2;
                }
            }

            uint result = 0;
            for (int i = offset; i < str.length; i++)
            {
                char c = str[i];
                if ((c < '0' || c > '9') && (c < 'a' || c > 'f'))
                {
                    break;
                }
                result *= 16;
                if (c >= '0' && c <= '9')
                {
                    result += (uint)(c - '0');
                }
                else
                {
                    result += (uint)(c - 'a') + 10;
                }
            }
            return result;
        }

        public static String ToDecimalString(int num)
        {
            //This functions exactly the same as its unsigned 
            //  counterpart but it adds a minus sign if the number
            //  is negative.
            String result = "";
            if (num != 0)
            {
                bool neg = num < 0;
                if (neg)
                {
                    num = -num;
                }

                while (num > 0)
                {
                    int rem = num%10;
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

                if (neg)
                {
                    result = "-" + result;
                }
            }
            else
            {
                result = "0";
            }
            return result;
        }
    }
}