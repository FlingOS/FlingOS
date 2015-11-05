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
    public static unsafe class SystemCallMethods
    {
        public const int IndefiniteSleepThread = Hardware.Processes.Thread.IndefiniteSleep;

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\SystemCallMethods")]
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
