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
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes.Exceptions;
using Kernel.Utilities;

namespace Kernel.Pipes
{
    /// <summary>
    ///     Represents a basic inpoint for any pipe class.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike an outpoint, an inpoint cannot exist without being connected to an outpoint and can only have one
    ///         connection.
    ///     </para>
    ///     <para>
    ///         An inpoint can only be created for existing outpoints. The principle driving this is that processes should only
    ///         ever
    ///         accept connections from offered openly offered points. They should not try to accept a connection from any
    ///         random
    ///         process that comes along and asks. In other words, a process offers data and another accepts. Instead of a
    ///         process
    ///         asking for data and another fulfilling the request.
    ///     </para>
    /// </remarks>
    public abstract unsafe class BasicInpoint : Object
    {
        /// <summary>
        ///     The Id of the process which owns the outpoint this inpoint is connected to.
        /// </summary>
        public uint OutProcessId { get; protected set; }

        /// <summary>
        ///     The Id of the pipe this inpoint is connected to.
        /// </summary>
        public int PipeId { get; protected set; }

        /// <summary>
        ///     The class of the pipe this inpoint is connected to.
        /// </summary>
        public PipeClasses Class { get; protected set; }

        /// <summary>
        ///     The subclass of the pipe this inpoint is connected to.
        /// </summary>
        public PipeSubclasses Subclass { get; protected set; }

        /// <summary>
        ///     The size of the buffer used within the core OS.
        /// </summary>
        public int BufferSize { get; protected set; }

        /// <summary>
        ///     Creates and connects a new pipe to the specified target process.
        /// </summary>
        /// <param name="anOutProcessId">The target process to connect to.</param>
        /// <param name="aClass">The class of pipe to create.</param>
        /// <param name="aSubclass">The subclass of pipe to create.</param>
        /// <param name="aBufferSize">The size of buffer to use within the core OS.</param>
        public BasicInpoint(uint anOutProcessId, PipeClasses aClass, PipeSubclasses aSubclass, int aBufferSize)
        {
            OutProcessId = anOutProcessId;
            Class = aClass;
            Subclass = aSubclass;
            BufferSize = aBufferSize;

            CreatePipeRequest* RequestPtr =
                (CreatePipeRequest*)
                    Heap.AllocZeroed((uint)sizeof(CreatePipeRequest), "BasicInPipe : Alloc CreatePipeRequest");
            if (RequestPtr != null)
            {
                try
                {
                    RequestPtr->BufferSize = aBufferSize;
                    RequestPtr->Class = aClass;
                    RequestPtr->Subclass = aSubclass;

                    SystemCallResults SysCallResult = SystemCalls.CreatePipe(anOutProcessId, RequestPtr);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            BasicConsole.WriteLine("BasicInPipe > CreatePipe: Unhandled!");
                            break;
                        case SystemCallResults.Fail:
                            BasicConsole.WriteLine("BasicInPipe > CreatePipe: Failed!");
                            break;
                        case SystemCallResults.OK:
                            //BasicConsole.WriteLine("BasicInPipe > CreatePipe: Succeeded.");
                            PipeId = RequestPtr->Result.Id;

                            //BasicConsole.Write("BasicInPipe > CreatePipe: New pipe id = ");
                            //BasicConsole.WriteLine(PipeId);
                            break;
                        default:
                            BasicConsole.WriteLine("BasicInPipe > CreatePipe: Unexpected system call result!");
                            break;
                    }
                }
                finally
                {
                    Heap.Free(RequestPtr);
                }
            }
            else
            {
                ExceptionMethods.Throw(new ArgumentException("BasicInPipe : Couldn't allocate memory to create pipe!"));
                //BasicConsole.WriteLine("BasicInPipe > RequestPtr null! No memory allocated.");
            }
        }

        /// <summary>
        ///     Reads up to the specified length of data into the specified buffer at the specified offset in the buffer.
        /// </summary>
        /// <param name="Data">The buffer to read into.</param>
        /// <param name="Offset">The offset in the buffer to write data to.</param>
        /// <param name="Length">The maximum length of data to read.</param>
        /// <param name="Blocking">Whether the read should be blocking or non-blocking.</param>
        /// <returns>The actual number of bytes read.</returns>
        public int Read(byte[] Data, int Offset, int Length, bool Blocking)
        {
            int BytesRead = 0;

            ReadPipeRequest* ReadPipeRequestPtr =
                (ReadPipeRequest*)
                    Heap.AllocZeroed((uint)sizeof(ReadPipeRequest), "BasicInPipe : Alloc ReadPipeRequest");
            try
            {
                if (ReadPipeRequestPtr != null)
                {
                    ReadPipeRequestPtr->PipeId = PipeId;
                    ReadPipeRequestPtr->Offset = Offset;
                    ReadPipeRequestPtr->Length = Math.Min(Data.Length - Offset, Length);
                    ReadPipeRequestPtr->OutBuffer = (byte*)ObjectUtilities.GetHandle(Data) + Array.FieldsBytesSize;
                    ReadPipeRequestPtr->Blocking = Blocking;

                    SystemCallResults SysCallResult = SystemCalls.ReadPipe(ReadPipeRequestPtr, out BytesRead);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Unhandled!");
                            ExceptionMethods.Throw(
                                new RWUnhandledException("BasicInPipe : Read Pipe unexpected unhandled!"));
                            break;
                        case SystemCallResults.Fail:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Failed!");
                            if (Blocking)
                            {
                                ExceptionMethods.Throw(
                                    new RWFailedException("BasicInPipe : Read Pipe unexpected failed! (Blocking call)"));
                            }
                            else
                            {
                                ExceptionMethods.Throw(
                                    new RWFailedException("BasicInPipe : Read Pipe failed. (Non-blocking call)"));
                            }
                            break;
                        case SystemCallResults.OK:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Succeeded.");
                            break;
                        default:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Unexpected system call result!");
                            ExceptionMethods.Throw(new RWUnhandledException("BasicInPipe : Read Pipe unexpected result!"));
                            break;
                    }
                }
                else
                {
                    ExceptionMethods.Throw(
                        new ArgumentException("BasicInPipe : Couldn't allocate memory to read from pipe!"));
                }
            }
            finally
            {
                if (ReadPipeRequestPtr != null)
                {
                    Heap.Free(ReadPipeRequestPtr);
                }
            }

            return BytesRead;
        }
    }
}