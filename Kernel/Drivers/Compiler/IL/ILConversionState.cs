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
        public void AddExternalLabel(string label)
        {
            Result.AddExternalLabel(label);
        }
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }

        public Types.FieldInfo GetFieldInfo(Type aType, string FieldName)
        {
            Types.TypeInfo aTypeInfo = TheILLibrary.GetTypeInfo(aType);
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName);
        }
        
        public Types.TypeInfo GetArrayTypeInfo()
        {
            return ILLibrary.SpecialClasses[typeof(Attributes.ArrayClassAttribute)].First();
        }

        public Types.MethodInfo GetHaltMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.HaltMethodAttribute)].First();
        }
        public Types.MethodInfo GetThrowNullReferenceExceptionMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.ThrowNullReferenceExceptionMethodAttribute)].First();
        }
        public Types.MethodInfo GetThrowIndexOutOfRangeExceptionMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.ThrowIndexOutOfRangeExceptionMethodAttribute)].First();
        }
        public Types.MethodInfo GetDecrementRefCountMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.DecrementRefCountMethodAttribute)].First();
        }
        public Types.MethodInfo GetNewArrMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.NewArrMethodAttribute)].First();
        }
        public Types.MethodInfo GetNewObjMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.NewObjMethodAttribute)].First();
        }

        public int GetTypeFieldOffset(string FieldName)
        {
            Types.TypeInfo aTypeInfo = ILLibrary.SpecialClasses[typeof(Attributes.TypeClassAttribute)].First();
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName).OffsetInBytes;
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
