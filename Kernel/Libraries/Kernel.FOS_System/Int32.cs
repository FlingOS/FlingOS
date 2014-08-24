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
    
namespace Kernel.FOS_System
{
    /// <summary>
    /// Replacement class for methods, properties and fields usually found on standard System.Int32 type.
    /// </summary>
    public static class Int32
    {
        /// <summary>
        /// Returns the maximum value of an Int32.
        /// </summary>
        public static int MaxValue
        {
            get
            {
                return 2147483647;
            }
        }

        /// <summary>
        /// Parses a string as an unsigned decimal integer.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="offset">The offset into the string at which to start parsing.</param>
        /// <returns>The parsed uint.</returns>
        public static uint Parse_DecimalUnsigned(FOS_System.String str, int offset)
        {
            uint result = 0;
            for(int i = offset; i < str.length; i++)
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
        /// Parses a string as an signed decimal integer.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>The parsed int.</returns>
        public static int Parse_DecimalSigned(FOS_System.String str)
        {
            bool neg = str.StartsWith("-");
            int result = (int)Parse_DecimalUnsigned(str, (neg ? 1 : 0));
            if (neg)
            {
                result *= -1;
            }
            return result;
        }
    }
}
