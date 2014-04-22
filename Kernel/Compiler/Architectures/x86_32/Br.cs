using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Br : ILOps.Br
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if branch test operand value is a floating point value.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //This will store the offset from the current next op's position
            //to the IL op to jump to.
            int ILOffset = 0;
            //This will store the jump operation - by default, the basic jump op
            string jumpOp = "jmp";

            string inverseJumpOp = "";
            bool isNegativeTest = false;

            //The value for the jump op to test against - currently always "0" since on jnz and jz ops are used.
            string testVal = "0";
            
            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Br:
                    //Load the IL offset as signed Int 32 from the value bytes.
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    break;
                case OpCodes.Br_S:
                    //Load the IL offset as signed Int 8 from the value bytes.
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    break;
                case OpCodes.Brtrue:
                    //See above.
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-true means we want to do the jump if the operand is not equal to 0
                    //i.e. not false
                    //Set the jump op to jnz - Jump if Not Zero
                    jumpOp = "jnz";
                    break;
                case OpCodes.Brtrue_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jnz";
                    break;
                case OpCodes.Brfalse:
                    //See above
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-true means we want to do the jump if the operand is equal to 0
                    //i.e. is false
                    //Set the jump op to jz - Jump if Zero
                    jumpOp = "jz";
                    break;
                case OpCodes.Brfalse_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jz";
                    break;

                case OpCodes.Beq:
                    //See above
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-equal means we want to do the jump if the operand is equal to the other operand
                    //i.e. A == B
                    //Set the jump op to je - Jump if equal
                    jumpOp = "je";
                    inverseJumpOp = "jne";
                    break;
                case OpCodes.Beq_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "je";
                    inverseJumpOp = "jne";
                    break;

                case OpCodes.Bne_Un:
                    //See above
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-equal means we want to do the jump if the operand is equal to the other operand
                    //i.e. A == B
                    //Set the jump op to je - Jump if not equal
                    jumpOp = "jne";
                    isNegativeTest = true;
                    break;
                case OpCodes.Bne_Un_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jne";
                    isNegativeTest = true;
                    break;

                case OpCodes.Bge:
                    //See above
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-greater-than-or-equal means we want to do the jump if the operand is greater than equal to 
                    //the other operand
                    //i.e. A >= B
                    //Set the jump op to jge - Jump if greater than or equal
                    jumpOp = "jge";
                    inverseJumpOp = "jl";
                    break;
                case OpCodes.Bge_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jge";
                    inverseJumpOp = "jl";
                    break;
                case OpCodes.Bge_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //See above
                    jumpOp = "jae";
                    inverseJumpOp = "jb";
                    break;
                case OpCodes.Bge_Un_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jae";
                    inverseJumpOp = "jb";
                    break;

                case OpCodes.Ble:
                    //See above
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-less-than-or-equal means we want to do the jump if the operand is less than equal to 
                    //the other operand
                    //i.e. A <= B
                    //Set the jump op to jle - Jump if less than or equal
                    jumpOp = "jle";
                    inverseJumpOp = "jg";
                    break;
                case OpCodes.Ble_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jle";
                    inverseJumpOp = "jg";
                    break;
                case OpCodes.Ble_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //See above
                    jumpOp = "jbe";
                    inverseJumpOp = "ja";
                    break;
                case OpCodes.Ble_Un_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jbe";
                    inverseJumpOp = "ja";
                    break;

                case OpCodes.Blt:
                    //See above
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //Branch-if-less-than means we want to do the jump if the operand is less than
                    //the other operand
                    //i.e. A < B
                    //Set the jump op to jl - Jump if less than
                    jumpOp = "jl";
                    isNegativeTest = true;
                    break;
                case OpCodes.Blt_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jl";
                    isNegativeTest = true;
                    break;
                case OpCodes.Blt_Un:
                    //See above : This is unsigned variant
                    ILOffset = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    //See above
                    jumpOp = "jb";
                    isNegativeTest = true;
                    break;
                case OpCodes.Blt_Un_S:
                    //See above
                    ILOffset = (int)(sbyte)anILOpInfo.ValueBytes[0];
                    //See above
                    jumpOp = "jb";
                    isNegativeTest = true;
                    break;

            }

            if (ILOffset == 0)
            {
                //Err..why bother jumping at all if the offset is 0?
                result.AppendLine("; No jump insert - pointless 0 distance jump"); //DEBUG INFO
            }
            else
            {
                //Get the IL number of the next op
                int startILNum = anILOpInfo.NextPosition;
                //Add the offset to get the IL op num to jump to
                int ILNumToGoTo = startILNum + ILOffset;
                //Find and mark the IL op to jump to as requiring a label.
                (from ops in aScannerState.CurrentILChunk.ILOpInfos
                 where (ops.Position == ILNumToGoTo)
                 select ops).First().ASMInsertLabel = true;

                //Pre-work out the asm label for jumping to
                string jumpToLabel = string.Format("{0}.IL_{1}_0", 
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    ILNumToGoTo);
                //Pre-work out the asm label for jumping to if the condition is not met.
                string endLabelName = string.Format("{0}.IL_{1}_End", 
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method), 
                    anILOpInfo.Position);
                
                //If the jump op is not a straightforward jump i.e. has one or more conditions
                if (jumpOp == "jz" || jumpOp == "jnz")
                {
                    //Pop from our stack the test item to use in the condition
                    StackItem testItem = aScannerState.CurrentStackFrame.Stack.Pop();
                    
                    if (testItem.isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Branch test based on float not supported!");
                    }
                    //If the test item is Int64 or UInt64
                    else if (testItem.sizeOnStackInBytes == 8)
                    {
                        //Compare first 32 bits (low bits)
                        //Then (if necessary) compare second 32 bits (high bits)
                        
                        //Pop the low bits
                        result.AppendLine("pop dword eax");
                        //Compare the low bits to the test value
                        result.AppendLine(string.Format("cmp eax, {0}", testVal));
                        //Pre-emptively pop the high bits
                        result.AppendLine("pop dword eax");
                        
                        //If we are testing for not equal to zero:
                        if (jumpOp == "jnz")
                        {
                            //If the low bits are not zero, do the jump
                            result.AppendLine(string.Format("jnz {0}", jumpToLabel));
                            //Otherwise, test if the high bits are not 0
                            //We use "cmp" not "test" as "cmp" uses +/- test val, "test" uses & with test val.
                            result.AppendLine(string.Format("cmp eax, {0}", testVal));
                            //If the high bits are not zero, do the jump
                            result.AppendLine(string.Format("jnz {0}", jumpToLabel));
                        }
                        //If we are testing for equal to zero:
                        else if (jumpOp == "jz")
                        {
                            //If the low bits are not zero, jump to the end of these test as condition has not been met
                            result.AppendLine(string.Format("jnz {0}", endLabelName));
                            //Otherwise, test if the high bits are zero
                            result.AppendLine(string.Format("cmp eax, {0}", testVal));
                            //If they are, do the jump
                            result.AppendLine(string.Format("jz {0}", jumpToLabel));
                            //Insert the end label to be jumped to if condition is not met (see above)
                            result.AppendLine(string.Format("{0}:", endLabelName));
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
                        result.AppendLine("pop dword eax");
                        //Compare the test item to the test value
                        result.AppendLine(string.Format("cmp eax, {0}", testVal));
                        //Do the specified jump
                        result.AppendLine(string.Format("{0} {1}",
                            jumpOp, jumpToLabel));
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid stack operand sizes!");
                    }
                }
                else if (jumpOp == "je" || jumpOp == "jne" ||
                         jumpOp == "jge" || jumpOp == "jae" ||
                         jumpOp == "jle" || jumpOp == "jbe" ||
                         jumpOp == "jl" || jumpOp == "jb")
                {
                    //Pop from our stack the test items to use in the condition
                    StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();
                    StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();

                    if (itemA.isFloat || itemB.isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Branch test based on float not supported!");
                    }
                    else if(itemA.sizeOnStackInBytes != itemB.sizeOnStackInBytes)
                    {
                        throw new InvalidOperationException("Branch test operands must be same size!");
                    }
                    else if (itemA.sizeOnStackInBytes == 8)
                    {
                        //Pop the test item B
                        result.AppendLine("pop dword ebx");
                        result.AppendLine("pop dword ecx");
                        //Pop the test item A
                        result.AppendLine("pop dword eax");
                        result.AppendLine("pop dword edx");

                        if(!isNegativeTest)
                        {
                            //Compare test item A high bits to test item B high bits
                            result.AppendLine("cmp edx, ecx");
                            //If they are not equal, abort the testing
                            result.AppendLine(string.Format("{0} {1}", inverseJumpOp, endLabelName));
                            //Else the igh bits are equal so test low bits
                            //Compare test item A low bits to test item B low bits
                            result.AppendLine("cmp eax, ebx");
                            //Do the specified jump
                            result.AppendLine(string.Format("{0} {1}", jumpOp, jumpToLabel));
                        }
                        else
                        {
                            //Compare test item A high bits to test item B high bits
                            result.AppendLine("cmp edx, ecx");
                            //Do the specified jump
                            result.AppendLine(string.Format("{0} {1}", jumpOp, jumpToLabel));
                            //Compare test item A low bits to test item B low bits
                            result.AppendLine("cmp eax, ebx");
                            //Do the specified jump
                            result.AppendLine(string.Format("{0} {1}", jumpOp, jumpToLabel));
                        }
                    }
                    else if (itemA.sizeOnStackInBytes == 4)
                    {
                        //Pop the test item B
                        result.AppendLine("pop dword ebx");
                        //Pop the test item A
                        result.AppendLine("pop dword eax");
                        //Compare test item A to test item B
                        result.AppendLine("cmp eax, ebx");
                        //Do the specified jump
                        result.AppendLine(string.Format("{0} {1}",
                            jumpOp, jumpToLabel));
                    }
                    else
                    {
                        throw new NotSupportedException("Branch test based on supplied stack item sizes not supported!");
                    }
                }
                else
                {
                    //Do the straightforward jump...
                    result.AppendLine(string.Format("{0} {1}",
                        jumpOp, jumpToLabel));
                }
            }

            return result.ToString().Trim();
        }
    }
}
