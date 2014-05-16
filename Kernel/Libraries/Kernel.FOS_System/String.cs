using System;

namespace Kernel.FOS_System
{
    /// <summary>
    /// Replacement class for methods, properties and fields usually found on standard System.String type.
    /// Also contains utility methods for low-level string manipulation.
    /// </summary>
    [Kernel.Compiler.PluggedClass]
    [Kernel.Compiler.StringClass]
    public class String : Object
    {
        /* If you add more fields here, remember to update the compiler and all the ASM files that depend on the string
           class structure ( i.e. do all the hard work! ;) )
         */
        public int length;

        /*   ----------- DO NOT CREATE A CONSTRUCTOR FOR THIS CLASS - IT WILL NEVER BE CALLED IF YOU DO ----------- */

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

        /// <summary>
        /// Creates a new, blank FOS_System.String of specified length.
        /// IMPORTANT NOTE: You MUST assign the return value of this to a variable / local / arg / 
        /// field etc. You may not use IL or C# that results in an IL Pop op of the return value
        /// of this method as it will screw up the GC RefCount handling.
        /// </summary>
        /// <param name="length">The length of the string to create.</param>
        /// <returns>The new string.</returns>
        [Compiler.NoGC]
        public static unsafe FOS_System.String New(int length)
        {
            if(length < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("Parameter \"length\" cannot be less than 0 in FOS_System.String.New(int length)."));
            }
            return (FOS_System.String)Utilities.ObjectUtilities.GetObject(GC.NewString(length));
        }

        public static unsafe FOS_System.String Concat(FOS_System.String str1, FOS_System.String str2)
        {
            FOS_System.String newStr = New(str1.length + str2.length);
            for (int i = 0; i < str1.length; i++)
            {
                newStr[i] = str1[i];
            }
            for (int i = 0; i < str2.length; i++)
            {
                newStr[i + str1.length] = str2[i];
            }
            return newStr;
        }

        public unsafe char this[int index]
        {
            get
            {
                byte* thisPtr = (byte*)Utilities.ObjectUtilities.GetHandle(this);
                thisPtr += 8; /*For fields inc. inherited*/
                return ((char*)thisPtr)[index];
            }
            set
            {
                byte* thisPtr = (byte*)Utilities.ObjectUtilities.GetHandle(this);
                thisPtr += 8; /*For fields inc. inherited*/
                ((char*)thisPtr)[index] = value;
            }
        }
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public unsafe char* GetCharPointer()
        {
            return (char*)(((byte*)Utilities.ObjectUtilities.GetHandle(this)) + 8/*For fields*/);
        }

        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static implicit operator FOS_System.String(string x)
        {
            return (FOS_System.String)(object)x;
        }
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static implicit operator string(FOS_System.String x)
        {
            return (string)(object)x;
        }
    }
}
