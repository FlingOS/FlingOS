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

using System.Runtime.InteropServices;
using Drivers.Compiler.Attributes;
using Drivers.Framework;
using Drivers.Framework.Exceptions;
using Drivers.Utilities;

namespace Drivers
{
    public delegate void PageFaultHandler(uint eip, uint errorCode, uint address);

    /// <summary>
    ///     Implements the lowest-level Drivers exception handling.
    /// </summary>
    public static unsafe class ExceptionMethods
    {
        /// <summary>
        ///     The reason the Drivers is halting. Useful for debugging purposes in case an exception causes
        ///     an immediate halt.
        /// </summary>
        public static String HaltReason = "";

        /// <summary>
        ///     The message to display when the Throw method panics.
        /// </summary>
        public static string Throw_PanicMessage = "Throw Panicked!";

        /// <summary>
        ///     The message to display when the Drivers panics.
        /// </summary>
        public static string UnhandledException_PanicMessage = "Unhandled exception! Panic!";

        public static ExceptionState* State;
        public static ExceptionState* DefaultState;

        public static PageFaultHandler ThePageFaultHandler = null;

        public static Exception CurrentException
        {
            [NoGC]
            [NoDebug]
            get
            {
                if (State != null &&
                    State->CurrentHandlerPtr != null)
                {
                    return (Exception)ObjectUtilities.GetObject(State->CurrentHandlerPtr->Ex);
                }
                return null;
            }
        }

        public static byte* StackPointer
        {
            [PluggedMethod(ASMFilePath = @"ASM\Exceptions\StackPointer")] get { return null; }
            [PluggedMethod(ASMFilePath = null)] set { }
        }

        public static byte* BasePointer
        {
            [PluggedMethod(ASMFilePath = @"ASM\Exceptions\BasePointer")] get { return null; }
            [PluggedMethod(ASMFilePath = null)] set { }
        }

        [NoGC]
        [NoDebug]
        static ExceptionMethods()
        {
        }

        /// <summary>
        ///     Adds a new Exception Handler Info structure to the stack and sets
        ///     it as the current handler.
        /// </summary>
        /// <param name="handlerPtr">A pointer to the first op of the catch or finally handler.</param>
        /// <param name="filterPtr">
        ///     0 = finally handler, 0xFFFFFFFF = catch handler with no filter.
        ///     Original intended use was as a pointer to the first op of the catch filter but never implemented like this.
        /// </param>
        [AddExceptionHandlerInfoMethod]
        [NoDebug]
        [NoGC]
        public static void AddExceptionHandlerInfo(
            void* handlerPtr,
            void* filterPtr)
        {
            if (State == null)
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine("Error! ExceptionMethods.State is null.");
                BasicConsole.DelayOutput(10);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            State->depth++;

            //if (filterPtr != null)
            //{
            //    BasicConsole.WriteLine("Enter try-catch block");
            //}
            //else
            //{
            //    BasicConsole.WriteLine("Enter try-finally block");
            //}

            AddExceptionHandlerInfo_EntryStackState* BasePtr = (AddExceptionHandlerInfo_EntryStackState*)BasePointer;

            uint LocalsSize = (uint)BasePtr - (uint)StackPointer;

            // Create space for setting up handler info
            StackPointer -= sizeof(ExceptionHandlerInfo);

            // Setup handler info
            ExceptionHandlerInfo* ExHndlrPtr = (ExceptionHandlerInfo*)StackPointer;
            ExHndlrPtr->EBP = BasePtr->EBP;
            //                  EBP + 8 (for ret addr, ebp) + 8 (for args) - sizeof(ExceptionHandlerInfo)
            ExHndlrPtr->ESP = (uint)BasePtr + 8 + 8 - (uint)sizeof(ExceptionHandlerInfo);
            ExHndlrPtr->FilterAddress = (byte*)filterPtr;
            ExHndlrPtr->HandlerAddress = (byte*)handlerPtr;
            ExHndlrPtr->PrevHandlerPtr = State->CurrentHandlerPtr;
            ExHndlrPtr->InHandler = 0;
            ExHndlrPtr->ExPending = 0;
            ExHndlrPtr->Ex = null;

            State->CurrentHandlerPtr = (ExceptionHandlerInfo*)((byte*)ExHndlrPtr + (LocalsSize + 12));

            StackPointer -= 8; // For duplicate (empty) args
            StackPointer -= 8; // For duplicate ebp, ret addr

            // Setup the duplicate stack data
            //  - Nothing to do for args - duplicate values don't matter
            //  - Copy over ebp and return address
            uint* DuplicateValsStackPointer = (uint*)StackPointer;
            *DuplicateValsStackPointer = BasePtr->EBP;
            *(DuplicateValsStackPointer + 1) = BasePtr->RetAddr;

            ShiftStack((byte*)ExHndlrPtr + sizeof(ExceptionHandlerInfo) - 4, LocalsSize + 12);

            // Shift stack pointer to correct position - eliminates "empty space" of duplicates
            StackPointer += 16;

            // MethodEnd will:
            //      - Add size of locals to esp
            //      - Pop EBP
            //      - Ret to ret address
            // Caller will:
            //      - Add size of args to esp
            // Which should leave the stack at the bottom of the (shifted up) ex handler info
        }

