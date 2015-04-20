#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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

namespace Kernel
{
    /// <summary>
    /// Implements the lowest-level kernel exception handling.
    /// </summary>
    [Compiler.PluggedClass]
    [Drivers.Compiler.Attributes.PluggedClass]
    public static unsafe class ExceptionMethods
    {
        /// <summary>
        /// The reason the kernel is halting. Useful for debugging purposes in case an exception causes
        /// an immediate halt.
        /// </summary>
        public static FOS_System.String HaltReason = "";

        /// <summary>
        /// The message to display when the Throw method panics.
        /// </summary>
        public static string Throw_PanicMessage = "Throw Panicked!";
        /// <summary>
        /// The message to display when the kernel panics.
        /// </summary>
        public static string UnhandledException_PanicMessage = "Unhandled exception! Panic!";

        /// <summary>
        /// The current exception - null as soon as the exception has been handled.
        /// </summary>
        //public static FOS_System.Exception CurrentException = null;
        /// <summary>
        /// Whether the current exception is pending being handled.
        /// </summary>
        //public static bool PendingException = false;
        /// <summary>
        /// Pointer to the current Exception Handler Info (a pointer to the
        /// struct on the stack).
        /// </summary>
        //public static ExceptionHandlerInfo* CurrentHandlerPtr = null;

        public static ExceptionState* State;
        public static ExceptionState* DefaultState;

        //public static ExceptionState* InterruptsState
        //{
        //    private get;
        //    set;
        //}
        //public static bool Print = false;

        public static bool InsideExHandling = false;

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        static ExceptionMethods()
        {
        }

        public static FOS_System.Exception CurrentException
        {
            [Drivers.Compiler.Attributes.NoGC]
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                if (State != null &&
                    State->CurrentHandlerPtr != null)
                {
                    return (FOS_System.Exception)Utilities.ObjectUtilities.GetObject(State->CurrentHandlerPtr->Ex);
                }
                return null;
            }
        }

