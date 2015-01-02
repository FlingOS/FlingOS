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
    /// A delegate for a method that can output an error message.
    /// </summary>
    /// <param name="ex">The exception to output.</param>
    public delegate void OutputErrorDelegate(Exception ex);
    /// <summary>
    /// A delegate for a method that can output an standard message.
    /// </summary>
    /// <param name="message">The message to output.</param>
    public delegate void OutputMessageDelegate(string message);
    /// <summary>
    /// A delegate for a method that can output an warning message.
    /// </summary>
    /// <param name="ex">The warning to output.</param>
    public delegate void OutputWarningDelegate(Exception ex);
}
