#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
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
    public static unsafe class ExceptionMethods
    {

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
        public static FOS_System.Exception CurrentException = null;
        /// <summary>
        /// Whether the current exception is pending being handled.
        /// </summary>
        public static bool PendingException = false;
        /// <summary>
        /// Pointer to the current Exception Handler Info (a pointer to the
        /// struct on the stack).
        /// </summary>
        public static ExceptionHandlerInfo* CurrentHandlerPtr = null;

        /// <summary>
        /// Adds a new Exception Handler Info structure to the stack and sets 
        /// it as the current handler.
        /// </summary>
        /// <param name="handlerPtr">A pointer to the first op of the catch or finally handler.</param>
        /// <param name="filterPtr">0 = finally handler, 0xFFFFFFFF = catch handler with no filter. 
        /// Original intended use was as a pointer to the first op of the catch filter but never implemented like this.</param>
        [Compiler.AddExceptionHandlerInfoMethod]
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Exceptions\AddExceptionHandlerInfo")]
        public static unsafe void AddExceptionHandlerInfo(
            void* handlerPtr,
            void* filterPtr)
        {

        }

        /// <summary>
        /// Throws the specified exception.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        [Compiler.ThrowExceptionMethod]
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\Throw")]
        public static unsafe void Throw(FOS_System.Exception ex)
        {
        }
        /// <summary>
        /// Throws the specified exception. Implementation used is eaxctly the 
        /// same as Throw (exact same plug used) just allows another way to 
        /// throw an exception.
        /// </summary>
        /// <param name="exPtr">The pointer to the exception to throw.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void ThrowFromPtr(UInt32* exPtr)
        {
        }

        /// <summary>
        /// Handles the current pending exception.
        /// </summary>
        [Compiler.HandleExceptionMethod]
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleException")]
        public static unsafe void HandleException()
        {
        }
        /// <summary>
        /// Handles cleanly leaving a critical section (i.e. try or catch block)
        /// </summary>
        /// <param name="continuePtr">A pointer to the instruction to continue execution at.</param>
        [Compiler.ExceptionsHandleLeaveMethod]
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleLeave")]
        public static unsafe void HandleLeave(void* continuePtr)
        {
        }
        /// <summary>
        /// Handles cleanly leaving a "finally" critical section (i.e. finally block). 
        /// This may result in an exception being passed to the next handler if it has not been caught &amp; handled yet.
        /// </summary>
        [Compiler.ExceptionsHandleEndFinallyMethod]
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Exceptions\HandleEndFinally")]
        public static unsafe void HandleEndFinally()
        {
        }

        /// <summary>
        /// Rethrows the current exception.
        /// </summary>
        public static void Rethrow()
        {
            PendingException = true;
            FOS_System.GC.IncrementRefCount(CurrentException);
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
            Throw(new FOS_System.Exceptions.DivideByZeroException());
        }
        /// <summary>
        /// Throws a divide by zero exception storing the specified exception address.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DivideByZeroException(uint address)
        {
            HaltReason = "Divide by zero exception.";
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
            Throw(new FOS_System.Exceptions.InvalidOpCodeException());
        }
        /// <summary>
        /// Throws a double fault exception.
        /// </summary>
        /// <remarks>
        /// Used by CPU interrupts to handle the creation of the exception object and calling Throw.
        /// </remarks>
        public static void Throw_DoubleFaultException()
        {
            HaltReason = "Double fault exception.";
            Throw(new FOS_System.Exceptions.DoubleFaultException());
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
        public static void Throw_PageFaultException(uint errorCode, uint address)
        {
            HaltReason = "Page fault exception.";
            Throw(new FOS_System.Exceptions.PageFaultException(errorCode, address));
        }

        /// <summary>
        /// Throws a Null Reference exception.
        /// </summary>
        /// <remarks>
        /// Used by compiler to handle the creation of the exception object and calling Throw.
        /// </remarks>
        [Compiler.ThrowNullReferenceExceptionMethod]
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
        public static void Throw_IndexOutOfRangeException()
        {
            HaltReason = "Index out of range exception.";
            Throw(new FOS_System.Exceptions.IndexOutOfRangeException());
        }        
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
        //DO NOT (!!!!!!!) MODIFY THIS STRUCTURE WITHOUT CHECKING / 
        //CHANGING THE ASM

        //DO NOT (!!!!!!!) MODIFY THIS STRUCTURE! PLEASE!! 
        
        //YOU ALMOST CERTAINLY DON'T KNOW WHAT YOU ARE DOING! :)

        /// <summary>
        /// The value of ESP when the handler info was created. This value of 
        /// ESP is also a pointer to the first byte of this Exception Handler Info structure.
        /// The ESP register is restored to this value when a handler is entered or re-entered.
        /// </summary>
        public UInt32 ESP;                                  //[CurrentHandlerPtr+0]
        /// <summary>
        /// The value of EBP when the handler info was created.
        /// The EBP register is restored to this value when a handler is entered or re-entered.
        /// </summary>
        public UInt32 EBP;                                  //[CurrentHandlerPtr+4]
        /// <summary>
        /// The address of the first op of the handler / a pointer to the first op of the handler.
        /// </summary>
        public byte* HandlerAddress;                        //[CurrentHandlerPtr+8]
        /// <summary>
        /// 0x00000000 = indicates this is a finally handler. 
        /// 0xFFFFFFFF = indicates this is a catch handler with no filter.
        /// 0xXXXXXXXX = The address of the first op of the filter - has not actually been implemented! Behaviour for such values is undetermined.
        /// </summary>
        public byte* FilterAddress;                         //[CurrentHandlerPtr+12]
        /// <summary>
        /// A pointer to the previous exception handler info (i.e. the address of the previous info).
        /// </summary>
        public ExceptionHandlerInfo* PrevHandlerAddress;    //[CurrentHandlerPtr+16]
        /// <summary>
        /// Whether execution is currently inside the try-section or the handler-section of this exception handler info.
        /// </summary>
        public UInt32 InHandler;                            //[CurrentHandlerPtr+20]

        //DO NOT (!!!!!!!) MODIFY THIS STRUCTURE WITHOUT CHECKING / 
        //CHANGING THE ASM

        //DO NOT (!!!!!!!) MODIFY THIS STRUCTURE! PLEASE!! 

        //YOU ALMOST CERTAINLY DON'T KNOW WHAT YOU ARE DOING! :)
    }
}