        /// <summary>
        /// Adds a new Exception Handler Info structure to the stack and sets 
        /// it as the current handler.
        /// </summary>
        /// <param name="handlerPtr">A pointer to the first op of the catch or finally handler.</param>
        /// <param name="filterPtr">0 = finally handler, 0xFFFFFFFF = catch handler with no filter. 
        /// Original intended use was as a pointer to the first op of the catch filter but never implemented like this.</param>
        [Compiler.AddExceptionHandlerInfoMethod]
        [Drivers.Compiler.Attributes.AddExceptionHandlerInfoMethod]
        //[Compiler.PluggedMethod(ASMFilePath=@"ASM\Exceptions\AddExceptionHandlerInfo")]
        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=@"ASM\Exceptions\AddExceptionHandlerInfo")]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void AddExceptionHandlerInfo(
            void* handlerPtr,
            void* filterPtr)
        {
            if (InsideExHandling)
            {
                return;
            }

            //if (Print)
            //{
            //    //BasicConsole.WriteLine("Adding exception handler info...");
            //    OutputCurrentStateInfo("AddExceptionHandlerInfo");
            //}

            //InsideExHandling = true;
            //BasicConsole.WriteLine((uint)BasePointer);
            //BasicConsole.WriteLine((uint)StackPointer);
            //BasicConsole.DumpMemory(State, sizeof(ExceptionState));
            //BasicConsole.DumpMemory(StackPointer, 36);
            //BasicConsole.DelayOutput(25);
            //InsideExHandling = false;

            //if ((uint)filterPtr != 0x0u)
            //{
            //    BasicConsole.WriteLine("Adding try-catch handler.");
            //}
            //else
            //{
            //    BasicConsole.WriteLine("Adding try-finally handler.");
            //}

            //{
            //    InsideExHandling = true;

            //    AddExceptionHandlerInfo_EntryStackState* EnterState = (AddExceptionHandlerInfo_EntryStackState*)BasePointer;

            //    BasicConsole.Write("Caller's EBP: ");
            //    BasicConsole.WriteLine(EnterState->EBP);

            //    BasicConsole.Write("Return (to caller) addr: ");
            //    BasicConsole.WriteLine(EnterState->RetAddr);

            //    BasicConsole.WriteLine("Top 20 DWords of stack:");
            //    BasicConsole.DumpMemory(StackPointer, 80);

            //    BasicConsole.DelayOutput(50);

            //    InsideExHandling = false;
            //}

            if (State == null)
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine("Error! ExceptionMethods.State is null.");
                BasicConsole.DelayOutput(10);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            //BasicConsole.WriteLine("Getting base ptr...");
            AddExceptionHandlerInfo_EntryStackState* BasePtr = (AddExceptionHandlerInfo_EntryStackState*)BasePointer;

            //BasicConsole.WriteLine("Getting stack ptr...");
            uint LocalsSize = (uint)BasePtr - (uint)StackPointer;

            //if ((uint)OriginStackPtr != ((uint)BasePtr) - 20)
            //{
            //    BasicConsole.WriteLine("Unexpected origin stack ptr! (1)");
            //}

            // Create space for setting up handler info
            //BasicConsole.WriteLine("Creating space...");
            StackPointer -= sizeof(ExceptionHandlerInfo);

            //if ((uint)StackPointer != ((uint)BasePtr) - 44)
            //{
            //    BasicConsole.WriteLine("Unexpected stack ptr! (1)");
            //}

            // Setup handler info
            //BasicConsole.WriteLine("Setting up handler...");
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

            //if ((uint)StackPointer != ((uint)BasePtr) - 60)
            //{
            //    BasicConsole.WriteLine("Unexpected stack ptr! (2)");
            //}

            // Setup the duplicate stack data
            //  - Nothing to do for args - duplicate values don't matter
            //  - Copy over ebp and return address
            //BasicConsole.WriteLine("Duplicating info...");
            uint* DuplicateValsStackPointer = (uint*)StackPointer;
            *DuplicateValsStackPointer = BasePtr->EBP;
            *(DuplicateValsStackPointer + 1) = BasePtr->RetAddr;
            //*(uint*)(StackPointer + 4) = BasePtr->RetAddress;
            if (*((uint*)(StackPointer)) != BasePtr->EBP)
            {
                BasicConsole.WriteLine("Base address not set properly!");
            }
            if (*((uint*)(StackPointer + 4)) != BasePtr->RetAddr)
            {
                BasicConsole.WriteLine("Ret address not set properly!");
            }

            //InsideExHandling = true;
            //BasicConsole.DumpMemory(State, sizeof(ExceptionState));
            //BasicConsole.DumpMemory(State->CurrentHandlerPtr, sizeof(ExceptionHandlerInfo));
            //BasicConsole.DumpMemory(StackPointer, 160);
            //BasicConsole.DelayOutput(25);
            //InsideExHandling = false;

            // Shift stack values up over the locals, base pointer, ret address and args
            //BasicConsole.WriteLine("Shifting stack...");
            ShiftStack((byte*)ExHndlrPtr + sizeof(ExceptionHandlerInfo) - 4, LocalsSize + 12);

            //InsideExHandling = true;
            //BasicConsole.DumpMemory(State, sizeof(ExceptionState));
            //BasicConsole.DumpMemory(State->CurrentHandlerPtr, sizeof(ExceptionHandlerInfo));
            //BasicConsole.DumpMemory(StackPointer, 160);
            //BasicConsole.DelayOutput(300);
            //InsideExHandling = false;

            // Shift stack pointer to correct position - eliminates "empty space" of duplicates
            StackPointer += 16;

            // MethodEnd will:
            //      - Add size of locals to esp
            //      - Pop EBP
            //      - Ret to ret address
            // Caller will:
            //      - Add size of args to esp
            // Which should leave the stack at the bottom of the (shifted up) ex handler info
            //BasicConsole.WriteLine("Returning...");
            //BasicConsole.DelayOutput(5);
        }
        private struct AddExceptionHandlerInfo_EntryStackState
        {
            public uint EBP;
            public uint RetAddr;
            public uint FilterPtr;
            public uint HandlerPtr;
        }

        /// <summary>
        /// Throws the specified exception.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        [Compiler.ThrowExceptionMethod]
        //[Drivers.Compiler.Attributes.ThrowExceptionMethod]
        //[Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\Throw")]
        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\Throw")]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void Throw(FOS_System.Exception ex)
        {
            //BasicConsole.WriteLine("Throwing exception...");
            //BasicConsole.DelayOutput(10);
            
            //InsideExHandling = true;
            //FOS_System.Heap.PreventAllocation = false;
            //FOS_System.GC.Enabled = true;
            //MethodEnterStackState* EnterState = (MethodEnterStackState*)BasePointer;
            //BasicConsole.Write("Return (to caller) addr: ");
            //BasicConsole.WriteLine(EnterState->RetAddr);
            //BasicConsole.Write("Caller's EBP: ");
            //BasicConsole.WriteLine(EnterState->EBP);
            //BasicConsole.WriteLine("Top 10 DWords of stack:");
            //BasicConsole.DumpMemory(StackPointer, 40);
            //OutputCurrentStateInfo("Throw");
            //BasicConsole.DelayOutput(50);
            //InsideExHandling = false;

            Kernel.FOS_System.GC.IncrementRefCount(ex);

            //BasicConsole.WriteLine("Creating exception item...");
            if (State->CurrentHandlerPtr->Ex != null)
            {
                //GC ref count remains consistent because the Ex pointer below is going to be replaced but
                //  same pointer stored in InnerException.
                // Result is ref count goes: +1 here, -1 below
                ex.InnerException = (Kernel.FOS_System.Exception)Utilities.ObjectUtilities.GetObject(State->CurrentHandlerPtr->Ex);
            }
            State->CurrentHandlerPtr->Ex = Utilities.ObjectUtilities.GetHandle(ex);
            State->CurrentHandlerPtr->ExPending = 1;

            HandleException();

            // We never expect to get here...
            HaltReason = "HandleException returned!";
            BasicConsole.WriteLine(HaltReason);
            // Try to cause fault
            *((byte*)0x800000000) = 0;
        }
        /// <summary>
        /// Throws the specified exception. Implementation used is exactly the 
        /// same as Throw (exact same plug used) just allows another way to 
        /// throw an exception.
        /// </summary>
        /// <param name="exPtr">The pointer to the exception to throw.</param>
        //[Compiler.PluggedMethod(ASMFilePath = null)]
        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void ThrowFromPtr(UInt32* exPtr)
        {
            Throw((FOS_System.Exception)Utilities.ObjectUtilities.GetObject(exPtr));
        }

        /// <summary>
        /// Handles the current pending exception.
        /// </summary>
        [Compiler.HandleExceptionMethod]
        [Drivers.Compiler.Attributes.HandleExceptionMethod]
        //[Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleException")]
        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleException")]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void HandleException()
        {
            //BasicConsole.WriteLine("Handling exception...");
            //if (Print)
            //{
            //    OutputCurrentStateInfo("HandleException");
            //}

            if(State != null)
            {
                if (State->CurrentHandlerPtr != null)
                {
                    if (State->CurrentHandlerPtr->InHandler != 0)
                    {
                        //BasicConsole.WriteLine("In handler. Shifting to previous...");

                        State->CurrentHandlerPtr->InHandler = 0;
                        if (State->CurrentHandlerPtr->PrevHandlerPtr != null)
                        {
                            State->CurrentHandlerPtr->PrevHandlerPtr->Ex = State->CurrentHandlerPtr->Ex;
                            State->CurrentHandlerPtr->PrevHandlerPtr->ExPending = State->CurrentHandlerPtr->ExPending;
                        }
                        State->CurrentHandlerPtr = State->CurrentHandlerPtr->PrevHandlerPtr;
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
                            //BasicConsole.WriteLine("Catch handler.");

                            CurrHandlerPtr->ExPending = 0;
                        }
                        //else
                        //{
                        //    BasicConsole.WriteLine("Finally handler.");
                        //}

                        CurrHandlerPtr->InHandler = 1;

                        //BasicConsole.WriteLine("Returning to handler...");
                        ArbitaryReturn(CurrHandlerPtr->EBP, CurrHandlerPtr->ESP, CurrHandlerPtr->HandlerAddress);
                    }
                }
            }
            
            // If we get to here, it's an unhandled exception
            HaltReason = "Unhandled / improperly handled exception!";
            BasicConsole.WriteLine(HaltReason);
            // Try to cause fault
            *((byte*)0x800000000) = 0;
        }
        /// <summary>
        /// Handles cleanly leaving a critical section (i.e. try or catch block)
        /// </summary>
        /// <param name="continuePtr">A pointer to the instruction to continue execution at.</param>
        [Compiler.ExceptionsHandleLeaveMethod]
        [Drivers.Compiler.Attributes.ExceptionsHandleLeaveMethod]
        //[Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleLeave")]
        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleLeave")]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void HandleLeave(void* continuePtr)
        {
            if (InsideExHandling)
            {
                return;
            }

            //if (Print)
            //{
            //    //BasicConsole.WriteLine("Handling leave...");
            //    OutputCurrentStateInfo("HandleLeave");
            //}

            if (State == null || 
                State->CurrentHandlerPtr == null)
            {
                // If we get to here, it's an unhandled exception
                HaltReason = "Cannot leave on null handler!";
                BasicConsole.WriteLine(HaltReason);
                BasicConsole.DelayOutput(5);

                //InsideExHandling = true;
                //FOS_System.Heap.PreventAllocation = false;
                //FOS_System.GC.Enabled = true;
                //MethodEnterStackState* EnterState = (MethodEnterStackState*)BasePointer;
                //BasicConsole.Write("Caller's EBP: ");
                //BasicConsole.WriteLine(EnterState->EBP);
                //BasicConsole.Write("Return (to caller) addr: ");
                //BasicConsole.WriteLine(EnterState->RetAddr);
                //BasicConsole.WriteLine("Top 10 DWords of stack:");
                //BasicConsole.DumpMemory(StackPointer, 40);
                //BasicConsole.DelayOutput(50);
                //InsideExHandling = false;


                // Try to cause fault
                *((byte*)0x800000000) = 0;
            }

            // Leaving a critical section cleanly
            // We need to handle 2 cases:
            // Case 1 : Leaving "try" or "catch" of a try-catch
            // Case 2 : Leaving the "try" of a try-finally

            //InsideExHandling = true;
            //BasicConsole.DumpMemory(State, sizeof(ExceptionState));
            //BasicConsole.DumpMemory(State->CurrentHandlerPtr, sizeof(ExceptionHandlerInfo));
            //BasicConsole.DelayOutput(25);
            //InsideExHandling = false;

            if ((uint)State->CurrentHandlerPtr->FilterAddress != 0x0u)
            {
                //BasicConsole.WriteLine("Leaving try-catch...");
                //BasicConsole.DelayOutput(10);

                // Case 1 : Leaving "try" or "catch" of a try-catch
            
                if (State->CurrentHandlerPtr->Ex != null)
                {
                    //BasicConsole.WriteLine("Cleaning up current exception...");

                    //BasicConsole.WriteLine("Decrementing ref count...");
                    FOS_System.GC.DecrementRefCount((FOS_System.Object)Utilities.ObjectUtilities.GetObject(State->CurrentHandlerPtr->Ex));
                    State->CurrentHandlerPtr->Ex = null;
                    //BasicConsole.WriteLine("Cleaned up.");
                }

                State->CurrentHandlerPtr->InHandler = 0;

                uint EBP = State->CurrentHandlerPtr->EBP;
                uint ESP = State->CurrentHandlerPtr->ESP;

                State->CurrentHandlerPtr = State->CurrentHandlerPtr->PrevHandlerPtr;
                
                ArbitaryReturn(EBP, ESP + (uint)sizeof(ExceptionHandlerInfo), (byte*)continuePtr);
            }
            else
            {
                //BasicConsole.WriteLine("Leaving try-finally...");
                //BasicConsole.DelayOutput(5);

                // Case 2 : Leaving the "try" of a try-finally
                
                State->CurrentHandlerPtr->InHandler = 1;

                ArbitaryReturn(State->CurrentHandlerPtr->EBP,
                               State->CurrentHandlerPtr->ESP,
                               State->CurrentHandlerPtr->HandlerAddress);
            }

        }
        /// <summary>
        /// Handles cleanly leaving a "finally" critical section (i.e. finally block). 
        /// This may result in an exception being passed to the next handler if it has not been caught &amp; handled yet.
        /// </summary>
        [Compiler.ExceptionsHandleEndFinallyMethod]
        [Drivers.Compiler.Attributes.ExceptionsHandleEndFinallyMethod]
        //[Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleEndFinally")]
        //[Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleEndFinally")]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void HandleEndFinally()
        {
            if (InsideExHandling)
            {
                return;
            }

            //if (Print)
            //{
            //    //BasicConsole.WriteLine("Handling end finally...");
            //    OutputCurrentStateInfo("HandleEndFinally");
            //}

            if (State == null ||
                State->CurrentHandlerPtr == null)
            {
                // If we get to here, it's an unhandled exception
                HaltReason = "Cannot end finally on null handler!";
                BasicConsole.WriteLine(HaltReason);
                BasicConsole.DelayOutput(5);
                
                //InsideExHandling = true;
                //FOS_System.Heap.PreventAllocation = false;
                //FOS_System.GC.Enabled = true;
                //MethodEnterStackState* EnterState = (MethodEnterStackState*)BasePointer;
                //BasicConsole.Write("Return (to caller) addr: ");
                //BasicConsole.WriteLine(EnterState->RetAddr);
                //BasicConsole.DelayOutput(10);
                //InsideExHandling = false;

                // Try to cause fault
                *((byte*)0x800000000) = 0;
            }

            // Leaving a "finally" critical section cleanly
            // We need to handle 2 cases:
            // Case 1 : Pending exception
            // Case 2 : No pending exception

            if (State->CurrentHandlerPtr->ExPending != 0)
            {
                //BasicConsole.WriteLine("Handle end finally...");
                //BasicConsole.WriteLine("Pending exception.");
                //BasicConsole.DelayOutput(10);

                // Case 1 : Pending exception
                // We call HandleException to continue passing the exception up the chain
                // 2. Call HandleException

                HandleException();
            }
            else
            {
                //BasicConsole.WriteLine("No pending exception.");
                //BasicConsole.DelayOutput(5);

                //InsideExHandling = true;
                //BasicConsole.DumpMemory(State, sizeof(ExceptionState));
                //BasicConsole.DumpMemory(State->CurrentHandlerPtr, sizeof(ExceptionHandlerInfo));
                //BasicConsole.DumpMemory(BasePointer, 16);
                //BasicConsole.DumpMemory((byte*)State->CurrentHandlerPtr->EBP, 16);
                //BasicConsole.DumpMemory((byte*)*((uint*)BasePointer), 16);
                //BasicConsole.DumpMemory((byte*)*((uint*)State->CurrentHandlerPtr->EBP), 16);
                //BasicConsole.DelayOutput(25);
                //InsideExHandling = false;

                // Case 2 : No pending exception
                // We return control as normal
                // 1. Set current exception handler to previous handler
                // 2. Do stack cleanup
                // 3. Return control as normal

                State->CurrentHandlerPtr->InHandler = 0;

                uint EBP = State->CurrentHandlerPtr->EBP;
                uint ESP = State->CurrentHandlerPtr->ESP;
                                        
                //if (State->CurrentHandlerPtr->PrevHandlerAddress == null)
                //{
                //    BasicConsole.WriteLine("Ending finally with no previous handler.");
                //}
                //else
                //{
                //    BasicConsole.WriteLine("Ending finally with a previous handler.");
                //}

                State->CurrentHandlerPtr = State->CurrentHandlerPtr->PrevHandlerPtr;

                ArbitaryReturn(EBP,
                    ESP + (uint)sizeof(ExceptionHandlerInfo),
                    (byte*)*((uint*)(BasePointer + 4)));
            }
        }

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static unsafe void PrintCurrentData()
        {
            InsideExHandling = true;

            MethodEnterStackState* EnterState = (MethodEnterStackState*)BasePointer;
            
            BasicConsole.Write("Caller's EBP: ");
            BasicConsole.WriteLine(EnterState->EBP);
            
            BasicConsole.Write("Return (to caller) addr: ");
            BasicConsole.WriteLine(EnterState->RetAddr);

            BasicConsole.WriteLine("Top 10 DWords of stack:");
            BasicConsole.DumpMemory(StackPointer, 40);

            BasicConsole.DelayOutput(50);

            InsideExHandling = false;
        }
        private struct MethodEnterStackState
        {
            public uint EBP;
            public uint RetAddr;
        }

        //[Drivers.Compiler.Attributes.NoDebug]
        //[Drivers.Compiler.Attributes.NoGC]
        //private static void OutputCurrentStateInfo(string Caller)
        //{
        //    if (State == DefaultState)
        //    {
        //        BasicConsole.WriteLine(Caller);
        //        BasicConsole.WriteLine("Current state: Default state");
        //    }
        //    else if (State == InterruptsState)
        //    {
        //        //BasicConsole.WriteLine("Current state: Interrupts state");
        //    }
        //    else
        //    {
        //        BasicConsole.WriteLine(Caller);
        //        BasicConsole.WriteLine("Current state: Other state");
        //    }
        //    //BasicConsole.DelayOutput(50);
        //}

        private static unsafe byte* StackPointer
        {
            [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=@"ASM\Exceptions\StackPointer")]
            get
            {
                return null;
            }
            [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=null)]
            set
            {
            }
        }
        private static unsafe byte* BasePointer
        {
            [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\BasePointer")]
            get
            {
                return null;
            }
            [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
            set
            {
            }
        }

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\ShiftStack")]
        private static void ShiftStack(byte* From_High, uint Dist)
        {
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Exceptions\ArbitaryReturn")]
        private static void ArbitaryReturn(uint EBP, uint ESP, byte* RetAddr)
        {
        }

        /// <summary>
        /// Rethrows the current exception.
        /// </summary>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Rethrow()
        {
            Throw(CurrentException);
        }

        /// <summary>
        /// Throws a divide by zero exception.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DivideByZeroException()
        {
            HaltReason = "Divide by zero exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new FOS_System.Exceptions.DivideByZeroException());
        }
        /// <summary>
        /// Throws a divide by zero exception storing the specified exception address.
        /// </summary>
        /// <param name="address">The address of the code that caused the exception.</param>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DivideByZeroException(uint address)
        {
            HaltReason = "Divide by zero exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new FOS_System.Exceptions.DivideByZeroException(address));
        }
        /// <summary>
        /// Throws an overflow exception.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_OverflowException()
        {
            HaltReason = "Overflow exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new FOS_System.Exceptions.OverflowException());
        }
        /// <summary>
        /// Throws an invalid op code exception.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_InvalidOpCodeException()
        {
            HaltReason = "Invalid op code exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new FOS_System.Exceptions.InvalidOpCodeException());
        }
        /// <summary>
        /// Throws a double fault exception.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
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
            Throw(new FOS_System.Exceptions.DoubleFaultException(errorCode));
        }
        /// <summary>
        /// Throws a stack exception.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_StackException()
        {
            HaltReason = "Stack exception.";
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(HaltReason);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            Throw(new FOS_System.Exceptions.StackException());
        }
        /// <summary>
        /// Throws a page fault exception.
        /// </summary>
        /// <param name="errorCode">The error code associated with the page fault.</param>
        /// <param name="address">The address which caused the fault.</param>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_PageFaultException(uint eip, uint errorCode, uint address)
        {
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Page fault exception!");

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

            Throw(new FOS_System.Exceptions.PageFaultException(errorCode, address));
        }

        /// <summary>
        /// Throws a Null Reference exception.
        /// </summary>
        /// <remarks>
        /// Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        [Compiler.ThrowNullReferenceExceptionMethod]
        [Drivers.Compiler.Attributes.ThrowNullReferenceExceptionMethod]
        public static void Throw_NullReferenceException()
        {
            HaltReason = "Null reference exception.";
            Throw(new FOS_System.Exceptions.NullReferenceException());
        }
        /// <summary>
        /// Throws an Array Type Mismatch exception.
        /// </summary>
        /// <remarks>
        /// Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        [Compiler.ThrowArrayTypeMismatchExceptionMethod]
        //[Drivers.Compiler.Attributes.ThrowArrayTypeMismatchExceptionMethod]
        public static void Throw_ArrayTypeMismatchException()
        {
            HaltReason = "Array type mismatch exception.";
            Throw(new FOS_System.Exceptions.ArrayTypeMismatchException());
        }
        /// <summary>
        /// Throws a Index Out Of Range exception.
        /// </summary>
        /// <remarks>
        /// Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        [Compiler.ThrowIndexOutOfRangeExceptionMethod]
        [Drivers.Compiler.Attributes.ThrowIndexOutOfRangeExceptionMethod]
        public static void Throw_IndexOutOfRangeException()
        {
            HaltReason = "Index out of range exception.";
            Throw(new FOS_System.Exceptions.IndexOutOfRangeException());
        }        
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ExceptionState
    {
        public ExceptionHandlerInfo* CurrentHandlerPtr;
    }
    /// <summary>
    /// Represents an Exception Handler Info.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This structure is so closely linked to the ASM code that modifying it is a big NO!
    /// </para>
    /// <para>
    /// It is created by the AddExceptionHandlerInfo method on the stack but could technically be put 
    /// anywhere in memory. The order of the fields in the structure matters!
    /// </para>
    /// </remarks>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ExceptionHandlerInfo
    {
        /// <summary>
        /// The value of ESP when the handler info was created. This value of 
        /// ESP is also a pointer to the first byte of this Exception Handler Info structure.
        /// The ESP register is restored to this value when a handler is entered or re-entered.
        /// </summary>
        public UInt32 ESP;
        /// <summary>
        /// The value of EBP when the handler info was created.
        /// The EBP register is restored to this value when a handler is entered or re-entered.
        /// </summary>
        public UInt32 EBP;
        /// <summary>
        /// The address of the first op of the handler / a pointer to the first op of the handler.
        /// </summary>
        public byte* HandlerAddress;
        /// <summary>
        /// 0x00000000 = indicates this is a finally handler. 
        /// 0xFFFFFFFF = indicates this is a catch handler with no filter.
        /// 0xXXXXXXXX = The address of the first op of the filter - has not actually been implemented! Behaviour for such values is undetermined.
        /// </summary>
        public byte* FilterAddress;
        /// <summary>
        /// A pointer to the previous exception handler info (i.e. the address of the previous info).
        /// </summary>
        public ExceptionHandlerInfo* PrevHandlerPtr;
        /// <summary>
        /// Whether execution is currently inside the try-section or the handler-section of this exception handler info.
        /// </summary>
        public UInt32 InHandler;

        public UInt32 ExPending;
         
        public void* Ex;
    }
}
