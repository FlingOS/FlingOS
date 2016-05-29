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
using Drivers.Compiler.ASM;
using Drivers.Compiler.Attributes;
using Drivers.Compiler.IL.ILOps;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     The IL Sanner manages scanning types, fields and methods to generate the final assembly code.
    /// </summary>
    public static class ILScanner
    {
        /// <summary>
        ///     Map of type IDs to the library from which they originated.
        /// </summary>
        /// <remarks>
        ///     Used to detect when types are external to the library being compiled.
        /// </remarks>
        private static readonly Dictionary<string, ILLibrary> ScannedTypes = new Dictionary<string, ILLibrary>();

        /// <summary>
        ///     The number of types scanned.
        /// </summary>
        /// <remarks>
        ///     Used as an ID generator for the types table(s).
        /// </remarks>
        private static int TypesScanned = 1;

        /// <summary>
        ///     Scans the specified library and any dependencies.
        /// </summary>
        /// <param name="TheLibrary">The library to scan.</param>
        /// <returns>
        ///     CompileResult.OK if completed successfully.
        ///     Otherwise CompileResult.PartialFail or CompileResult.Error depending on
        ///     the extent of the problem.
        /// </returns>
        public static CompileResult Scan(ILLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            if (TheLibrary.ILScanned)
            {
                return result;
            }
            TheLibrary.ILScanned = true;

            foreach (ILLibrary depLib in TheLibrary.Dependencies)
            {
                Scan(depLib);
            }

            // Create / Add Static Fields ASM Blocks
            Dictionary<string, ASMBlock> StaticFieldsBlocks = new Dictionary<string, ASMBlock>();
            ASMBlock DefaultStaticFieldsBlock = new ASMBlock
            {
                Priority = long.MinValue/2 - 9,
                PageAlign = true,
                PageAlignLabel =
                    Utilities.FilterIdentifierForInvalidChars(TheLibrary.TheAssembly.GetName().Name) + "_default"
            };
            InitialiseBssBlock(DefaultStaticFieldsBlock);
            TheLibrary.TheASMLibrary.ASMBlocks.Add(DefaultStaticFieldsBlock);
            StaticFieldsBlocks.Add("default", DefaultStaticFieldsBlock);

            // Create / Add Types Table ASM Block
            ASMBlock TypesTableBlock = new ASMBlock
            {
                Priority = long.MinValue/2 - 8
            };
            InitialiseDataBlock(TypesTableBlock);
            TheLibrary.TheASMLibrary.ASMBlocks.Add(TypesTableBlock);

            // Create / Add Method Tables ASM Block
            ASMBlock MethodTablesBlock = new ASMBlock
            {
                Priority = long.MinValue/2 + 0
            };
            InitialiseDataBlock(MethodTablesBlock);
            TheLibrary.TheASMLibrary.ASMBlocks.Add(MethodTablesBlock);

            // Create / Add Field Tables ASM Block
            ASMBlock FieldTablesBlock = new ASMBlock
            {
                Priority = long.MinValue/2 + 1
            };
            InitialiseDataBlock(FieldTablesBlock);
            TheLibrary.TheASMLibrary.ASMBlocks.Add(FieldTablesBlock);

            // Don't use foreach or you get collection modified exceptions
            for (int i = 0; i < TheLibrary.TypeInfos.Count; i++)
            {
                TypeInfo aTypeInfo = TheLibrary.TypeInfos[i];
                if (!ScannedTypes.ContainsKey(aTypeInfo.ID) && !aTypeInfo.UnderlyingType.IsInterface)
                {
                    ScannedTypes.Add(aTypeInfo.ID, TheLibrary);
                    ScanStaticFields(TheLibrary, aTypeInfo, StaticFieldsBlocks);
                    ScanType(TheLibrary, aTypeInfo, TypesTableBlock);
                    ScanMethods(TheLibrary, aTypeInfo, MethodTablesBlock);
                    ScanFields(TheLibrary, aTypeInfo, FieldTablesBlock);
                }
            }

            foreach (MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                ILBlock anILBlock = TheLibrary.ILBlocks[aMethodInfo];
                CompileResult singleResult = CompileResult.OK;

                if (anILBlock.Plugged)
                {
                    singleResult = ScanPluggedILBlock(TheLibrary, aMethodInfo, anILBlock);
                }
                else
                {
                    singleResult = ScanNonpluggedILBlock(TheLibrary, aMethodInfo, anILBlock);
                }

                if (result != CompileResult.OK)
                {
                    result = singleResult;
                }
            }

            // Create / Add String Literals ASM Block

            #region String Literals Block

            ASMBlock StringLiteralsBlock = new ASMBlock
            {
                Priority = long.MinValue/2 - 10
            };
            InitialiseDataBlock(StringLiteralsBlock);
            TheLibrary.TheASMLibrary.ASMBlocks.Add(StringLiteralsBlock);

            string StringTypeId = ILLibrary.SpecialClasses[typeof(StringClassAttribute)].First().ID;
            StringLiteralsBlock.AddExternalLabel(StringTypeId);
            foreach (KeyValuePair<string, string> aStringLiteral in TheLibrary.StringLiterals)
            {
                string value = aStringLiteral.Value;
                byte[] lengthBytes = BitConverter.GetBytes(value.Length);

                ASMOp newLiteralOp = TargetArchitecture.CreateASMOp(OpCodes.StringLiteral,
                    aStringLiteral.Key, StringTypeId, lengthBytes, value.ToCharArray());

                StringLiteralsBlock.Append(newLiteralOp);
            }

            #endregion

            return result;
        }

        private static void InitialiseTextBlock(ASMBlock TheBlock)
        {
            ASMOp newHeaderOp = TargetArchitecture.CreateASMOp(OpCodes.Header, "text");
            TheBlock.ASMOps.Insert(0, newHeaderOp);
        }

        private static void InitialiseDataBlock(ASMBlock TheBlock)
        {
            ASMOp newHeaderOp = TargetArchitecture.CreateASMOp(OpCodes.Header, "data");
            TheBlock.ASMOps.Insert(0, newHeaderOp);
        }

        private static void InitialiseBssBlock(ASMBlock TheBlock)
        {
            ASMOp newHeaderOp = TargetArchitecture.CreateASMOp(OpCodes.Header, "bss");
            TheBlock.ASMOps.Insert(0, newHeaderOp);
        }

        /// <summary>
        ///     Scans the specified type (excludes fields and methods).
        /// </summary>
        /// <param name="TheLibrary">The library currently being compiled.</param>
        /// <param name="TheTypeInfo">The type to scan.</param>
        /// <param name="TypesTableBlock">The ASM block for the types table for the library currently being compiled.</param>
        private static void ScanType(ILLibrary TheLibrary, TypeInfo TheTypeInfo, ASMBlock TypesTableBlock)
        {
            string TypeId = TheTypeInfo.ID;
            string SizeVal = TheTypeInfo.SizeOnHeapInBytes.ToString();
            string IdVal = TypesScanned++.ToString();
            string StackSizeVal = TheTypeInfo.SizeOnStackInBytes.ToString();
            string IsValueTypeVal = TheTypeInfo.IsValueType ? "1" : "0";
            string MethodTablePointer = TypeId + "_MethodTable";
            string IsPointerTypeVal = TheTypeInfo.IsPointer ? "1" : "0";
            string BaseTypeIdVal = "0";
            if (TheTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!TheTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    TypeInfo baseTypeInfo = TheLibrary.GetTypeInfo(TheTypeInfo.UnderlyingType.BaseType);
                    BaseTypeIdVal = baseTypeInfo.ID;
                    //Declared external to this library, so won't appear in this library's type tables
                    if ((ScannedTypes.ContainsKey(baseTypeInfo.ID) &&
                         ScannedTypes[baseTypeInfo.ID] != TheLibrary) ||
                        !TheLibrary.TypeInfos.Contains(baseTypeInfo))
                    {
                        TypesTableBlock.AddExternalLabel(BaseTypeIdVal);
                    }
                }
            }
            string FieldTablePointer = TypeId + "_FieldTable";
            string TypeSignatureLiteralLabel = TheLibrary.AddStringLiteral(TheTypeInfo.UnderlyingType.FullName);
            // Legacy
            string TypeIdLiteralLabel = TheLibrary.AddStringLiteral(TheTypeInfo.ID);

            TypeInfo typeTypeInfo = ILLibrary.SpecialClasses[typeof(TypeClassAttribute)].First();
            List<FieldInfo> OrderedFields =
                typeTypeInfo.FieldInfos.Where(x => !x.IsStatic).OrderBy(x => x.OffsetInBytes).ToList();
            List<Tuple<string, TypeInfo>> FieldInformation = new List<Tuple<string, TypeInfo>>();
            foreach (FieldInfo aTypeField in OrderedFields)
            {
                TypeInfo FieldTypeInfo = TheLibrary.GetTypeInfo(aTypeField.FieldType);
                FieldInformation.Add(new Tuple<string, TypeInfo>(aTypeField.Name, FieldTypeInfo));
            }

            ASMOp newTypeTableOp = TargetArchitecture.CreateASMOp(OpCodes.TypeTable,
                TypeId, SizeVal, IdVal, StackSizeVal, IsValueTypeVal, MethodTablePointer, IsPointerTypeVal,
                BaseTypeIdVal, FieldTablePointer, TypeSignatureLiteralLabel, TypeIdLiteralLabel, FieldInformation);
            TypesTableBlock.Append(newTypeTableOp);

            TypesTableBlock.AddExternalLabel(MethodTablePointer);
            TypesTableBlock.AddExternalLabel(FieldTablePointer);
            TypesTableBlock.AddExternalLabel(TypeSignatureLiteralLabel);
            TypesTableBlock.AddExternalLabel(TypeIdLiteralLabel);
        }

        /// <summary>
        ///     Scans the specified type's static fields.
        /// </summary>
        /// <param name="TheLibrary">The library currently being compiled.</param>
        /// <param name="TheTypeInfo">The type to scan the static fields of.</param>
        /// <param name="StaticFieldsBlock">The ASM block for the static fields for the library currently being compiled.</param>
        private static void ScanStaticFields(ILLibrary TheLibrary, TypeInfo TheTypeInfo,
            Dictionary<string, ASMBlock> StaticFieldsBlocks)
        {
            foreach (FieldInfo aFieldInfo in TheTypeInfo.FieldInfos)
            {
                if (aFieldInfo.IsStatic)
                {
                    TypeInfo fieldTypeInfo = TheLibrary.GetTypeInfo(aFieldInfo.FieldType);

                    string FieldID = aFieldInfo.ID;
                    string Size = fieldTypeInfo.SizeOnStackInBytes.ToString();

                    ASMOp newStaticFieldOp = TargetArchitecture.CreateASMOp(OpCodes.StaticField, FieldID, Size);
                    if (!StaticFieldsBlocks.ContainsKey(aFieldInfo.Group))
                    {
                        ASMBlock NewStaticFieldsBlock = new ASMBlock
                        {
                            Priority = long.MinValue/2 - 9,
                            PageAlign = true,
                            PageAlignLabel = aFieldInfo.Group
                        };
                        InitialiseBssBlock(NewStaticFieldsBlock);
                        TheLibrary.TheASMLibrary.ASMBlocks.Add(NewStaticFieldsBlock);

                        StaticFieldsBlocks.Add(aFieldInfo.Group, NewStaticFieldsBlock);
                    }
                    StaticFieldsBlocks[aFieldInfo.Group].Append(newStaticFieldOp);
                }
            }
        }

        /// <summary>
        ///     Scans the specified type's methods.
        /// </summary>
        /// <param name="TheLibrary">The library currently being compiled.</param>
        /// <param name="TheTypeInfo">The type to scan the methods of.</param>
        /// <param name="MethodTablesBlock">The ASM block for the methods table for the library currently being compiled.</param>
        private static void ScanMethods(ILLibrary TheLibrary, TypeInfo TheTypeInfo, ASMBlock MethodTablesBlock)
        {
            string currentTypeId = TheTypeInfo.ID;
            string currentTypeName = TheTypeInfo.UnderlyingType.FullName;

            List<Tuple<string, string>> AllMethodInfo = new List<Tuple<string, string>>();

            if (TheTypeInfo.UnderlyingType.BaseType == null ||
                TheTypeInfo.UnderlyingType.BaseType.FullName != "System.Array")
            {
                foreach (MethodInfo aMethodInfo in TheTypeInfo.MethodInfos)
                {
                    if (!aMethodInfo.IsStatic && !aMethodInfo.UnderlyingInfo.IsAbstract)
                    {
                        string methodID = aMethodInfo.ID;
                        string methodIDValue = aMethodInfo.IDValue.ToString();

                        MethodTablesBlock.AddExternalLabel(methodID);

                        AllMethodInfo.Add(new Tuple<string, string>(methodID, methodIDValue));
                    }
                }
            }

            string parentTypeMethodTablePtr = "0";
            bool parentPtrIsExternal = false;
            if (TheTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!TheTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    TypeInfo baseTypeInfo = TheLibrary.GetTypeInfo(TheTypeInfo.UnderlyingType.BaseType);
                    parentPtrIsExternal = (ScannedTypes.ContainsKey(baseTypeInfo.ID) &&
                                           ScannedTypes[baseTypeInfo.ID] != TheLibrary)
                                          || !TheLibrary.TypeInfos.Contains(baseTypeInfo);
                    parentTypeMethodTablePtr = baseTypeInfo.ID + "_MethodTable";
                }
            }
            {
                string methodID = parentTypeMethodTablePtr;
                string methodIDValue = "0";

                if (parentPtrIsExternal)
                {
                    MethodTablesBlock.AddExternalLabel(methodID);
                }

                AllMethodInfo.Add(new Tuple<string, string>(methodID, methodIDValue));
            }

            List<Tuple<string, int>> TableEntryFieldInfos = GetSpecialClassFieldInfo(TheLibrary,
                typeof(MethodInfoStructAttribute));

            ASMOp newMethodTableOp = TargetArchitecture.CreateASMOp(OpCodes.MethodTable,
                currentTypeId, currentTypeName, AllMethodInfo, TableEntryFieldInfos);
            MethodTablesBlock.Append(newMethodTableOp);
        }

        /// <summary>
        ///     Scans the specified type's non-static fields.
        /// </summary>
        /// <param name="TheLibrary">The library currently being compiled.</param>
        /// <param name="TheTypeInfo">The type to scan the non-static fields of.</param>
        /// <param name="FieldTablesBlock">The ASM block for the fields table for the library currently being compiled.</param>
        private static void ScanFields(ILLibrary TheLibrary, TypeInfo TheTypeInfo, ASMBlock FieldTablesBlock)
        {
            string currentTypeId = TheTypeInfo.ID;
            string currentTypeName = TheTypeInfo.UnderlyingType.FullName;
            List<Tuple<string, string, string>> AllFieldInfo = new List<Tuple<string, string, string>>();

            if (TheTypeInfo.UnderlyingType.BaseType == null ||
                (TheTypeInfo.UnderlyingType.BaseType.FullName != "System.Array" &&
                 TheTypeInfo.UnderlyingType.BaseType.FullName != "System.MulticastDelegate"))
            {
                TypeInfo ObjectTypeInfo =
                    TheLibrary.GetTypeInfo(
                        ILLibrary.SpecialMethods[typeof(GetObjectTypeMethodAttribute)].First()
                            .UnderlyingInfo.DeclaringType);

                foreach (FieldInfo anOwnField in TheTypeInfo.FieldInfos)
                {
                    if (!anOwnField.IsStatic)
                    {
                        TypeInfo FieldTypeInfo = TheLibrary.GetTypeInfo(anOwnField.FieldType);

                        string fieldOffsetVal = anOwnField.OffsetInBytes.ToString();
                        string fieldSizeVal =
                            (FieldTypeInfo.IsValueType
                                ? FieldTypeInfo.SizeOnHeapInBytes
                                : FieldTypeInfo.SizeOnStackInBytes).ToString();
                        string fieldTypeIdVal = FieldTypeInfo.UnderlyingType.IsInterface
                            ? ObjectTypeInfo.ID
                            : FieldTypeInfo.ID;

                        FieldTablesBlock.AddExternalLabel(fieldTypeIdVal);
                        AllFieldInfo.Add(new Tuple<string, string, string>(fieldOffsetVal, fieldSizeVal, fieldTypeIdVal));
                    }
                }
            }

            string parentTypeFieldTablePtr = "0";
            bool parentPtrIsExternal = false;
            if (TheTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!TheTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    TypeInfo baseTypeInfo = TheLibrary.GetTypeInfo(TheTypeInfo.UnderlyingType.BaseType);
                    parentPtrIsExternal = (ScannedTypes.ContainsKey(baseTypeInfo.ID) &&
                                           ScannedTypes[baseTypeInfo.ID] != TheLibrary) ||
                                          !TheLibrary.TypeInfos.Contains(baseTypeInfo);
                    parentTypeFieldTablePtr = baseTypeInfo.ID + "_FieldTable";
                }
            }
            {
                string fieldOffsetVal = "0";
                string fieldSizeVal = "0";
                string fieldTypeIdVal = parentTypeFieldTablePtr;

                if (parentPtrIsExternal)
                {
                    FieldTablesBlock.AddExternalLabel(fieldTypeIdVal);
                }

                AllFieldInfo.Add(new Tuple<string, string, string>(fieldOffsetVal, fieldSizeVal, fieldTypeIdVal));
            }

            List<Tuple<string, int>> TableEntryFieldInfos = GetSpecialClassFieldInfo(TheLibrary,
                typeof(FieldInfoStructAttribute));

            ASMOp newFieldTableOp = TargetArchitecture.CreateASMOp(OpCodes.FieldTable,
                currentTypeId, currentTypeName, AllFieldInfo, TableEntryFieldInfos);
            FieldTablesBlock.Append(newFieldTableOp);
        }

        private static List<Tuple<string, int>> GetSpecialClassFieldInfo(ILLibrary TheLibrary, Type SpecialClassType)
        {
            TypeInfo InformationAboutInfoStruct = ILLibrary.SpecialClasses[SpecialClassType].First();
            List<FieldInfo> InfoStruct_OrderedFields =
                InformationAboutInfoStruct.FieldInfos.Where(x => !x.IsStatic).OrderBy(x => x.OffsetInBytes).ToList();
            List<Tuple<string, int>> InfoStruct_OrderedFieldInfo_Subset = new List<Tuple<string, int>>();
            foreach (FieldInfo aField in InfoStruct_OrderedFields)
            {
                TypeInfo FieldTypeInfo = TheLibrary.GetTypeInfo(aField.FieldType);
                InfoStruct_OrderedFieldInfo_Subset.Add(new Tuple<string, int>(aField.Name,
                    FieldTypeInfo.IsValueType ? FieldTypeInfo.SizeOnHeapInBytes : FieldTypeInfo.SizeOnStackInBytes));
            }
            return InfoStruct_OrderedFieldInfo_Subset;
        }

        /// <summary>
        ///     Scans the specified plugged IL block.
        /// </summary>
        /// <param name="TheLibrary">The library currently being compiled.</param>
        /// <param name="theMethodInfo">The method which generated the IL block.</param>
        /// <param name="theILBlock">The IL block to scan.</param>
        /// <returns>CompileResult.OK.</returns>
        private static CompileResult ScanPluggedILBlock(ILLibrary TheLibrary, MethodInfo theMethodInfo,
            ILBlock theILBlock)
        {
            TheLibrary.TheASMLibrary.ASMBlocks.Add(new ASMBlock
            {
                PlugPath = theILBlock.PlugPath,
                OriginMethodInfo = theMethodInfo,
                Priority = theMethodInfo.Priority
            });

            return CompileResult.OK;
        }

        /// <summary>
        ///     Scans the specified non-plugged IL block.
        /// </summary>
        /// <param name="TheLibrary">The library currently being compiled.</param>
        /// <param name="theMethodInfo">The method which generated the IL block.</param>
        /// <param name="theILBlock">The IL block to scan.</param>
        /// <returns>CompileResult.OK.</returns>
        private static CompileResult ScanNonpluggedILBlock(ILLibrary TheLibrary, MethodInfo theMethodInfo,
            ILBlock theILBlock)
        {
            CompileResult result = CompileResult.OK;

            ASMBlock TheASMBlock = new ASMBlock
            {
                OriginMethodInfo = theMethodInfo,
                Priority = theMethodInfo.Priority
            };
            InitialiseTextBlock(TheASMBlock);

            ILConversionState convState = new ILConversionState
            {
                TheILLibrary = TheLibrary,
                CurrentStackFrame = new StackFrame(),
                Input = theILBlock,
                Result = TheASMBlock
            };
            foreach (ILOp anOp in theILBlock.ILOps)
            {
                try
                {
                    string commentText = TheASMBlock.GenerateILOpLabel(convState.PositionOf(anOp), "") + "  --  " +
                                         anOp.opCode + " -- Offset: " + anOp.Offset.ToString("X2");

                    ASMOp newCommentOp = TargetArchitecture.CreateASMOp(OpCodes.Comment, commentText);
                    TheASMBlock.ASMOps.Add(newCommentOp);

                    int currCount = TheASMBlock.ASMOps.Count;
                    if (anOp is MethodStart)
                    {
                        TargetArchitecture.MethodStartOp.Convert(convState, anOp);
                    }
                    else if (anOp is MethodEnd)
                    {
                        TargetArchitecture.MethodEndOp.Convert(convState, anOp);
                    }
                    else if (anOp is StackSwitch)
                    {
                        TargetArchitecture.StackSwitchOp.Convert(convState, anOp);
                    }
                    else
                    {
                        ILOp ConverterOp = TargetArchitecture.TargetILOps[(ILOp.OpCodes)anOp.opCode.Value];
                        ConverterOp.Convert(convState, anOp);
                    }

                    if (anOp.LabelRequired)
                    {
                        if (currCount < TheASMBlock.ASMOps.Count)
                        {
                            TheASMBlock.ASMOps[currCount].ILLabelPosition = convState.PositionOf(anOp);
                            TheASMBlock.ASMOps[currCount].RequiresILLabel = true;
                        }
                    }
                }
                catch (KeyNotFoundException)
                {
                    result = CompileResult.PartialFailure;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode],
                            Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value), "Conversion for IL op not found."));
                }
                catch (InvalidOperationException ex)
                {
                    result = CompileResult.PartialFailure;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode],
                            Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value), ex.Message));
                }
                catch (NotSupportedException ex)
                {
                    result = CompileResult.PartialFailure;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode],
                            Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value),
                            "An IL op reported something as not supported : " + ex.Message));
                }
                catch (Exception ex)
                {
                    result = CompileResult.Fail;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode],
                            Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value), ex.Message));
                }
            }

            TheLibrary.TheASMLibrary.ASMBlocks.Add(TheASMBlock);

            return result;
        }
    }
}