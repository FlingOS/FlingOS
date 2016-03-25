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

namespace Kernel.FOS_System
{
    /// <summary>
    /// Methods for converting to and from byte arrays.
    /// </summary>
    public static class ByteConverter
    {
        /// <summary>
        /// Converts 2 bytes from the specified byte array at the specified index into a UInt16.
        /// </summary>
        /// <param name="n">The byte array from which to convert bytes.</param>
        /// <param name="aPos">The index of the first of the two bytes to convert.</param>
        /// <returns>The converted UInt16.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static UInt16 ToUInt16(byte[] n, UInt32 aPos)
        {
            return (UInt16)((UInt16)n[aPos + 1] << 8 | (UInt16)n[aPos]);
        }
        /// <summary>
        /// Converts 4 bytes from the specified byte array at the specified index into a UInt32.
        /// </summary>
        /// <param name="n">The byte array from which to convert bytes.</param>
        /// <param name="aPos">The index of the first of the four bytes to convert.</param>
        /// <returns>The converted UInt32.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static UInt32 ToUInt32(byte[] n, UInt32 aPos)
        {
            return ((UInt32)n[aPos + 3] << 24 | (UInt32)n[aPos + 2] << 16 |
                    (UInt32)n[aPos + 1] << 8 | (UInt32)n[aPos]);
        }
        /// <summary>
        /// Converts 8 bytes from the specified byte array at the specified index into a UInt64.
        /// </summary>
        /// <param name="n">The byte array from which to convert bytes.</param>
        /// <param name="aPos">The index of the first of the four bytes to convert.</param>
        /// <returns>The converted UInt64.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static UInt64 ToUInt64(byte[] n, UInt32 aPos)
        {
            return ((UInt64)n[aPos + 7] << 54 | (UInt64)n[aPos + 6] << 48 | 
                    (UInt64)n[aPos + 5] << 40 | (UInt64)n[aPos + 4] << 32 |
                    (UInt64)n[aPos + 3] << 24 | (UInt64)n[aPos + 2] << 16 |
                    (UInt64)n[aPos + 1] << 8  | (UInt64)n[aPos]);
        }
        /// <summary>
        /// Converts the specified ASCII encoded string into an array of ASCII encoded bytes.
        /// </summary>
        /// <param name="asciiString">The ASCII encoded string to convert.</param>
        /// <returns>The ASCII encoded bytes.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static byte[] GetASCIIBytes(FOS_System.String asciiString)
        {
            byte[] result = new byte[asciiString.length];
            for(int i = 0; i < asciiString.length; i++)
            {
                result[i] = (byte)asciiString[i];
            }
            return result;
        }
        /// <summary>
        /// Converts the specified ASCII encoded string into an array of UTF16 encoded bytes.
        /// </summary>
        /// <param name="asciiString">The ASCII encoded string to convert.</param>
        /// <param name="offset">The offset within the ASCII string at which to start converting.</param>
        /// <param name="count">The number of characters to convert.</param>
        /// <returns>The UTF16 encoded bytes.</returns>
        /// <remarks>
        /// This method does not add the null termination character "\0" to the end of the bytes.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        public static byte[] GetUTF16Bytes(FOS_System.String asciiString, int offset, int count)
        {
            if (count == 0)
            {
                return new byte[0];
            }

            byte[] result = new byte[count * 2];
            int endIndex = offset + count;
            if (endIndex > asciiString.length)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("ByteConverter.GetUTF16Bytes: offset + count >= asciiString.length!"));
            }
            for (int i = offset, j = 0; i < endIndex; i++, j += 2)
            {
                result[j] = (byte)asciiString[i];
                result[j + 1] = (byte)(asciiString[i] >> 8);
            }
            return result;
        }
        /// <summary>
        /// Converts the specified bytes to an ASCII encoded string, treating the bytes as ASCII encoded bytes.
        /// </summary>
        /// <param name="n">The bytes to convert.</param>
        /// <param name="aStart">The index in the array at which to start converting bytes.</param>
        /// <param name="aCharCount">The number of characters to convert.</param>
        /// <returns>The ASCII encoded string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static FOS_System.String GetASCIIStringFromASCII(byte[] n, UInt32 aStart, UInt32 aCharCount)
        {
            // Shortcut all of the effort below
            if (aCharCount == 0 || n[0] == 0)
            {
                return FOS_System.String.New(0);
            }

            {
                uint maxExtent = (aCharCount + aStart);
                uint i = aStart;
                for (;  i < n.Length && i < maxExtent; i++)
                {
                    if (n[i] == 0)
                    {
                        break;
                    }
                }
                aCharCount = i - aStart;
            }

            FOS_System.String result = FOS_System.String.New((int)aCharCount);

            if (result == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            else
            {
                for (int i = 0; i < aCharCount && (i + aStart) < n.Length; i++)
                {
                    UInt32 pos = (UInt32)(aStart + i);
                    UInt16 aChar = (UInt16)(n[pos]);
                    if (aChar == 0)
                    {
                        return result.Substring(0, i);
                    }
                    result[i] = (char)aChar;
                }
            }

            return result;
        }
        /// <summary>
        /// Converts the specified bytes to an ASCII encoded string, treating the bytes as ASCII encoded bytes.
        /// </summary>
        /// <param name="n">The bytes to convert.</param>
        /// <param name="aStart">The index in the array at which to start converting bytes.</param>
        /// <param name="aCharCount">The number of characters to convert.</param>
        /// <returns>The ASCII encoded string.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        public static unsafe FOS_System.String GetASCIIStringFromASCII(byte* n, UInt32 aStart, UInt32 aCharCount)
        {
            // Shortcut all of the effort below
            if (aCharCount == 0 || n[0] == 0)
            {
                return FOS_System.String.New(0);
            }

            {
                uint maxExtent = (aCharCount + aStart);
                uint i = aStart;
                for (; i < maxExtent; i++)
                {
                    if (n[i] == 0)
                    {
                        break;
                    }
                }
                aCharCount = i - aStart;
            }

            FOS_System.String result = FOS_System.String.New((int)aCharCount);
            
            if (result == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            else
            {
                for (int i = 0; i < aCharCount; i++)
                {
                    UInt32 pos = (UInt32)(aStart + i);
                    UInt16 aChar = (UInt16)(n[pos]);
                    if (aChar == 0)
                    {
                        return result.Substring(0, i);
                    }
                    result[i] = (char)aChar;
                }
            }

            return result;
        }
        /// <summary>
        /// Converts the specified bytes to an ASCII encoded string, treating the bytes as UTF16 encoded bytes.
        /// </summary>
        /// <param name="n">The bytes to convert.</param>
        /// <param name="aStart">The index in the array at which to start converting bytes.</param>
        /// <param name="aCharCount">The number of characters to convert.</param>
        /// <returns>The ASCII encoded string.</returns>
        /// <remarks>
        /// This method does not handle removing the null termination character if it is present.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        public static FOS_System.String GetASCIIStringFromUTF16(byte[] n, UInt32 aStart, UInt32 aCharCount)
        {
            //If you change this method, change the pointer version below too.

            // Shortcut all of the effort below
            if (aCharCount == 0 || (n[0] == 0 && n[1] == 0))
            {
                return FOS_System.String.New(0);
            }

            {
                uint maxExtent = (aCharCount * 2) + aStart;
                uint i = aStart;
                for (; i < n.Length && i < maxExtent; i += 2)
                {
                    if (n[i] == 0 && n[i + 1] == 0)
                    {
                        break;
                    }
                }
                aCharCount = (i - aStart) / 2;
            }

            FOS_System.String result = FOS_System.String.New((int)aCharCount);
            
            if (result == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            else
            {
                for (int i = 0; i < aCharCount && ((i * 2) + aStart + 1) < n.Length; i++)
                {
                    UInt32 pos = (UInt32)(aStart + (i * 2));
                    UInt16 aChar = (UInt16)(n[pos + 1] << 8 | n[pos]);
                    if (aChar == 0)
                    {
                        return result.Substring(0, i);
                    }
                    result[i] = (char)aChar;
                }
            }
            return result;
        }
        /// <summary>
        /// Converts the specified bytes to an ASCII encoded string, treating the bytes as UTF16 encoded bytes.
        /// </summary>
        /// <param name="n">Pointer to the bytes to convert.</param>
        /// <param name="aStart">The index in the array at which to start converting bytes.</param>
        /// <param name="aCharCount">The number of characters to convert.</param>
        /// <returns>The ASCII encoded string.</returns>
        /// <remarks>
        /// This method does not handle removing the null termination character if it is present.
        /// </remarks>
        public unsafe static FOS_System.String GetASCIIStringFromUTF16(byte* n, UInt32 aStart, UInt32 aCharCount)
        {
            //If you change this method, change the array version above too.

            // Shortcut all of the effort below
            if (aCharCount == 0 || (n[0] == 0 && n[1] == 0))
            {
                return FOS_System.String.New(0);
            }

            {
                uint maxExtent = (aCharCount * 2) + aStart;
                uint i = aStart;
                for (; i < maxExtent; i += 2)
                {
                    if (n[i] == 0 && n[i + 1] == 0)
                    {
                        break;
                    }
                }
                aCharCount = (i - aStart) / 2;
            }

            FOS_System.String result = FOS_System.String.New((int)aCharCount);

            if (result == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            else
            {
                for (int i = 0; i < aCharCount; i++)
                {
                    UInt32 pos = (UInt32)(aStart + (i * 2));
                    UInt16 aChar = (UInt16)(n[pos + 1] << 8 | n[pos]);
                    if (aChar == 0)
                    {
                        return result.Substring(0, i);
                    }
                    result[i] = (char)aChar;
                }
            }

            return result;
        }
    }
}
