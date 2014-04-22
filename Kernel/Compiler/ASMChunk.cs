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
