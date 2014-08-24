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

namespace Kernel.Hardware.CPUs
{
    /// <summary>
    /// Represents an x86 32-bit CPU.
    /// </summary>
    [Compiler.PluggedClass]
    public class CPUx86_32 : Devices.CPU
    {
        /// <summary>
        /// Halts the CPU using the Hlt instruction.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\CPUs\CPUx86_32\Halt")]
        public override void Halt()
        {
        }

        /// <summary>
        /// The main x86 CPU instance.
        /// </summary>
        public static CPUx86_32 TheCPU;
        /// <summary>
        /// Initialises the main x86 CPU instance.
        /// </summary>
        public static void Init()
        {
            if (TheCPU == null)
            {
                TheCPU = new CPUx86_32();
            }
        }
    }
}
