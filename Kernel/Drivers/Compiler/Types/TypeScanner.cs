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

            Type[] types = TheLibrary.TheAssembly.GetTypes();
            foreach (Type aType in types)
            {
                ScanType(TheLibrary, aType);
            }
        }

        public static TypeInfo ScanType(IL.ILLibrary TheLibrary, Type aType)
        {
            TypeInfo newTypeInfo = new TypeInfo()
            {
                UnderlyingType = aType,
                ContainsPlugs = aType.GetCustomAttribute(typeof(Attributes.PluggedClassAttribute)) != null
            };

            TheLibrary.TypeInfos.Add(newTypeInfo);

            // Static Fields
            System.Reflection.FieldInfo[] staticFields = aType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (System.Reflection.FieldInfo aFieldInfo in staticFields)
            {
                newTypeInfo.FieldInfos.Add(new FieldInfo()
                {
                    UnderlyingInfo = aFieldInfo,
                    IsStatic = true
                });
            }

            // Instance Fields
            System.Reflection.FieldInfo[] nonStaticFields = aType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (System.Reflection.FieldInfo aFieldInfo in nonStaticFields)
            {
                newTypeInfo.FieldInfos.Add(new FieldInfo()
                {
                    UnderlyingInfo = aFieldInfo,
                    IsStatic = false
                });
            }

            // Plugged / Unplugged Methods
            System.Reflection.MethodInfo[] allMethods = aType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToArray();

            foreach (System.Reflection.MethodInfo aMethodInfo in allMethods)
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
                        if (!TheLibrary.SpecialMethods.ContainsKey(aCustAttr.GetType()))
                        {
                            TheLibrary.SpecialMethods.Add(aCustAttr.GetType(), new List<MethodInfo>());
                        }
                        TheLibrary.SpecialMethods[aCustAttr.GetType()].Add(newMethodInfo);
                    }
                }
            }

            // Plugged / unplugged Constructors
            ConstructorInfo[] staticConstructors = aType.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                           .ToArray();
            foreach (ConstructorInfo aConstructorInfo in staticConstructors)
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
                        if (!TheLibrary.SpecialMethods.ContainsKey(aCustAttr.GetType()))
                        {
                            TheLibrary.SpecialMethods.Add(aCustAttr.GetType(), new List<MethodInfo>());
                        }
                        TheLibrary.SpecialMethods[aCustAttr.GetType()].Add(newMethodInfo);
                    }
                }
            }

            return newTypeInfo;
        }
    }
}
