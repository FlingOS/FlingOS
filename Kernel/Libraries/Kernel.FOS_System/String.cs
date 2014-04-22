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
            return 0;
        }

        /// <summary>
        /// Gets a pointer to the first byte (that represents a character) of the specified string.
        /// </summary>
        /// <param name="aString">The string to get a pointer to.</param>
        /// <returns>A pointer to the first byte (that represents a character) of the specified string.</returns>
        /// <remarks>
        /// Skips over the length dword at the start of the string.
        /// </remarks>
        [Kernel.Compiler.PluggedMethod(ASMFilePath = @"ASM\String\GetPointer")]
        public static unsafe byte* GetBytePointer(string aString)
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the first char (that represents a character) of the specified string.
        /// </summary>
        /// <param name="aString">The string to get a pointer to.</param>
        /// <returns>A pointer to the first char (that represents a character) of the specified string.</returns>
        /// <remarks>
        /// Skips over the length dword at the start of the string.
        /// </remarks>
        [Kernel.Compiler.PluggedMethod(ASMFilePath = null)]
        public static unsafe char* GetCharPointer(string aString)
        {
            return null;
        }
    }
}