        /// <summary>
        ///     Throws the specified exception.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        [NoDebug]
        [NoGC]
        public static void Throw(Exception ex)
        {
            GC.IncrementRefCount(ex);

            //BasicConsole.WriteLine("Exception thrown:");
            //BasicConsole.WriteLine(ex.Message);

            if (State->CurrentHandlerPtr->Ex != null)
            {
                //GC ref count remains consistent because the Ex pointer below is going to be replaced but
                //  same pointer stored in InnerException.
                // Result is ref count goes: +1 here, -1 below
                ex.InnerException = (Exception)ObjectUtilities.GetObject(State->CurrentHandlerPtr->Ex);
            }
            if (ex.InstructionAddress == 0)
            {
                ex.InstructionAddress = *((uint*)BasePointer + 1);
            }
            State->CurrentHandlerPtr->Ex = ObjectUtilities.GetHandle(ex);
            State->CurrentHandlerPtr->ExPending = 1;

            HandleException();

            // We never expect to get here...
            HaltReason = "HandleException returned!";
            BasicConsole.WriteLine(HaltReason);
            // Try to cause fault
            *(byte*)0x800000000 = 0;
        }

        /// <summary>
        ///     Throws the specified exception. Implementation used is exactly the
        ///     same as Throw (exact same plug used) just allows another way to
        ///     throw an exception.
        /// </summary>
        /// <param name="exPtr">The pointer to the exception to throw.</param>
        //        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        [NoDebug]
        [NoGC]
        public static void ThrowFromPtr(uint* exPtr)
        {
            Exception ex = (Exception)ObjectUtilities.GetObject(exPtr);
            ex.InstructionAddress = *((uint*)BasePointer + 1);
            Throw(ex);
        }

        /// <summary>
        ///     Handles the current pending exception.
        /// </summary>
        [HandleExceptionMethod]
        [NoDebug]
        [NoGC]
        public static void HandleException()
        {
            //BasicConsole.WriteLine("Handle exception");

            if (State != null)
            {
                if (State->CurrentHandlerPtr != null)
                {
                    if (State->CurrentHandlerPtr->InHandler != 0)
                    {
                        State->CurrentHandlerPtr->InHandler = 0;
                        if (State->CurrentHandlerPtr->PrevHandlerPtr != null)
                        {
                            State->CurrentHandlerPtr->PrevHandlerPtr->Ex = State->CurrentHandlerPtr->Ex;
                            State->CurrentHandlerPtr->PrevHandlerPtr->ExPending = State->CurrentHandlerPtr->ExPending;
                        }
                        State->CurrentHandlerPtr = State->CurrentHandlerPtr->PrevHandlerPtr;
                        State->depth--;
                        State->history[State->history_pos++] = (uint)State->CurrentHandlerPtr->HandlerAddress;
                        if (State->history_pos > 31)
                        {
                            State->history_pos = 0;
                        }
                    }
                }

                ExceptionHandlerInfo* CurrHandlerPtr = State->CurrentHandlerPtr;
                if (CurrHandlerPtr != null)
                {
                    if ((uint)CurrHandlerPtr->HandlerAddress != 0x00000000u)
                    {
                        if ((uint)CurrHandlerPtr->FilterAddress != 0x00000000u)
                        {
                            //Catch handler
                            CurrHandlerPtr->ExPending = 0;
                        }

                        CurrHandlerPtr->InHandler = 1;

                        ArbitaryReturn(CurrHandlerPtr->EBP, CurrHandlerPtr->ESP, CurrHandlerPtr->HandlerAddress);
                    }
                }
            }

            // If we get to here, it's an unhandled exception
            HaltReason = "Unhandled / improperly handled exception!";
            BasicConsole.WriteLine(HaltReason);
            // Try to cause fault
            *(byte*)0x800000000 = 0;
        }

