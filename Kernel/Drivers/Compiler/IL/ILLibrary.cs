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
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Drivers.Compiler.IL
{
    public class ILLibrary
    {
        public Assembly TheAssembly;

        public ASM.ASMLibrary TheASMLibrary = new ASM.ASMLibrary();

        public List<ILLibrary> Dependencies = new List<ILLibrary>();
        
        public List<Types.TypeInfo> TypeInfos = new List<Types.TypeInfo>();

        public Dictionary<Types.MethodInfo, ILBlock> ILBlocks = new Dictionary<Types.MethodInfo, ILBlock>();

        public static Dictionary<Type, List<Types.TypeInfo>> SpecialClasses = new Dictionary<Type, List<Types.TypeInfo>>();
        public static Dictionary<Type, List<Types.MethodInfo>> SpecialMethods = new Dictionary<Type, List<Types.MethodInfo>>();

        public bool ILRead = false;
        public bool ILPreprocessed = false;
        public bool ILScanned = false;

        public static StaticConstructorDependency TheStaticConstructorDependencyTree = new StaticConstructorDependency()
        {
            TheConstructor = null
        };

        public Dictionary<string, string> StringLiterals = new Dictionary<string, string>();

        public Types.TypeInfo GetTypeInfo(Type theType, bool FullyProcess = true)
        {
            return GetTypeInfo(theType, true, FullyProcess);
        }
        private Types.TypeInfo GetTypeInfo(Type theType, bool topLevel, bool FullyProcess)
        {
            foreach (Types.TypeInfo aTypeInfo in TypeInfos)
            {
                if (aTypeInfo.UnderlyingType.Equals(theType))
                {
                    return aTypeInfo;
                }
            }

            foreach (ILLibrary depLib in Dependencies)
            {
                Types.TypeInfo result = depLib.GetTypeInfo(theType, false, FullyProcess);
                if (result != null)
                {
                    return result;
                }
            }

            if (topLevel)
            {
                Types.TypeInfo theTypeInfo = Types.TypeScanner.ScanType(this, theType);
                if (FullyProcess)
                {
                    int count = 0;
                    do
                    {
                        count = TypeInfos.Count;
                        int start = count - 1;
                        for (int i = start; i < count; i++)
                        {
                            Types.TypeScanner.ProcessType(this, TypeInfos[i]);
                        }
                        for (int i = start; i < count; i++)
                        {
                            Types.TypeScanner.ProcessTypeFields(this, TypeInfos[i]);
                        }
                    }
                    while (count < TypeInfos.Count);
                }
                return theTypeInfo;
            }
            else
            {
                return null;
            }
        }

        public Types.MethodInfo GetMethodInfo(MethodBase theMethod)
        {
            foreach (Types.TypeInfo aTypeInfo in TypeInfos)
            {
                foreach (Types.MethodInfo aMethodInfo in aTypeInfo.MethodInfos)
                {
                    if (aMethodInfo.UnderlyingInfo.Equals(theMethod))
                    {
                        return aMethodInfo;
                    }
                }
            }

            foreach (ILLibrary depLib in Dependencies)
            {
                Types.MethodInfo result = depLib.GetMethodInfo(theMethod);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public Types.FieldInfo GetFieldInfo(Types.TypeInfo aTypeInfo, string FieldName)
        {
            foreach (Types.FieldInfo aFieldInfo in aTypeInfo.FieldInfos)
            {
                if (aFieldInfo.Name.Equals(FieldName))
                {
                    return aFieldInfo;
                }
            }

            if (aTypeInfo.UnderlyingType.BaseType != null &&
                !aTypeInfo.UnderlyingType.AssemblyQualifiedName.Contains("mscorlib"))
            {
                Types.TypeInfo baseTypeInfo = GetTypeInfo(aTypeInfo.UnderlyingType.BaseType);
                return GetFieldInfo(baseTypeInfo, FieldName);
            }
            
            throw new NullReferenceException("Field \"" + FieldName + "\" not found in type \"" + aTypeInfo.ToString() + "\".");
        }

        public ILBlock GetILBlock(Types.MethodInfo theInfo, bool checkDepLibs = true)
        {
            if (ILBlocks.ContainsKey(theInfo))
            {
                return ILBlocks[theInfo];
            }

            if (checkDepLibs)
            {
                foreach (ILLibrary depLib in Dependencies)
                {
                    ILBlock result = depLib.GetILBlock(theInfo);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            
            return null;
        }

        public string AddStringLiteral(string value)
        {
            //TODO: Don't add identical strings multiple times

            string ID = Utilities.FilterIdentifierForInvalidChars("StringLiteral_" + Guid.NewGuid().ToString());
            StringLiterals.Add(ID, value);
            return ID;
        }

        public override int GetHashCode()
        {
            return TheAssembly.FullName.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if(obj is ILLibrary)
            {
                return this.GetHashCode() == ((ILLibrary)obj).GetHashCode();
            }

            return base.Equals(obj);
        }
        public override string ToString()
        {
            return TheAssembly.FullName;
        }

        public List<ILLibrary> Flatten()
        {
            List<ILLibrary> result = new List<ILLibrary>();

            foreach (ILLibrary aDepLib in Dependencies)
            {
                List<ILLibrary> intermResult = aDepLib.Flatten();
                foreach (ILLibrary subLib in intermResult)
                {
                    if (!result.Contains(subLib))
                    {
                        result.Add(subLib);
                    }
                }
            }

            if (!result.Contains(this))
            {
                result.Add(this);
            }

            return result;
        }
    }
}
