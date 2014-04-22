using System;
namespace Kernel.FOS_System
{
    /// <summary>
    /// Replacement class for methods, properties and fields usually found on standard System.Int64 type.
    /// </summary>
    public static class Int64
    {
        /// <summary>
        /// Returns the maximum value of an Int32.
        /// </summary>
        public static long MaxValue
        {
            get
            {
                return 9223372036854775807;
            }
        }
    }
}
