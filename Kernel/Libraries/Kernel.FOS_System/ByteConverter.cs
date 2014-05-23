using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    public static class ByteConverter
    {
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static UInt16 ToUInt16(byte[] n, UInt32 aPos)
        {
            return (UInt16)(n[aPos + 1] << 8 | n[aPos]);
        }
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static UInt32 ToUInt32(byte[] n, UInt32 aPos)
        {
            return (UInt32)(n[aPos + 3] << 24 | n[aPos + 2] << 16 | n[aPos + 1] << 8 | n[aPos]);
        }
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
            }
            return result;
        }
        [Compiler.NoDebug]
        public static FOS_System.String GetAsciiString(byte[] n, UInt32 aStart, UInt32 aCharCount)
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
        [Compiler.NoDebug]
        public static FOS_System.String GetUtf16String(byte[] n, UInt32 aStart, UInt32 aCharCount)
        {
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
