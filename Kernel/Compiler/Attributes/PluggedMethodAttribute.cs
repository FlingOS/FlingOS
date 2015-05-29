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

namespace Kernel.Compiler
{
    /// <summary>
    /// <para>
    /// Indicates a method is plugged. 
    /// </para>
    /// <para>
    /// Use the ASMFilePath without any file extensions to specify the path 
    /// to the ASM file that contains the plug. The extension is automatically
    /// added based on the target architecture. E.g. "PlugFileName.x86_32.asm".
    /// See remarks for more info.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute must be applied to a method which is plugged to tell
    /// the compiler where the ASM for the plug can be found.
    /// </para>
    /// <para>
    /// The file name should be a relative path, relative to the build directory.
    /// The file name extension is added automatically so different ASM plugs
    /// can be used for different target architectures.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PluggedMethodAttribute : Attribute
    {
        /// <summary>
        /// The relative path to the ASM plug file, excluding extension(s).
        /// </summary>
        /// <remarks>
        /// Please see class remarks for details.
        /// </remarks>
        public string ASMFilePath
        {
            get;
            set;
        }
    }
}
