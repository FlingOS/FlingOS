using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Debug.Debugger
{
    /// <summary>
    /// Utility methods for the debugger.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts the specified address bytes to an numeric address
        /// </summary>
        /// <param name="addressBytes">The bytes to convert.</param>
        /// <returns>The numeric address and the size of the address in bytes.</returns>
        public static Tuple<ulong, byte> BytesToAddress(byte[] addressBytes)
        {
            byte numBytes = (byte)addressBytes.Length;
            ulong result = 0;

            if (numBytes == 4)
            {
                result = BitConverter.ToUInt32(addressBytes, 0);
            }
            else if (numBytes == 8)
            {
                result = BitConverter.ToUInt64(addressBytes, 0);
            }
            else
            {
                throw new NotSupportedException("Address too large!");
            }

            return new Tuple<ulong, byte>(result, numBytes);
        }
        /// <summary>
        /// Converts the specified address description to its bytes representation.
        /// </summary>
        /// <param name="address">The address decription to convert.</param>
        /// <returns>The address bytes.</returns>
        public static byte[] AddressToBytes(Tuple<ulong, byte> address)
        {
            byte numBytes = address.Item2;
            byte[] result = null;

            if (numBytes == 4)
            {
                result = BitConverter.GetBytes((uint)address.Item1);
            }
            else if (numBytes == 8)
            {
                result = BitConverter.GetBytes((ulong)address.Item1);
            }
            else
            {
                throw new NotSupportedException("Address too large!");
            }

            return result;
        }

        /// <summary>
        /// Converts the specified bytes to their human-readable representation
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <param name="type">The type-signature of the type represented by the bytes.</param>
        /// <returns>The value string.</returns>
        public static string GetValueStr(byte[] bytes, string type = null)
        {
            if (bytes == null)
            {
                return "Null value bytes!";
            }

            object result = "";
            switch (type)
            {
                case "System.Void":
                    result = "";
                    break;
                case "System.String":
                    result = "\"" + Encoding.Unicode.GetString(bytes) + "\"";
                    break;

                case "System.Boolean":
                    result = BitConverter.ToBoolean(bytes, 0);
                    break;
                case "System.Byte":
                    result = bytes[0];
                    break;
                case "System.Char":
                    result = BitConverter.ToChar(bytes, 0);
                    break;
                case "System.Decimal":
                    result = GetValueStr(bytes);
                    break;
                case "System.Double":
                    result = BitConverter.ToDouble(bytes, 0);
                    break;
                case "System.Int16":
                    result = BitConverter.ToInt16(bytes, 0);
                    break;
                case "System.Int32":
                    result = BitConverter.ToInt32(bytes, 0);
                    break;
                case "System.Int64":
                    result = BitConverter.ToInt64(bytes, 0);
                    break;
                case "System.IntPtr":
                    result = BitConverter.ToUInt32(bytes, 0);
                    break;
                case "System.SByte":
                    result = (sbyte)bytes[0];
                    break;
                case "System.Single":
                    result = BitConverter.ToSingle(bytes, 0);
                    break;
                case "System.UInt16":
                    result = BitConverter.ToUInt16(bytes, 0).ToString().PadLeft(10, '0')
                        + string.Format("  (0x{0:X4})", BitConverter.ToUInt16(bytes, 0));
                    break;
                case "System.UInt32":
                    result = BitConverter.ToUInt32(bytes, 0).ToString().PadLeft(10, '0')
                        + string.Format("  (0x{0:X8})", BitConverter.ToUInt32(bytes, 0));
                    break;
                case "System.UInt64":
                    result = BitConverter.ToUInt64(bytes, 0).ToString().PadLeft(10, '0')
                        + string.Format("  (0x{0:X16})", BitConverter.ToUInt64(bytes, 0));
                    break;
                default:
                    switch (bytes.Length)
                    {
                        case 0:
                            {
                                result = " - ";
                            }
                            break;
                        case 1:
                            {
                                byte val = bytes[0];
                                result = string.Format("0x{0:X2}", val);
                            }
                            break;
                        case 2:
                            {
                                ushort val = BitConverter.ToUInt16(bytes, 0);
                                result = string.Format("0x{0:X4}", val);
                            }
                            break;
                        case 4:
                            {
                                uint val = BitConverter.ToUInt32(bytes, 0);
                                result = string.Format("0x{0:X8}", val);
                            }
                            break;
                        case 8:
                            {
                                ulong val = BitConverter.ToUInt64(bytes, 0);
                                result = string.Format("0x{0:X16}", val);
                            }
                            break;
                        case 16:
                            {
                                ulong val = BitConverter.ToUInt64(bytes, 0);
                                result = string.Format("0x{0:X16}", val);
                                val = BitConverter.ToUInt64(bytes, 8);
                                result += string.Format(" 0x{0:X16}", val);
                            }
                            break;
                        default:
                            {
                                result = "Unsupported size";
                            }
                            break;
                    }
                    break;
            }
            return result.ToString();
        }        
    }
}
