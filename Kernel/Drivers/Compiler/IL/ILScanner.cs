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
    public static class ILScanner
    {
        private static System.Reflection.Assembly TargetArchitectureAssembly = null;
        public static Dictionary<ILOp.OpCodes, ILOp> TargetILOps = new Dictionary<ILOp.OpCodes, ILOp>();
        public static ILOps.MethodStart MethodStartOp;
        public static ILOps.MethodEnd MethodEndOp;
        public static ILOps.StackSwitch StackSwitchOp;

        public static bool Init()
        {
            bool OK = true;

            OK = LoadTargetArchiecture();

            return OK;
        }
        private static bool LoadTargetArchiecture()
        {
            bool OK = false;

            try
            {
                switch (Options.TargetArchitecture)
                {
                    case "x86":
                        {
                            string dir = System.IO.Path.GetDirectoryName(typeof(ILCompiler).Assembly.Location);
                            string fileName = System.IO.Path.Combine(dir, @"Drivers.Compiler.Architectures.x86.dll");
                            fileName = System.IO.Path.GetFullPath(fileName);
                            TargetArchitectureAssembly = System.Reflection.Assembly.LoadFrom(fileName);
                            OK = true;
                        }
                        break;
                    default:
                        OK = false;
                        throw new ArgumentException("Unrecognised target architecture!");
                }

                if (OK)
                {
                    Type[] AllTypes = TargetArchitectureAssembly.GetTypes();
                    foreach (Type aType in AllTypes)
                    {
                        if (aType.IsSubclassOf(typeof(ILOp)))
                        {
                            if (aType.IsSubclassOf(typeof(ILOps.MethodStart)))
                            {
                                MethodStartOp = (ILOps.MethodStart)aType.GetConstructor(new Type[0]).Invoke(new object[0]);
                            }
                            else if (aType.IsSubclassOf(typeof(ILOps.MethodEnd)))
                            {
                                MethodEndOp = (ILOps.MethodEnd)aType.GetConstructor(new Type[0]).Invoke(new object[0]);
                            }
                            else if (aType.IsSubclassOf(typeof(ILOps.StackSwitch)))
                            {
                                StackSwitchOp = (ILOps.StackSwitch)aType.GetConstructor(new Type[0]).Invoke(new object[0]);
                            }
                            else
                            {
                                ILOps.ILOpTargetAttribute[] targetAttrs = (ILOps.ILOpTargetAttribute[])aType.GetCustomAttributes(typeof(ILOps.ILOpTargetAttribute), true);
                                if (targetAttrs == null || targetAttrs.Length == 0)
                                {
                                    throw new Exception("ILScanner could not load target architecture ILOp because target attribute was not specified!");
                                }
                                else
                                {
                                    foreach (ILOps.ILOpTargetAttribute targetAttr in targetAttrs)
                                    {
                                        TargetILOps.Add(targetAttr.Target, (ILOp)aType.GetConstructor(new Type[0]).Invoke(new object[0]));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OK = false;
                Logger.LogError(Errors.ILCompiler_LoadTargetArchError_ErrorCode, "", 0, 
                    string.Format(Errors.ErrorMessages[Errors.ILCompiler_LoadTargetArchError_ErrorCode],
                                    ex.Message));
            }

            return OK;
        }

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
            
            // Create / Add Static Fields ASM Block
            ASM.ASMBlock StaticFieldsBlock = new ASM.ASMBlock()
            {
                Priority = (long.MaxValue / 2) - 1
            };
            TheLibrary.TheASMLibrary.ASMBlocks.Add(StaticFieldsBlock);

            // Create / Add Types Table ASM Block
            ASM.ASMBlock TypesTableBlock = new ASM.ASMBlock()
            {
                Priority = (long.MaxValue / 2) - 1
            };
            TheLibrary.TheASMLibrary.ASMBlocks.Add(TypesTableBlock);

            // Create / Add Method Tables ASM Block
            ASM.ASMBlock MethodTablesBlock = new ASM.ASMBlock()
            {
                Priority = (long.MaxValue / 2) + 0
            };
            TheLibrary.TheASMLibrary.ASMBlocks.Add(MethodTablesBlock);

            // Create / Add Field Tables ASM Block
            ASM.ASMBlock FieldTablesBlock = new ASM.ASMBlock()
            {
                Priority = (long.MaxValue / 2) + 1
            };
            TheLibrary.TheASMLibrary.ASMBlocks.Add(FieldTablesBlock);

            // Don't use foreach or you get collection modified exceptions
            for (int i = 0; i < TheLibrary.TypeInfos.Count; i++)
            {
                Types.TypeInfo aTypeInfo = TheLibrary.TypeInfos[i];
                ScanStaticFields(TheLibrary, aTypeInfo, StaticFieldsBlock);
                ScanType(TheLibrary, aTypeInfo, TypesTableBlock);
                ScanMethods(TheLibrary, aTypeInfo, MethodTablesBlock);
                ScanFields(TheLibrary, aTypeInfo, FieldTablesBlock);
            }

            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
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

                if (result == CompileResult.OK)
                {
                    result = singleResult;
                }
            }

            // Create / Add String Literals ASM Block
            #region String Literals Block

            ASM.ASMBlock StringLiteralsBlock = new ASM.ASMBlock()
            {
                Priority = (long.MaxValue / 2) - 2
            };
            TheLibrary.TheASMLibrary.ASMBlocks.Add(StringLiteralsBlock);

            string StringTypeId = ILLibrary.SpecialClasses[typeof(Attributes.StringClassAttribute)].First().ID;
            StringLiteralsBlock.AddExternalLabel(StringTypeId);
            foreach (KeyValuePair<string, string> aStringLiteral in TheLibrary.StringLiterals)
            {
                string value = aStringLiteral.Value;
                Encoding xEncoding = Encoding.ASCII;
                var NumBytes = xEncoding.GetByteCount(value);
                var stringData = new byte[4 + NumBytes];
                Array.Copy(BitConverter.GetBytes(value.Length), 0, stringData, 0, 4);
                Array.Copy(xEncoding.GetBytes(value), 0, stringData, 4, NumBytes);

                StringBuilder LiteralASM = new StringBuilder();
                //This is UTF-16 (Unicode)/ASCII text
                LiteralASM.AppendLine(string.Format("GLOBAL {0}:data", aStringLiteral.Key));
                LiteralASM.AppendLine(string.Format("{0}:", aStringLiteral.Key));
                //Put in type info as FOS_System.String type
                LiteralASM.AppendLine(string.Format("dd {0}", StringTypeId));
                //Put in string length bytes
                LiteralASM.Append("db ");
                for (int i = 0; i < 3; i++)
                {
                    LiteralASM.Append(stringData[i]);
                    LiteralASM.Append(", ");
                }
                LiteralASM.Append(stringData[3]);
                //Put in string characters (as words)
                LiteralASM.Append("\ndw ");
                for (int i = 4; i < (stringData.Length - 1); i++)
                {
                    LiteralASM.Append(stringData[i]);
                    LiteralASM.Append(", ");
                }
                LiteralASM.Append(stringData.Last());
                LiteralASM.AppendLine();
                StringLiteralsBlock.Append(new ASM.ASMGeneric()
                {
                    Text = LiteralASM.ToString()
                });
            }

            #endregion

            return result;
        }

        private static int TypesScanned = 1;
        private static void ScanType(ILLibrary TheLibrary, Types.TypeInfo TheTypeInfo, ASM.ASMBlock TypesTableBlock)
        {
            string TypeId = TheTypeInfo.ID;
            string SizeVal = TheTypeInfo.SizeOnHeapInBytes.ToString();
            string IdVal = (TypesScanned++).ToString();
            string StackSizeVal = TheTypeInfo.SizeOnStackInBytes.ToString();
            string IsValueTypeVal = (TheTypeInfo.IsValueType ? "1" : "0");
            string MethodTablePointer = TypeId + "_MethodTable";
            string IsPointerTypeVal = (TheTypeInfo.IsPointer ? "1" : "0");
            string BaseTypeIdVal = "0";
            if (TheTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!TheTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    Types.TypeInfo baseTypeInfo = TheLibrary.GetTypeInfo(TheTypeInfo.UnderlyingType.BaseType);
                    BaseTypeIdVal = baseTypeInfo.ID;
                    //Declared external to this library, so won't appear in this library's type tables
                    if (!TheLibrary.TypeInfos.Contains(baseTypeInfo))
                    {
                        TypesTableBlock.AddExternalLabel(BaseTypeIdVal);
                    }
                }
            }
            string FieldTablePointer = TypeId + "_FieldTable";
            string TypeSignatureLiteralLabel = TheLibrary.AddStringLiteral(TheTypeInfo.ID); // Legacy
            string TypeIdLiteralLabel = TheLibrary.AddStringLiteral(TheTypeInfo.ID);

            StringBuilder ASMResult = new StringBuilder();
            ASMResult.AppendLine(TypeId + ":");

            Types.TypeInfo typeTypeInfo = ILLibrary.SpecialClasses[typeof(Attributes.TypeClassAttribute)].First();
            List<Types.FieldInfo> OrderedFields = typeTypeInfo.FieldInfos.OrderBy(x => x.OffsetInBytes).ToList();
            foreach (Types.FieldInfo aTypeField in OrderedFields)
            {
                Types.TypeInfo FieldTypeInfo = TheLibrary.GetTypeInfo(aTypeField.FieldType);
                string allocStr = GetAllocStringForSize(
                    FieldTypeInfo.IsValueType ? FieldTypeInfo.SizeOnHeapInBytes : FieldTypeInfo.SizeOnStackInBytes);
                switch (aTypeField.Name)
                {
                    case "Size":
                        ASMResult.AppendLine(allocStr + " " + SizeVal);
                        break;
                    case "Id":
                        ASMResult.AppendLine(allocStr + " " + IdVal);
                        break;
                    case "StackSize":
                        ASMResult.AppendLine(allocStr + " " + StackSizeVal);
                        break;
                    case "IsValueType":
                        ASMResult.AppendLine(allocStr + " " + IsValueTypeVal);
                        break;
                    case "MethodTablePtr":
                        ASMResult.AppendLine(allocStr + " " + MethodTablePointer);
                        break;
                    case "IsPointer":
                        ASMResult.AppendLine(allocStr + " " + IsPointerTypeVal);
                        break;
                    case "TheBaseType":
                        ASMResult.AppendLine(allocStr + " " + BaseTypeIdVal);
                        break;
                    case "FieldTablePtr":
                        ASMResult.AppendLine(allocStr + " " + FieldTablePointer);
                        break;
                    case "Signature":
                        ASMResult.AppendLine(allocStr + " " + TypeSignatureLiteralLabel);
                        break;
                    case "IdString":
                        ASMResult.AppendLine(allocStr + " " + TypeIdLiteralLabel);
                        break;
                }
            }
            ASMResult.AppendLine();

            TypesTableBlock.Append(new ASM.ASMGeneric()
            {
                Text = ASMResult.ToString()
            });
            TypesTableBlock.AddExternalLabel(MethodTablePointer);
            TypesTableBlock.AddExternalLabel(FieldTablePointer);
            TypesTableBlock.AddExternalLabel(TypeSignatureLiteralLabel);
            TypesTableBlock.AddExternalLabel(TypeIdLiteralLabel);
        }
        private static void ScanStaticFields(ILLibrary TheLibrary, Types.TypeInfo TheTypeInfo, ASM.ASMBlock StaticFieldsBlock)
        {
        }
        private static void ScanMethods(ILLibrary TheLibrary, Types.TypeInfo TheTypeInfo, ASM.ASMBlock MethodTablesBlock)
        {
        }
        private static void ScanMethod(ILLibrary TheLibrary, Types.TypeInfo DeclarerTypeInfo, Types.MethodInfo TheMethodInfo, ASM.ASMBlock MethodTablesBlock)
        {
        }
        private static void ScanFields(ILLibrary TheLibrary, Types.TypeInfo TheTypeInfo, ASM.ASMBlock FieldTablesBlock)
        {
        }
        private static void ScanField(ILLibrary TheLibrary, Types.TypeInfo DeclarerTypeInfo, Types.MethodInfo TheMethodInfo, ASM.ASMBlock FieldTablesBlock)
        {
        }

        private static string GetAllocStringForSize(int numBytes)
        {
            switch (numBytes)
            {
                case 1:
                    return "db";
                    break;
                case 2:
                    return "dw";
                    break;
                case 4:
                    return "dd";
                    break;
                default:
                    return "NOSIZEALLOC";
                    break;
            }
        }

        private static CompileResult ScanPluggedILBlock(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            TheLibrary.TheASMLibrary.ASMBlocks.Add(new ASM.ASMBlock()
            {
                PlugPath = theILBlock.PlugPath,
                OriginMethodInfo = theMethodInfo
            });

            return CompileResult.OK;
        }
        private static CompileResult ScanNonpluggedILBlock(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            CompileResult result = CompileResult.OK;

            ASM.ASMBlock TheASMBlock = new ASM.ASMBlock()
            {
                OriginMethodInfo = theMethodInfo
            };
            
            ILConversionState convState = new ILConversionState()
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
                    TheASMBlock.ASMOps.Add(new ASM.ASMComment() { Text = TheASMBlock.GenerateILOpLabel(convState.PositionOf(anOp), "") + "  --  " + anOp.opCode.ToString() });

                    int currCount = TheASMBlock.ASMOps.Count;
                    if (anOp is ILOps.MethodStart || anOp is ILOps.MethodEnd)
                    {
                        anOp.Convert(convState, anOp);
                    }
                    else
                    {
                        ILOp ConverterOp = TargetILOps[(ILOp.OpCodes)anOp.opCode.Value];
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
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode], "Conversion IL op not found: " + Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value) + "."));
                }
                catch (InvalidOperationException)
                {
                    result = CompileResult.PartialFailure;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode], Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value)));
                }
                catch (NotSupportedException ex)
                {
                    result = CompileResult.PartialFailure;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode], "An IL op reported something as not supported. " + Enum.GetName(typeof(ILOp.OpCodes), anOp.opCode.Value) + ". " + ex.Message));
                }
                catch (Exception ex)
                {
                    result = CompileResult.Fail;

                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode], ex.Message));
                }
            }

            TheLibrary.TheASMLibrary.ASMBlocks.Add(TheASMBlock);

            return result;
        }
    }
}
