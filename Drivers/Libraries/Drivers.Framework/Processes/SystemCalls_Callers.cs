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

using Drivers.Compiler.Attributes;
using Drivers.Framework.Processes.Requests.Pipes;
using Drivers.Utilities;

namespace Drivers.Framework.Processes
{
    public delegate void ThreadStartMethod();

    public delegate int ISRHanderDelegate(uint isrNumber);

    public delegate int IRQHanderDelegate(uint irqNumber);

    public delegate int SyscallHanderDelegate(uint syscallNumber, uint param1, uint param2, uint param3,
        ref uint Return2, ref uint Return3, ref uint Return4,
        uint callerProcessId, uint callerThreadId);

    /// <summary>
    ///     Contains callers and handlers for system calls.
    /// </summary>
    public static unsafe class SystemCalls
    {
        /// <summary>
        ///     The value used to indicate a thread should sleep indefinitely.
        /// </summary>
        public const int IndefiniteSleepThread = -1;

        //TODO: Implement methods for remaining system calls

        /// <summary>
        ///     Performs the specified system call.
        /// </summary>
        /// <param name="callNumber">The number of the system call to make.</param>
        /// <param name="Param1">Param value 1 of the system call.</param>
        /// <param name="Param2">Param value 2 of the system call.</param>
        /// <param name="Param3">Param value 3 of the system call.</param>
        /// <param name="Return1">Return value 1 of the system call - always a System Call Response.</param>
        /// <param name="Return2">Return value 2 of the system call.</param>
        /// <param name="Return3">Return value 3 of the system call.</param>
        /// <param name="Return4">Return value 4 of the system call.</param>
        [PluggedMethod(ASMFilePath = @"ASM\SystemCalls")]
        public static void Call(SystemCallNumbers callNumber,
            uint Param1,
            uint Param2,
            uint Param3,
            ref uint Return1,
            ref uint Return2,
            ref uint Return3,
            ref uint Return4)
        {
        }