        /// <summary>
        ///     Handles cleanly leaving a critical section (i.e. try or catch block)
        /// </summary>
        /// <param name="continuePtr">A pointer to the instruction to continue execution at.</param>
        [ExceptionsHandleLeaveMethod]
        [NoDebug]
        [NoGC]
        public static void HandleLeave(void* continuePtr)
        {
            if (State == null ||
                State->CurrentHandlerPtr == null)
            {
                // If we get to here, it's an unhandled exception
                HaltReason = "";
                if (State == null)
                {
                    HaltReason = "Cannot leave on null handler! Address: 0x         - Null state";
                }
                else if (State->CurrentHandlerPtr == null)
                {
                    HaltReason = "Cannot leave on null handler! Address: 0x         - Null handler";
                }
                else
                {
                    HaltReason = "Cannot leave on null handler! Address: 0x         - Unexpected reason";
                }


                uint y = *(uint*)(BasePointer + 4);
                int offset = 48;

                #region Address

                while (offset > 40)
                {
                    uint rem = y & 0xFu;
                    switch (rem)
                    {
                        case 0:
                            HaltReason[offset] = '0';
                            break;
                        case 1:
                            HaltReason[offset] = '1';
                            break;
                        case 2:
                            HaltReason[offset] = '2';
                            break;
                        case 3:
                            HaltReason[offset] = '3';
                            break;
                        case 4:
                            HaltReason[offset] = '4';
                            break;
                        case 5:
                            HaltReason[offset] = '5';
                            break;
                        case 6:
                            HaltReason[offset] = '6';
                            break;
                        case 7:
                            HaltReason[offset] = '7';
                            break;
                        case 8:
                            HaltReason[offset] = '8';
                            break;
                        case 9:
                            HaltReason[offset] = '9';
                            break;
                        case 10:
                            HaltReason[offset] = 'A';
                            break;
                        case 11:
                            HaltReason[offset] = 'B';
                            break;
                        case 12:
                            HaltReason[offset] = 'C';
                            break;
                        case 13:
                            HaltReason[offset] = 'D';
                            break;
                        case 14:
                            HaltReason[offset] = 'E';
                            break;
                        case 15:
                            HaltReason[offset] = 'F';
                            break;
                    }
                    y >>= 4;
                    offset--;
                }

                #endregion

                BasicConsole.WriteLine(HaltReason);

                if (State != null)
                {
                    if (State->depth > 0)
                    {
                        BasicConsole.WriteLine("    -- Positive depth");
                    }
                    else if (State->depth == 0)
                    {
                        BasicConsole.WriteLine("    -- Zero depth");
                    }
                    else if (State->depth < 0)
                    {
                        BasicConsole.WriteLine("    -- Negative depth");
                    }

                    int pos = State->history_pos;
                    do
                    {
                        BasicConsole.Write(State->history[pos]);
                        BasicConsole.Write(" ");

                        pos--;
                        if (pos == -1)
                        {
                            pos = 31;
                        }
                    } while (pos != State->history_pos);
                }

                BasicConsole.DelayOutput(5);

                // Try to cause fault
                *(byte*)0x800000000 = 0;
            }

            // Leaving a critical section cleanly
            // We need to handle 2 cases:
            // Case 1 : Leaving "try" or "catch" of a try-catch
            // Case 2 : Leaving the "try" of a try-finally

            if ((uint)State->CurrentHandlerPtr->FilterAddress != 0x0u)
            {
                // Case 1 : Leaving "try" or "catch" of a try-catch
                //BasicConsole.WriteLine("Leave try or catch of try-catch");

                if (State->CurrentHandlerPtr->Ex != null)
                {
                    GC.DecrementRefCount((Object)ObjectUtilities.GetObject(State->CurrentHandlerPtr->Ex));
                    State->CurrentHandlerPtr->Ex = null;
                }

                State->CurrentHandlerPtr->InHandler = 0;

                uint EBP = State->CurrentHandlerPtr->EBP;
                uint ESP = State->CurrentHandlerPtr->ESP;

                State->CurrentHandlerPtr = State->CurrentHandlerPtr->PrevHandlerPtr;
                State->depth--;
                State->history[State->history_pos++] = (uint)State->CurrentHandlerPtr->HandlerAddress;
                if (State->history_pos > 31)
                {
                    State->history_pos = 0;
                }

                ArbitaryReturn(EBP, ESP + (uint)sizeof(ExceptionHandlerInfo), (byte*)continuePtr);
            }
            else
            {
                // Case 2 : Leaving the "try" of a try-finally

                State->CurrentHandlerPtr->InHandler = 1;

                byte* handlerAddress = State->CurrentHandlerPtr->HandlerAddress;

                //BasicConsole.WriteLine("Leave try of try-finally");
                //BasicConsole.Write("Handler address: ");
                //BasicConsole.WriteLine((uint)handlerAddress);
                //BasicConsole.Write("Continue ptr: ");
                //BasicConsole.WriteLine((uint)continuePtr);

                State->CurrentHandlerPtr->HandlerAddress = (byte*)continuePtr;

                ArbitaryReturn(State->CurrentHandlerPtr->EBP,
                    State->CurrentHandlerPtr->ESP,
                    handlerAddress);
            }
        }

