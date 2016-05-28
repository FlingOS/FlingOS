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
using FlingOops.Interfaces;

namespace FlingOops
{
    public static class Kernel
    {
        [PluggedMethod(ASMFilePath = "ASM\\Kernel")]
        [SequencePriority(Priority = long.MinValue)]
        public static void Boot()
        {
        }

        [MainMethod]
        [NoGC]
        public static void Main()
        {
#if MIPS
            FlingOops.MIPS.CI20.Kernel.Start();
#elif x86
            x86.Kernel.Start();
#endif

            BasicConsole.Init();
            BasicConsole.WriteLine("Kernel executing...");

            InterfaceTests.RunTests();
            CompilerTests.RunTests();

#if MIPS
            FlingOops.MIPS.CI20.Kernel.End();
#elif x86
            x86.Kernel.End();
#endif
        }

        [CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }
    }
}