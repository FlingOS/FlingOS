using System;

namespace Kernel.Core.Processes
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
        
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static SystemCallResults StartThread(Kernel.Hardware.Processes.ThreadStartMethod startMethod)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.StartThread, (uint)Utilities.ObjectUtilities.GetHandle(startMethod), 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }
        [Drivers.Compiler.Attributes.NoDebug]
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
        
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
        [Drivers.Compiler.Attributes.NoDebug]
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