        /// <summary>
        ///     Handles cleanly leaving a "finally" critical section (i.e. finally block).
        ///     This may result in an exception being passed to the next handler if it has not been caught &amp; handled yet.
        /// </summary>
        [ExceptionsHandleEndFinallyMethod]
        [NoDebug]
        [NoGC]
        public static void HandleEndFinally()
        {
            if (State == null ||
                State->CurrentHandlerPtr == null)
            {
                // If we get to here, it's an unhandled exception
                if (State == null)
                {
                    HaltReason = "Cannot end finally in null state!";
                }
                else if (State->CurrentHandlerPtr == null)
                {
                    HaltReason = "Cannot end finally on null handler!";
                }
                else
                {
                    HaltReason = "Cannot end finally for unexpected reason!";
                }
                BasicConsole.WriteLine(HaltReason);
                BasicConsole.DelayOutput(5);

                // Try to cause fault
                *(byte*)0x800000000 = 0;
            }

            // Leaving a "finally" critical section cleanly
            // We need to handle 2 cases:
            // Case 1 : Pending exception
            // Case 2 : No pending exception

            //BasicConsole.WriteLine("Handle end finally");

            if (State->CurrentHandlerPtr->ExPending != 0)
            {
                // Case 1 : Pending exception

                HandleException();
            }
            else
            {
                // Case 2 : No pending exception

                State->CurrentHandlerPtr->InHandler = 0;

                uint EBP = State->CurrentHandlerPtr->EBP;
                uint ESP = State->CurrentHandlerPtr->ESP;
                byte* retAddr = State->CurrentHandlerPtr->HandlerAddress; //(byte*)*((uint*)(BasePointer + 4));

                //BasicConsole.Write("Continue ptr (from HandlerAddress): ");
                //BasicConsole.WriteLine((uint)State->CurrentHandlerPtr->HandlerAddress);
                //BasicConsole.Write("Actual continue addr (from EBP): ");
                //BasicConsole.WriteLine(*((uint*)(BasePointer + 4)));

                State->CurrentHandlerPtr = State->CurrentHandlerPtr->PrevHandlerPtr;
                State->depth--;
                State->history[State->history_pos++] = (uint)State->CurrentHandlerPtr->HandlerAddress;
                if (State->history_pos > 31)
                {
                    State->history_pos = 0;
                }

                ArbitaryReturn(EBP,
                    ESP + (uint)sizeof(ExceptionHandlerInfo),
                    retAddr);
            }
        }

