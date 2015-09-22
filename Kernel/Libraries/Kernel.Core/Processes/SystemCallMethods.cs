using System;

namespace Kernel.Core.Processes
{
    public static unsafe class SystemCallMethods
    {
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        static SystemCallMethods()
        {
        }

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
        public static SystemCallResults Sleep(int ms)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.Sleep, (uint)ms, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
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
        public static SystemCallResults Ping()
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.Semaphore, 0, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SystemCallResults)Return1;
        }











        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static uint RequestPages(int numPages)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(SystemCallNumbers.RequestPages, (uint)numPages, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return Return1;
        }
    }
}
