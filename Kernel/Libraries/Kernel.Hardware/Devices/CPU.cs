#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
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
