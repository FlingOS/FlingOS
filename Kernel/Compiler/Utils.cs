#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;

namespace Kernel.Compiler
{
    /// <summary>
    /// Static utility methods used throughout the compiler.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// The string of characters which are illegal to use in ASM labels.
        /// </summary>
        public const string IllegalIdentifierChars = "&,+$<>{}-`\'/\\ ()[]*!=.";
        /// <summary>
        /// Replaces illegal characters from an ASM label (identifier) with an underscore ('_')
        /// </summary>
        /// <param name="x">The label to filter.</param>
        /// <returns>The filtered label.</returns>
        public static string FilterIdentifierForInvalidChars(string x)
        {
            string xTempResult = x;
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return String.Intern(xTempResult);
        }

        /// <summary>
        /// Gets the signature of the specified method. A method's signature will (probably) always be unique.
        /// </summary>
        /// <param name="aMethod">The method to get the signature of.</param>
        /// <returns>The method's siganture string.</returns>
        public static string GetMethodSignature(MethodBase aMethod)
        {
            string[] paramTypes = aMethod.GetParameters().Select(x => x.ParameterType).Select(x => x.FullName).ToArray();
            string returnType = "";
            string declaringType = "";
            string methodName = "";
            if (aMethod.IsConstructor || aMethod is ConstructorInfo)
            {
                returnType = typeof(void).FullName;
                declaringType = aMethod.DeclaringType.FullName;
                methodName = aMethod.Name;
            }
            else
            {
                returnType = ((MethodInfo)aMethod).ReturnType.FullName;
                declaringType = aMethod.DeclaringType.FullName;
                methodName = aMethod.Name;
            }
            
            return GetMethodSignature(returnType, declaringType, methodName, paramTypes);
        }
        /// <summary>
        /// Gets the signature of the specified method using specified method information. A method's signature will (probably) always be unique.
        /// </summary>
        /// <param name="returnType">The fully qualified name of the type of the return value.</param>
        /// <param name="declaringType">The fully qualified name of the type that declares the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="paramTypes">The fully qualified names of the types of the paramters of the method.</param>
        /// <returns>The method's siganture string.</returns>
        public static string GetMethodSignature(string returnType, string declaringType, string methodName, string[] paramTypes)
        {
            string aMethodSignature = "";
            aMethodSignature = returnType + "_RETEND_" + declaringType + "_DECLEND_" + methodName + "_NAMEEND_(";
            bool firstParam = true;
            foreach (string aParam in paramTypes)
            {
                if (!firstParam)
                {
                    aMethodSignature += ",";
                }
                firstParam = false;

                aMethodSignature += aParam;
            }
            aMethodSignature += ")";
            return aMethodSignature;
        }
        /// <summary>
        /// Creates a method ID string (ASM label) from the specified method signature.
        /// </summary>
        /// <param name="methodSignature">The method signature to use.</param>
        /// <returns>The method ID (ASM label).</returns>
        public static string CreateMethodID(string methodSignature)
        {
            return "method_" + FilterIdentifierForInvalidChars(methodSignature);
        }

        /// <summary>
        /// Reverse the method signature i.e. returns a string array of:
        /// Return type full name,
        /// Declaring type full name,
        /// Method name
        /// </summary>
        /// <param name="methodSig">The method signature to reverse.</param>
        /// <returns>See summary.</returns>
        public static string[] ReverseMethodSignature(string methodSig)
        {
            string[] result = new string[3];

            int RETENDIndex = methodSig.IndexOf("_RETEND_");
            int DECLENDIndex = methodSig.IndexOf("_DECLEND_");
            int NAMEENDIndex = methodSig.IndexOf("_NAMEEND_");

            result[0] = methodSig.Substring(0, RETENDIndex);
            result[1] = methodSig.Substring(RETENDIndex + 8, DECLENDIndex - RETENDIndex - 8);
            result[2] = methodSig.Substring(DECLENDIndex + 9, NAMEENDIndex - DECLENDIndex - 9);

            return result;
        }
        /// <summary>
        /// Gets the signature of the specified field. A field's signature will (probably) always be unique.
        /// </summary>
        /// <param name="aField">The field to get the signature of.</param>
        /// <returns>The field's siganture string.</returns>
        public static string GetFieldSignature(FieldInfo aField)
        {
            return aField.FieldType.FullName + " " + aField.DeclaringType.FullName + "." + aField.Name;
        }

