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
using Drivers.Compiler.Types;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     Represents the current state of the IL Scanner during compilation of a single, non-plugged IL block.
    /// </summary>
    public class ILConversionState
    {
        /// <summary>
        ///     Tracks the stack state within the IL block.
        /// </summary>
        public StackFrame CurrentStackFrame = null;

        /// <summary>
        ///     The IL block being compiled.
        /// </summary>
        public ILBlock Input;

        /// <summary>
        ///     The output ASM block being produced from the Input IL block.
        /// </summary>
        public ASMBlock Result;

        /// <summary>
        ///     The IL library being compiled.
        /// </summary>
        public ILLibrary TheILLibrary;

        /// <summary>
        ///     Appends the specified ASM op to the current output ASM block.
        /// </summary>
        /// <param name="anOp">The ASM op to append.</param>
        public void Append(ASMOp anOp)
        {
            Result.Append(anOp);
        }

        /// <summary>
        ///     Adds the specified label as an external dependency.
        /// </summary>
        /// <param name="label">The label to add.</param>
        public void AddExternalLabel(string label)
        {
            Result.AddExternalLabel(label);
        }

        /// <summary>
        ///     Gets the position (index) of the specified IL op.
        /// </summary>
        /// <param name="anOp">The op to get the position of.</param>
        /// <returns>The position.</returns>
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }

        /// <summary>
        ///     Gets the field info by name for the specified field of the specified type.
        /// </summary>
        /// <param name="aType">The type to which the field belongs.</param>
        /// <param name="FieldName">The name of the field to get.</param>
        /// <returns>The field information.</returns>
        public FieldInfo GetFieldInfo(Type aType, string FieldName)
        {
            TypeInfo aTypeInfo = TheILLibrary.GetTypeInfo(aType);
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName);
        }

        /// <summary>
        ///     Gets the type info for the replacement Array type.
        /// </summary>
        /// <returns>The type info.</returns>
        public TypeInfo GetArrayTypeInfo()
        {
            return ILLibrary.SpecialClasses[typeof(ArrayClassAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the Halt method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetHaltMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(HaltMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the ThrowNullReferenceException method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetThrowNullReferenceExceptionMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(ThrowNullReferenceExceptionMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the ThrowIndexOutOfRangeException method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetThrowIndexOutOfRangeExceptionMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(ThrowIndexOutOfRangeExceptionMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the DecrementRefCount method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetDecrementRefCountMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(DecrementRefCountMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the New Array method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetNewArrMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(NewArrMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the New Object method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetNewObjMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(NewObjMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the method info for the GetObjectType method.
        /// </summary>
        /// <returns>The type info.</returns>
        public MethodInfo GetObjectTypeMethodInfo()
        {
            return ILLibrary.SpecialMethods[typeof(GetObjectTypeMethodAttribute)].First();
        }

        /// <summary>
        ///     Gets the offset of the specified field of the replacement Type class from the start of the Type class when it is
        ///     held in memory.
        /// </summary>
        /// <param name="FieldName">The name of the field to get the offset of.</param>
        /// <returns>The offset in bytes.</returns>
        public int GetTypeFieldOffset(string FieldName)
        {
            TypeInfo aTypeInfo = ILLibrary.SpecialClasses[typeof(TypeClassAttribute)].First();
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName).OffsetInBytes;
        }
    }

    /// <summary>
    ///     Represents a stack state.
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        ///     The stack.
        /// </summary>
        private readonly Dictionary<int, Stack<StackItem>> Stacks = new Dictionary<int, Stack<StackItem>>();

        public StackFrame()
        {
            Stacks.Add(-1, new Stack<StackItem>());
        }

        public Stack<StackItem> GetStack(ILOp op)
        {
            return GetStack(op.Offset);
        }

        public Stack<StackItem> GetStack(int ILOffset)
        {
            int NearestKey = Stacks.Keys.Where(x => x <= ILOffset).OrderBy(x => x).Last();
            return Stacks[NearestKey];
        }

        public void ForkStack(ILOp CurrentOp, ILOp DestinationOp)
        {
            // Multiple branches to the same location, in which case, we just have to hope the stacks match up! :)
            //  TODO: Find a way to verify the stack match properly.
            if (Stacks.ContainsKey(DestinationOp.Offset)) return;

            Stack<StackItem> CurrentStack = GetStack(CurrentOp.Offset);
            Stack<StackItem> NewStack = new Stack<StackItem>(CurrentStack.Reverse());
            Stacks.Add(DestinationOp.Offset, NewStack);
        }
    }

    /// <summary>
    ///     Represents an item on a stack.
    /// </summary>
    public class StackItem
    {
        /// <summary>
        ///     Whether the item is a floating point number or not.
        /// </summary>
        /// <remarks>
        ///     The Drivers Compiler does not currently have any target architectures which support
        ///     floating point numbers. Thus tracking of whether items are floating point or not is
        ///     untested and may be innacurate.
        /// </remarks>
        public bool isFloat;

        /// <summary>
        ///     Whether the item is managed by the garbage collector or not (i.e. whether it is a
        ///     reference).
        /// </summary>
        public bool isGCManaged;

        /// <summary>
        ///     Whether the item is a reference to an object which has just been created by the GC.
        /// </summary>
        public bool isNewGCObject = false;

        /// <summary>
        ///     Whether the item is of a value type or not.
        /// </summary>
        public bool isValue = false;

        /// <summary>
        ///     The size of the item on the stack in bytes.
        /// </summary>
        public int sizeOnStackInBytes;
    }
}