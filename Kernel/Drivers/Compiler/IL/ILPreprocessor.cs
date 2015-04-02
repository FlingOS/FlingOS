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

namespace Drivers.Compiler.IL
{
    public static class ILPreprocessor
    {
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

            foreach (IL.ILLibrary aDependency in TheLibrary.Dependencies)
            {
                Preprocess(aDependency);
            }
            
            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                PreprocessMethodInfo(TheLibrary, aMethodInfo);
            }

            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                InjectGeneral(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                InjectGC(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                InjectTryCatchFinally(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                
                PreprocessILOps(TheLibrary, aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
            }
        }

        public static void PreprocessSpecialClasses(ILLibrary RootLibrary)
        {
            //Is there anything to do here?
        }
        public static void PreprocessSpecialMethods(ILLibrary RootLibrary)
        {
            // Setup calls to Static Constructors
            Types.MethodInfo CallStaticConstructorsInfo = ILLibrary.SpecialMethods[typeof(Attributes.CallStaticConstructorsMethodAttribute)].First();
            ILBlock CallStaticConstructorsBlock = RootLibrary.GetILBlock(CallStaticConstructorsInfo);
            List<System.Reflection.ConstructorInfo> staticConstructorsToCall = ILLibrary.TheStaticConstructorDependencyTree.Flatten();
            foreach (System.Reflection.ConstructorInfo anInfo in staticConstructorsToCall)
            {
                CallStaticConstructorsBlock.ILOps.Insert(CallStaticConstructorsBlock.ILOps.Count - 1,
                    new ILOp()
                    {
                        opCode = System.Reflection.Emit.OpCodes.Call,
                        ValueBytes = null,
                        MethodToCall = anInfo
                    }
                );
            }
        }

        private static void PreprocessMethodInfo(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo)
        {
            Types.TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(theMethodInfo.UnderlyingInfo.DeclaringType);
            int ID = GetMethodIDGenerator(TheLibrary, aTypeInfo);
            theMethodInfo.IDValue = ID + 1;
            aTypeInfo.MethodIDGenerator++;
        }
        private static int GetMethodIDGenerator(ILLibrary TheLibrary, Type aType)
        {
            Types.TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(aType);
            return GetMethodIDGenerator(TheLibrary, aTypeInfo);
        }
        private static int GetMethodIDGenerator(ILLibrary TheLibrary, Types.TypeInfo aTypeInfo)
        {
            int totalGen = 0;
            if (aTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!aTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    totalGen += GetMethodIDGenerator(TheLibrary, aTypeInfo.UnderlyingType.BaseType);
                }
            }
            return totalGen;
        }
        private static void PreprocessILOps(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            int totalLocalsOffset = 0;
            foreach (Types.VariableInfo aVarInfo in theMethodInfo.LocalInfos)
            {
                //Causes processing of the type - in case it hasn't already been processed
                Types.TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(aVarInfo.UnderlyingType);
                aVarInfo.TheTypeInfo = aTypeInfo;
                aVarInfo.Offset = totalLocalsOffset;
                totalLocalsOffset += aTypeInfo.SizeOnStackInBytes;
            }

            int totalArgsSize = 0;
            if (!theMethodInfo.IsStatic)
            {
                Types.VariableInfo newVarInfo = new Types.VariableInfo()
                {
                    UnderlyingType = theMethodInfo.UnderlyingInfo.DeclaringType,
                    Position = 0,
                    TheTypeInfo = TheLibrary.GetTypeInfo(theMethodInfo.UnderlyingInfo.DeclaringType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);

                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }
            System.Reflection.ParameterInfo[] args = theMethodInfo.UnderlyingInfo.GetParameters();
            foreach (System.Reflection.ParameterInfo argItem in args)
            {
                Types.VariableInfo newVarInfo = new Types.VariableInfo()
                {
                    UnderlyingType = argItem.ParameterType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(argItem.ParameterType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);
                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }

            System.Reflection.ParameterInfo returnArgItem = (theMethodInfo.IsConstructor ? null : ((System.Reflection.MethodInfo)theMethodInfo.UnderlyingInfo).ReturnParameter);
            if (returnArgItem != null)
            {
                Types.VariableInfo newVarInfo = new Types.VariableInfo()
                {
                    UnderlyingType = returnArgItem.ParameterType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(returnArgItem.ParameterType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);
                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }

            int offset = totalArgsSize;
            for (int i = 0; i < theMethodInfo.ArgumentInfos.Count; i++)
            {
                offset -= theMethodInfo.ArgumentInfos[i].TheTypeInfo.SizeOnStackInBytes;
                theMethodInfo.ArgumentInfos[i].Offset = offset;
            }

            StaticConstructorDependency staticConstructorDependencyRoot = null;
            if (theMethodInfo.UnderlyingInfo is System.Reflection.ConstructorInfo &&
                        theMethodInfo.IsStatic)
            {
                System.Reflection.ConstructorInfo aConstructor = (System.Reflection.ConstructorInfo)theMethodInfo.UnderlyingInfo;
                staticConstructorDependencyRoot = ILLibrary.TheStaticConstructorDependencyTree[aConstructor];
                if (staticConstructorDependencyRoot == null)
                {
                    staticConstructorDependencyRoot = new StaticConstructorDependency()
                    {
                        TheConstructor = aConstructor
                    };
                    ILLibrary.TheStaticConstructorDependencyTree.Children.Add(staticConstructorDependencyRoot);
                }
            }

            ILPreprocessState preprosState = new ILPreprocessState()
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

                        // TODO - IL level comments
                        // result.ASM.AppendLine("; Call to method defined in MSCorLib ignored:"); // DEBUG INFO
                        // result.ASM.AppendLine("; " + anILOpInfo.MethodToCall.DeclaringType.FullName + "." + anILOpInfo.MethodToCall.Name); // DEBUG INFO

                        //If the method is a call to a constructor in MsCorLib:
                        if (theOp.MethodToCall is System.Reflection.ConstructorInfo)
                        {
                            //Then we can presume it was a call to a base-class constructor (e.g. the Object constructor)
                            //  and so we just need to remove any args that were loaded onto the stack.
                            // TODO: result.ASM.AppendLine("; Method to call was constructor so removing params"); // DEBUG INFO
                            
                            //Remove args from stack
                            //If the constructor was non-static, then the first arg is the instance reference.
                            if (!theOp.MethodToCall.IsStatic)
                            {
                                i++;
                                theILBlock.ILOps.Insert(i, new ILOp()
                                {
                                    opCode = System.Reflection.Emit.OpCodes.Pop
                                });
                            }
                            foreach (System.Reflection.ParameterInfo anInfo in theOp.MethodToCall.GetParameters())
                            {
                                i++;
                                theILBlock.ILOps.Insert(i, new ILOp()
                                {
                                    opCode = System.Reflection.Emit.OpCodes.Pop
                                });
                            }
                        }
                    }
                }

                try
                {
                    ILOp ConverterOp = ILScanner.TargetILOps[(ILOp.OpCodes)theOp.opCode.Value];

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
                                {
                                    int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                                    System.Reflection.MethodBase methodBaseInf = theMethodInfo.UnderlyingInfo.Module.ResolveMethod(metadataToken);
                                    if (!(methodBaseInf.IsConstructor || methodBaseInf is System.Reflection.ConstructorInfo))
                                    {
                                        System.Reflection.MethodInfo methodInf = (System.Reflection.MethodInfo)methodBaseInf;
                                        System.Reflection.ConstructorInfo[] staticConstructors = 
                                            methodInf.DeclaringType.GetConstructors(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                                .Concat(methodInf.DeclaringType.GetConstructors(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                                                .ToArray();
                                        if (staticConstructors.Length > 0)
                                        {
                                            System.Reflection.ConstructorInfo TheConstructor = staticConstructors[0];
                                            if (staticConstructorDependencyRoot[TheConstructor] == null)
                                            {
                                                staticConstructorDependencyRoot.Children.Add(new StaticConstructorDependency()
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
                                    System.Reflection.FieldInfo fieldInf = theMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
                                    System.Reflection.ConstructorInfo[] staticConstructors = fieldInf.DeclaringType.GetConstructors(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                                                   .Concat(fieldInf.DeclaringType.GetConstructors(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                                                                   .ToArray();
                                    if (staticConstructors.Length > 0)
                                    {
                                        System.Reflection.ConstructorInfo TheConstructor = staticConstructors[0];
                                        if (staticConstructorDependencyRoot[TheConstructor] == null)
                                        {
                                            staticConstructorDependencyRoot.Children.Add(new StaticConstructorDependency()
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
                        "Il Preprocessor error: " + ex.Message);
                }
            }
        }
        private static void InjectGeneral(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            // Inject MethodStart op
            theILBlock.ILOps.Insert(0, ILScanner.MethodStartOp);
            // Inject MethodEnd op just before anywhere where there is a ret
            for (int i = 0; i < theILBlock.ILOps.Count; i++)
            {
                ILOp theOp = theILBlock.ILOps[i];

                if ((ILOp.OpCodes)theOp.opCode.Value == ILOp.OpCodes.Ret)
                {
                    theILBlock.ILOps.Insert(i, ILScanner.MethodEndOp);
                    i++;
                }
            }
        }
        private static void InjectGC(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            //TODO
        }
        private static void InjectTryCatchFinally(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
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
                    if ((int)theOp.opCode.Value == (int)ILOp.OpCodes.Leave)
                    {
                        ILOffset = BitConverter.ToInt32(theOp.ValueBytes, 0);
                    }
                    else
                    {
                        ILOffset = (int)theOp.ValueBytes[0];
                    }

                    theILBlock.ILOps.Insert(i, new ILOp()
                    {
                        opCode = System.Reflection.Emit.OpCodes.Ldftn,
                        LoadAtILOffset = theOp.NextOffset + ILOffset,
                        MethodToCall = theILBlock.TheMethodInfo.UnderlyingInfo
                    });
                    theILBlock.ILOps.Insert(i + 1, new ILOp()
                    {
                        opCode = System.Reflection.Emit.OpCodes.Call,
                        MethodToCall = ILLibrary.SpecialMethods[typeof(Attributes.ExceptionsHandleLeaveMethodAttribute)].First().UnderlyingInfo
                    });

                    i++;
                }
                else if ((int)theOp.opCode.Value == (int)ILOp.OpCodes.Endfinally)
                {
                    //Endfinally is for leaving a (critical) finally section
                    //We handle it by a higher-level implementation rather than 
                    //  leaving it to each architecture to implement.

                    //Endfinally is handled by inserting a call to the Exceptions.HandleEndFinally method

                    theILBlock.ILOps.RemoveAt(i);
                    theILBlock.ILOps.Insert(i, new ILOp()
                    {
                        opCode = System.Reflection.Emit.OpCodes.Call,
                        MethodToCall = ILLibrary.SpecialMethods[typeof(Attributes.ExceptionsHandleEndFinallyMethodAttribute)].First().UnderlyingInfo
                    });
                }
            }


            foreach (ExceptionHandledBlock exBlock in theILBlock.ExceptionHandledBlocks)
            {
                #region Try-sections
                //      Insert the start of try-block

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
                #endregion

                #region Catch-sections

                // Strip the first pop of catch-handler

                foreach (CatchBlock aCatchBlock in exBlock.CatchBlocks)
                {
                    ILOp catchPopOp = theILBlock.At(aCatchBlock.Offset);
                    theILBlock.ILOps.Remove(catchPopOp);
                }

                #endregion
            }
        }
    }
}
