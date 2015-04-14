#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Drivers.Compiler.Types
{
    public static class TypeScanner
    {
        public static void ScanTypes(IL.ILLibrary TheLibrary)
        {
            if (TheLibrary == null)
            {
                return;
            }
            else if (TheLibrary.TypeInfos.Count != 0)
            {
                //Already scanned
                return;
            }

            foreach (IL.ILLibrary aDependency in TheLibrary.Dependencies)
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

        public static TypeInfo ScanType(IL.ILLibrary TheLibrary, Type aType)
        {
            if(TheLibrary.TypeInfos.Where(x => x.UnderlyingType.Equals(aType)).Count() > 0)
            {
                return TheLibrary.TypeInfos.Where(x => x.UnderlyingType.Equals(aType)).First();
            }

            string typeName = aType.Name;
            TypeInfo newTypeInfo = new TypeInfo()
            {
                UnderlyingType = aType,
                ContainsPlugs = aType.GetCustomAttribute(typeof(Attributes.PluggedClassAttribute)) != null
            };

            TheLibrary.TypeInfos.Add(newTypeInfo);

            {
                object[] CustAttrs = aType.GetCustomAttributes(false);
                foreach (object aCustAttr in CustAttrs)
                {
                    if (!aCustAttr.GetType().AssemblyQualifiedName.Contains("mscorlib"))
                    {
                        if (!IL.ILLibrary.SpecialClasses.ContainsKey(aCustAttr.GetType()))
                        {
                            IL.ILLibrary.SpecialClasses.Add(aCustAttr.GetType(), new List<TypeInfo>());
                        }
                        IL.ILLibrary.SpecialClasses[aCustAttr.GetType()].Add(newTypeInfo);
                    }
                }
            }

            //Ignore all internal data of types from mscorlib except for value types such as
            //  int, uint etc. and associated pointer types
            if (!aType.AssemblyQualifiedName.Contains("mscorlib"))
            {
                // All Fields
                System.Reflection.FieldInfo[] allFields = aType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (System.Reflection.FieldInfo aFieldInfo in allFields)
                {
                    if (aFieldInfo.DeclaringType.Equals(newTypeInfo.UnderlyingType))
                    {
                        newTypeInfo.FieldInfos.Add(new FieldInfo()
                        {
                            UnderlyingInfo = aFieldInfo
                        });
                    }
                }

                // Plugged / Unplugged Methods
                System.Reflection.MethodInfo[] allMethods = aType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToArray();

                foreach (System.Reflection.MethodInfo aMethodInfo in allMethods)
                {
                    if (aMethodInfo.DeclaringType.Equals(aType))
                    {
                        MethodInfo newMethodInfo = new MethodInfo()
                        {
                            UnderlyingInfo = aMethodInfo,
                            PlugAttribute = (Attributes.PluggedMethodAttribute)aMethodInfo.GetCustomAttribute(typeof(Attributes.PluggedMethodAttribute))
                        };
                        newTypeInfo.MethodInfos.Add(newMethodInfo);

                        object[] CustAttrs = aMethodInfo.GetCustomAttributes(false);
                        foreach (object aCustAttr in CustAttrs)
                        {
                            if (!aCustAttr.GetType().AssemblyQualifiedName.Contains("mscorlib"))
                            {
                                if (!IL.ILLibrary.SpecialMethods.ContainsKey(aCustAttr.GetType()))
                                {
                                    IL.ILLibrary.SpecialMethods.Add(aCustAttr.GetType(), new List<MethodInfo>());
                                }
                                IL.ILLibrary.SpecialMethods[aCustAttr.GetType()].Add(newMethodInfo);
                            }
                        }
                    }
                }

                // Plugged / unplugged Constructors
                ConstructorInfo[] staticConstructors = aType.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                               .ToArray();
                foreach (ConstructorInfo aConstructorInfo in staticConstructors)
                {
                    if (aConstructorInfo.DeclaringType.Equals(aType))
                    {
                        MethodInfo newMethodInfo = new MethodInfo()
                        {
                            UnderlyingInfo = aConstructorInfo,
                            PlugAttribute = (Attributes.PluggedMethodAttribute)aConstructorInfo.GetCustomAttribute(typeof(Attributes.PluggedMethodAttribute))
                        };
                        newTypeInfo.MethodInfos.Add(newMethodInfo);

                        object[] CustAttrs = aConstructorInfo.GetCustomAttributes(false);
                        foreach (object aCustAttr in CustAttrs)
                        {
                            if (!aCustAttr.GetType().AssemblyQualifiedName.Contains("mscorlib"))
                            {
                                if (!IL.ILLibrary.SpecialMethods.ContainsKey(aCustAttr.GetType()))
                                {
                                    IL.ILLibrary.SpecialMethods.Add(aCustAttr.GetType(), new List<MethodInfo>());
                                }
                                IL.ILLibrary.SpecialMethods[aCustAttr.GetType()].Add(newMethodInfo);
                            }
                        }
                    }
                }
            }

            return newTypeInfo;
        }
        public static void ProcessType(IL.ILLibrary TheLibrary, TypeInfo theTypeInfo)
        {
            if(theTypeInfo.Processed)
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
                        theTypeInfo.SizeOnHeapInBytes += fieldTypeInfo.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : Options.AddressSizeInBytes;
                    }
                }
            }
        }
        public static void ProcessTypeFields(IL.ILLibrary TheLibrary, TypeInfo theTypeInfo)
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
                    totalOffset += fieldTypeInfo.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : fieldTypeInfo.SizeOnStackInBytes;
                }
            }
        }

        private static int GetSizeOnStackInBytes(Type theType)
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
                    result = Options.AddressSizeInBytes;
                }
                else
                {
                    List<System.Reflection.FieldInfo> AllFields = theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    //This is a value type from a struct
                    result = 0;
                    foreach (System.Reflection.FieldInfo anInfo in AllFields)
                    {
                        result += GetSizeOnStackInBytes(anInfo.FieldType);
                    }
                }
            }

            return result;
        }
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
                    result = Options.AddressSizeInBytes;
                }
                else if (theType.IsPointer)
                {
                    result = 4;
                }
                else
                {
                    List<System.Reflection.FieldInfo> AllFields = theType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

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
