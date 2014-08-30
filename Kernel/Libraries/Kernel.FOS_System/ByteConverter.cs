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
        [Compiler.NoDebug]
        [Compiler.NoGC]
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
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static UInt32 ToUInt32(byte[] n, UInt32 aPos)
        {
            return ((UInt32)n[aPos + 3] << 24 | (UInt32)n[aPos + 2] << 16 |
                    (UInt32)n[aPos + 1] << 8  | (UInt32)n[aPos]);
        }
        /// <summary>
        /// Converts 8 bytes from the specified byte array at the specified index into a UInt64.
        /// </summary>
        /// <param name="n">The byte array from which to convert bytes.</param>
        /// <param name="aPos">The index of the first of the four bytes to convert.</param>
        /// <returns>The converted UInt64.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
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
        [Compiler.NoDebug]
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
        [Compiler.NoDebug]
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
        [Compiler.NoDebug]
        public static FOS_System.String GetASCIIStringFromASCII(byte[] n, UInt32 aStart, UInt32 aCharCount)
        {
            FOS_System.String result = FOS_System.String.New((int)aCharCount);
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

            return result;
        }
        /// <summary>
        /// Converts the specified bytes to an ASCII encoded string, treating the bytes as ASCII encoded bytes.
        /// </summary>
        /// <param name="n">The bytes to convert.</param>
        /// <param name="aStart">The index in the array at which to start converting bytes.</param>
        /// <param name="aCharCount">The number of characters to convert.</param>
        /// <returns>The ASCII encoded string.</returns>
        [Compiler.NoDebug]
        public static unsafe FOS_System.String GetASCIIStringFromASCII(byte* n, UInt32 aStart, UInt32 aCharCount)
        {
            FOS_System.String result = FOS_System.String.New((int)aCharCount);
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
        [Compiler.NoDebug]
        public static FOS_System.String GetASCIIStringFromUTF16(byte[] n, UInt32 aStart, UInt32 aCharCount)
        {
            //If you change this method, change the pointer version below too.

            FOS_System.String result = FOS_System.String.New((int)aCharCount);
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

            FOS_System.String result = FOS_System.String.New((int)aCharCount);
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

            return result;
        }
    }
}
