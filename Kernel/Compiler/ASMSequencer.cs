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

namespace Kernel.Compiler
{
    /// <summary>
    /// Used to sequence (order) the ASM chunks into the order they will be outputted to the final .asm file.
    /// </summary>
    public class ASMSequencer
    {
        /// <summary>
        /// Stores a reference to the method to call for outputting an error message.
        /// </summary>
        public OutputErrorDelegate OutputError;
        /// <summary>
        /// Stores a reference to the method to call for outputting a standard message.
        /// </summary>
        public OutputMessageDelegate OutputMessage;
        /// <summary>
        /// Stores a reference to the method to call for outputting a warning message.
        /// </summary>
        public OutputWarningDelegate OutputWarning;

        /// <summary>
        /// The compiler settings to use.
        /// </summary>
        public Settings TheSettings;

        /// <summary>
        /// The ordered list of ASM chunks.
        /// </summary>
        public List<ASMChunk> SequencedASMChunks;

        /// <summary>
        /// Initialises a new ASM squencer with specified settings and handlers.
        /// </summary>
        /// <param name="aSettings">The settings to use.</param>
        /// <param name="anOutputError">The reference to the method to call to output an error message.</param>
        /// <param name="anOutputMessage">The reference to the method to call to output a standard message.</param>
        /// <param name="anOutputWarning">The reference to the method to call to output a warning message.</param>
        public ASMSequencer(Settings aSettings,
                            OutputErrorDelegate anOutputError,
                            OutputMessageDelegate anOutputMessage,
                            OutputWarningDelegate anOutputWarning)
        {
            TheSettings = aSettings;
            OutputError = anOutputError;
            OutputMessage = anOutputMessage;
            OutputWarning = anOutputWarning;
        }

        /// <summary>
        /// Takes the list of ASM chunks and executes the sequencing process on them. The ordered result is stored in SequencedASMChunks.
        /// </summary>
        /// <param name="inASMChunks">The ASM chunks to sequence.</param>
        /// <returns>True if sequencing was successful. Otherwise false.</returns>
        public bool Execute(List<ASMChunk> inASMChunks)
        {
            bool OK = true;

            SequencedASMChunks = new List<ASMChunk>();
            SequencedASMChunks.AddRange(inASMChunks);
            SequencedASMChunks.Sort((ASMChunk x, ASMChunk y) => { return GetOrder(x, y); });

            return OK;
        }
        /// <summary>
        /// Compares the two ASM chunks and returns a number indicating how to order them.
        /// </summary>
        /// <param name="a">The first ASM chunk.</param>
        /// <param name="b">The second ASM chunk.</param>
        /// <returns>Returns &lt; 0 if a goes before b. 0 if it doesn't matter. &gt; 0 if a goes after b.</returns>
        public int GetOrder(ASMChunk a, ASMChunk b)
        {
            return a.SequencePriority.CompareTo(b.SequencePriority);
        }
    }
}
