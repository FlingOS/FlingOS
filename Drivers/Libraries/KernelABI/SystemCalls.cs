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

//using Kernel.Shared;

namespace KernelABI
{
    public static class SystemCalls
    {
        public delegate void ThreadStartMethod();

        /*
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\SystemCalls")]
        public static void Call(Kernel.Shared.SystemCalls callNumber,
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
        public static void Sleep(uint ms)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(Kernel.Shared.SystemCalls.Sleep, ms, 0, 0, ref Return1, ref Return2, ref Return3, ref Return4);
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void PlayNote(MusicalNote note, MusicalNoteValue duration, uint bpm)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(Kernel.Shared.SystemCalls.PlayNote, (uint)note, (uint)duration, bpm, ref Return1, ref Return2, ref Return3, ref Return4);
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static SemaphoreResponses Semaphore_Allocate(int limit, ref int Id)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(Kernel.Shared.SystemCalls.Semaphore, (uint)SemaphoreRequests.Allocate, (uint)Id, (uint)limit, ref Return1, ref Return2, ref Return3, ref Return4);
            Id = (int)Return2;
            return (SemaphoreResponses)Return1;
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static SemaphoreResponses Semaphore_Wait(int Id)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(Kernel.Shared.SystemCalls.Semaphore, (uint)SemaphoreRequests.Wait, (uint)Id, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SemaphoreResponses)Return1;
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static SemaphoreResponses Semaphore_Signal(int Id)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(Kernel.Shared.SystemCalls.Semaphore, (uint)SemaphoreRequests.Signal, (uint)Id, 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (SemaphoreResponses)Return1;
        }
        
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe ThreadResponses Thread_Create(ThreadStartMethod startMethod)
        {
            uint Return1 = 0;
            uint Return2 = 0;
            uint Return3 = 0;
            uint Return4 = 0;
            Call(Kernel.Shared.SystemCalls.Thread, (uint)ThreadRequests.Create, (uint)(ObjectUtilities.GetHandle(startMethod)), 0, ref Return1, ref Return2, ref Return3, ref Return4);
            return (ThreadResponses)Return1;
        }
        */
    }
}