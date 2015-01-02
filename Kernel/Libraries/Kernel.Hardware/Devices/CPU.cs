#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Devices
{
    /// <summary>
    /// Represents a CPU in the machine.
    /// </summary>
    public abstract class CPU : Device
    {
        /// <summary>
        /// Halts the CPU (e.g. using x86 hlt instruction)
        /// </summary>
        public abstract void Halt();

        /// <summary>
        /// The default CPU.
        /// </summary>
        public static CPU Default;
        /// <summary>
        /// Initialises the default CPU.
        /// </summary>
        /// <remarks>
        /// Currently just straight up initialises the x86 CPU class. Should actually detect,
        /// either at compile time or runtime, which CPU artchitecture the OS is being run on.
        /// </remarks>
        public static void InitDefault()
        {
            CPUs.CPUx86_32.Init();
            Default = CPUs.CPUx86_32.TheCPU;
        }
    }
}
