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
using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Br : IL.ILOps.Br
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            JmpOp jumpOp = JmpOp.Jump;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Br:
                case OpCodes.Br_S:
                    break;
                case OpCodes.Brtrue:
                    jumpOp = JmpOp.JumpNotZero;
                    break;
                case OpCodes.Brtrue_S:
                    jumpOp = JmpOp.JumpNotZero;
                    break;
                case OpCodes.Brfalse:
                    jumpOp = JmpOp.JumpZero;
                    break;
                case OpCodes.Brfalse_S:
                    jumpOp = JmpOp.JumpZero;
                    break;
                case OpCodes.Beq:
                    jumpOp = JmpOp.JumpEqual;
                    break;
                case OpCodes.Beq_S:
                    jumpOp = JmpOp.JumpEqual;
                    break;
                case OpCodes.Bne_Un:
                    jumpOp = JmpOp.JumpNotEqual;
                    break;
                case OpCodes.Bne_Un_S:
                    jumpOp = JmpOp.JumpNotEqual;
                    break;
                case OpCodes.Bge:
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    break;
                case OpCodes.Bge_S:
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    break;
                case OpCodes.Bge_Un:
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    break;
                case OpCodes.Bge_Un_S:
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    break;
                case OpCodes.Ble:
                    jumpOp = JmpOp.JumpLessThanEqual;
                    break;
                case OpCodes.Ble_S:
                    jumpOp = JmpOp.JumpLessThanEqual;
                    break;
                case OpCodes.Ble_Un:
                    jumpOp = JmpOp.JumpLessThanEqual;
                    break;
                case OpCodes.Ble_Un_S:
                    jumpOp = JmpOp.JumpLessThanEqual;
                    break;
                case OpCodes.Blt:
                    jumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Blt_S:
                    jumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Blt_Un:
                    jumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Blt_Un_S:
                    jumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Bgt:
                    jumpOp = JmpOp.JumpGreaterThan;
                    break;
                case OpCodes.Bgt_S:
                    jumpOp = JmpOp.JumpGreaterThan;
                    break;
                case OpCodes.Bgt_Un:
                    jumpOp = JmpOp.JumpGreaterThan;
                    break;
                case OpCodes.Bgt_Un_S:
                    jumpOp = JmpOp.JumpGreaterThan;
                    break;
            }

            if (jumpOp == JmpOp.JumpZero || jumpOp == JmpOp.JumpNotZero)
            {
                //Pop from our stack the test item to use in the condition
                StackItem testItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            }
            else if (jumpOp == JmpOp.JumpEqual || jumpOp == JmpOp.JumpNotEqual ||
                     jumpOp == JmpOp.JumpGreaterThanEqual ||
                     jumpOp == JmpOp.JumpLessThanEqual ||
                     jumpOp == JmpOp.JumpLessThan ||
                     jumpOp == JmpOp.JumpGreaterThan)
            {
                //Pop from our stack the test items to use in the condition
                StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            }

            ILOp targetOp = GetTargetILOp(conversionState, theOp);
            if (targetOp != null)
            {
                conversionState.CurrentStackFrame.ForkStack(theOp, targetOp);
            }
        }

        public override void Preprocess(ILPreprocessState preprocessState, ILOp theOp)
        {
            //Mark it as requiring a label
            ILOp targetOp = GetTargetILOp(preprocessState, theOp);
            if (targetOp != null)
            {
                targetOp.LabelRequired = true;
            }
        }

        private static ILOp GetTargetILOp(ILPreprocessState preprocessState, ILOp theOp)
        {
            int ILOffset = GetTargetILOffset(theOp);

            if (ILOffset != 0)
            {
                //Get the IL number of the next op
                int startILNum = theOp.NextOffset;
                //Add the offset to get the IL op num to jump to
                int ILNumToGoTo = startILNum + ILOffset;

                //Find the IL op to jump to 
                return preprocessState.Input.At(ILNumToGoTo);
            }
            return null;
        }

        private static int GetTargetILOffset(ILOp theOp)
        {
            //This will store the offset from the current next op's position
            //to the IL op to jump to.
            int ILOffset = 0;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Br:
                    //Load the IL offset as signed Int 32 from the value bytes.
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Br_S:
                    //Load the IL offset as signed Int 8 from the value bytes.
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Brtrue:
                    //See above.
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Brtrue_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Brfalse:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Brfalse_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;

                case OpCodes.Beq:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Beq_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;

                case OpCodes.Bne_Un:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Bne_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;

                case OpCodes.Bge:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Bge_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Bge_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Bge_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;

                case OpCodes.Ble:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ble_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Ble_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ble_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;

                case OpCodes.Blt:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Blt_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Blt_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Blt_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;

                case OpCodes.Bgt:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Bgt_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Bgt_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Bgt_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
            }

            return ILOffset;
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if branch test operand value is a floating point value.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //This will store the offset from the current next op's position
            //to the IL op to jump to.
            int ILOffset = 0;
            //This will store the jump operation - by default, the basic jump op
            JmpOp jumpOp = JmpOp.Jump;
            bool UnsignedTest = false;

            JmpOp inverseJumpOp = JmpOp.None;
            bool isNegativeTest = false;

            //The value for the jump op to test against - currently always "0" since on jnz and jz ops are used.
            string testVal = "0";

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Br:
                    //Load the IL offset as signed Int 32 from the value bytes.
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Br_S:
                    //Load the IL offset as signed Int 8 from the value bytes.
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    break;
                case OpCodes.Brtrue:
                    //See above.
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-true means we want to do the jump if the operand is not equal to 0
                    //i.e. not false
                    //Set the jump op to jnz - Jump if Not Zero
                    jumpOp = JmpOp.JumpNotZero;
                    break;
                case OpCodes.Brtrue_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpNotZero;
                    break;
                case OpCodes.Brfalse:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-true means we want to do the jump if the operand is equal to 0
                    //i.e. is false
                    //Set the jump op to jz - Jump if Zero
                    jumpOp = JmpOp.JumpZero;
                    break;
                case OpCodes.Brfalse_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpZero;
                    break;

                case OpCodes.Beq:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-equal means we want to do the jump if the operand is equal to the other operand
                    //i.e. A == B
                    //Set the jump op to je - Jump if equal
                    jumpOp = JmpOp.JumpEqual;
                    inverseJumpOp = JmpOp.JumpNotEqual;
                    break;
                case OpCodes.Beq_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpEqual;
                    inverseJumpOp = JmpOp.JumpNotEqual;
                    break;

                case OpCodes.Bne_Un:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-equal means we want to do the jump if the operand is equal to the other operand
                    //i.e. A == B
                    //Set the jump op to je - Jump if not equal
                    jumpOp = JmpOp.JumpNotEqual;
                    isNegativeTest = true;
                    break;
                case OpCodes.Bne_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpNotEqual;
                    isNegativeTest = true;
                    break;

                case OpCodes.Bge:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-greater-than-or-equal means we want to do the jump if the operand is greater than equal to 
                    //the other operand
                    //i.e. A >= B
                    //Set the jump op to jge - Jump if greater than or equal
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    inverseJumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Bge_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    inverseJumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Bge_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    inverseJumpOp = JmpOp.JumpLessThan;
                    break;
                case OpCodes.Bge_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpGreaterThanEqual;
                    inverseJumpOp = JmpOp.JumpLessThan;
                    break;

                case OpCodes.Ble:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-less-than-or-equal means we want to do the jump if the operand is less than equal to 
                    //the other operand
                    //i.e. A <= B
                    //Set the jump op to jle - Jump if less than or equal
                    jumpOp = JmpOp.JumpLessThanEqual;
                    inverseJumpOp = JmpOp.JumpGreaterThan;
                    break;
                case OpCodes.Ble_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpLessThanEqual;
                    inverseJumpOp = JmpOp.JumpGreaterThan;
                    break;
                case OpCodes.Ble_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpLessThanEqual;
                    inverseJumpOp = JmpOp.JumpGreaterThan;
                    break;
                case OpCodes.Ble_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpLessThanEqual;
                    inverseJumpOp = JmpOp.JumpGreaterThan;
                    break;

                case OpCodes.Blt:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-less-than means we want to do the jump if the operand is less than
                    //the other operand
                    //i.e. A < B
                    //Set the jump op to jl - Jump if less than
                    jumpOp = JmpOp.JumpLessThan;
                    isNegativeTest = true;
                    break;
                case OpCodes.Blt_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpLessThan;
                    isNegativeTest = true;
                    break;
                case OpCodes.Blt_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpLessThan;
                    isNegativeTest = true;
                    break;
                case OpCodes.Blt_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpLessThan;
                    isNegativeTest = true;
                    break;

                case OpCodes.Bgt:
                    //See above
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Branch-if-greater-than means we want to do the jump if the operand is greater than
                    //the other operand
                    //i.e. A > B
                    //Set the jump op to jg - Jump if greater than
                    jumpOp = JmpOp.JumpGreaterThan;
                    isNegativeTest = true;
                    break;
                case OpCodes.Bgt_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    //See above
                    jumpOp = JmpOp.JumpGreaterThan;
                    isNegativeTest = true;
                    break;
                case OpCodes.Bgt_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpGreaterThan;
                    isNegativeTest = true;
                    break;
                case OpCodes.Bgt_Un_S:
                    //See above
                    ILOffset = (sbyte)theOp.ValueBytes[0];
                    UnsignedTest = true;
                    //See above
                    jumpOp = JmpOp.JumpGreaterThan;
                    isNegativeTest = true;
                    break;
            }

            if (ILOffset == 0)
            {
                //Err..why bother jumping at all if the offset is 0?
                conversionState.Append(new Comment("No jump insert - pointless 0 distance jump"));
            }
            else
            {
                //Get the IL number of the next op
                int startILNum = theOp.NextOffset;
                //Add the offset to get the IL op num to jump to
                int ILNumToGoTo = startILNum + ILOffset;

                //Find the IL op to jump to 
                ILOp opToGoTo = conversionState.Input.At(ILNumToGoTo);
                int opToGoToPosition = conversionState.PositionOf(opToGoTo);
                int currOpPosition = conversionState.PositionOf(theOp);

                //If the jump op is not a straightforward jump i.e. has one or more conditions
                if (jumpOp == JmpOp.JumpZero || jumpOp == JmpOp.JumpNotZero)
                {
                    //Pop from our stack the test item to use in the condition
                    StackItem testItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

                    if (testItem.isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Branch test based on float not supported!");
                    }
                    //If the test item is Int64 or UInt64           TODO: or Struct
                    if (testItem.sizeOnStackInBytes == 8)
                    {
                        //Compare first 32 bits (low bits)
                        //Then (if necessary) compare second 32 bits (high bits)

                        //Pop the low bits
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                        //Compare the low bits to the test value
                        conversionState.Append(new Cmp {Arg2 = testVal, Arg1 = "EAX"});
                        //Pre-emptively pop the high bits
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});

                        //If we are testing for not equal to zero:
                        if (jumpOp == JmpOp.JumpNotZero)
                        {
                            //If the low bits are not zero, do the jump
                            conversionState.Append(new Jmp {JumpType = jumpOp, DestILPosition = opToGoToPosition});
                            //Otherwise, test if the high bits are not 0
                            //We use "cmp" not "test" as "cmp" uses +/- test val, "test" uses & with test val.
                            conversionState.Append(new Cmp {Arg2 = testVal, Arg1 = "EAX"});
                            //If the high bits are not zero, do the jump
                            conversionState.Append(new Jmp {JumpType = jumpOp, DestILPosition = opToGoToPosition});
                        }
                        //If we are testing for equal to zero:
                        else if (jumpOp == JmpOp.JumpZero)
                        {
                            //If the low bits are not zero, jump to the end of these test as condition has not been met
                            conversionState.Append(new Jmp
                            {
                                JumpType = JmpOp.JumpNotZero,
                                DestILPosition = currOpPosition,
                                Extension = "End"
                            });
                            //Otherwise, test if the high bits are zero
                            conversionState.Append(new Cmp {Arg2 = testVal, Arg1 = "EAX"});
                            //If they are, do the jump
                            conversionState.Append(new Jmp {JumpType = jumpOp, DestILPosition = opToGoToPosition});
                            //Insert the end label to be jumped to if condition is not met (see above)
                            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End"});
                        }
                        else
                        {
                            //Check that no extra ops have been added in the switch block above!
                            throw new NotSupportedException("Unsupported 64-bit branch op!");
                        }
                    }
                    //If the test item is Int32 or UInt32
                    else if (testItem.sizeOnStackInBytes == 4)
                    {
                        //Pop the test item
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                        //Compare the test item to the test value
                        conversionState.Append(new Cmp {Arg2 = testVal, Arg1 = "EAX"});
                        //Do the specified jump
                        conversionState.Append(new Jmp {JumpType = jumpOp, DestILPosition = opToGoToPosition});
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid stack operand sizes!");
                    }
                }
                else if (jumpOp == JmpOp.JumpEqual || jumpOp == JmpOp.JumpNotEqual ||
                         jumpOp == JmpOp.JumpGreaterThanEqual ||
                         jumpOp == JmpOp.JumpLessThanEqual ||
                         jumpOp == JmpOp.JumpLessThan ||
                         jumpOp == JmpOp.JumpGreaterThan)
                {
                    //Pop from our stack the test items to use in the condition
                    StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                    StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

                    if (itemA.isFloat || itemB.isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Branch test based on float not supported!");
                    }
                    if (itemA.sizeOnStackInBytes != itemB.sizeOnStackInBytes)
                    {
                        throw new InvalidOperationException("Branch test operands must be same size!");
                    }
                    //If the test item is Int64 or UInt64           TODO: or Struct
                    if (itemA.sizeOnStackInBytes == 8)
                    {
                        //Pop the test item B
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "ECX"});
                        //Pop the test item A
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EDX"});

                        if (!isNegativeTest)
                        {
                            //Compare test item A high bits to test item B high bits
                            conversionState.Append(new Cmp {Arg2 = "ECX", Arg1 = "EDX"});
                            //If they are not equal, abort the testing
                            conversionState.Append(new Jmp
                            {
                                JumpType = inverseJumpOp,
                                DestILPosition = currOpPosition,
                                Extension = "End",
                                UnsignedTest = UnsignedTest
                            });
                            //Else the igh bits are equal so test low bits
                            //Compare test item A low bits to test item B low bits
                            conversionState.Append(new Cmp {Arg2 = "EBX", Arg1 = "EAX"});
                            //Do the specified jump
                            conversionState.Append(new Jmp
                            {
                                JumpType = jumpOp,
                                DestILPosition = opToGoToPosition,
                                UnsignedTest = UnsignedTest
                            });

                            //Insert the end label to be jumped to if condition is not met (see above)
                            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End"});
                        }
                        else
                        {
                            //Compare test item A high bits to test item B high bits
                            conversionState.Append(new Cmp {Arg2 = "ECX", Arg1 = "EDX"});
                            //Do the specified jump
                            conversionState.Append(new Jmp
                            {
                                JumpType = jumpOp,
                                DestILPosition = opToGoToPosition,
                                UnsignedTest = UnsignedTest
                            });
                            //Compare test item A low bits to test item B low bits
                            conversionState.Append(new Cmp {Arg2 = "EBX", Arg1 = "EAX"});
                            //Do the specified jump
                            conversionState.Append(new Jmp
                            {
                                JumpType = jumpOp,
                                DestILPosition = opToGoToPosition,
                                UnsignedTest = UnsignedTest
                            });
                        }
                    }
                    else if (itemA.sizeOnStackInBytes == 4)
                    {
                        //Pop the test item B
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});
                        //Pop the test item A
                        conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                        //Compare test item A to test item B
                        conversionState.Append(new Cmp {Arg2 = "EBX", Arg1 = "EAX"});
                        //Do the specified jump
                        conversionState.Append(new Jmp
                        {
                            JumpType = jumpOp,
                            DestILPosition = opToGoToPosition,
                            UnsignedTest = UnsignedTest
                        });
                    }
                    else
                    {
                        throw new NotSupportedException("Branch test based on supplied stack item sizes not supported!");
                    }
                }
                else
                {
                    //Do the straightforward jump...
                    conversionState.Append(new Jmp {JumpType = jumpOp, DestILPosition = opToGoToPosition});
                }

                // Fork the stack after the processing of the jump (since processing of jump may pop some stuff)
                conversionState.CurrentStackFrame.ForkStack(theOp, opToGoTo);
            }
        }
    }
}