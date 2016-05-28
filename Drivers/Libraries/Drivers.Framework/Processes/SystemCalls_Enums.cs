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

namespace Drivers.Framework.Processes
{
    /// <summary>
    ///     The complete list of system calls.
    /// </summary>
    public enum SystemCallNumbers : uint
    {
        /// <summary>
        ///     Indicates an invalid system call has been made.
        /// </summary>
        /// <remarks>
        ///     Useful for guarding against errors.
        /// </remarks>
        INVALID = 0,

        /// <summary>
        ///     Registers a method of the caller process as a handler for an interrupt service routine.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Cannot register a handler for any ISR less than 49.
        ///     </para>
        ///     <para>
        ///         Only one handler method for all ISRs is accepted. Specify address 0xFFFFFFFF to avoid re-specifying the handler
        ///         function.
        ///     </para>
        /// </remarks>
        RegisterISRHandler,

        /// <summary>
        ///     Deregisters the caller process for handling an interrupt service routine.
        /// </summary>
        DeregisterISRHandler,

        /// <summary>
        ///     Registers a method of the caller process as a handler for an interrupt request.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Cannot register a handler for any IRQ greater than 15.
        ///     </para>
        ///     <para>
        ///         Only one handler method for all IRQs is accepted. Specify address 0xFFFFFFFF to avoid re-specifying the handler
        ///         function.
        ///     </para>
        /// </remarks>
        RegisterIRQHandler,

        /// <summary>
        ///     Deregisters the caller process for handling an interrupt request.
        /// </summary>
        DeregisterIRQHandler,

        /// <summary>
        ///     Registers a method of the caller process as a handler for a system call.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Currently, a handler can be registered for any system call number. However, calling prioritisation
        ///         is implemented so the registered handler may not see all calls if higher priority (/earlier) handlers
        ///         deicde to handle the call.
        ///     </para>
        ///     <para>
        ///         Only one handler method for all system calls is accepted. Specify address 0xFFFFFFFF to avoid re-specifying the
        ///         handler function.
        ///     </para>
        /// </remarks>
        RegisterSyscallHandler,

        /// <summary>
        ///     Deregisters the caller process for handling a system call.
        /// </summary>
        DeregisterSyscallHandler,
        IsPhysicalAddressMapped,
        IsVirtualAddressMapped,
        GetVirtualAddress,
        GetPhysicalAddress,
        RequestPages,
        UnmapPages,
        SharePages,
        AcceptPages,
        StartProcess,
        EndProcess,
        SetProcessAttributes,
        GetProcessList,
        WaitOnProcess,
        StartThread,
        EndThread,
        SetThreadAttributes,
        GetThreadList,
        WaitOnThread,
        SleepThread,
        WakeThread,
        CreateSemaphore,
        ShareSemaphore,
        ReleaseSemaphore,
        WaitSemaphore,
        SignalSemaphore,
        GetTime,
        SetTime,
        GetUpTime,

        /// <summary>
        ///     Registers the process as offering a specific pipe outpoint type.
        /// </summary>
        RegisterPipeOutpoint,

        /// <summary>
        ///     Gets the number of available pipe outpoints of a specific type.
        /// </summary>
        GetNumPipeOutpoints,

        /// <summary>
        ///     Gets descriptions of the available pipe outpoints of a specific type.
        /// </summary>
        /// <remarks>
        ///     Use a GetNumPipeOutpoints system call to determine the number of available outpoints of the wanted type. Then use
        ///     that
        ///     number to allocate a sufficiently large array.
        /// </remarks>
        GetPipeOutpoints,

        /// <summary>
        ///     Creates a pipe, of a specific type, from the caller process to the specified outpoint (of another or the same
        ///     process).
        /// </summary>
        CreatePipe,

        /// <summary>
        ///     Waits for a pipe of a specific type to be created to the caller process.
        /// </summary>
        WaitOnPipeCreate,

        /// <summary>
        ///     Reads from the specified pipe. Blocking and non-blocking calls supported on the same pipe.
        /// </summary>
        ReadPipe,

        /// <summary>
        ///     Writes to the specified pipe. Blocking and non-blocking calls supported on the same pipe.
        /// </summary>
        WritePipe,

        /// <summary>
        ///     Send an 8-byte message immediately to a process.
        /// </summary>
        SendMessage,

        /// <summary>
        ///     Receive an 8-byte message from a process.
        /// </summary>
        /// <remarks>
        ///     This is invoked by a Send Message system call and should only be used
        ///     by handler functions. You cannot invoke a "Receive Message" system call
        ///     directly.
        /// </remarks>
        ReceiveMessage
    }

    /// <summary>
    ///     Results of system calls.
    /// </summary>
    /// <remarks>
    ///     Distinct values from the system call numbers are used. This is for debugging purposes since the
    ///     call number and the result number share a register which can lead to accidental contamination in
    ///     incorrect implementations.
    /// </remarks>
    public enum SystemCallResults : uint
    {
        /// <summary>
        ///     The system call was not handled by any process.
        /// </summary>
        Unhandled = 0xC0DEC0DE,

        /// <summary>
        ///     The system call was handled successfully.
        /// </summary>
        OK = 0xC1DEC1DE,

        /// <summary>
        ///     The system call was deferred for later processing.
        /// </summary>
        /// <remarks>
        ///     This is only supposed to be seen within the core OS. A caller should never see this value.
        /// </remarks>
        Deferred = 0xC2DEC2DE,

        /// <summary>
        ///     The system call failed.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         In the case of non-blocking system calls, a failure is expected. Unhandled should be used to indicate an error
        ///         in a non-blocking system call. In all other cases, this indicates an error occurred.
        ///     </para>
        ///     <para>
        ///         Note that an error may be a permissions failure of some description or it may be an actual exception.
        ///     </para>
        /// </remarks>
        Fail = 0xC3DEC3DE,

        /// <summary>
        ///     The system call was handled successfully but other system call handlers are also allowed to process the
        ///     call.
        /// </summary>
        /// <remarks>
        ///     This is only supposed to be seen within the core OS. A caller should never see this value.
        /// </remarks>
        OK_PermitActions = 0xC4DEC4DE,

        /// <summary>
        ///     The system call was deferred but other system call handlers are also allowed to process the
        ///     call.
        /// </summary>
        /// <remarks>
        ///     This is only supposed to be seen within the core OS. A caller should never see this value.
        /// </remarks>
        Deferred_PermitActions = 0xC5DEC5DE,

        /// <summary>
        ///     The system call was unhandled but the handler method is requesting another thread is woken.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is only supposed to be seen within the core OS. A caller should never see this value.
        ///     </para>
        ///     <para>
        ///         This is used so a system call can be used as a notification to a process. A method returning
        ///         this cannot also handle the system call.
        ///     </para>
        /// </remarks>
        RequestAction_WakeThread = 0xC6DEC6DE,

        /// <summary>
        ///     The system call was handled successfully but the thread should not be woken.
        /// </summary>
        OK_NoWake = 0xC7DEC7DE
    }
}