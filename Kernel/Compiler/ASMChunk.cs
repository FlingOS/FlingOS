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
using Kernel.Debug.Data;

namespace Kernel.Compiler
{
    /// <summary>
    /// Represents a section of assembler  (e.g. ASM for a method) to be included in the final .asm file
    /// </summary>
    public class ASMChunk
    {
        /// <summary>
        /// The priority of the ASMChunk - used in sequencing the final assembler code.
        /// </summary>
        public long SequencePriority
        {
            get;
            set;
        }

        /// <summary>
        /// The actual assembler code of the ASM chunk.
        /// </summary>
        public StringBuilder ASM
        {
            get;
            set;
        }

        /// <summary>
        /// The debug database method that relates to this ASM chunk
        /// </summary>
        public DB_Method DBMethod;

        /// <summary>
        /// Initialises a new ASM with no text.
        /// </summary>
        public ASMChunk()
        {
            ASM = new StringBuilder();
        }

        /// <summary>
        /// Outputs the assembler chunk to the specified text stream.
        /// </summary>
        /// <param name="writer">The text stream to output to.</param>
        public void Output(System.IO.StreamWriter writer)
        {
            //Do not add extra ouptut stuff here! See caller of this method

            writer.Write(ASM);
        }
    }
}
