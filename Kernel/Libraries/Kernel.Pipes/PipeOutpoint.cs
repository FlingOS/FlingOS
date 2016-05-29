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
using Kernel.Framework.Collections;
using Kernel.Framework.Processes.Requests.Pipes;

namespace Kernel.Pipes
{
    /// <summary>
    ///     Represents a pipe outpoint. Used only within the core OS.
    /// </summary>
    /// <remarks>
    ///     For outpoints used by processes, see <see cref="BasicOutpoint" />.
    /// </remarks>
    public class PipeOutpoint : Object
    {
        /// <summary>
        ///     The class of pipe which can connect to the outpoint.
        /// </summary>
        public PipeClasses Class;

        /// <summary>
        ///     The maximum number of connections allowed to the outpoint. Also see
        ///     <see cref="PipeConstants.UnlimitedConnections" />.
        /// </summary>
        /// <seealso cref="PipeConstants.UnlimitedConnections" />
        public int MaxConnections;

        /// <summary>
        ///     The number of pipes currently connected to the outpoint.
        /// </summary>
        public int NumConnections;

        /// <summary>
        ///     The Id of the process which owns the outpoint.
        /// </summary>
        public uint ProcessId;

        /// <summary>
        ///     The subclass of pipe which can connect to the outpoint.
        /// </summary>
        public PipeSubclasses Subclass;

        /// <summary>
        ///     List of threads waiting for a pipe to connect to the outpoint.
        /// </summary>
        public UInt64List WaitingThreads;

        /// <summary>
        ///     Initialises a new instance of an outpoint.
        /// </summary>
        /// <param name="OwnerProcessId">The Id of the process which owns the outpoint.</param>
        /// <param name="pipeClass">The class of pipe which can connect to the outpoint.</param>
        /// <param name="pipeSubclass">The subclass of pipe which can connect to the outpoint.</param>
        /// <param name="MaximumConnections">
        ///     The maximum number of connections allowed to the outpoint. Also see
        ///     <see cref="PipeConstants.UnlimitedConnections" />.
        /// </param>
        /// <seealso cref="PipeConstants.UnlimitedConnections" />
        public PipeOutpoint(uint OwnerProcessId, PipeClasses pipeClass, PipeSubclasses pipeSubclass,
            int MaximumConnections)
        {
            ProcessId = OwnerProcessId;
            Class = pipeClass;
            Subclass = pipeSubclass;
            MaxConnections = MaximumConnections;

            WaitingThreads = new UInt64List();
        }
    }
}