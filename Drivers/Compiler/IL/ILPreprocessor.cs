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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Drivers.Compiler.Attributes;
using Drivers.Compiler.IL.ILOps;
using Drivers.Compiler.Types;
using FieldInfo = System.Reflection.FieldInfo;
using MethodInfo = Drivers.Compiler.Types.MethodInfo;
using TypeInfo = Drivers.Compiler.Types.TypeInfo;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     The IL preprocessor manages prehandling of special methods and classes and injection
    ///     of IL ops for specfic situations (such as GC handling).
    /// </summary>
    public static class ILPreprocessor
    {
        private static readonly Dictionary<string, int> InterfaceMethodIds = new Dictionary<string, int>();
        private static int InterfaceMethodIdGenerator = -22;
        /* Tasks of the IL Preprocessor:
         *      - Pre-processing for special classes / methods:
         *              - Static constructors
         *      - Pre-scan IL ops to:
         *              - Type Scan any local variable which are of an unscanned types
         *      - Inject general ops (method start, method end, etc.)
         *      - Inject GC IL ops
         *      - Inject wrapping try-finally for GC
         *      - Inject IL ops for try-catch-finally
         */

        /// <summary>
        ///     Preprocesses the specified IL library and any dependencies.
        /// </summary>
        /// <param name="TheLibrary">The library to preprocess.</param>
        public static void Preprocess(ILLibrary TheLibrary)
        {
            if (TheLibrary == null)
            {
                return;
            }

            if (TheLibrary.ILPreprocessed)
            {
                return;
            }
            TheLibrary.ILPreprocessed = true;

            foreach (ILLibrary aDependency in TheLibrary.Dependencies)
            {
                Preprocess(aDependency);
            }

            foreach (MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                PreprocessMethodInfo(TheLibrary, aMethodInfo);
            }

            foreach (MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                if (!aMethodInfo.IsPlugged)
                {
                    DealWithCatchHandlers(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                    InjectGeneral1(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                    InjectGC(TheLibrary, aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                    InjectGeneral2(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                    InjectTryCatchFinally(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);

                    PreprocessILOps(TheLibrary, aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                }
            }

            TheLibrary.ILBlocks =
                TheLibrary.ILBlocks.Where(x => !x.Value.TheMethodInfo.UnderlyingInfo.DeclaringType.IsInterface)
                    .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        ///     Preprocesses any special classes within the specified library.
        /// </summary>
        /// <param name="RootLibrary">The root library being compiled.</param>
        public static void PreprocessSpecialClasses(ILLibrary RootLibrary)
        {
            //Is there anything to do here?
        }

        /// <summary>
        ///     Preprocesses any special methods within the specified library.
        /// </summary>
        /// <param name="RootLibrary">The root library being compiled.</param>
        public static void PreprocessSpecialMethods(ILLibrary RootLibrary)
        {
            // Setup calls to Static Constructors
            MethodInfo CallStaticConstructorsInfo =
                ILLibrary.SpecialMethods[typeof(CallStaticConstructorsMethodAttribute)].First();
            ILBlock CallStaticConstructorsBlock = RootLibrary.GetILBlock(CallStaticConstructorsInfo);
            List<ConstructorInfo> staticConstructorsToCall = ILLibrary.TheStaticConstructorDependencyTree.Flatten();
            foreach (ConstructorInfo anInfo in staticConstructorsToCall)
            {
                CallStaticConstructorsBlock.ILOps.Insert(CallStaticConstructorsBlock.ILOps.Count - 1,
                    new ILOp
                    {
                        opCode = OpCodes.Call,
                        Offset = 0,
                        ValueBytes = null,
                        MethodToCall = anInfo
                    }
                    );
            }
        }

        /// <summary>
        ///     Preprocesses the specified method.
        /// </summary>
        /// <param name="TheLibrary">The library being compiled.</param>
        /// <param name="theMethodInfo">The method to preprocess.</param>
        private static void PreprocessMethodInfo(ILLibrary TheLibrary, MethodInfo theMethodInfo)
        {
            if (theMethodInfo.Preprocessed)
            {
                return;
            }
            theMethodInfo.Preprocessed = true;

            if (theMethodInfo.Signature.Contains("ITest"))
            {
                Debugger.Break();
            }

            string sig = theMethodInfo.Signature;
            bool SetMethodID = true;
            if (!theMethodInfo.IsConstructor)
            {
                System.Reflection.MethodInfo methodInf = (System.Reflection.MethodInfo)theMethodInfo.UnderlyingInfo;
                if (methodInf.GetBaseDefinition() != methodInf)
                {
                    MethodInfo baseMethodInfo = TheLibrary.GetMethodInfo(methodInf.GetBaseDefinition());
                    PreprocessMethodInfo(TheLibrary, baseMethodInfo);
                    theMethodInfo.IDValue = baseMethodInfo.IDValue;
                    SetMethodID = false;
                }
                else
                {
                    MethodBase InterfaceMethod = GetInterfaceMethod(theMethodInfo);
                    if (InterfaceMethod != null)
                    {
                        theMethodInfo.IDValue = GetInterfaceMethodIDValue(InterfaceMethod);
                        SetMethodID = false;
                    }
                }
            }
            if (SetMethodID)
            {
                TypeInfo declarerTypeInfo = TheLibrary.GetTypeInfo(theMethodInfo.UnderlyingInfo.DeclaringType);
                int ID = GetMethodIDGenerator(TheLibrary, declarerTypeInfo);
                theMethodInfo.IDValue = ID + 1;
                declarerTypeInfo.MethodIDGenerator++;
            }

            int totalLocalsOffset = 0;
            theMethodInfo.LocalInfos = theMethodInfo.LocalInfos.OrderBy(x => x.Position).ToList();
            foreach (VariableInfo aVarInfo in theMethodInfo.LocalInfos)
            {
                //Causes processing of the type - in case it hasn't already been processed
                TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(aVarInfo.UnderlyingType);
                aVarInfo.TheTypeInfo = aTypeInfo;
                // Order of the following two lines matters
                //      Offset = Cumulative offset - size of current local
                totalLocalsOffset -= aTypeInfo.SizeOnStackInBytes;
                aVarInfo.Offset = totalLocalsOffset;
            }

            int totalArgsSize = 0;
            if (!theMethodInfo.IsStatic)
            {
                VariableInfo newVarInfo = new VariableInfo
                {
                    UnderlyingType = theMethodInfo.UnderlyingInfo.DeclaringType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(theMethodInfo.UnderlyingInfo.DeclaringType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);

                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }
            ParameterInfo[] args = theMethodInfo.UnderlyingInfo.GetParameters();
            foreach (ParameterInfo argItem in args)
            {
                VariableInfo newVarInfo = new VariableInfo
                {
                    UnderlyingType = argItem.ParameterType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(argItem.ParameterType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);
                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }

            ParameterInfo returnArgItem = theMethodInfo.IsConstructor
                ? null
                : ((System.Reflection.MethodInfo)theMethodInfo.UnderlyingInfo).ReturnParameter;
            if (returnArgItem != null)
            {
                VariableInfo newVarInfo = new VariableInfo
                {
                    UnderlyingType = returnArgItem.ParameterType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(returnArgItem.ParameterType)
                };

                theMethodInfo.ReturnInfo = newVarInfo;
                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }

            int offset = totalArgsSize + 8;
            theMethodInfo.ArgumentInfos = theMethodInfo.ArgumentInfos.OrderBy(x => x.Position).ToList();
            for (int i = 0; i < theMethodInfo.ArgumentInfos.Count; i++)
            {
                offset -= theMethodInfo.ArgumentInfos[i].TheTypeInfo.SizeOnStackInBytes;
                theMethodInfo.ArgumentInfos[i].Offset = offset;
            }
        }

        /// <summary>
        ///     Gets the next unique ID for a method of the specified type.
        /// </summary>
        /// <remarks>
        ///     Used for generatign IDs to go in the method tables for use in virtual calls (callvirt Il ops).
        /// </remarks>
        /// <param name="TheLibrary">The IL library being compiled.</param>
        /// <param name="aType">The type to get the next method ID from.</param>
        /// <returns>The next unique method ID.</returns>
        private static int GetMethodIDGenerator(ILLibrary TheLibrary, Type aType, bool useMethodCount = false)
        {
            TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(aType);
            return GetMethodIDGenerator(TheLibrary, aTypeInfo, useMethodCount);
        }

        /// <summary>
        ///     Gets the next unique ID for a method of the specified type.
        /// </summary>
        /// <remarks>
        ///     Used for generatign IDs to go in the method tables for use in virtual calls (callvirt Il ops).
        /// </remarks>
        /// <param name="TheLibrary">The IL library being compiled.</param>
        /// <param name="aTypeInfo">The type to get the next method ID from.</param>
        /// <returns>The next unique method ID.</returns>
        private static int GetMethodIDGenerator(ILLibrary TheLibrary, TypeInfo aTypeInfo, bool useMethodCount = false)
        {
            int totalGen = 0;
            if (useMethodCount)
            {
                totalGen = aTypeInfo.MethodInfos.Count;
            }
            else
            {
                totalGen = aTypeInfo.MethodIDGenerator;
            }
            if (aTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!aTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    totalGen += GetMethodIDGenerator(TheLibrary, aTypeInfo.UnderlyingType.BaseType, true);
                }
            }
            return totalGen;
        }

        private static MethodBase GetInterfaceMethod(MethodInfo methodInf)
        {
            Type DeclaringType = methodInf.UnderlyingInfo.DeclaringType;
            if (DeclaringType.IsInterface)
            {
                return methodInf.UnderlyingInfo;
            }

            Type[] InterfaceTypes = DeclaringType.GetInterfaces();
            foreach (Type InterfaceType in InterfaceTypes)
            {
                MethodBase[] InterfaceMethods = InterfaceType.GetMethods();
                foreach (MethodBase AnInterfaceMethod in InterfaceMethods)
                {
                    if (AnInterfaceMethod.Name == methodInf.UnderlyingInfo.Name &&
                        MatchingParameters(AnInterfaceMethod.GetParameters(), methodInf.UnderlyingInfo.GetParameters()))
                    {
                        // Assume one is an implementation of the other
                        //TODO: Is there a better way of doing this? Because IL allows for name-hiding/name-reuse that might cause this to be wrong

                        return AnInterfaceMethod;
                    }
                }
            }
            return null;
        }

        private static bool MatchingParameters(ParameterInfo[] params1, ParameterInfo[] params2)
        {
            if (params1.Length != params2.Length)
            {
                return false;
            }

            for (int i = 0; i < params1.Length; i++)
            {
                if (params1[i].ParameterType != params2[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetInterfaceMethodIDValue(MethodBase interfaceMethod)
        {
            string sig = MethodInfo.GetMethodSignature(interfaceMethod);
            if (InterfaceMethodIds.ContainsKey(sig))
            {
                return InterfaceMethodIds[sig];
            }
            int id = InterfaceMethodIdGenerator--;
            InterfaceMethodIds.Add(sig, id);
            return id;
        }

        /// <summary>
        ///     Preprocesses the IL ops of the specified method/IL block.
        /// </summary>
        /// <param name="TheLibrary">The library being compiled.</param>
        /// <param name="theMethodInfo">The method to preprocess.</param>
        /// <param name="theILBlock">The IL block for the method to preprocess.</param>
        private static void PreprocessILOps(ILLibrary TheLibrary, MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            StaticConstructorDependency staticConstructorDependencyRoot = null;
            if (theMethodInfo.UnderlyingInfo is ConstructorInfo &&
                theMethodInfo.IsStatic)
            {
                ConstructorInfo aConstructor = (ConstructorInfo)theMethodInfo.UnderlyingInfo;
                staticConstructorDependencyRoot = ILLibrary.TheStaticConstructorDependencyTree[aConstructor];
                if (staticConstructorDependencyRoot == null)
                {
                    staticConstructorDependencyRoot = new StaticConstructorDependency
                    {
                        TheConstructor = aConstructor
                    };
                    ILLibrary.TheStaticConstructorDependencyTree.Children.Add(staticConstructorDependencyRoot);
                }
            }

            ILPreprocessState preprosState = new ILPreprocessState
            {
                TheILLibrary = TheLibrary,
                Input = theILBlock
            };

            for (int i = 0; i < theILBlock.ILOps.Count; i++)
            {
                ILOp theOp = theILBlock.ILOps[i];

                // Remove cast class ops
                if ((ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Castclass)
                {
                    theILBlock.ILOps.RemoveAt(i);
                    i--;
                    continue;
                }
                if ((ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Nop)
                {
                    if (theMethodInfo.ApplyDebug)
                    {
                        theOp.IsDebugOp = true;
                    }
                }
                else if ((ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Call)
                {
                    if (theOp.MethodToCall != null &&
                        theOp.MethodToCall.DeclaringType.AssemblyQualifiedName.Contains("mscorlib"))
                    {
                        //We do not want to process ops which attempt to call methods in mscorlib!
                        theILBlock.ILOps.RemoveAt(i);
                        i--;

                        //We do not allow calls to methods declared in MSCorLib.
                        //Some of these calls can just be ignored (e.g. GetTypeFromHandle is
                        //  called by typeof operator).
                        //Ones which can't be ignored, will result in an error...by virtue of
                        //  the fact that they were ignored when they were required.

                        //But just to make sure we save ourselves a headache later when
                        //  runtime debugging, output a message saying we ignored the call.

                        // TODO: IL level comments
                        // result.ASM.AppendLine("; Call to method defined in MSCorLib ignored:"); // DEBUG INFO
                        // result.ASM.AppendLine("; " + anILOpInfo.MethodToCall.DeclaringType.FullName + "." + anILOpInfo.MethodToCall.Name); // DEBUG INFO

                        //If the method is a call to a constructor in MsCorLib:
                        if (theOp.MethodToCall is ConstructorInfo)
                        {
                            //Then we can presume it was a call to a base-class constructor (e.g. the Object constructor)
                            //  and so we just need to remove any args that were loaded onto the stack.
                            // TODO: result.ASM.AppendLine("; Method to call was constructor so removing params"); // DEBUG INFO

                            //Remove args from stack
                            //If the constructor was non-static, then the first arg is the instance reference.
                            if (!theOp.MethodToCall.IsStatic)
                            {
                                i++;
                                theILBlock.ILOps.Insert(i, new ILOp
                                {
                                    opCode = OpCodes.Pop,
                                    Offset = theOp.Offset
                                });
                            }
                            foreach (ParameterInfo anInfo in theOp.MethodToCall.GetParameters())
                            {
                                i++;
                                theILBlock.ILOps.Insert(i, new ILOp
                                {
                                    opCode = OpCodes.Pop,
                                    Offset = theOp.Offset
                                });
                            }
                        }
                    }
                }

                try
                {
                    ILOp ConverterOp = TargetArchitecture.TargetILOps[(ILOp.OpCodes)theOp.opCode.Value];

                    ConverterOp.Preprocess(preprosState, theOp);

                    if (staticConstructorDependencyRoot != null)
                    {
                        //Create our static constructor dependency tree

                        //Each of these ops could try to access a static method or field
                        switch ((ILOp.OpCodes)theOp.opCode.Value)
                        {
                            case ILOp.OpCodes.Call:
                                //Check if the method to call is static and not a constructor itself
                                //If so, we must add the declaring type's static constructors to the tree
                                if (theOp.ValueBytes != null && theOp.ValueBytes.Length > 0)
                                {
                                    int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                                    MethodBase methodBaseInf =
                                        theMethodInfo.UnderlyingInfo.Module.ResolveMethod(metadataToken);
                                    if (!(methodBaseInf.IsConstructor || methodBaseInf is ConstructorInfo))
                                    {
                                        System.Reflection.MethodInfo methodInf =
                                            (System.Reflection.MethodInfo)methodBaseInf;
                                        ConstructorInfo[] staticConstructors =
                                            methodInf.DeclaringType.GetConstructors(BindingFlags.Static |
                                                                                    BindingFlags.Public)
                                                .Concat(
                                                    methodInf.DeclaringType.GetConstructors(BindingFlags.Static |
                                                                                            BindingFlags.NonPublic))
                                                .ToArray();
                                        if (staticConstructors.Length > 0)
                                        {
                                            ConstructorInfo TheConstructor = staticConstructors[0];
                                            if (staticConstructorDependencyRoot[TheConstructor] == null)
                                            {
                                                staticConstructorDependencyRoot.Children.Add(new StaticConstructorDependency
                                                {
                                                    TheConstructor = TheConstructor
                                                });
                                            }
                                        }
                                    }
                                }
                                break;
                            case ILOp.OpCodes.Ldsfld:
                            case ILOp.OpCodes.Ldsflda:
                            case ILOp.OpCodes.Stsfld:
                            {
                                int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                                FieldInfo fieldInf = theMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
                                ConstructorInfo[] staticConstructors =
                                    fieldInf.DeclaringType.GetConstructors(BindingFlags.Static | BindingFlags.Public)
                                        .Concat(
                                            fieldInf.DeclaringType.GetConstructors(BindingFlags.Static |
                                                                                   BindingFlags.NonPublic))
                                        .ToArray();
                                if (staticConstructors.Length > 0)
                                {
                                    ConstructorInfo TheConstructor = staticConstructors[0];
                                    if (staticConstructorDependencyRoot[TheConstructor] == null)
                                    {
                                        staticConstructorDependencyRoot.Children.Add(new StaticConstructorDependency
                                        {
                                            TheConstructor = TheConstructor
                                        });
                                    }
                                }
                            }
                                break;
                        }
                    }
                }
                catch (KeyNotFoundException)
                {
                    //Ignore - will be caught by Il scanner
                }
                catch (Exception ex)
                {
                    Logger.LogError("ILPRE", theILBlock.TheMethodInfo.ToString(), 0,
                        "Il Preprocessor error: PreprocessILOps: " + ex.Message);
                }
            }
        }

        /// <summary>
        ///     Injects the first set of general IL ops.
        /// </summary>
        /// <remarks>
        ///     Ops that must be inserted before the GC ops are inserted.
        /// </remarks>
        /// <param name="theMethodInfo">The method to inject ops into.</param>
        /// <param name="theILBlock">The IL block for the method to inject ops into.</param>
        private static void InjectGeneral1(MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            // Inject MethodStart op
            theILBlock.ILOps.Insert(0, new MethodStart
            {
                Offset = -1
            });
        }

        /// <summary>
        ///     Injects the second set of general IL ops.
        /// </summary>
        /// <remarks>
        ///     Ops that must be inserted after the GC ops are inserted.
        /// </remarks>
        /// <param name="theMethodInfo">The method to inject ops into.</param>
        /// <param name="theILBlock">The IL block for the method to inject ops into.</param>
        private static void InjectGeneral2(MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            // Inject MethodEnd op just before anywhere where there is a ret
            for (int i = 0; i < theILBlock.ILOps.Count; i++)
            {
                ILOp theOp = theILBlock.ILOps[i];

                if ((ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Ret)
                {
                    theILBlock.ILOps.Insert(i, new MethodEnd
                    {
                        Offset = theOp.Offset,
                        BytesSize = theOp.BytesSize
                    });
                    theOp.Offset = -1;
                    i++;
                }
            }
        }

        /// <summary>
        ///     Injects the garbage collector related IL ops into the specified method.
        /// </summary>
        /// <param name="TheLibrary">The library being compiled.</param>
        /// <param name="theMethodInfo">The method to inject ops into.</param>
        /// <param name="theILBlock">The IL block for the method to inject ops into.</param>
        private static void InjectGC(ILLibrary TheLibrary, MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            if (theMethodInfo.ApplyGC)
            {
                // Find the index of the MethodStart op
                int MethodStartOpPos = theILBlock.PositionOf(theILBlock.ILOps.Where(x => x is MethodStart).First());

                // Inject ops for incrementing ref. count of args at start of method
                int InjectIncArgsRefCountPos = MethodStartOpPos + 1;
                foreach (VariableInfo argInfo in theMethodInfo.ArgumentInfos)
                {
                    if (argInfo.TheTypeInfo.IsGCManaged)
                    {
                        theILBlock.ILOps.Insert(InjectIncArgsRefCountPos, new ILOp
                        {
                            opCode = OpCodes.Ldarg,
                            Offset = -1,
                            ValueBytes = BitConverter.GetBytes(argInfo.Position)
                        });
                        theILBlock.ILOps.Insert(InjectIncArgsRefCountPos + 1, new ILOp
                        {
                            opCode = OpCodes.Call,
                            Offset = -1,
                            MethodToCall =
                                ILLibrary.SpecialMethods[typeof(IncrementRefCountMethodAttribute)].First()
                                    .UnderlyingInfo
                        });
                    }
                }

                // The following two things can be done within the same loop:

                // Inject ops for inc./dec. ref. counts of objects when written to:
                //      - Arguments / Locals
                //      - Fields / Static Fields
                //      - Elements of Arrays

                // Add Cleanup Block and Inject finally-block ops for it
                //      - Also remember the op for storing return value (if any)

                ILPreprocessState preprocessState = new ILPreprocessState
                {
                    TheILLibrary = TheLibrary,
                    Input = theILBlock,
                    CurrentStackFrame = new StackFrame()
                };

                ExceptionHandledBlock CleanupExBlock = new ExceptionHandledBlock();

                for (int opIndx = 0; opIndx < theILBlock.ILOps.Count; opIndx++)
                {
                    ILOp currOp = theILBlock.ILOps[opIndx];
                    bool IncRefCount = false;
                    int incOpIndexBy = 0;

                    switch ((ILOp.OpCodes)currOp.opCode.Value)
                    {
                        case ILOp.OpCodes.Stsfld:

                            #region Stsfld

                        {
                            int metadataToken = Utilities.ReadInt32(currOp.ValueBytes, 0);
                            FieldInfo theField = theMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
                            TypeInfo theFieldTypeInfo = TheLibrary.GetTypeInfo(theField.FieldType);
                            if (theFieldTypeInfo.IsGCManaged)
                            {
                                theILBlock.ILOps.Insert(opIndx, new ILOp
                                {
                                    opCode = OpCodes.Ldsfld,
                                    Offset = currOp.Offset,
                                    ValueBytes = currOp.ValueBytes
                                });
                                theILBlock.ILOps.Insert(opIndx + 1, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = currOp.Offset,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });

                                IncRefCount = true;
                                incOpIndexBy = 2;
                            }
                        }

                            #endregion

                            break;
                        case ILOp.OpCodes.Stloc:
                        case ILOp.OpCodes.Stloc_0:
                        case ILOp.OpCodes.Stloc_1:
                        case ILOp.OpCodes.Stloc_2:
                        case ILOp.OpCodes.Stloc_3:
                        case ILOp.OpCodes.Stloc_S:

                            #region Stloc

                        {
                            ushort localIndex = 0;
                            switch ((ILOp.OpCodes)currOp.opCode.Value)
                            {
                                case ILOp.OpCodes.Stloc:
                                    localIndex = (ushort)Utilities.ReadInt16(currOp.ValueBytes, 0);
                                    break;
                                case ILOp.OpCodes.Stloc_0:
                                    localIndex = 0;
                                    break;
                                case ILOp.OpCodes.Stloc_1:
                                    localIndex = 1;
                                    break;
                                case ILOp.OpCodes.Stloc_2:
                                    localIndex = 2;
                                    break;
                                case ILOp.OpCodes.Stloc_3:
                                    localIndex = 3;
                                    break;
                                case ILOp.OpCodes.Stloc_S:
                                    localIndex = currOp.ValueBytes[0];
                                    break;
                            }
                            TypeInfo LocalTypeInfo = theMethodInfo.LocalInfos[localIndex].TheTypeInfo;
                            if (LocalTypeInfo.IsGCManaged)
                            {
                                theILBlock.ILOps.Insert(opIndx, new ILOp
                                {
                                    opCode = OpCodes.Ldloc,
                                    Offset = currOp.Offset,
                                    ValueBytes = BitConverter.GetBytes(localIndex)
                                });
                                theILBlock.ILOps.Insert(opIndx + 1, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = currOp.Offset,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });

                                IncRefCount = true;
                                incOpIndexBy = 2;
                            }
                        }

                            #endregion

                            break;
                        case ILOp.OpCodes.Stfld:

                            #region Stfld

                        {
                            int metadataToken = Utilities.ReadInt32(currOp.ValueBytes, 0);
                            FieldInfo theField = theMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
                            TypeInfo theFieldTypeInfo = TheLibrary.GetTypeInfo(theField.FieldType);
                            if (theFieldTypeInfo.IsGCManaged)
                            {
                                // Items on stack:
                                //  - Object reference
                                //  - (New) Value to store
                                //
                                // We want to load the current value of the field
                                //  for which we must duplicate the object ref
                                // But first, we must remove the (new) value to store
                                //  off the stack, while also storing it to put back
                                //  on the stack after so the store can continue
                                //
                                // So:
                                //      1. Switch value to store and object ref on stack
                                //      3. Duplicate the object ref
                                //      4. Load the field value
                                //      5. Call dec ref count
                                //      6. Switch value to store and object ref back again

                                //USE A SWITCH STACK ITEMS OP!!

                                theILBlock.ILOps.Insert(opIndx, new StackSwitch
                                {
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 2
                                });

                                theILBlock.ILOps.Insert(opIndx + 1, new ILOp
                                {
                                    Offset = currOp.Offset,
                                    opCode = OpCodes.Dup
                                });
                                theILBlock.ILOps.Insert(opIndx + 2, new ILOp
                                {
                                    opCode = OpCodes.Ldfld,
                                    Offset = currOp.Offset,
                                    ValueBytes = currOp.ValueBytes
                                });
                                theILBlock.ILOps.Insert(opIndx + 3, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = currOp.Offset,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });

                                theILBlock.ILOps.Insert(opIndx + 4, new StackSwitch
                                {
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 2
                                });

                                IncRefCount = true;
                                incOpIndexBy = 5;
                            }
                        }

                            #endregion

                            break;
                        case ILOp.OpCodes.Stelem:
                        case ILOp.OpCodes.Stelem_Ref:

                            #region Stelem / Stelem_Ref

                        {
                            bool doDecrement = false;
                            bool isRefOp = false;
                            if ((ILOp.OpCodes)currOp.opCode.Value == ILOp.OpCodes.Stelem_Ref)
                            {
                                doDecrement = preprocessState.CurrentStackFrame.GetStack(currOp).Peek().isGCManaged;
                                isRefOp = true;
                            }
                            else
                            {
                                int metadataToken = Utilities.ReadInt32(currOp.ValueBytes, 0);
                                Type elementType = theMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                                doDecrement = TheLibrary.GetTypeInfo(elementType).IsGCManaged;
                            }

                            if (doDecrement)
                            {
                                // Items on stack:
                                //  - Array reference
                                //  - Index
                                //  - (New) Value to store
                                //
                                // We want to load the current value of the element at Index in the array
                                //  for which we must duplicate the array ref and index
                                // But first, we must remove the (new) value to store
                                //  off the stack, while also storing it to put back
                                //  on the stack after so the store can continue
                                //
                                // So:
                                //      1. Switch (rotate) 1 times the top 3 values so that index is on top
                                //      2. Duplicate the index
                                //      3. Switch (rotate) 2 times the top 4 values so that array ref is on top
                                //      4. Duplicate the array ref
                                //      5. Switch (rotate) 4 times the top 5 values so that duplicate array ref and index are on top
                                //      6. Do LdElem op to load existing element value
                                //      7. Call GC.DecrementRefCount
                                //      8. Switch (rotate) 1 times the top 3 values so that the stack is in its original state
                                //      (9. Continue to increment ref count as normal)
                                //
                                // The following is a diagram of the stack manipulation occurring here:
                                //      Key: A=Array ref, I=Index, V=Value to store, E=Loaded element
                                //      Top-most stack item appears last
                                //  
                                //     1) Rotate x 1    2) Duplicate       3)  Rot x 2         4)  Dup
                                //  A,I,V ---------> V,A,I ---------> V,A,I,I ---------> I,I,V,A ---------> I,I,V,A,A
                                //
                                //
                                //          5) Rot x 4           6) Ldelem        7) Call GC (Dec)
                                //  I,I,V,A,A ---------> I,V,A,A,I ---------> I,V,A,E ---------> I,V,A
                                //
                                //
                                //      8) Rot x 1       9)  Dup         10) Call GC (Inc)
                                //  I,V,A ---------> A,I,V ---------> A,I,V,V ---------> A,I,V

                                #region 1.

                                theILBlock.ILOps.Insert(opIndx, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(3),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 3
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 2.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new ILOp
                                {
                                    Offset = currOp.Offset,
                                    opCode = OpCodes.Dup
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 3.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(4),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 4
                                });
                                incOpIndexBy++;

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(4),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 4
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 4.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new ILOp
                                {
                                    Offset = currOp.Offset,
                                    opCode = OpCodes.Dup
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 5.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(5),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 5
                                });
                                incOpIndexBy++;
                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(5),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 5
                                });
                                incOpIndexBy++;
                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(5),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 5
                                });
                                incOpIndexBy++;
                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(5),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 5
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 6.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new ILOp
                                {
                                    opCode = isRefOp ? OpCodes.Ldelem_Ref : OpCodes.Ldelem,
                                    Offset = currOp.Offset,
                                    ValueBytes = currOp.ValueBytes
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 7.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = currOp.Offset,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });
                                incOpIndexBy++;

                                #endregion

                                #region 8.

                                theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new StackSwitch
                                {
                                    ValueBytes = BitConverter.GetBytes(3),
                                    Offset = currOp.Offset,
                                    StackSwitch_Items = 3
                                });
                                incOpIndexBy++;

                                #endregion

                                IncRefCount = true;
                            }
                        }

                            #endregion

                            break;
                        case ILOp.OpCodes.Starg:
                        case ILOp.OpCodes.Starg_S:

                            #region Starg

                        {
                            ushort index = (ILOp.OpCodes)currOp.opCode.Value == ILOp.OpCodes.Starg_S
                                ? currOp.ValueBytes[0]
                                : (ushort)Utilities.ReadInt16(currOp.ValueBytes, 0);
                            if (theMethodInfo.ArgumentInfos[index].TheTypeInfo.IsGCManaged)
                            {
                                theILBlock.ILOps.Insert(opIndx, new ILOp
                                {
                                    opCode = OpCodes.Ldarg,
                                    Offset = currOp.Offset,
                                    ValueBytes = BitConverter.GetBytes(index)
                                });
                                theILBlock.ILOps.Insert(opIndx + 1, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = currOp.Offset,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });

                                IncRefCount = true;
                                incOpIndexBy = 2;
                            }
                        }

                            #endregion

                            break;
                    }

                    if (IncRefCount &&
                        !preprocessState.CurrentStackFrame.GetStack(currOp).Peek().isNewGCObject)
                    {
                        theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new ILOp
                        {
                            Offset = currOp.Offset,
                            opCode = OpCodes.Dup
                        });
                        incOpIndexBy++;
                        theILBlock.ILOps.Insert(opIndx + incOpIndexBy, new ILOp
                        {
                            opCode = OpCodes.Call,
                            Offset = currOp.Offset,
                            MethodToCall =
                                ILLibrary.SpecialMethods[typeof(IncrementRefCountMethodAttribute)].First()
                                    .UnderlyingInfo
                        });
                        incOpIndexBy++;
                    }

                    // If op changed
                    if (theILBlock.ILOps[opIndx] != currOp)
                    {
                        theILBlock.ILOps[opIndx].Offset = currOp.Offset;
                        theILBlock.ILOps[opIndx].BytesSize = currOp.BytesSize;
                    }

                    // <= is correct. E.g. if 1 extra op added, incOpIndex=1 so <= results in currOp processed
                    //      + extra op processed
                    bool UseNextOpAsCleanupStart = false;
                    for (int i = 0; i <= incOpIndexBy; i++)
                    {
                        currOp = theILBlock.ILOps[opIndx];

                        if (UseNextOpAsCleanupStart)
                        {
                            UseNextOpAsCleanupStart = false;

                            CleanupExBlock.Offset = currOp.Offset;
                        }

                        if (currOp is MethodStart)
                        {
                            TargetArchitecture.MethodStartOp.PerformStackOperations(preprocessState,
                                theILBlock.ILOps[opIndx]);
                            UseNextOpAsCleanupStart = true;
                        }
                        else if (currOp is MethodEnd)
                        {
                            TargetArchitecture.MethodEndOp.PerformStackOperations(preprocessState,
                                theILBlock.ILOps[opIndx]);
                        }
                        else if (currOp is StackSwitch)
                        {
                            TargetArchitecture.StackSwitchOp.PerformStackOperations(preprocessState,
                                theILBlock.ILOps[opIndx]);
                        }
                        else
                        {
                            // Leave unsupported ops for the IL Scanner to deal with (or later code e.g. castclass op)
                            if (TargetArchitecture.TargetILOps.ContainsKey((ILOp.OpCodes)currOp.opCode.Value))
                            {
                                ILOp ConverterOp = TargetArchitecture.TargetILOps[(ILOp.OpCodes)currOp.opCode.Value];
                                ConverterOp.PerformStackOperations(preprocessState, currOp);
                            }
                        }

                        if (i > 0)
                        {
                            opIndx++;
                        }
                    }
                }

                if (theMethodInfo.ArgumentInfos.Count > 0 ||
                    theMethodInfo.LocalInfos.Count > 0)
                {
                    bool AddCleanupBlock = false;
                    foreach (VariableInfo anArgInfo in theMethodInfo.ArgumentInfos)
                    {
                        if (anArgInfo.TheTypeInfo.IsGCManaged)
                        {
                            AddCleanupBlock = true;
                            break;
                        }
                    }
                    if (!AddCleanupBlock)
                    {
                        foreach (VariableInfo aLocInfo in theMethodInfo.LocalInfos)
                        {
                            if (aLocInfo.TheTypeInfo.IsGCManaged)
                            {
                                AddCleanupBlock = true;
                                break;
                            }
                        }
                    }

                    if (AddCleanupBlock)
                    {
                        ILOp lastOp = theILBlock.ILOps.Last();
                        int lastOpOffset = lastOp.Offset;
                        int lastOpIndex = theILBlock.ILOps.Count - 1;
                        bool MethodHasReturnValue = false;

                        // If there is a return value, we will need to temp store it
                        if (theMethodInfo.UnderlyingInfo is System.Reflection.MethodInfo)
                        {
                            Type returnType = ((System.Reflection.MethodInfo)theMethodInfo.UnderlyingInfo).ReturnType;
                            //Void return type = no return value
                            if (returnType != typeof(void))
                            {
                                // Add local variable for storing return value
                                int lastLocalOffset = theMethodInfo.LocalInfos.Count > 0
                                    ? theMethodInfo.LocalInfos.Last().Offset
                                    : 0;
                                TypeInfo returnTypeInfo = TheLibrary.GetTypeInfo(returnType);
                                theMethodInfo.LocalInfos.Add(new VariableInfo
                                {
                                    UnderlyingType = returnType,
                                    TheTypeInfo = returnTypeInfo,
                                    Position = theMethodInfo.LocalInfos.Count,
                                    Offset = lastLocalOffset - returnTypeInfo.SizeOnStackInBytes,
                                    // Local variable offsets are negative from EBP
                                    SkipCleanup = true
                                });

                                if (returnTypeInfo.IsGCManaged)
                                {
                                    // Add ops for incrementing ref count of return value, updare op offsets
                                    theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                    {
                                        Offset = lastOpOffset,
                                        opCode = OpCodes.Dup
                                    });
                                    lastOpIndex++;
                                    theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                    {
                                        opCode = OpCodes.Call,
                                        Offset = lastOpOffset,
                                        MethodToCall =
                                            ILLibrary.SpecialMethods[typeof(IncrementRefCountMethodAttribute)].First()
                                                .UnderlyingInfo
                                    });
                                    lastOpIndex++;
                                }

                                // Add op for storing return value, update op offsets
                                theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                {
                                    opCode = OpCodes.Stloc,
                                    Offset = lastOpOffset,
                                    BytesSize = lastOp.BytesSize,
                                    ValueBytes = BitConverter.GetBytes(theMethodInfo.LocalInfos.Count - 1)
                                });
                                lastOpIndex++;

                                MethodHasReturnValue = true;
                            }
                        }

                        ILOp leaveOp;
                        // Add the Leave op of the try-block
                        theILBlock.ILOps.Insert(lastOpIndex, leaveOp = new ILOp
                        {
                            opCode = OpCodes.Leave,
                            Offset = lastOpOffset,
                            BytesSize = lastOp.BytesSize,
                            ValueBytes = BitConverter.GetBytes(0)
                        });
                        lastOpIndex++;

                        FinallyBlock CleanupFinallyBlock = new FinallyBlock
                        {
                            Offset = lastOpOffset + lastOp.BytesSize,
                            Length = 0
                        };
                        CleanupExBlock.Length = lastOpOffset - CleanupExBlock.Offset;
                        CleanupExBlock.FinallyBlocks.Add(CleanupFinallyBlock);

                        int cleanupOpsOffset = lastOpOffset + 1;

                        // Add cleanup code for local variables (including the return value local)
                        foreach (VariableInfo aLocInfo in theMethodInfo.LocalInfos)
                        {
                            if (aLocInfo.TheTypeInfo.IsGCManaged && !aLocInfo.SkipCleanup)
                            {
                                theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                {
                                    opCode = OpCodes.Ldloc,
                                    Offset = cleanupOpsOffset,
                                    BytesSize = 1,
                                    ValueBytes = BitConverter.GetBytes(aLocInfo.Position)
                                });
                                cleanupOpsOffset++;
                                lastOpIndex++;

                                theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = cleanupOpsOffset,
                                    BytesSize = 1,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });
                                cleanupOpsOffset++;
                                lastOpIndex++;

                                CleanupFinallyBlock.Length += 2;
                            }
                        }

                        // Add cleanup code for arguments
                        foreach (VariableInfo anArgInfo in theMethodInfo.ArgumentInfos)
                        {
                            if (anArgInfo.TheTypeInfo.IsGCManaged && !anArgInfo.SkipCleanup)
                            {
                                theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                {
                                    opCode = OpCodes.Ldarg,
                                    Offset = cleanupOpsOffset,
                                    BytesSize = 1,
                                    ValueBytes = BitConverter.GetBytes(anArgInfo.Position)
                                });
                                cleanupOpsOffset++;
                                lastOpIndex++;

                                theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                                {
                                    opCode = OpCodes.Call,
                                    Offset = cleanupOpsOffset,
                                    BytesSize = 1,
                                    MethodToCall =
                                        ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First()
                                            .UnderlyingInfo
                                });
                                cleanupOpsOffset++;
                                lastOpIndex++;

                                CleanupFinallyBlock.Length += 2;
                            }
                        }

                        // Add end finally op
                        theILBlock.ILOps.Insert(lastOpIndex, leaveOp.LoadAtILOpAfterOp = new ILOp
                        {
                            opCode = OpCodes.Endfinally,
                            Offset = cleanupOpsOffset,
                            BytesSize = 1
                        });
                        cleanupOpsOffset++;
                        lastOpIndex++;

                        CleanupFinallyBlock.Length++;

                        // Add restore return value op
                        if (MethodHasReturnValue)
                        {
                            theILBlock.ILOps.Insert(lastOpIndex, new ILOp
                            {
                                opCode = OpCodes.Ldloc,
                                Offset = cleanupOpsOffset,
                                BytesSize = 1,
                                ValueBytes = BitConverter.GetBytes(theMethodInfo.LocalInfos.Count - 1)
                            });
                            cleanupOpsOffset++;
                            lastOpIndex++;
                        }

                        // Add ex block to the method
                        theILBlock.ExceptionHandledBlocks.Add(CleanupExBlock);

                        // Replace any Ret ops contained within Cleanup Block with:
                        //      - Op to store return value (if any)
                        //      - Leave op
                        bool Inside = false;
                        for (int opIndx = 0; opIndx < theILBlock.ILOps.Count; opIndx++)
                        {
                            if (theILBlock.ILOps[opIndx].Offset > CleanupExBlock.Offset + CleanupExBlock.Length)
                            {
                                break;
                            }
                            if (theILBlock.ILOps[opIndx].Offset >= CleanupExBlock.Offset)
                            {
                                Inside = true;
                            }

                            if (Inside &&
                                (ILOp.OpCodes)theILBlock.ILOps[opIndx].opCode.Value == ILOp.OpCodes.Ret)
                            {
                                ILOp ARetOp = theILBlock.ILOps[opIndx];
                                theILBlock.ILOps.RemoveAt(opIndx);

                                if (MethodHasReturnValue)
                                {
                                    theILBlock.ILOps.Insert(opIndx, new ILOp
                                    {
                                        opCode = OpCodes.Stloc,
                                        Offset = ARetOp.Offset,
                                        ValueBytes = BitConverter.GetBytes(theMethodInfo.LocalInfos.Count - 1)
                                    });
                                    theILBlock.ILOps.Insert(opIndx + 1, new ILOp
                                    {
                                        Offset = ARetOp.Offset,
                                        opCode = OpCodes.Leave,
                                        ValueBytes = BitConverter.GetBytes(0),
                                        LoadAtILOpAfterOp = leaveOp.LoadAtILOpAfterOp
                                    });
                                }
                                else
                                {
                                    theILBlock.ILOps.Insert(opIndx, new ILOp
                                    {
                                        Offset = ARetOp.Offset,
                                        opCode = OpCodes.Leave,
                                        ValueBytes = BitConverter.GetBytes(0),
                                        LoadAtILOpAfterOp = leaveOp.LoadAtILOpAfterOp
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Handles catch handles (of exception blocks) for the specified method.
        /// </summary>
        /// <param name="theMethodInfo">The method to handle.</param>
        /// <param name="theILBlock">The IL block for the method to handle.</param>
        private static void DealWithCatchHandlers(MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            foreach (ExceptionHandledBlock exBlock in theILBlock.ExceptionHandledBlocks)
            {
                #region Catch-sections

                // Strip the first pop of catch-handler

                foreach (CatchBlock aCatchBlock in exBlock.CatchBlocks)
                {
                    ILOp catchPopOp = theILBlock.At(aCatchBlock.Offset);
                    int pos = theILBlock.PositionOf(catchPopOp);
                    theILBlock.ILOps.RemoveAt(pos);
                    theILBlock.ILOps.Insert(pos, new ILOp
                    {
                        opCode = OpCodes.Nop,
                        Offset = catchPopOp.Offset,
                        BytesSize = catchPopOp.BytesSize
                    });
                }

                #endregion
            }
        }

        /// <summary>
        ///     Injects the try-catch-finally related IL ops into the specified method.
        /// </summary>
        /// <param name="theMethodInfo">The method to inject ops into.</param>
        /// <param name="theILBlock">The IL block for the method to inject ops into.</param>
        private static void InjectTryCatchFinally(MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            // Replace Leave and Leave_S ops
            for (int i = 0; i < theILBlock.ILOps.Count; i++)
            {
                ILOp theOp = theILBlock.ILOps[i];

                if ((ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Leave ||
                    (ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Leave_S)
                {
                    theILBlock.ILOps.RemoveAt(i);

                    int ILOffset = 0;
                    if (theOp.LoadAtILOpAfterOp == null)
                    {
                        if (theOp.opCode.Value == (int)ILOp.OpCodes.Leave)
                        {
                            ILOffset = BitConverter.ToInt32(theOp.ValueBytes, 0);
                        }
                        else
                        {
                            ILOffset = theOp.ValueBytes[0];
                        }
                    }

                    theILBlock.ILOps.Insert(i, new ILOp
                    {
                        Offset = theOp.Offset,
                        BytesSize = theOp.BytesSize,
                        opCode = OpCodes.Ldftn,
                        LoadAtILOffset = theOp.NextOffset + ILOffset,
                        LoadAtILOpAfterOp = theOp.LoadAtILOpAfterOp,
                        MethodToCall = theILBlock.TheMethodInfo.UnderlyingInfo
                    });
                    theILBlock.ILOps.Insert(i + 1, new ILOp
                    {
                        opCode = OpCodes.Call,
                        Offset = theOp.Offset,
                        MethodToCall =
                            ILLibrary.SpecialMethods[typeof(ExceptionsHandleLeaveMethodAttribute)].First()
                                .UnderlyingInfo
                    });

                    i++;
                }
                else if (theOp.opCode.Value == (int)ILOp.OpCodes.Endfinally)
                {
                    //Endfinally is for leaving a (critical) finally section
                    //We handle it by a higher-level implementation rather than 
                    //  leaving it to each architecture to implement.

                    //Endfinally is handled by inserting a call to the Exceptions.HandleEndFinally method

                    theILBlock.ILOps.RemoveAt(i);

                    ILOp newOp;
                    theILBlock.ILOps.Insert(i, newOp = new ILOp
                    {
                        Offset = theOp.Offset,
                        BytesSize = theOp.BytesSize,
                        opCode = OpCodes.Call,
                        MethodToCall =
                            ILLibrary.SpecialMethods[typeof(ExceptionsHandleEndFinallyMethodAttribute)].First()
                                .UnderlyingInfo
                    });

                    //Replace references to this endfinally op
                    for (int j = 0; j < theILBlock.ILOps.Count; j++)
                    {
                        if (theILBlock.ILOps[j].LoadAtILOpAfterOp == theOp)
                        {
                            theILBlock.ILOps[j].LoadAtILOpAfterOp = newOp;
                        }
                    }
                }
            }


            foreach (ExceptionHandledBlock exBlock in theILBlock.ExceptionHandledBlocks)
            {
                #region Try-sections

                //      Insert the start of try-block
                //          Also remember that other ops (e.g. branch and leave) can hold references
                //          to the first op INSIDE a try block, even if they themselves are OUTSIDE 
                //          the block. This means we need to correct which op has the Offset value
                //          set so those ops point at the new start op of the try block.

                //Consists of adding a new ExceptionHandlerInfos
                //  built from the info in exBlock so we:
                //      - Add infos for all finally blocks first
                //      - Then add infos for all catch blocks
                //  Since finally code is always run after catch code in C#,
                //      by adding catch handlers after finally handlers, they 
                //      appear as the inner-most exception handlers and so get 
                //      run before finally handlers.

                //To add a new ExceptionHandlerInfo we must set up args for 
                //  calling Exceptions.AddExceptionHandlerInfo:
                // 1. We load a pointer to the handler
                //      - This is calculated from an offset from the start of the function to the handler
                // 2. We load a pointer to the filter
                //      - This is calculated from an offset from the start of the function to the filter
                //      Note: Filter has not been implemented as an actual filter. 
                //            At the moment, 0x00000000 indicates a finally handler,
                //                           0xFFFFFFFF indicates no filter block 
                //                                      (i.e. an unfiltered catch handler)
                //                           0xXXXXXXXX has undetermined behaviour!

                //For each finally block:
                int insertPos = theILBlock.PositionOf(theILBlock.At(exBlock.Offset));
                foreach (FinallyBlock finBlock in exBlock.FinallyBlocks)
                {
                    // 1. Load the pointer to the handler code:
                    theILBlock.ILOps.Insert(insertPos++, new ILOp
                    {
                        opCode = OpCodes.Ldftn,
                        Offset = exBlock.Offset,
                        MethodToCall = theMethodInfo.UnderlyingInfo,
                        LoadAtILOffset = finBlock.Offset
                    });
                    theILBlock.ILOps.Insert(insertPos++, new ILOp
                    {
                        opCode = OpCodes.Ldc_I4,
                        Offset = exBlock.Offset,
                        ValueBytes = BitConverter.GetBytes(0x00000000)
                    });
                    theILBlock.ILOps.Insert(insertPos++, new ILOp
                    {
                        opCode = OpCodes.Call,
                        Offset = exBlock.Offset,
                        MethodToCall =
                            ILLibrary.SpecialMethods[typeof(AddExceptionHandlerInfoMethodAttribute)].First()
                                .UnderlyingInfo
                    });
                }
                foreach (CatchBlock catchBlock in exBlock.CatchBlocks)
                {
                    theILBlock.ILOps.Insert(insertPos++, new ILOp
                    {
                        opCode = OpCodes.Ldftn,
                        Offset = exBlock.Offset,
                        MethodToCall = theMethodInfo.UnderlyingInfo,
                        LoadAtILOffset = catchBlock.Offset
                    });
                    theILBlock.ILOps.Insert(insertPos++, new ILOp
                    {
                        opCode = OpCodes.Ldc_I4,
                        Offset = exBlock.Offset,
                        ValueBytes = BitConverter.GetBytes(0xFFFFFFFF)
                    });
                    theILBlock.ILOps.Insert(insertPos++, new ILOp
                    {
                        opCode = OpCodes.Call,
                        Offset = exBlock.Offset,
                        MethodToCall =
                            ILLibrary.SpecialMethods[typeof(AddExceptionHandlerInfoMethodAttribute)].First()
                                .UnderlyingInfo
                    });
                }

                #endregion
            }
        }
    }
}