using System;

namespace Kernel.FOS_System
{
    /// <summary>
    /// Replacement class for methods, properties and fields usually found on standard System.String type.
    /// Also contains utility methods for low-level string manipulation.
    /// </summary>
    [Kernel.Compiler.PluggedClass]
    public class String : Object
    {
        /// <summary>
        /// Gets the length of the specified string.
        /// </summary>
        /// <param name="aString">The string to get the length of.</param>
        /// <returns>The length of the specified string.</returns>
        [Kernel.Compiler.PluggedMethod(ASMFilePath=@"ASM\String\GetLength")]
        public static int GetLength(string aString)
        {
            //Stub for use by testing frameworks.
            return aString.Length;
        }

        /// <summary>
        /// Gets a pointer to the first char (that represents a character) of the specified string.
        /// </summary>
        /// <param name="aString">The string to get a pointer to.</param>
        /// <returns>A pointer to the first char (that represents a character) of the specified string.</returns>
        /// <remarks>
        /// Skips over the length dword at the start of the string.
        /// </remarks>
        [Kernel.Compiler.PluggedMethod(ASMFilePath = @"ASM\String\GetPointer")]
        public static unsafe char* GetCharPointer(string aString)
        {
            //Stub for use by testing frameworks.
            return (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAuto(aString);
        }

        public static unsafe string New(int length)
        {
            if(length < 0)
            {
                new Exceptions.IndexOutOfRangeException();
            }
            return null;
        }

        public static unsafe string Concat(string str1, string str2)
        {
            return null;
        }
    }
}
