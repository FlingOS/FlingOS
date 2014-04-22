using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kernel.Debug.Data;

namespace Kernel.Debug.Debugger
{
    /// <summary>
    /// Represents an assembler file
    /// </summary>
    public class ASMFile
    {
        /// <summary>
        /// The file path to the ASM file to open.
        /// </summary>
        private string filePath;

        /// <summary>
        /// All the ASM lines read from the ASM file split by newline characters.
        /// </summary>
        public string ASM;
        
        /// <summary>
        /// Initialises a new ASMFile instance with the specified path to the ASm file to open.
        /// </summary>
        /// <param name="aFilePath">The path to the ASM file to open.</param>
        public ASMFile(string aFilePath)
        {
            filePath = aFilePath;
            Open();
        }

        /// <summary>
        /// Opens the ASM file and scans it for important information such as label positions
        /// </summary>
        private void Open()
        {
            ASM = File.ReadAllText(filePath);
        }

        /// <summary>
        /// Gets the assembler text for the specified method.
        /// </summary>
        /// <param name="method">The method to get the ASM for.</param>
        /// <returns>The ASM text.</returns>
        public string GetMethodASM(DB_Method method)
        {
            string result = null;

            result = ASM.Substring(method.ASMStartPos, method.ASMEndPos - method.ASMStartPos);

            return result;
        }

        /// <summary>
        /// Gets the assembler text for the specified IL Op.
        /// </summary>
        /// <param name="anILOp">The IL Op to get the ASM for.</param>
        /// <returns>The ASM text.</returns>
        public string GetILOpASM(DB_ILOpInfo anILOp)
        {
            string result = null;

            string methodASM = GetMethodASM(anILOp.DB_Method);
            result = methodASM.Substring(anILOp.ASMStartPos, anILOp.ASMEndPos - anILOp.ASMStartPos);

            return result;
        }
    }
}
