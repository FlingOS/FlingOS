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
            Types.TypeInfo aTypeInfo = TheILLibrary.GetTypeInfo(aType);
            return aTypeInfo.GetFieldInfo(FieldName);
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
        public Types.MethodInfo GetThrowIndexOutOfRangeExceptionMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.ThrowIndexOutOfRangeExceptionMethodAttribute)].First();
        }
        public Types.MethodInfo GetDecrementRefCountMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.DecrementRefCountMethodAttribute)].First();
        }
        public Types.MethodInfo GetNewArrMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.NewArrMethodAttribute)].First();
        }
        public Types.MethodInfo GetNewObjMethodInfo()
        {
            return TheILLibrary.SpecialMethods[typeof(Attributes.NewObjMethodAttribute)].First();
        }

        public int GetTypeFieldOffset(string FieldName)
        {
            Types.TypeInfo aTypeInfo = TheILLibrary.SpecialClasses[typeof(Attributes.TypeClassAttribute)].First();
            return aTypeInfo.GetFieldInfo(FieldName).OffsetInBytes;
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
