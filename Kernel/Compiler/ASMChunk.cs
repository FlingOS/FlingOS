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
