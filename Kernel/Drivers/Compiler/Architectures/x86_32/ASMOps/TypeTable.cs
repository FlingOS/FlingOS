using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class TypeTable : ASM.ASMTypeTable
    {
        public TypeTable(string typeId, string sizeVal, string idVal, string stackSizeVal, string isValueTypeVal, string methodTablePointer, string isPointerTypeVal, string baseTypeIdVal, string fieldTablePointer, string typeSignatureLiteralLabel, string typeIdLiteralLabel, List<Tuple<string, Types.TypeInfo>> fieldInformation)
            : base(typeId, sizeVal, idVal, stackSizeVal, isValueTypeVal, methodTablePointer, isPointerTypeVal, baseTypeIdVal, fieldTablePointer, typeSignatureLiteralLabel, typeIdLiteralLabel, fieldInformation)
        {
        }

        public override string Convert(ASM.ASMBlock theBlock)
        {
            StringBuilder ASMResult = new StringBuilder();
            ASMResult.AppendLine("GLOBAL " + TypeId + ":data");
            ASMResult.AppendLine(TypeId + ":");

            foreach (Tuple<string, Types.TypeInfo> aFieldInfo in FieldInformation)
            {
                string allocStr = ASMUtilities.GetAllocStringForSize(
                    aFieldInfo.Item2.IsValueType ? aFieldInfo.Item2.SizeOnHeapInBytes : aFieldInfo.Item2.SizeOnStackInBytes);
                switch (aFieldInfo.Item1)
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

            return ASMResult.ToString();
        }
    }
}
