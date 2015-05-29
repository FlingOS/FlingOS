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
