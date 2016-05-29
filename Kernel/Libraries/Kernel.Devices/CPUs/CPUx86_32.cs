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

namespace Kernel.Devices.CPUs
{
    /// <summary>
    ///     Represents an x86 32-bit CPU.
    /// </summary>
    public class CPUx86_32 : CPU
    {
#if x86
        static CPUx86_32()
        {
            Default = new CPUx86_32();
        }
#endif

        /// <summary>
        ///     Halts the CPU using the Hlt instruction.
        /// </summary>
        [PluggedMethod(ASMFilePath = @"ASM\CPUs\CPUx86_32\Halt")]
        public override void Halt()
        {
        }
    }
}