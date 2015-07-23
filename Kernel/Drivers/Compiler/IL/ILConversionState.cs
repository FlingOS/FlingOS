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
    /// <summary>
    /// Represents the current state of the IL Compiler during compilation of a single, non-plugged IL block.
    /// </summary>
    public class ILConversionState
    {
        /// <summary>
        /// The IL library being compiled.
        /// </summary>
        public ILLibrary TheILLibrary;
        /// <summary>
        /// The IL block being compiled.
        /// </summary>
        public ILBlock Input;
        /// <summary>
        /// The output ASM block being produced from the Input IL block.
        /// </summary>
        public ASM.ASMBlock Result;

        /// <summary>
        /// Tracks the stack state within the IL block.
        /// </summary>
        public StackFrame CurrentStackFrame = null;

        /// <summary>
        /// Appends the specified ASM op to the current output ASM block.
        /// </summary>
        /// <param name="anOp">The ASM op to append.</param>
        public void Append(ASM.ASMOp anOp)
        {
            Result.Append(anOp);
        }
        /// <summary>
        /// Adds the specified label as an external dependency.
        /// </summary>
        /// <param name="label">The label to add.</param>
        public void AddExternalLabel(string label)
        {
            Result.AddExternalLabel(label);
        }
        /// <summary>
        /// Gets the position (index) of the specified IL op.
        /// </summary>
        /// <param name="anOp">The op to get the position of.</param>
        /// <returns>The position.</returns>
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }

        /// <summary>
        /// Gets the field info by name for the specified field of the specified type.
        /// </summary>
        /// <param name="aType">The type to which the field belongs.</param>
        /// <param name="FieldName">The name of the field to get.</param>
        /// <returns>The field information.</returns>
        public Types.FieldInfo GetFieldInfo(Type aType, string FieldName)
        {
            Types.TypeInfo aTypeInfo = TheILLibrary.GetTypeInfo(aType);
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName);
        }
        
        /// <summary>
        /// Gets the type info for the replacement Array type.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.TypeInfo GetArrayTypeInfo()
        {
            return ILLibrary.SpecialClasses[typeof(Attributes.ArrayClassAttribute)].First();
        }

        /// <summary>
        /// Gets the method info for the Halt method.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.MethodInfo GetHaltMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.HaltMethodAttribute)].First();
        }
        /// <summary>
        /// Gets the method info for the ThrowNullReferenceException method.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.MethodInfo GetThrowNullReferenceExceptionMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.ThrowNullReferenceExceptionMethodAttribute)].First();
        }
        /// <summary>
        /// Gets the method info for the ThrowIndexOutOfRangeException method.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.MethodInfo GetThrowIndexOutOfRangeExceptionMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.ThrowIndexOutOfRangeExceptionMethodAttribute)].First();
        }
        /// <summary>
        /// Gets the method info for the DecrementRefCount method.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.MethodInfo GetDecrementRefCountMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.DecrementRefCountMethodAttribute)].First();
        }
        /// <summary>
        /// Gets the method info for the New Array method.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.MethodInfo GetNewArrMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.NewArrMethodAttribute)].First();
        }
        /// <summary>
        /// Gets the method info for the New Object method.
        /// </summary>
        /// <returns>The type info.</returns>
        public Types.MethodInfo GetNewObjMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(Attributes.NewObjMethodAttribute)].First();
        }

        /// <summary>
        /// Gets the offset of the specified field of the replacement Type class from the start of the Type class when it is held in memory.
        /// </summary>
        /// <param name="FieldName">The name of the field to get the offset of.</param>
        /// <returns>The offset in bytes.</returns>
        public int GetTypeFieldOffset(string FieldName)
        {
            Types.TypeInfo aTypeInfo = ILLibrary.SpecialClasses[typeof(Attributes.TypeClassAttribute)].First();
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName).OffsetInBytes;
        }
    }

    /// <summary>
    /// Represents a stack state.
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// The stack.
        /// </summary>
        public Stack<StackItem> Stack = new Stack<StackItem>();
    }
    /// <summary>
    /// Represents an item on a stack.
    /// </summary>
    public class StackItem
    {
        /// <summary>
        /// The size of the item on the stack in bytes.
        /// </summary>
        public int sizeOnStackInBytes;
        /// <summary>
        /// Whether the item is a floating point number or not.
        /// </summary>
        /// <remarks>
        /// The Drivers Compiler does not currently have any target architectures which support 
        /// floating point numbers. Thus tracking of whether items are floating point or not is 
        /// untested and may be innacurate.
        /// </remarks>
        public bool isFloat;
        /// <summary>
        /// Whether the item is managed by the garbage collector or not (i.e. whether it is a 
        /// reference).
        /// </summary>
        public bool isGCManaged;

        /// <summary>
        /// Whether the item is a reference to an object which has just been created by the GC.
        /// </summary>
        public bool isNewGCObject = false;
    }
}
