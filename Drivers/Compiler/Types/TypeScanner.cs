#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Drivers.Compiler.Attributes;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Types
{
    /// <summary>
    ///     Manages scanning the types in an IL library.
    /// </summary>
    public static class TypeScanner
    {
        /// <summary>
        ///     Scans the library for types.
        /// </summary>
        /// <param name="TheLibrary">The library to scan.</param>
        public static void ScanTypes(ILLibrary TheLibrary)
        {
            if (TheLibrary == null)
            {
                return;
            }
            if (TheLibrary.TypeInfos.Count != 0)
            {
                //Already scanned
                return;
            }

            foreach (ILLibrary aDependency in TheLibrary.Dependencies)
            {
                ScanTypes(aDependency);
            }

            List<Type> types = TheLibrary.TheAssembly.GetTypes().ToList();

            //Add in the standard types (which come from mscorlib)
            //#region Standard Types (from mscorlib)

            //types.Add(typeof(object));

            //types.Add(typeof(float));
            //types.Add(typeof(double));
            //types.Add(typeof(decimal));
            //types.Add(typeof(string));
            //types.Add(typeof(IntPtr));

            //types.Add(typeof(void));
            //types.Add(typeof(bool));
            //types.Add(typeof(byte));
            //types.Add(typeof(sbyte));
            //types.Add(typeof(char));
            //types.Add(typeof(int));
            //types.Add(typeof(long));
            //types.Add(typeof(Int16));
            //types.Add(typeof(Int32));
            //types.Add(typeof(Int64));
            //types.Add(typeof(UInt16));
            //types.Add(typeof(UInt32));
            //types.Add(typeof(UInt64));

            //types.Add(typeof(void*));
            //types.Add(typeof(bool*));
            //types.Add(typeof(byte*));
            //types.Add(typeof(sbyte*));
            //types.Add(typeof(char*));
            //types.Add(typeof(int*));
            //types.Add(typeof(long*));
            //types.Add(typeof(Int16*));
            //types.Add(typeof(Int32*));
            //types.Add(typeof(Int64*));
            //types.Add(typeof(UInt16*));
            //types.Add(typeof(UInt32*));
            //types.Add(typeof(UInt64*));

            //#endregion

            foreach (Type aType in types)
            {
                ScanType(TheLibrary, aType);
            }

            for (int i = 0; i < TheLibrary.TypeInfos.Count; i++)
            {
                ProcessType(TheLibrary, TheLibrary.TypeInfos[i]);
            }

            for (int i = 0; i < TheLibrary.TypeInfos.Count; i++)
            {
                ProcessTypeFields(TheLibrary, TheLibrary.TypeInfos[i]);
            }
        }

        /// <summary>
        ///     Scans a type to generate type info for the type. Also scans methods and constructors of the type
        ///     amongst some other information.
        /// </summary>
        /// <param name="TheLibrary">The library from which the type originated.</param>
        /// <param name="aType">The type to scan.</param>
        /// <returns>The new type info.</returns>
        public static TypeInfo ScanType(ILLibrary TheLibrary, Type aType)
        {
            if (TheLibrary.TypeInfos.Where(x => x.UnderlyingType.Equals(aType)).Count() > 0)
            {
                return TheLibrary.TypeInfos.Where(x => x.UnderlyingType.Equals(aType)).First();
            }

            string typeName = aType.Name;
            TypeInfo newTypeInfo = new TypeInfo
            {
                UnderlyingType = aType
            };

            TheLibrary.TypeInfos.Add(newTypeInfo);

            {
                object[] CustAttrs = aType.GetCustomAttributes(false);
                foreach (object aCustAttr in CustAttrs)
                {
                    if (!aCustAttr.GetType().AssemblyQualifiedName.Contains("mscorlib"))
                    {
                        if (!ILLibrary.SpecialClasses.ContainsKey(aCustAttr.GetType()))
                        {
                            ILLibrary.SpecialClasses.Add(aCustAttr.GetType(), new List<TypeInfo>());
                        }
                        ILLibrary.SpecialClasses[aCustAttr.GetType()].Add(newTypeInfo);
                    }
                }
            }

            //Ignore all internal data of types from mscorlib except for value types such as
            //  int, uint etc. and associated pointer types
            if (!aType.AssemblyQualifiedName.Contains("mscorlib"))
            {
                // All Fields
                System.Reflection.FieldInfo[] allFields =
                    aType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                    BindingFlags.Public);
                foreach (System.Reflection.FieldInfo aFieldInfo in allFields)
                {
                    if (aFieldInfo.DeclaringType.Equals(newTypeInfo.UnderlyingType))
                    {
                        newTypeInfo.FieldInfos.Add(new FieldInfo
                        {
                            UnderlyingInfo = aFieldInfo
                        });
                    }
                }

                // Plugged / Unplugged Methods
                System.Reflection.MethodInfo[] allMethods =
                    aType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                     BindingFlags.Static).ToArray();

                foreach (System.Reflection.MethodInfo aMethodInfo in allMethods)
                {
                    if (aMethodInfo.DeclaringType.Equals(aType))
                    {
                        MethodInfo newMethodInfo = new MethodInfo
                        {
                            UnderlyingInfo = aMethodInfo,
                            PlugAttribute =
                                (PluggedMethodAttribute)aMethodInfo.GetCustomAttribute(typeof(PluggedMethodAttribute))
                        };
                        newTypeInfo.MethodInfos.Add(newMethodInfo);

                        object[] CustAttrs = aMethodInfo.GetCustomAttributes(false);
                        foreach (object aCustAttr in CustAttrs)
                        {
                            if (!aCustAttr.GetType().AssemblyQualifiedName.Contains("mscorlib"))
                            {
                                if (!ILLibrary.SpecialMethods.ContainsKey(aCustAttr.GetType()))
                                {
                                    ILLibrary.SpecialMethods.Add(aCustAttr.GetType(), new List<MethodInfo>());
                                }
                                ILLibrary.SpecialMethods[aCustAttr.GetType()].Add(newMethodInfo);
                            }
                        }
                    }
                }

                // Plugged / unplugged Constructors
                ConstructorInfo[] allConstructors =
                    aType.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                          BindingFlags.NonPublic)
                        .ToArray();
                foreach (ConstructorInfo aConstructorInfo in allConstructors)
                {
                    if (aConstructorInfo.DeclaringType.Equals(aType))
                    {
                        MethodInfo newMethodInfo = new MethodInfo
                        {
                            UnderlyingInfo = aConstructorInfo,
                            PlugAttribute =
                                (PluggedMethodAttribute)
                                    aConstructorInfo.GetCustomAttribute(typeof(PluggedMethodAttribute))
                        };
                        newTypeInfo.MethodInfos.Add(newMethodInfo);

                        object[] CustAttrs = aConstructorInfo.GetCustomAttributes(false);
                        foreach (object aCustAttr in CustAttrs)
                        {
                            if (!aCustAttr.GetType().AssemblyQualifiedName.Contains("mscorlib"))
                            {
                                if (!ILLibrary.SpecialMethods.ContainsKey(aCustAttr.GetType()))
                                {
                                    ILLibrary.SpecialMethods.Add(aCustAttr.GetType(), new List<MethodInfo>());
                                }
                                ILLibrary.SpecialMethods[aCustAttr.GetType()].Add(newMethodInfo);
                            }
                        }
                    }
                }
            }

            return newTypeInfo;
        }

        /// <summary>
        ///     Processes the specified type info to fill in the required data.
        /// </summary>
        /// <param name="TheLibrary">The library from which the type originated.</param>
        /// <param name="theTypeInfo">The type info to process.</param>
        public static void ProcessType(ILLibrary TheLibrary, TypeInfo theTypeInfo)
        {
            if (theTypeInfo.Processed)
            {
                return;
            }

            theTypeInfo.Processed = true;

            theTypeInfo.IsGCManaged = GetIsGCManaged(theTypeInfo.UnderlyingType);

            if (theTypeInfo.IsValueType || theTypeInfo.IsPointer)
            {
                theTypeInfo.SizeOnStackInBytes = GetSizeOnStackInBytes(theTypeInfo.UnderlyingType);
                theTypeInfo.SizeOnHeapInBytes = GetSizeOnHeapInBytes(theTypeInfo.UnderlyingType);
            }
            else
            {
                theTypeInfo.SizeOnStackInBytes = GetSizeOnStackInBytes(theTypeInfo.UnderlyingType);

                theTypeInfo.SizeOnHeapInBytes = 0;
                if (theTypeInfo.UnderlyingType.BaseType != null)
                {
                    Type baseType = theTypeInfo.UnderlyingType.BaseType;
                    if (!baseType.AssemblyQualifiedName.Contains("mscorlib"))
                    {
                        TypeInfo baseTypeInfo = TheLibrary.GetTypeInfo(baseType, false);
                        ProcessType(TheLibrary, baseTypeInfo);
                        theTypeInfo.SizeOnHeapInBytes += baseTypeInfo.SizeOnHeapInBytes;
                    }
                }
                foreach (FieldInfo aFieldInfo in theTypeInfo.FieldInfos)
                {
                    if (!aFieldInfo.IsStatic)
                    {
                        TypeInfo fieldTypeInfo = TheLibrary.GetTypeInfo(aFieldInfo.FieldType, false);
                        if (fieldTypeInfo.IsValueType || fieldTypeInfo.IsPointer)
                        {
                            ProcessType(TheLibrary, fieldTypeInfo);
                        }
                        theTypeInfo.SizeOnHeapInBytes += fieldTypeInfo.IsValueType
                            ? fieldTypeInfo.SizeOnHeapInBytes
                            : Options.AddressSizeInBytes;
                    }
                }
            }
        }

        /// <summary>
        ///     Processes the specified type's fields to fill in required data.
        /// </summary>
        /// <param name="TheLibrary">The library from which the type originated.</param>
        /// <param name="theTypeInfo">The type info to process.</param>
        public static void ProcessTypeFields(ILLibrary TheLibrary, TypeInfo theTypeInfo)
        {
            if (theTypeInfo.ProcessedFields)
            {
                return;
            }

            theTypeInfo.ProcessedFields = true;

            int totalOffset = 0;

            //Base class fields
            if (theTypeInfo.UnderlyingType.BaseType != null)
            {
                Type baseType = theTypeInfo.UnderlyingType.BaseType;
                if (!baseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    totalOffset = TheLibrary.GetTypeInfo(baseType).SizeOnHeapInBytes;
                }
            }

            foreach (FieldInfo aFieldInfo in theTypeInfo.FieldInfos)
            {
                if (!aFieldInfo.IsStatic)
                {
                    aFieldInfo.OffsetInBytes = totalOffset;
                    TypeInfo fieldTypeInfo = TheLibrary.GetTypeInfo(aFieldInfo.FieldType);
                    totalOffset += fieldTypeInfo.IsValueType
                        ? fieldTypeInfo.SizeOnHeapInBytes
                        : fieldTypeInfo.SizeOnStackInBytes;
                }
            }
        }

        /// <summary>
        ///     Gets the size, in bytes, of the specified type when it is represented on the stack.
        /// </summary>
        /// <param name="theType">The type to determine the stack size of.</param>
        /// <returns>The size in bytes.</returns>
        public static int GetSizeOnStackInBytes(Type theType)
        {
            //Assume its a pointer/reference unless it is:
            // - A value type
            string name = theType.Name;
            int result = Options.AddressSizeInBytes;

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
                else if (theType.AssemblyQualifiedName == typeof(ushort).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(short).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(uint).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(int).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(ulong).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(long).AssemblyQualifiedName)
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
                    result = Options.AddressSizeInBytes;
                }
                else
                {
                    List<System.Reflection.FieldInfo> AllFields =
                        theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    //This is a value type from a struct
                    result = 0;
                    foreach (System.Reflection.FieldInfo anInfo in AllFields)
                    {
                        result += GetSizeOnHeapInBytes(anInfo.FieldType);
                    }

                    // Min struct size of 4
                    result = Math.Max(result, 4);

                    // Round struct size up to multiple of 4 (ensures 4-byte stack alignment)
                    if (result%4 != 0)
                    {
                        result += 4 - result%4;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Gets the size, in bytes, of the specified type when it is allocated on the heap.
        /// </summary>
        /// <param name="theType">The type to determine the heap size of.</param>
        /// <returns>The size in bytes.</returns>
        private static int GetSizeOnHeapInBytes(Type theType)
        {
            //Assume its a pointer/reference unless it is:
            // - A value type
            int result = Options.AddressSizeInBytes;

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
                else if (theType.AssemblyQualifiedName == typeof(ushort).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(short).AssemblyQualifiedName)
                {
                    result = 2;
                }
                else if (theType.AssemblyQualifiedName == typeof(uint).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(int).AssemblyQualifiedName)
                {
                    result = 4;
                }
                else if (theType.AssemblyQualifiedName == typeof(ulong).AssemblyQualifiedName ||
                         theType.AssemblyQualifiedName == typeof(long).AssemblyQualifiedName)
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
                    result = Options.AddressSizeInBytes;
                }
                else if (theType.IsPointer)
                {
                    result = 4;
                }
                else if (theType.Name.Contains("FixedBuffer"))
                {
                    return theType.StructLayoutAttribute.Size;
                }
                else
                {
                    List<System.Reflection.FieldInfo> AllFields =
                        theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    //This is a value type from a struct
                    result = 0;
                    foreach (System.Reflection.FieldInfo anInfo in AllFields)
                    {
                        result += GetSizeOnHeapInBytes(anInfo.FieldType);
                    }
                }
            }
            else if (theType.IsPointer)
            {
                result = Options.AddressSizeInBytes;
            }

            return result;
        }

        /// <summary>
        ///     Determines whether the specified type is managed by the garbage collector or not.
        /// </summary>
        /// <param name="theType">The type to check.</param>
        /// <returns>True if it is managed by the GC. Otherwise, false.</returns>
        private static bool GetIsGCManaged(Type theType)
        {
            bool isGCManaged = true;

            if (theType != null && (theType.IsValueType ||
                                    theType.IsPointer ||
                                    typeof(Delegate).IsAssignableFrom(theType)))
            {
                isGCManaged = false;
            }

            return isGCManaged;
        }
    }
}