        [PluggedMethod(ASMFilePath = @"ASM\Exceptions\ShiftStack")]
        private static void ShiftStack(byte* From_High, uint Dist)
        {
        }

        [PluggedMethod(ASMFilePath = @"ASM\Exceptions\ArbitaryReturn")]
        private static void ArbitaryReturn(uint EBP, uint ESP, byte* RetAddr)
        {
        }

        /// <summary>
        ///     Rethrows the current exception.
        /// </summary>
        [NoDebug]
        [NoGC]
        public static void Rethrow()
        {
            Throw(CurrentException);
        }

        /// <summary>
        ///     Throws a divide by zero exception.
        /// </summary>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DivideByZeroException()
        {
            HaltReason = "Divide by zero exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new DivideByZeroException());
        }

        /// <summary>
        ///     Throws a divide by zero exception storing the specified exception address.
        /// </summary>
        /// <param name="address">The address of the code that caused the exception.</param>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DivideByZeroException(uint address)
        {
            HaltReason = "Divide by zero exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new DivideByZeroException(address));
        }

        /// <summary>
        ///     Throws an overflow exception.
        /// </summary>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_OverflowException()
        {
            HaltReason = "Overflow exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new OverflowException("Processor reported an overflow."));
        }

        /// <summary>
        ///     Throws an invalid op code exception.
        /// </summary>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_InvalidOpCodeException()
        {
            HaltReason = "Invalid op code exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new InvalidOpCodeException());
        }

        /// <summary>
        ///     Throws a double fault exception.
        /// </summary>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DoubleFaultException(uint address, uint errorCode)
        {
            HaltReason = "Double fault exception. Address: 0x         Error code: 0x        ";

            uint y = address;
            int offset = 42;

            #region Address

            while (offset > 34)
            {
                uint rem = y & 0xFu;
                switch (rem)
                {
                    case 0:
                        HaltReason[offset] = '0';
                        break;
                    case 1:
                        HaltReason[offset] = '1';
                        break;
                    case 2:
                        HaltReason[offset] = '2';
                        break;
                    case 3:
                        HaltReason[offset] = '3';
                        break;
                    case 4:
                        HaltReason[offset] = '4';
                        break;
                    case 5:
                        HaltReason[offset] = '5';
                        break;
                    case 6:
                        HaltReason[offset] = '6';
                        break;
                    case 7:
                        HaltReason[offset] = '7';
                        break;
                    case 8:
                        HaltReason[offset] = '8';
                        break;
                    case 9:
                        HaltReason[offset] = '9';
                        break;
                    case 10:
                        HaltReason[offset] = 'A';
                        break;
                    case 11:
                        HaltReason[offset] = 'B';
                        break;
                    case 12:
                        HaltReason[offset] = 'C';
                        break;
                    case 13:
                        HaltReason[offset] = 'D';
                        break;
                    case 14:
                        HaltReason[offset] = 'E';
                        break;
                    case 15:
                        HaltReason[offset] = 'F';
                        break;
                }
                y >>= 4;
                offset--;
            }

            #endregion

            y = errorCode;
            offset = 65;

            #region Error Code

            while (offset > 57)
            {
                uint rem = y & 0xFu;
                switch (rem)
                {
                    case 0:
                        HaltReason[offset] = '0';
                        break;
                    case 1:
                        HaltReason[offset] = '1';
                        break;
                    case 2:
                        HaltReason[offset] = '2';
                        break;
                    case 3:
                        HaltReason[offset] = '3';
                        break;
                    case 4:
                        HaltReason[offset] = '4';
                        break;
                    case 5:
                        HaltReason[offset] = '5';
                        break;
                    case 6:
                        HaltReason[offset] = '6';
                        break;
                    case 7:
                        HaltReason[offset] = '7';
                        break;
                    case 8:
                        HaltReason[offset] = '8';
                        break;
                    case 9:
                        HaltReason[offset] = '9';
                        break;
                    case 10:
                        HaltReason[offset] = 'A';
                        break;
                    case 11:
                        HaltReason[offset] = 'B';
                        break;
                    case 12:
                        HaltReason[offset] = 'C';
                        break;
                    case 13:
                        HaltReason[offset] = 'D';
                        break;
                    case 14:
                        HaltReason[offset] = 'E';
                        break;
                    case 15:
                        HaltReason[offset] = 'F';
                        break;
                }
                y >>= 4;
                offset--;
            }

            #endregion

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new DoubleFaultException(errorCode));
        }