        #region ISRs, IRQs and Syscalls

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the ISR to register for.</param>
        /// <returns>OK if successfully registered.</returns>
        [NoGC]
        public static SystemCallResults RegisterISRHandler(uint ISRNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterISRHandler, ISRNum, 0xFFFFFFFF, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the ISR to register for.</param>
        /// <param name="handler">The handler function to call for all handled ISR events.</param>
        /// <returns>OK if successfully registered.</returns>
        [NoGC]
        public static SystemCallResults RegisterISRHandler(uint ISRNum, ISRHanderDelegate handler)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterISRHandler, ISRNum, (uint)ObjectUtilities.GetHandle(handler), 0, ref Return1,
                ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the ISR to deregister.</param>
        /// <returns>OK if successfully deregistered.</returns>
        [NoGC]
        public static SystemCallResults DeregisterISRHandler(uint ISRNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.DeregisterISRHandler, ISRNum, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the IRQ to register for.</param>
        /// <returns>OK if successfully registered.</returns>
        [NoGC]
        public static SystemCallResults RegisterIRQHandler(uint IRQNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterIRQHandler, IRQNum, 0xFFFFFFFF, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the IRQ to register for.</param>
        /// <param name="handler">The handler function to call for all handled IRQ events.</param>
        /// <returns>OK if successfully registered.</returns>
        [NoGC]
        public static SystemCallResults RegisterIRQHandler(uint IRQNum, IRQHanderDelegate handler)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterIRQHandler, IRQNum, (uint)ObjectUtilities.GetHandle(handler), 0, ref Return1,
                ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the IRQ to deregister.</param>
        /// <returns>OK if successfully deregistered.</returns>
        [NoGC]
        public static SystemCallResults DeregisterIRQHandler(uint IRQNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.DeregisterIRQHandler, IRQNum, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the system call to register for.</param>
        /// <returns>OK if successfully registered.</returns>
        [NoGC]
        public static SystemCallResults RegisterSyscallHandler(SystemCallNumbers syscall)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterSyscallHandler, (uint)syscall, 0xFFFFFFFF, 0, ref Return1, ref Return2,
                ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the system call to register for.</param>
        /// <param name="handler">The handler function to call for all handled system call events.</param>
        /// <returns>OK if successfully registered.</returns>
        [NoGC]
        public static SystemCallResults RegisterSyscallHandler(SystemCallNumbers syscall, SyscallHanderDelegate handler)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterSyscallHandler, (uint)syscall, (uint)ObjectUtilities.GetHandle(handler), 0,
                ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the system call to deregister.</param>
        /// <returns>OK if successfully deregistered.</returns>
        [NoGC]
        public static SystemCallResults DeregisterSyscallHandler(SystemCallNumbers syscall)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.DeregisterSyscallHandler, (uint)syscall, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        #endregion

        #region Processes - Full TODO

        //TODO: Start Process syscall
        //TODO: End Process syscall
        //TODO: Set Process Attributes syscall
        //TODO: Get Process List syscall
        //TODO: Wait On Process syscall

        #endregion

        #region Threads - Partial TODO

        //TODO: End Thread syscall
        //TODO: Set Thread Attributes syscall
        //TODO: Get Thread List syscall
        //TODO: Wait On Thread syscall

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="startMethod">The main method of the thread.</param>
        /// <param name="NewThreadId">OUT: The Id of the new thread.</param>
        /// <returns>OK if the new thread was started.</returns>
        [NoGC]
        public static SystemCallResults StartThread(ThreadStartMethod startMethod, out uint NewThreadId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.StartThread, (uint)ObjectUtilities.GetHandle(startMethod), 0, 0, ref Return1,
                ref Return2, ref Return3, ref Return4);
            NewThreadId = Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <remarks>
        ///     This method will, if the call was successful, return after the length of time has been slept for.
        /// </remarks>
        /// <param name="ms">
        ///     The number of milliseconds to sleep the current thread for. Use <see cref="IndefiniteSleepThread" />
        ///     to sleep the thread indefinitely.
        /// </param>
        /// <returns>OK if slept successfully.</returns>
        [NoGC]
        public static SystemCallResults SleepThread(int ms)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.SleepThread, (uint)ms, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="ThreadId">The Id of the thread to wake. Must be owned by the current process.</param>
        /// <returns>OK if sucessfully woke the thread.</returns>
        [NoGC]
        public static SystemCallResults WakeThread(uint ThreadId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WakeThread, ThreadId, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        #endregion

        #region IPC

        #region Pipes

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe outpoint to register.</param>
        /// <param name="Subclass">The subclass of pipe outpoint to register.</param>
        /// <param name="MaxConnections">The maximum number of connections to the outpoint. Use -1 for unlimited.</param>
        /// <returns>OK if the outpoint was registered successfully.</returns>
        [NoGC]
        public static SystemCallResults RegisterPipeOutpoint(PipeClasses Class, PipeSubclasses Subclass,
            int MaxConnections)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterPipeOutpoint, (uint)Class, (uint)Subclass, (uint)MaxConnections,
                ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe outpoint to search for.</param>
        /// <param name="Subclass">The subclass of pipe outpoint to search for.</param>
        /// <param name="NumOutpoints">The number of matching outpoints found.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults GetNumPipeOutpoints(PipeClasses Class, PipeSubclasses Subclass,
            out int NumOutpoints)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetNumPipeOutpoints, (uint)Class, (uint)Subclass, 0, ref Return1, ref Return2,
                ref Return3, ref Return4);
            NumOutpoints = (int)Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe outpoint to search for.</param>
        /// <param name="Subclass">The subclass of pipe outpoint to search for.</param>
        /// <param name="RequestPtr">A pointer to a request structure, including a pre-allocated descriptors array.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults GetPipeOutpoints(PipeClasses Class, PipeSubclasses Subclass,
            PipeOutpointsRequest* RequestPtr)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetPipeOutpoints, (uint)Class, (uint)Subclass, (uint)RequestPtr, ref Return1,
                ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="OutProcessId">The Id of the process which owns the outpoint to connect to.</param>
        /// <param name="RequestPtr">A pointer to a request structure.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults CreatePipe(uint OutProcessId, CreatePipeRequest* RequestPtr)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.CreatePipe, OutProcessId, (uint)RequestPtr, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe to wait for.</param>
        /// <param name="Subclass">The subclass of pipe to wait for.</param>
        /// <param name="NewPipeId">The Id of the newly created pipe.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults WaitOnPipeCreate(PipeClasses Class, PipeSubclasses Subclass, out int NewPipeId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WaitOnPipeCreate, (uint)Class, (uint)Subclass, 0, ref Return1, ref Return2,
                ref Return3, ref Return4);
            NewPipeId = (int)Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Request">A pointer to a read pipe request structure.</param>
        /// <param name="BytesRead">The actual number of bytes read from the pipe.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults ReadPipe(ReadPipeRequest* Request, out int BytesRead)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.ReadPipe, (uint)Request, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            BytesRead = (int)Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Request">A pointer to a write pipe request structure.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults WritePipe(WritePipeRequest* Request)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WritePipe, (uint)Request, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        #endregion

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="TargetProcessId">The Id of the process to try to send the message to.</param>
        /// <param name="message1">The first value of the message.</param>
        /// <param name="message2">The second value of the message.</param>
        /// <returns>OK if the system call was successful.</returns>
        [NoGC]
        public static SystemCallResults SendMessage(uint TargetProcessId, uint message1, uint message2)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.SendMessage, TargetProcessId, message1, message2, ref Return1, ref Return2,
                ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        #endregion

        #region Memory

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="StartPhys">The physical address to request pages start at.</param>
        /// <param name="Count">The number of (contiguous) pages to request.</param>
        /// <param name="StartAddress">Out. The virtual address mapped pages start from.</param>
        /// <returns>OK if new pages mapped.</returns>
        [NoGC]
        public static SystemCallResults RequestPages(uint StartVirt, uint Count, out uint StartAddress)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RequestPages, 0xFFFFFFFF, StartVirt, Count, ref Return1, ref Return2, ref Return3,
                ref Return4);
            StartAddress = Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="StartPhys">The physical address to request pages start at.</param>
        /// <param name="StartVirt">The virtual address to request pages start at (or 0xFFFFFFFF for 'don't care').</param>
        /// <param name="Count">The number of (contiguous) pages to request.</param>
        /// <param name="StartAddress">Out. The virtual address mapped pages start from.</param>
        /// <returns>OK if new pages mapped.</returns>
        [NoGC]
        public static SystemCallResults RequestPages(uint StartVirt, uint StartPhys, uint Count, out uint StartAddress)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RequestPages, StartPhys, StartVirt, Count, ref Return1, ref Return2, ref Return3,
                ref Return4);
            StartAddress = Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="Count">The number of (contiguous) pages to request.</param>
        /// <param name="StartAddress">Out. The virtual address mapped pages start from.</param>
        /// <returns>OK if new pages mapped.</returns>
        [NoGC]
        public static SystemCallResults RequestPages(uint Count, out uint StartAddress)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RequestPages, 0xFFFFFFFF, 0xFFFFFFFF, Count, ref Return1, ref Return2, ref Return3,
                ref Return4);
            StartAddress = Return2;
            return (SystemCallResults)Return1;
        }

        /// <summary>
        ///     Performs the named system call.
        /// </summary>
        /// <param name="StartPhys">The physical address to request pages start at.</param>
        /// <param name="Count">The number of (contiguous) pages to request.</param>
        /// <param name="StartAddress">Out. The virtual address mapped pages start from.</param>
        /// <returns>OK if new pages mapped.</returns>
        [NoGC]
        public static SystemCallResults RequestPhysicalPages(uint StartPhys, uint Count, out uint StartAddress)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RequestPages, StartPhys, 0xFFFFFFFF, Count, ref Return1, ref Return2, ref Return3,
                ref Return4);
            StartAddress = Return2;
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults UnmapPages(uint StartVirtualAddress, uint Count)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.UnmapPages, StartVirtualAddress, Count, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults SharePages(uint StartVirtualAddress, uint Count, uint TargetProcessId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.SharePages, StartVirtualAddress, Count, TargetProcessId, ref Return1, ref Return2,
                ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults IsPhysicalAddressMapped(uint PhysicalAddress, out bool IsMapped)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.IsPhysicalAddressMapped, PhysicalAddress, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            IsMapped = Return2 != 0;
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults IsVirtualAddressMapped(uint VirtualAddress, out bool IsMapped)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.IsVirtualAddressMapped, VirtualAddress, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            IsMapped = Return2 != 0;
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults GetPhysicalAddress(uint VirtualAddress, out uint PhysicalAddress)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetPhysicalAddress, VirtualAddress, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            PhysicalAddress = Return2;
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults GetVirtualAddress(uint PhysicalAddress, out uint VirtualAddress)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetVirtualAddress, PhysicalAddress, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            VirtualAddress = Return2;
            return (SystemCallResults)Return1;
        }

        #endregion

        #region Semaphores

        [NoGC]
        public static SystemCallResults CreateSemaphore(int Limit, out int SemaphoreId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.CreateSemaphore, (uint)Limit, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            SemaphoreId = (int)Return2;
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults ShareSemaphore(int SemaphoreId, uint TargetProcessId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.ShareSemaphore, (uint)SemaphoreId, TargetProcessId, 0, ref Return1, ref Return2,
                ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults ReleaseSemaphore(int SemaphoreId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.ReleaseSemaphore, (uint)SemaphoreId, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults WaitSemaphore(int SemaphoreId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WaitSemaphore, (uint)SemaphoreId, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults SignalSemaphore(int SemaphoreId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.SignalSemaphore, (uint)SemaphoreId, 0, 0, ref Return1, ref Return2, ref Return3,
                ref Return4);
            return (SystemCallResults)Return1;
        }

        #endregion

        #region Time - Partial TODO

        //TODO: Set Time syscall

        [NoGC]
        public static SystemCallResults GetTime(out ulong UTCTime)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetTime, 0, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            UTCTime = ((ulong)Return3 << 32) | Return2;
            return (SystemCallResults)Return1;
        }

        [NoGC]
        public static SystemCallResults GetUpTime(out long UpTime)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetUpTime, 0, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            UpTime = ((long)Return3 << 32) | Return2;
            return (SystemCallResults)Return1;
        }

        #endregion

        #region Devices - Full TODO

        //TODO: Register Device syscall
        //TODO: Deregister Device syscall
        //TODO: Assign Device syscall
        //TODO: Release Device syscall

        #endregion
    }
}