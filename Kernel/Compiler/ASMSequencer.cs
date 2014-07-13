#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
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