        /// <summary>
        ///     Throws a stack exception.
        /// </summary>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_StackException()
        {
            HaltReason = "Stack exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new StackException());
        }

        /// <summary>
        ///     Throws a page fault exception.
        /// </summary>
        /// <param name="errorCode">The error code associated with the page fault.</param>
        /// <param name="address">The address which caused the fault.</param>
        /// <remarks>
        ///     Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_PageFaultException(uint eip, uint errorCode, uint address)
        {
            if (ThePageFaultHandler != null)
            {
                ThePageFaultHandler(eip, errorCode, address);
            }
            else
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine("Page fault exception!");
                BasicConsole.DelayOutput(10);

                HaltReason = "Page fault exception. Address: 0x        , errorCode: 0x        , eip: 0x        ";

                uint y = address;
                int offset = 40;

                #region Address

                while (offset > 32)
                {
                    uint rem = y & 0xFu;
                    switch (rem)
                    {
                        case 0:
                            HaltReason[offset] = '0';
                            break;
                        case 1:
                            HaltReason[offset] = '1';
                            break;
                        case 2:
                            HaltReason[offset] = '2';
                            break;
                        case 3:
                            HaltReason[offset] = '3';
                            break;
                        case 4:
                            HaltReason[offset] = '4';
                            break;
                        case 5:
                            HaltReason[offset] = '5';
                            break;
                        case 6:
                            HaltReason[offset] = '6';
                            break;
                        case 7:
                            HaltReason[offset] = '7';
                            break;
                        case 8:
                            HaltReason[offset] = '8';
                            break;
                        case 9:
                            HaltReason[offset] = '9';
                            break;
                        case 10:
                            HaltReason[offset] = 'A';
                            break;
                        case 11:
                            HaltReason[offset] = 'B';
                            break;
                        case 12:
                            HaltReason[offset] = 'C';
                            break;
                        case 13:
                            HaltReason[offset] = 'D';
                            break;
                        case 14:
                            HaltReason[offset] = 'E';
                            break;
                        case 15:
                            HaltReason[offset] = 'F';
                            break;
                    }
                    y >>= 4;
                    offset--;
                }

                #endregion

                y = errorCode;
                offset = 63;

                #region Error Code

                while (offset > 55)
                {
                    uint rem = y & 0xFu;
                    switch (rem)
                    {
                        case 0:
                            HaltReason[offset] = '0';
                            break;
                        case 1:
                            HaltReason[offset] = '1';
                            break;
                        case 2:
                            HaltReason[offset] = '2';
                            break;
                        case 3:
                            HaltReason[offset] = '3';
                            break;
                        case 4:
                            HaltReason[offset] = '4';
                            break;
                        case 5:
                            HaltReason[offset] = '5';
                            break;
                        case 6:
                            HaltReason[offset] = '6';
                            break;
                        case 7:
                            HaltReason[offset] = '7';
                            break;
                        case 8:
                            HaltReason[offset] = '8';
                            break;
                        case 9:
                            HaltReason[offset] = '9';
                            break;
                        case 10:
                            HaltReason[offset] = 'A';
                            break;
                        case 11:
                            HaltReason[offset] = 'B';
                            break;
                        case 12:
                            HaltReason[offset] = 'C';
                            break;
                        case 13:
                            HaltReason[offset] = 'D';
                            break;
                        case 14:
                            HaltReason[offset] = 'E';
                            break;
                        case 15:
                            HaltReason[offset] = 'F';
                            break;
                    }
                    y >>= 4;
                    offset--;
                }

                #endregion

                y = eip;
                offset = 80;

                #region EIP

                while (offset > 72)
                {
                    uint rem = y & 0xFu;
                    switch (rem)
                    {
                        case 0:
                            HaltReason[offset] = '0';
                            break;
                        case 1:
                            HaltReason[offset] = '1';
                            break;
                        case 2:
                            HaltReason[offset] = '2';
                            break;
                        case 3:
                            HaltReason[offset] = '3';
                            break;
                        case 4:
                            HaltReason[offset] = '4';
                            break;
                        case 5:
                            HaltReason[offset] = '5';
                            break;
                        case 6:
                            HaltReason[offset] = '6';
                            break;
                        case 7:
                            HaltReason[offset] = '7';
                            break;
                        case 8:
                            HaltReason[offset] = '8';
                            break;
                        case 9:
                            HaltReason[offset] = '9';
                            break;
                        case 10:
                            HaltReason[offset] = 'A';
                            break;
                        case 11:
                            HaltReason[offset] = 'B';
                            break;
                        case 12:
                            HaltReason[offset] = 'C';
                            break;
                        case 13:
                            HaltReason[offset] = 'D';
                            break;
                        case 14:
                            HaltReason[offset] = 'E';
                            break;
                        case 15:
                            HaltReason[offset] = 'F';
                            break;
                    }
                    y >>= 4;
                    offset--;
                }

                #endregion

                BasicConsole.WriteLine(HaltReason);
                BasicConsole.SetTextColour(BasicConsole.default_colour);

                PrintStackTrace();

                Throw(new PageFaultException(errorCode, address));
            }
        }

        /// <summary>
        ///     Throws a Null Reference exception.
        /// </summary>
        /// <remarks>
        ///     Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        [ThrowNullReferenceExceptionMethod]
        public static void Throw_NullReferenceException(uint address)
        {
            HaltReason = "Null reference exception. Instruction: 0x        ";
            FillString(address, 48, HaltReason);

            bool BCPOEnabled = BasicConsole.PrimaryOutputEnabled;
            BasicConsole.PrimaryOutputEnabled = true;
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.DelayOutput(10);
            BasicConsole.PrimaryOutputEnabled = BCPOEnabled;

            PrintStackTrace();

            Exception ex = new NullReferenceException();
            ex.InstructionAddress = address;
            Throw(ex);
        }

        /// <summary>
        ///     Throws an Array Type Mismatch exception.
        /// </summary>
        /// <remarks>
        ///     Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        //[Drivers.Compiler.Attributes.ThrowArrayTypeMismatchExceptionMethod]
        public static void Throw_ArrayTypeMismatchException()
        {
            HaltReason = "Array type mismatch exception.";
            Throw(new ArrayTypeMismatchException());
        }

        /// <summary>
        ///     Throws a Index Out Of Range exception.
        /// </summary>
        /// <remarks>
        ///     Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        [ThrowIndexOutOfRangeExceptionMethod]
        public static void Throw_IndexOutOfRangeException()
        {
            HaltReason = "Index out of range exception.";
            Exception ex = new IndexOutOfRangeException(0, 0);
            ex.InstructionAddress = *((uint*)BasePointer + 1);
            Throw(ex);
        }

        [PluggedMethod(ASMFilePath = @"ASM\GetEIP")]
        public static void GetEIP()
        {
        }

        [NoGC]
        [NoDebug]
        public static void PrintStackTrace()
        {
            uint* EBP = (uint*)BasePointer;
            while ((uint)EBP%4096 != 0)
            {
                String msg = "EBP: 0x        , Return Address: 0x        , Prev EBP: 0x        ";
                //EBP: 14
                //Return address: 42
                //Prev EBP: 64

                uint ReturnAddress = *(EBP + 1);
                uint PrevEBP = *EBP;
                FillString((uint)EBP, 14, msg);
                FillString(ReturnAddress, 42, msg);
                FillString(PrevEBP, 64, msg);
                BasicConsole.WriteLine(msg);

                EBP = (uint*)PrevEBP;
            }
        }

        [NoDebug]
        [NoGC]
        public static void FillString(uint value, int offset, String str)
        {
            int end = offset - 8;
            while (offset > end)
            {
                uint rem = value & 0xFu;
                switch (rem)
                {
                    case 0:
                        str[offset] = '0';
                        break;
                    case 1:
                        str[offset] = '1';
                        break;
                    case 2:
                        str[offset] = '2';
                        break;
                    case 3:
                        str[offset] = '3';
                        break;
                    case 4:
                        str[offset] = '4';
                        break;
                    case 5:
                        str[offset] = '5';
                        break;
                    case 6:
                        str[offset] = '6';
                        break;
                    case 7:
                        str[offset] = '7';
                        break;
                    case 8:
                        str[offset] = '8';
                        break;
                    case 9:
                        str[offset] = '9';
                        break;
                    case 10:
                        str[offset] = 'A';
                        break;
                    case 11:
                        str[offset] = 'B';
                        break;
                    case 12:
                        str[offset] = 'C';
                        break;
                    case 13:
                        str[offset] = 'D';
                        break;
                    case 14:
                        str[offset] = 'E';
                        break;
                    case 15:
                        str[offset] = 'F';
                        break;
                }
                value >>= 4;
                offset--;
            }
        }

        private struct AddExceptionHandlerInfo_EntryStackState
        {
            public uint EBP;
            public uint RetAddr;
            public uint FilterPtr;
            public uint HandlerPtr;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ExceptionState
    {
        public ExceptionHandlerInfo* CurrentHandlerPtr;
        public int depth;
        public fixed uint history [32];
        public int history_pos;
    }

    /// <summary>
    ///     Represents an Exception Handler Info.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This structure is so closely linked to the ASM code that modifying it is a big NO!
    ///     </para>
    ///     <para>
    ///         It is created by the AddExceptionHandlerInfo method on the stack but could technically be put
    ///         anywhere in memory. The order of the fields in the structure matters!
    ///     </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ExceptionHandlerInfo
    {
        /// <summary>
        ///     The value of ESP when the handler info was created. This value of
        ///     ESP is also a pointer to the first byte of this Exception Handler Info structure.
        ///     The ESP register is restored to this value when a handler is entered or re-entered.
        /// </summary>
        public uint ESP;

        /// <summary>
        ///     The value of EBP when the handler info was created.
        ///     The EBP register is restored to this value when a handler is entered or re-entered.
        /// </summary>
        public uint EBP;

        /// <summary>
        ///     The address of the first op of the handler / a pointer to the first op of the handler.
        /// </summary>
        public byte* HandlerAddress;

        /// <summary>
        ///     0x00000000 = indicates this is a finally handler.
        ///     0xFFFFFFFF = indicates this is a catch handler with no filter.
        ///     0xXXXXXXXX = The address of the first op of the filter - has not actually been implemented! Behaviour for such
        ///     values is undetermined.
        /// </summary>
        public byte* FilterAddress;

        /// <summary>
        ///     A pointer to the previous exception handler info (i.e. the address of the previous info).
        /// </summary>
        public ExceptionHandlerInfo* PrevHandlerPtr;

        /// <summary>
        ///     Whether execution is currently inside the try-section or the handler-section of this exception handler info.
        /// </summary>
        public uint InHandler;

        public uint ExPending;

        public void* Ex;
    }
}