        /// <summary>
        /// Gets the number of bytes used by a given type when it is represented on the stack.
        /// </summary>
        /// <param name="theType">The type to get the size of.</param>
        /// <returns>The number of bytes used to represent the specified type on the stack.</returns>
        public static int GetNumBytesForType(Type theType)
        {
            //Assume its a 32-bit pointer/reference unless it is:
            // - A value type
            //TODO - this should be moved to the target architecture's library
            int result = 4;
            
            if (theType.IsValueType)
            {
                if (theType.AssemblyQualifiedName == typeof(void).AssemblyQualifiedName)
                {
                    result = 0;
                }                
                else if (theType.AssemblyQualifiedName == typeof(byte).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(sbyte).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt16).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int16).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt32).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int32).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt64).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int64).AssemblyQualifiedName)
                {
                    result = 8;
                }
                else if (theType.AssemblyQualifiedName == typeof(string).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(char).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(float).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(double).AssemblyQualifiedName)
                {
                    result = 8;
                }
                else if (theType.AssemblyQualifiedName == typeof(bool).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(decimal).AssemblyQualifiedName)
                {
                    result = 16;
                }
                else if (theType.AssemblyQualifiedName == typeof(IntPtr).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else
                {
                    List<FieldInfo> AllFields = theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    //This is a value type from a struct
                    result = 0;
                    foreach (FieldInfo anInfo in AllFields)
                    {
                        result += GetNumBytesForType(anInfo.FieldType);
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// Gets the number of bytes used by a given type when it is represented in memory such as in an array.
        /// </summary>
        /// <param name="theType">The type to get the size of.</param>
        /// <returns>The number of bytes used to represent the specified type in an array.</returns>
        public static int GetSizeForType(Type theType)
        {
            //Assume its a 32-bit pointer/reference unless it is:
            // - A value type
            //TODO - this should be moved to the target architecture's library
            int result = 4;

            if (theType.IsValueType)
            {
                if (theType.AssemblyQualifiedName == typeof(void).AssemblyQualifiedName)
                {
                    result = 0;
                }
                else if (theType.AssemblyQualifiedName == typeof(byte).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(sbyte).AssemblyQualifiedName)
                {
                    result = 1;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt16).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int16).AssemblyQualifiedName)
                {
                    result = 2;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt32).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int32).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(UInt64).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(Int64).AssemblyQualifiedName)
                {
                    result = 8;
                }
                else if (theType.AssemblyQualifiedName == typeof(string).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(char).AssemblyQualifiedName)
                {
                    result = 2;
                }
                else if (theType.AssemblyQualifiedName == typeof(float).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(double).AssemblyQualifiedName)
                {
                    result = 8;
                }
                else if (theType.AssemblyQualifiedName == typeof(bool).AssemblyQualifiedName)
                {
                    result = 1;
                }
                else if (theType.AssemblyQualifiedName == typeof(decimal).AssemblyQualifiedName)
                {
                    result = 16;
                }
                else if (theType.AssemblyQualifiedName == typeof(IntPtr).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else
                {
                    List<FieldInfo> AllFields = theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    //This is a value type from a struct
                    result = 0;
                    foreach (FieldInfo anInfo in AllFields)
                    {
                        result += GetSizeForType(anInfo.FieldType);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the mnemonic for a given number of bytes (e.g. 1 = byte, 2 = word, 4 = dword, 8 = qword)
        /// </summary>
        /// <param name="numBytes">The number of bytes. Shouldbe a power of 2.</param>
        /// <returns>The mnemonic or null if the number of bytes was not recognised.</returns>
        public static string GetOpSizeString(int numBytes)
        {
            string size = null;
            switch (numBytes)
            {
                case 1:
                    size = "byte";
                    break;
                case 2:
                    size = "word";
                    break;
                case 4:
                    size = "dword";
                    break;
                case 8:
                    size = "qword";
                    break;
            }
            return size;
        }

        /// <summary>
        /// Determines whether the specified type is a floating point number (inc. single and double precision).
        /// </summary>
        /// <param name="aType">The type to check.</param>
        /// <returns>True if the type is a flaoting point type. Otherwise false.</returns>
        public static bool IsFloat(Type aType)
        {
            bool isFloat = false;

            if (aType.AssemblyQualifiedName == typeof(float).AssemblyQualifiedName ||
                aType.AssemblyQualifiedName == typeof(double).AssemblyQualifiedName)
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

        /// <summary>
        /// Determines whether the specified type is managed by the 
        /// garbage collector or not.
        /// </summary>
        /// <param name="theType">The type to check for GC management.</param>
        /// <returns>Whether the specified type is managed by the 
        /// garbage collector or not.</returns>
        public static bool IsGCManaged(Type theType)
        {
            bool isGCManaged = true;

            if(theType.IsValueType || theType.IsPointer)
            {
                isGCManaged = false;
            }

            return isGCManaged;
        }

        public static string GetMD5Hash(byte[] inputBytes)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
