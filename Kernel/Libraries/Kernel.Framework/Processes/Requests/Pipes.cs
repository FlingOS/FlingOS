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

namespace Kernel.Framework.Processes.Requests.Pipes
{
    /// <summary>
    ///     Global, constant values used in the pipes subsystem.
    /// </summary>
    public static class PipeConstants
    {
        /// <summary>
        ///     Value indicating an outpoint accepts an unlimited number of connections.
        /// </summary>
        public const int UnlimitedConnections = -1;
    }

    /// <summary>
    ///     The available pipe classes.
    /// </summary>
    public enum PipeClasses : uint
    {
        /// <summary>
        ///     Standard pipe used for sending and receiving string data.
        /// </summary>
        Standard,
        Storage,
        File,
        PCI
    }

    /// <summary>
    ///     The available pipe subclasses.
    /// </summary>
    public enum PipeSubclasses : uint
    {
        /// <summary>
        ///     Standard out pipe for sending output data out of a process (e.g. console output to the Window Manager).
        /// </summary>
        Standard_Out,

        /// <summary>
        ///     Standard in pipe for receiving input data into a process (e.g. key chars from the Window Manager).
        /// </summary>
        Standard_In,
        Storage_Command,
        Storage_Data_In,
        Storage_Data_Out,
        File_Command,
        File_Data_In,
        File_Data_Out,
        PCI_PortAccess_In,
        PCI_PortAccess_Out
    }

    /// <summary>
    ///     Represents a Create Pipe request (used in a system call).
    /// </summary>
    public struct CreatePipeRequest
    {
        /// <summary>
        ///     The size of the buffer to use in the new pipe.
        /// </summary>
        public int BufferSize;

        /// <summary>
        ///     The class of pipe to create.
        /// </summary>
        public PipeClasses Class;

        /// <summary>
        ///     The subclass of pipe to create.
        /// </summary>
        public PipeSubclasses Subclass;

        /// <summary>
        ///     The result of the create request.
        /// </summary>
        public PipeDescriptor Result;
    }

    /// <summary>
    ///     Represents a Wait on Pipe Create request (used in a system call).
    /// </summary>
    public struct WaitOnPipeCreateRequest
    {
        /// <summary>
        ///     The class of pipe to wait for.
        /// </summary>
        public PipeClasses Class;

        /// <summary>
        ///     The subclass of pipe to wait for.
        /// </summary>
        public PipeSubclasses Subclass;

        /// <summary>
        ///     The result of the wait request.
        /// </summary>
        public PipeDescriptor Result;
    }

    /// <summary>
    ///     Represents a Read Pipe request (used in a system call).
    /// </summary>
    public unsafe struct ReadPipeRequest
    {
        /// <summary>
        ///     The Id of the pipe to read.
        /// </summary>
        public int PipeId;

        /// <summary>
        ///     A pointer to the buffer to read data into.
        /// </summary>
        public byte* OutBuffer;

        /// <summary>
        ///     The offset in the outBuffer to start writing at.
        /// </summary>
        public int Offset;

        /// <summary>
        ///     The maximum length of data to read.
        /// </summary>
        public int Length;

        /// <summary>
        ///     Whether the request should be blocking or non-blocking.
        /// </summary>
        public bool Blocking;

        /// <summary>
        ///     Whether the request has been aborted or not.
        /// </summary>
        public bool Aborted;
    }

    /// <summary>
    ///     Represents a Write Pipe request (used in a system call).
    /// </summary>
    public unsafe struct WritePipeRequest
    {
        /// <summary>
        ///     The Id of the pipe to read.
        /// </summary>
        public int PipeId;

        /// <summary>
        ///     A pointer to the buffer to write data from.
        /// </summary>
        public byte* InBuffer;

        /// <summary>
        ///     The offset in the inBuffer to start reading from.
        /// </summary>
        public int Offset;

        /// <summary>
        ///     The exact length of data to write.
        /// </summary>
        public int Length;

        /// <summary>
        ///     Whether the request should be blocking or non-blocking.
        /// </summary>
        public bool Blocking;

        /// <summary>
        ///     Whether the request has been aborted or not.
        /// </summary>
        public bool Aborted;
    }

    /// <summary>
    ///     Describes a pipe. Used to pass pipe information back from a system call.
    /// </summary>
    public struct PipeDescriptor
    {
        /// <summary>
        ///     The Id of the pipe.
        /// </summary>
        public int Id;

        public PipeClasses Class;
        public PipeSubclasses Subclass;

        public int BufferSize;

        public uint InpointProcessId;
        public uint OutpointProcessId;
    }


    /// <summary>
    ///     Describes an outpoint. Used to return outpoint data from a system call.
    /// </summary>
    public struct PipeOutpointDescriptor
    {
        /// <summary>
        ///     The Id of the process which owns the outpoint.
        /// </summary>
        public uint ProcessId;
    }

    /// <summary>
    ///     Represents a Get Outpoints request (used in a system call).
    /// </summary>
    public unsafe struct PipeOutpointsRequest
    {
        /// <summary>
        ///     The maximum number of outpoint descriptors to return.
        /// </summary>
        public int MaxDescriptors;

        /// <summary>
        ///     The requested outpoint descriptors.
        /// </summary>
        public PipeOutpointDescriptor* Outpoints;
    }
}