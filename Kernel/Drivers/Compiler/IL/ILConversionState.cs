using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.IL
{
    public class ILConversionState
    {
        public ILLibrary TheILLibrary;
        public ILBlock Input;
        public ASM.ASMBlock Result;

        public StackFrame CurrentStackFrame = null;

        public void Append(ASM.ASMOp anOp)
        {
            Result.Append(anOp);
        }
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }

        public Types.FieldInfo GetFieldInfo(Type aType, string FieldName)
        {
            return GetFieldInfo(aType, FieldName);
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
            throw new NullReferenceException("Field \"" + FieldName + "\" not found in type \"" + aTypeInfo.ToString() + "\".");
        }
        public int GetFieldOffset(Type aType, string FieldName)
        {
            return GetFieldInfo(aType, FieldName).OffsetInBytes;
        }
        public int GetFieldOffset(Types.TypeInfo aTypeInfo, string FieldName)
        {
            return GetFieldInfo(aTypeInfo, FieldName).OffsetInBytes;
        }

        public Types.TypeInfo GetArrayTypeInfo()
        {
            return TheILLibrary.SpecialClasses[typeof(Attributes.ArrayClassAttribute)].First();
        }

        public Types.MethodInfo GetHaltMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.HaltMethodAttribute)].First();
        }
        public Types.MethodInfo GetThrowNullReferenceExceptionMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.ThrowNullReferenceExceptionMethodAttribute)].First();
        }
        public Types.MethodInfo GetDecrementRefCountMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.DecrementRefCountMethodAttribute)].First();
        }

        public int GetTypeFieldOffset(string FieldName)
        {
            Types.TypeInfo aTypeInfo = TheILLibrary.SpecialClasses[typeof(Attributes.TypeClassAttribute)].First();
            return GetFieldOffset(aTypeInfo, FieldName);
        }
    }

    public class StackFrame
    {
        public Stack<StackItem> Stack = new Stack<StackItem>();
    }
    public class StackItem
    {
        public int sizeOnStackInBytes;
        public bool isFloat;
        public bool isGCManaged;

        public bool isNewGCObject = false;
    }
}
