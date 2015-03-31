using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler
{
    public static class Utilities
    {
        public static bool IsFloat(Type aType)
        {
            bool isFloat = false;

            if (aType.Equals(typeof(float)) ||
                aType.Equals(typeof(double)))
            {
                isFloat = true;
            }

            return isFloat;
        }

        /// <summary>
        /// Reads a signed integer 16 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int16 ReadInt16(byte[] bytes, int offset)
        {
            return BitConverter.ToInt16(bytes, offset);
        }
        /// <summary>
        /// Reads a signed integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int32 ReadInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToInt32(bytes, offset);
        }
        /// <summary>
        /// Reads an unsigned integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static UInt32 ReadUInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }
        /// <summary>
        /// Reads a signed integer 64 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int64 ReadInt64(byte[] bytes, int offset)
        {
            return BitConverter.ToInt64(bytes, offset);
        }
        /// <summary>
        /// Reads a single-precision (32-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static float ReadFloat32(byte[] bytes, int offset)
        {
            return (float)(BitConverter.ToSingle(bytes, 0));
        }
        /// <summary>
        /// Reads a double-precision (64-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static double ReadFloat64(byte[] bytes, int offset)
        {
            return (double)(BitConverter.ToDouble(bytes, 0));
        }


        public static string CleanFileName(string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }


        public const string IllegalIdentifierChars = "&,+$<>{}-`\'/\\ ()[]*!=.";
        public static string FilterIdentifierForInvalidChars(string x)
        {
            string xTempResult = x;
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return String.Intern(xTempResult);
        }

    }
}
