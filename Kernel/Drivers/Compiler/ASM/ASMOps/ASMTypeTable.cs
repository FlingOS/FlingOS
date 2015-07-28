using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    [ASMOpTarget(Target=OpCodes.TypeTable)]
    public abstract class ASMTypeTable : ASMOp
    {
        public string TypeId;
        public string SizeVal;
        public string IdVal;
        public string StackSizeVal;
        public string IsValueTypeVal;
        public string MethodTablePointer;
        public string IsPointerTypeVal;
        public string BaseTypeIdVal;
        public string FieldTablePointer;
        public string TypeSignatureLiteralLabel;
        public string TypeIdLiteralLabel;
        public List<Tuple<string, Types.TypeInfo>> FieldInformation;

        public ASMTypeTable(string typeId, string sizeVal, string idVal, string stackSizeVal, string isValueTypeVal, string methodTablePointer, string isPointerTypeVal, string baseTypeIdVal, string fieldTablePointer, string typeSignatureLiteralLabel, string typeIdLiteralLabel, List<Tuple<string, Types.TypeInfo>> fieldInformation)
        {
            TypeId = typeId;
            SizeVal = sizeVal;
            IdVal = idVal;
            StackSizeVal = stackSizeVal;
            IsValueTypeVal = isValueTypeVal;
            MethodTablePointer = methodTablePointer;
            IsPointerTypeVal = isPointerTypeVal;
            BaseTypeIdVal = baseTypeIdVal;
            FieldTablePointer = fieldTablePointer;
            TypeSignatureLiteralLabel = typeSignatureLiteralLabel;
            TypeIdLiteralLabel = typeIdLiteralLabel;
            FieldInformation = fieldInformation;
        }
    }
}
