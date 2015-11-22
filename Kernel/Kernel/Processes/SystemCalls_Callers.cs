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

namespace Kernel.Processes
{
    /// <summary>
    /// Contains callers and handlers for system calls.
    /// </summary>
    public static unsafe partial class SystemCalls
    {
        /// <summary>
        /// The value used to indicate a thread should sleep indefinitely.
        /// </summary>
        public const int IndefiniteSleepThread = Hardware.Processes.Thread.IndefiniteSleep;

        /// <summary>
        /// Performs the specified system call.
        /// </summary>
        /// <param name="callNumber">The number of the system call to make.</param>
        /// <param name="Param1">Param value 1 of the system call.</param>
        /// <param name="Param2">Param value 2 of the system call.</param>
        /// <param name="Param3">Param value 3 of the system call.</param>
        /// <param name="Return1">Return value 1 of the system call - always a System Call Response.</param>
        /// <param name="Return2">Return value 2 of the system call.</param>
        /// <param name="Return3">Return value 3 of the system call.</param>
        /// <param name="Return4">Return value 4 of the system call.</param>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\SystemCalls")]
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

        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the ISR to register for.</param>
        /// <returns>OK if successfully registered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterISRHandler(uint ISRNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterISRHandler, ISRNum, 0xFFFFFFFF, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the ISR to register for.</param>
        /// <param name="handler">The handler function to call for all handled ISR events.</param>
        /// <returns>OK if successfully registered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterISRHandler(uint ISRNum, Hardware.Processes.ISRHanderDelegate handler)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterISRHandler, ISRNum, (uint)Utilities.ObjectUtilities.GetHandle(handler), 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the ISR to deregister.</param>
        /// <returns>OK if successfully deregistered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults DeregisterISRHandler(uint ISRNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.DeregisterISRHandler, ISRNum, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the IRQ to register for.</param>
        /// <returns>OK if successfully registered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterIRQHandler(uint IRQNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterIRQHandler, IRQNum, 0xFFFFFFFF, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the IRQ to register for.</param>
        /// <param name="handler">The handler function to call for all handled IRQ events.</param>
        /// <returns>OK if successfully registered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterIRQHandler(uint IRQNum, Hardware.Processes.IRQHanderDelegate handler)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterIRQHandler, IRQNum, (uint)Utilities.ObjectUtilities.GetHandle(handler), 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the IRQ to deregister.</param>
        /// <returns>OK if successfully deregistered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults DeregisterIRQHandler(uint IRQNum)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.DeregisterIRQHandler, IRQNum, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the system call to register for.</param>
        /// <returns>OK if successfully registered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterSyscallHandler(SystemCallNumbers syscall)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterSyscallHandler, (uint)syscall, 0xFFFFFFFF, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the system call to register for.</param>
        /// <param name="handler">The handler function to call for all handled system call events.</param>
        /// <returns>OK if successfully registered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterSyscallHandler(SystemCallNumbers syscall, Hardware.Processes.SyscallHanderDelegate handler)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterSyscallHandler, (uint)syscall, (uint)Utilities.ObjectUtilities.GetHandle(handler), 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="ISRNum">The number of the system call to deregister.</param>
        /// <returns>OK if successfully deregistered.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults DeregisterSyscallHandler(SystemCallNumbers syscall)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.DeregisterSyscallHandler, (uint)syscall, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="startMethod">The main method of the thread.</param>
        /// <param name="NewThreadId">OUT: The Id of the new thread.</param>
        /// <returns>OK if the new thread was started.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults StartThread(Hardware.Processes.ThreadStartMethod startMethod, out uint NewThreadId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.StartThread, (uint)Utilities.ObjectUtilities.GetHandle(startMethod), 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            NewThreadId = (uint)Return2;
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <remarks>
        /// This method will, if the call was successful, return after the length of time has been slept for.
        /// </remarks>
        /// <param name="ms">The number of milliseconds to sleep the current thread for. Use <see cref="IndefiniteSleepThread"/> to sleep the thread indefinitely.</param>
        /// <returns>OK if slept successfully.</returns>
        [Drivers.Compiler.Attributes.NoGC]
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
        /// Performs the named system call.
        /// </summary>
        /// <param name="ThreadId">The Id of the thread to wake. Must be owned by the current process.</param>
        /// <returns>OK if sucessfully woke the thread.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults WakeThread(uint ThreadId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WakeThread, ThreadId, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe outpoint to register.</param>
        /// <param name="Subclass">The subclass of pipe outpoint to register.</param>
        /// <param name="MaxConnections">The maximum number of connections to the outpoint. Use -1 for unlimited.</param>
        /// <returns>OK if the outpoint was registered successfully.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults RegisterPipeOutpoint(Pipes.PipeClasses Class, Pipes.PipeSubclasses Subclass, int MaxConnections)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RegisterPipeOutpoint, (uint)Class, (uint)Subclass, (uint)MaxConnections, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe outpoint to search for.</param>
        /// <param name="Subclass">The subclass of pipe outpoint to search for.</param>
        /// <param name="NumOutpoints">The number of matching outpoints found.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults GetNumPipeOutpoints(Pipes.PipeClasses Class, Pipes.PipeSubclasses Subclass, out int NumOutpoints)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetNumPipeOutpoints, (uint)Class, (uint)Subclass, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            NumOutpoints = (int)Return2;
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe outpoint to search for.</param>
        /// <param name="Subclass">The subclass of pipe outpoint to search for.</param>
        /// <param name="RequestPtr">A pointer to a request structure, including a pre-allocated descriptors array.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults GetPipeOutpoints(Pipes.PipeClasses Class, Pipes.PipeSubclasses Subclass, Pipes.PipeOutpointsRequest* RequestPtr)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.GetPipeOutpoints, (uint)Class, (uint)Subclass, (uint)RequestPtr, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="OutProcessId">The Id of the process which owns the outpoint to connect to.</param>
        /// <param name="RequestPtr">A pointer to a request structure.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults CreatePipe(uint OutProcessId, Pipes.CreatePipeRequest* RequestPtr)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.CreatePipe, OutProcessId, (uint)RequestPtr, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="Class">The class of pipe to wait for.</param>
        /// <param name="Subclass">The subclass of pipe to wait for.</param>
        /// <param name="NewPipeId">The Id of the newly created pipe.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults WaitOnPipeCreate(Pipes.PipeClasses Class, Pipes.PipeSubclasses Subclass, out int NewPipeId)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WaitOnPipeCreate, (uint)Class, (uint)Subclass, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            NewPipeId = (int)Return2;
            return (SystemCallResults)Return1;
        }
        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="Request">A pointer to a read pipe request structure.</param>
        /// <param name="BytesRead">The actual number of bytes read from the pipe.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults ReadPipe(Pipes.ReadPipeRequest* Request, out int BytesRead)
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
        /// Performs the named system call.
        /// </summary>
        /// <param name="Request">A pointer to a write pipe request structure.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults WritePipe(Pipes.WritePipeRequest* Request)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.WritePipe, (uint)Request, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }

        /// <summary>
        /// Performs the named system call.
        /// </summary>
        /// <param name="TargetProcessId">The Id of the process to try to send the message to.</param>
        /// <param name="message1">The first value of the message.</param>
        /// <param name="message2">The second value of the message.</param>
        /// <returns>OK if the system call was successful.</returns>
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults SendMessage(uint TargetProcessId, uint message1, uint message2)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.SendMessage, TargetProcessId, message1, message2, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }








        //[Drivers.Compiler.Attributes.NoDebug]
        //[Drivers.Compiler.Attributes.NoGC]
        //public static uint RequestPages(int numPages)
        //{
        //    uint Return1 = 0;
        //    uint Return2 = 0;
        //    uint Return3 = 0;
        //    uint Return4 = 0;
        //    Call(SystemCallNumbers.RequestPages, (uint)numPages, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
        //    return Return1;
        //}
    }
}
