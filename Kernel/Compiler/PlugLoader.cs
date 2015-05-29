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

namespace Kernel.Compiler
{
    /// <summary>
    /// Used to load plugs ASM for a specified target architecture from plug files.
    /// </summary>
    public static class PlugLoader
    {
        /// <summary>
        /// Loads the plug's ASM from the specified file path (which should not included target architecture or file extension)
        /// </summary>
        /// <param name="anASMPlugFilePath">The file path to the plug to load.</param>
        /// <param name="aSettings">The current compiler settings - used to get the target architecture.</param>
        /// <returns>The ASM for the plug or null if loading failed.</returns>
        /// <exception cref="System.Exception">
        /// Thrown if the specified plug file fails to load.
        /// </exception>
        public static string LoadPlugASM(string anASMPlugFilePath, Settings aSettings)
        {
            string result = null;

            string fullASMPlugPath = GetFullASMPlugFilePath(anASMPlugFilePath, aSettings);
            if (File.Exists(fullASMPlugPath))
            {
                result = File.ReadAllText(fullASMPlugPath);
            }

            if(result == null)
            {
                throw new Exception("Failed to load plug! Path=" + fullASMPlugPath);
            }

            result = result.Replace("%KERNEL_MAIN_METHOD%", aSettings[Settings.KernelMainMethodKey]);
            result = result.Replace("%KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%", aSettings[Settings.CallStaticConstructorsMethodKey]);

            return result;
        }
        /// <summary>
        /// Gets the full (usable) path to a specified plug.
        /// </summary>
        /// <param name="anASMPlugFilePath">The file path to the plug to load.</param>
        /// <param name="aSettings">The current compiler settings - used to get the target architecture.</param>
        /// <returns>The full (usable) file path to the specified plug.</returns>
        public static string GetFullASMPlugFilePath(string anASMPlugFilePath, Settings aSettings)
        {
            string result = anASMPlugFilePath;

            result += "." + aSettings[Settings.TargetArchitectureKey] + ".asm";

            return Path.GetFullPath(result);
        }
    }
}
