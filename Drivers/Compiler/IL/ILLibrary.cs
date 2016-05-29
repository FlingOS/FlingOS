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
using System.Reflection;
using Drivers.Compiler.ASM;
using Drivers.Compiler.Types;
using FieldInfo = Drivers.Compiler.Types.FieldInfo;
using MethodInfo = Drivers.Compiler.Types.MethodInfo;
using TypeInfo = Drivers.Compiler.Types.TypeInfo;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     Represents a library (or executable) as a set of IL blocks.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An IL library also keeps track of the ASM library that it compiles to. It keeps track
    ///         of any and all (non-.Net Framework) dependencies and provides mechanisms for searching,
    ///         storing and caching both itself and its dependencies.
    ///     </para>
    ///     <para>
    ///         The IL Library class also contains several important mechanisms for keeping track of
    ///         global methods and data that must exist for the built executable or library to work.
    ///         These include keeping track of static constructors and flatenning dependencies such
    ///         that the tree of dependencies becomes a list with no duplicates.
    ///     </para>
    /// </remarks>
    public class ILLibrary
    {
        /// <summary>
        ///     The list of special classes found within the library and any of its dependencies.
        /// </summary>
        /// <remarks>
        ///     The key is the type of the attribute. The value is a list of all classes that were marked with the attribute.
        /// </remarks>
        public static Dictionary<Type, List<TypeInfo>> SpecialClasses = new Dictionary<Type, List<TypeInfo>>();

        /// <summary>
        ///     The list of special methods found within the library and any of its dependencies.
        /// </summary>
        /// <remarks>
        ///     The key is the type of the attribute. The value is a list of all methods that were marked with the attribute.
        /// </remarks>
        public static Dictionary<Type, List<MethodInfo>> SpecialMethods = new Dictionary<Type, List<MethodInfo>>();

        /// <summary>
        ///     The static constructor dependency tree for the library.
        /// </summary>
        /// <remarks>
        ///     Static constructors must be called in order such that constructors which depend/use
        ///     other classes which have static constructors are called after their dependencies.
        /// </remarks>
        public static StaticConstructorDependency TheStaticConstructorDependencyTree = new StaticConstructorDependency
        {
            TheConstructor = null
        };

        /// <summary>
        ///     The list of dependencies for the IL library.
        /// </summary>
        public List<ILLibrary> Dependencies = new List<ILLibrary>();

        /// <summary>
        ///     The list of all IL blocks generated for the library.
        /// </summary>
        /// <remarks>
        ///     This maps a method to the IL block generated from it.
        /// </remarks>
        public Dictionary<MethodInfo, ILBlock> ILBlocks = new Dictionary<MethodInfo, ILBlock>();

        /// <summary>
        ///     Whether the IL Preprocessor has been executed for the library.
        /// </summary>
        /// <remarks>
        ///     Prevents the IL Compiler executing the IL Preprocessor more than once for the same IL Library.
        /// </remarks>
        public bool ILPreprocessed = false;

        /// <summary>
        ///     Whether the IL Reader has been executed for the library.
        /// </summary>
        /// <remarks>
        ///     Prevents the IL Compiler executing the IL Reader more than once for the same IL Library.
        /// </remarks>
        public bool ILRead = false;

        /// <summary>
        ///     Whether the IL Scanner has been executed for the library.
        /// </summary>
        /// <remarks>
        ///     Prevents the IL Compiler executing the IL Scanner more than once for the same IL Library.
        /// </remarks>
        public bool ILScanned = false;

        /// <summary>
        ///     A map of identifiers to string literals.
        /// </summary>
        /// <remarks>
        ///     String literals are strings which declared using double quotes within C# code.
        /// </remarks>
        public Dictionary<string, string> StringLiterals = new Dictionary<string, string>();

        /// <summary>
        ///     The ASM Library generated by compiling the IL Library.
        /// </summary>
        public ASMLibrary TheASMLibrary = new ASMLibrary();

        /// <summary>
        ///     The assembly which the IL Library was loaded from.
        /// </summary>
        public Assembly TheAssembly;

        /// <summary>
        ///     The list of type infos about all the types within the library.
        /// </summary>
        /// <remarks>
        ///     Generated by the TypeScanner.
        /// </remarks>
        public List<TypeInfo> TypeInfos = new List<TypeInfo>();

        /// <summary>
        ///     Gets the type info for the specified actual type.
        /// </summary>
        /// <param name="theType">The type to get type info for.</param>
        /// <param name="FullyProcess">
        ///     Default: true.
        ///     Should only be set to false by the TypeScanner.
        ///     Used to prevent reentrancy (/circular) processing within the type scanner.
        /// </param>
        /// <returns>The type info or null if not found.</returns>
        public TypeInfo GetTypeInfo(Type theType, bool FullyProcess = true)
        {
            return GetTypeInfo(theType, true, FullyProcess);
        }

        /// <summary>
        ///     Gets the type info for the specified actual type.
        /// </summary>
        /// <param name="theType">The type to get type info for.</param>
        /// <param name="topLevel">Whether to search the library's dependencies or not. Faslse to search dependencies.</param>
        /// <param name="FullyProcess">
        ///     Default: true.
        ///     Should only be set to false by the TypeScanner.
        ///     Used to prevent reentrancy (/circular) processing within the type scanner.
        /// </param>
        /// <returns>The type info or null if not found.</returns>
        private TypeInfo GetTypeInfo(Type theType, bool topLevel, bool FullyProcess)
        {
            foreach (TypeInfo aTypeInfo in TypeInfos)
            {
                if (aTypeInfo.UnderlyingType.Equals(theType))
                {
                    return aTypeInfo;
                }
            }

            foreach (ILLibrary depLib in Dependencies)
            {
                TypeInfo result = depLib.GetTypeInfo(theType, false, FullyProcess);
                if (result != null)
                {
                    return result;
                }
            }

            if (topLevel)
            {
                TypeInfo theTypeInfo = TypeScanner.ScanType(this, theType);
                if (FullyProcess)
                {
                    int count = 0;
                    do
                    {
                        count = TypeInfos.Count;
                        int start = count - 1;
                        for (int i = start; i < count; i++)
                        {
                            TypeScanner.ProcessType(this, TypeInfos[i]);
                        }
                        for (int i = start; i < count; i++)
                        {
                            TypeScanner.ProcessTypeFields(this, TypeInfos[i]);
                        }
                    } while (count < TypeInfos.Count);
                }
                return theTypeInfo;
            }
            return null;
        }

        /// <summary>
        ///     Gets the method info for the specified method.
        /// </summary>
        /// <param name="theMethod">The method to get method info for.</param>
        /// <returns>The method info or null if not found.</returns>
        public MethodInfo GetMethodInfo(MethodBase theMethod)
        {
            foreach (TypeInfo aTypeInfo in TypeInfos)
            {
                foreach (MethodInfo aMethodInfo in aTypeInfo.MethodInfos)
                {
                    if (aMethodInfo.UnderlyingInfo.Equals(theMethod))
                    {
                        return aMethodInfo;
                    }
                }
            }

            foreach (ILLibrary depLib in Dependencies)
            {
                MethodInfo result = depLib.GetMethodInfo(theMethod);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the field info for the specified field from the specified type.
        /// </summary>
        /// <param name="aTypeInfo">The type to get the field from.</param>
        /// <param name="FieldName">The name of the field to get info for.</param>
        /// <returns>The field info.</returns>
        /// <exception cref="System.NullReferenceException">Thrown if the field is not found.</exception>
        public FieldInfo GetFieldInfo(TypeInfo aTypeInfo, string FieldName)
        {
            foreach (FieldInfo aFieldInfo in aTypeInfo.FieldInfos)
            {
                if (aFieldInfo.Name.Equals(FieldName))
                {
                    return aFieldInfo;
                }
            }

            if (aTypeInfo.UnderlyingType.BaseType != null &&
                !aTypeInfo.UnderlyingType.AssemblyQualifiedName.Contains("mscorlib"))
            {
                TypeInfo baseTypeInfo = GetTypeInfo(aTypeInfo.UnderlyingType.BaseType);
                return GetFieldInfo(baseTypeInfo, FieldName);
            }

            throw new NullReferenceException("Field \"" + FieldName + "\" not found in type \"" + aTypeInfo +
                                             "\".");
        }

        /// <summary>
        ///     Gets the IL block for the specified method.
        /// </summary>
        /// <param name="theInfo">The method to get the IL block of.</param>
        /// <param name="checkDepLibs">Whether to check dependencies or not.</param>
        /// <returns>The IL block or null if none found.</returns>
        public ILBlock GetILBlock(MethodInfo theInfo, bool checkDepLibs = true)
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

        /// <summary>
        ///     Adds a string literal and returns its ID.
        /// </summary>
        /// <param name="value">The value of the string literal to add.</param>
        /// <returns>The ID of the string literal.</returns>
        public string AddStringLiteral(string value)
        {
            //TODO: Don't add identical strings multiple times

            string ID = Utilities.FilterIdentifierForInvalidChars("StringLiteral_" + Guid.NewGuid());
            StringLiterals.Add(ID, value);
            return ID;
        }

        /// <summary>
        ///     Gets a hash code for the library using the underlying assembly's full name.
        /// </summary>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            return TheAssembly.FullName.GetHashCode();
        }

        /// <summary>
        ///     Compares the library to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the object is an ILLibrary and the hash codes match. Otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ILLibrary)
            {
                return GetHashCode() == ((ILLibrary)obj).GetHashCode();
            }

            return base.Equals(obj);
        }

        /// <summary>
        ///     Gets a human-readable identifier for the IL library.
        /// </summary>
        /// <remarks>
        ///     Uses the underlying assembly's full name.
        /// </remarks>
        /// <returns>The identifier.</returns>
        public override string ToString()
        {
            return TheAssembly.FullName;
        }

        /// <summary>
        ///     Flattens the tree of dependencies such that there are no duplicates. No order is guaranteed.
        /// </summary>
        /// <returns>The flattened list.</returns>
        public List<ILLibrary> Flatten()
        {
            List<ILLibrary> result = new List<ILLibrary>();

            foreach (ILLibrary aDepLib in Dependencies)
            {
                List<ILLibrary> interimResult = aDepLib.Flatten();
                foreach (ILLibrary subLib in interimResult)
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