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

using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Pipes;

namespace Kernel.Pipes
{
    /// <summary>
    ///     Represents a pipe inpoint. Used only within the core OS.
    /// </summary>
    /// <remarks>
    ///     For inpoints used by processes, see <see cref="BasicInpoint" />.
    /// </remarks>
    public class PipeInpoint : Object
    {
        /// <summary>
        ///     The class of pipe connected to the inpoint.
        /// </summary>
        public PipeClasses Class;

        /// <summary>
        ///     The Id of the process which owns the inpoint.
        /// </summary>
        public uint ProcessId;

        /// <summary>
        ///     The subclass of pipe connected to the inpoint.
        /// </summary>
        public PipeSubclasses Subclass;

        /// <summary>
        ///     Initialises a new inpoint instance.
        /// </summary>
        /// <param name="OwnerProcessId">The process which owns the inpoint.</param>
        /// <param name="pipeClass">The class of pipe connected to the inpoint.</param>
        /// <param name="pipeSubclass">The subclass of pipe connected to the inpoint.</param>
        public PipeInpoint(uint OwnerProcessId, PipeClasses pipeClass, PipeSubclasses pipeSubclass)
        {
            ProcessId = OwnerProcessId;
            Class = pipeClass;
            Subclass = pipeSubclass;
        }
    }
}