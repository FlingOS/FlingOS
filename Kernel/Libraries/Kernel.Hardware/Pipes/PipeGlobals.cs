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

namespace Kernel.Pipes
{
    /// <summary>
    /// Global, constant values used in the pipes subsystem.
    /// </summary>
    public static class PipeConstants
    {
        /// <summary>
        /// Value indicating an outpoint accepts an unlimited number of connections.
        /// </summary>
        public const int UnlimitedConnections = -1;
    }
    /// <summary>
    /// The available pipe classes.
    /// </summary>
    public enum PipeClasses : uint
    {
        /// <summary>
        /// Standard pipe used for sending and receiving string data.
        /// </summary>
        Standard
    }
    /// <summary>
    /// The available pipe subclasses.
    /// </summary>
    public enum PipeSubclasses : uint
    {
        /// <summary>
        /// Standard out pipe for sending output data out of a process (e.g. console output to the Window Manager).
        /// </summary>
        Standard_Out,
        /// <summary>
        /// Standard in pipe for receiving input data into a process (e.g. key chars from the Window Manager).
        /// </summary>
        Standard_In
    }
}
