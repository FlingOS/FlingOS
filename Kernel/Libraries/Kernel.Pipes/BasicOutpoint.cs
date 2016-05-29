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
    ///     Represents a basic outpoint for any pipe class.
    /// </summary>
    public abstract unsafe class BasicOutpoint : Object
    {
        /// <summary>
        ///     The class of pipe allowed to connect to the outpoint.
        /// </summary>
        public PipeClasses Class { get; protected set; }

        /// <summary>
        ///     The subclass of pipe allowed to connect to the outpoint.
        /// </summary>
        public PipeSubclasses Subclass { get; protected set; }

        /// <summary>
        ///     Creates and registers a new outpoint of the specified class and subclass.
        /// </summary>
        /// <param name="aClass">The class of pipe allowed to connect to the outpoint.</param>
        /// <param name="aSubclass">The subclass of pipe allowed to connect to the outpoint.</param>
        /// <param name="MaxConnections">
        ///     The maximum number of connections allowed. Use <see cref="PipeConstants.UnlimitedConnections" /> for unlimited
        ///     connections.
        /// </param>
        public BasicOutpoint(PipeClasses aClass, PipeSubclasses aSubclass, int MaxConnections)
        {
            Class = aClass;
            Subclass = aSubclass;

            SystemCallResults SysCallResult = SystemCalls.RegisterPipeOutpoint(Class, Subclass, MaxConnections);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    //BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Unhandled!");
                    ExceptionMethods.Throw(
                        new ArgumentException("BasicOutPipe : Register Pipe Outpoint system call unhandled!"));
                    break;
                case SystemCallResults.Fail:
                    //BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Failed!");
                    ExceptionMethods.Throw(
                        new ArgumentException("BasicOutPipe : Register Pipe Outpoint system call failed!"));
                    break;
                case SystemCallResults.OK:
                    //BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Succeeded.");
                    break;
                default:
                    //BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Unexpected system call result!");
                    ExceptionMethods.Throw(
                        new ArgumentException("BasicOutPipe : Register Pipe Outpoint system call unexpected result!"));
                    break;
            }
        }

        /// <summary>
        ///     Waits for a pipe to connect to the outpoint.
        /// </summary>
        /// <returns>The Id of the newly connected pipe.</returns>
        public int WaitForConnect(out uint InProcessId)
        {
            int aPipeId = 0;
            InProcessId = 0;

            WaitOnPipeCreateRequest* request =
                (WaitOnPipeCreateRequest*)
                    Heap.AllocZeroed((uint)sizeof(WaitOnPipeCreateRequest), "BasicOutPipe : WaitForConnect");
            try
            {
                request->Class = Class;
                request->Subclass = Subclass;

                SystemCallResults SysCallResult = SystemCalls.WaitOnPipeCreate(request);
                switch (SysCallResult)
                {
                    case SystemCallResults.Unhandled:
                        //BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Unhandled!");
                        ExceptionMethods.Throw(new ArgumentException("BasicOutPipe : Wait On Pipe Create unhandled!"));
                        break;
                    case SystemCallResults.Fail:
                        //BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Failed!");
                        ExceptionMethods.Throw(new ArgumentException("BasicOutPipe : Wait On Pipe Create failed!"));
                        break;
                    case SystemCallResults.OK:
                        aPipeId = request->Result.Id;
                        InProcessId = request->Result.InpointProcessId;

                        //BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Succeeded.");
                        //BasicConsole.Write("BasicOutPipe > New pipe id: ");
                        //BasicConsole.WriteLine(aPipeId);
                        break;
                    default:
                        //BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Unexpected system call result!");
                        ExceptionMethods.Throw(
                            new ArgumentException("BasicOutPipe : Wait On Pipe Create unexpected result!"));
                        break;
                }
            }
            finally
            {
                if (request != null)
                {
                    Heap.Free(request);
                }
            }

            return aPipeId;
        }

        /// <summary>
        ///     Writes the specified buffer to the specified pipe.
        /// </summary>
        /// <param name="PipeId">The Id of the pipe to write to.</param>
        /// <param name="Data">The buffer to write to the pipe.</param>
        /// <param name="Offset">The offset in the buffer to start writing from.</param>
        /// <param name="Length">The length of data to write from the buffer. (Clamped)</param>
        /// <param name="Blocking">Whether the write call should be blocking or not.</param>
        /// <remarks>
        ///     <para>
        ///         Id required since an outpoint can be connected to multiple
        ///     </para>
        ///     <para>
        ///         Non-blocking calls will still be deferred but will fail if the data cannot be written
        ///         on first pass through the pipe's R/W queue.
        ///     </para>
        /// </remarks>
        public void Write(int PipeId, byte[] Data, int Offset, int Length, bool Blocking)
        {
            WritePipeRequest* WritePipeRequestPtr =
                (WritePipeRequest*)
                    Heap.AllocZeroed((uint)sizeof(WritePipeRequest), "BasicOutPipe : Alloc WritePipeRequest");
            try
            {
                if (WritePipeRequestPtr != null)
                {
                    WritePipeRequestPtr->Offset = Offset;
                    WritePipeRequestPtr->PipeId = PipeId;
                    WritePipeRequestPtr->Length = Math.Min(Data.Length - Offset, Length);
                    WritePipeRequestPtr->InBuffer = (byte*)ObjectUtilities.GetHandle(Data) + Array.FieldsBytesSize;
                    WritePipeRequestPtr->Blocking = Blocking;

                    SystemCallResults SysCallResult = SystemCalls.WritePipe(WritePipeRequestPtr);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            //BasicConsole.WriteLine("BasicOutPipe > WritePipe: Unhandled!");
                            ExceptionMethods.Throw(
                                new RWUnhandledException("BasicOutPipe : Write Pipe unexpected unhandled!"));
                            break;
                        case SystemCallResults.Fail:
                            //BasicConsole.WriteLine("BasicOutPipe > WritePipe: Failed!");
                            if (Blocking)
                            {
                                ExceptionMethods.Throw(
                                    new RWFailedException("BasicOutPipe : Write Pipe unexpected failed! (Blocking call)"));
                            }
                            else
                            {
                                ExceptionMethods.Throw(
                                    new RWFailedException("BasicOutPipe : Write Pipe failed. (Non-blocking call)"));
                            }
                            break;
                        case SystemCallResults.OK:
                            //BasicConsole.WriteLine("BasicOutPipe > WritePipe: Succeeded.");
                            break;
                        default:
                            //BasicConsole.WriteLine("BasicOutPipe > WritePipe: Unexpected system call result!");
                            ExceptionMethods.Throw(
                                new RWUnhandledException("BasicOutPipe : Write Pipe unexpected result!"));
                            break;
                    }
                }
                else
                {
                    ExceptionMethods.Throw(
                        new ArgumentException("BasicOutPipe : Couldn't allocate memory to write to pipe!"));
                }
            }
            finally
            {
                if (WritePipeRequestPtr != null)
                {
                    Heap.Free(WritePipeRequestPtr);
                }
            }
        }

        /// <summary>
        ///     Gets the number of available outpoints of the specified class and subclass.
        /// </summary>
        /// <param name="numOutpoints">Out : The number of outpoints (correct iff SysCallResult is OK).</param>
        /// <param name="SysCallResult">Out : The result of the system call. Check this is set to OK.</param>
        /// <param name="Class">The class of pipe to search for.</param>
        /// <param name="Subclass">The subclass of pipe to search for.</param>
        public static void GetNumPipeOutpoints(out int numOutpoints, out SystemCallResults SysCallResult,
            PipeClasses Class, PipeSubclasses Subclass)
        {
            SysCallResult = SystemCalls.GetNumPipeOutpoints(Class, Subclass, out numOutpoints);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    //BasicConsole.WriteLine("BasicServerHelpers > GetNumPipeOutpoints: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    //BasicConsole.WriteLine("BasicServerHelpers > GetNumPipeOutpoints: Failed!");
                    break;
                case SystemCallResults.OK:
                    //BasicConsole.WriteLine("BasicServerHelpers > GetNumPipeOutpoints: Succeeded.");

                    //BasicConsole.Write("BasicServerHelpers > Num pipe outpoints: ");
                    //BasicConsole.WriteLine(numOutpoints);
                    break;
                default:
                    //BasicConsole.WriteLine("BasicServerHelpers > GetNumPipeOutpoints: Unexpected system call result!");
                    break;
            }
        }

        /// <summary>
        ///     Gets the outpoint desciptors of the available outpoints of the specified class and subclass.
        /// </summary>
        /// <param name="numOutpoints">The known number of available outpoints. Use GetNumPipeOutpoints to obtain this number.</param>
        /// <param name="SysCallResult">Out : The result of the system call. Check this is set to OK.</param>
        /// <param name="OutpointDescriptors">Out : The array of outpoint descriptors.</param>
        /// <param name="Class">The class of pipe to search for.</param>
        /// <param name="Subclass">The subclass of pipe to search for.</param>
        public static void GetOutpointDescriptors(int numOutpoints, out SystemCallResults SysCallResult,
            out PipeOutpointDescriptor[] OutpointDescriptors, PipeClasses Class, PipeSubclasses Subclass)
        {
            OutpointDescriptors = new PipeOutpointDescriptor[numOutpoints];

            PipeOutpointsRequest* RequestPtr =
                (PipeOutpointsRequest*)
                    Heap.AllocZeroed((uint)sizeof(PipeOutpointsRequest),
                        "BasicServerHelpers : Alloc PipeOutpointsRequest");
            if (RequestPtr != null)
            {
                try
                {
                    RequestPtr->MaxDescriptors = numOutpoints;
                    RequestPtr->Outpoints =
                        (PipeOutpointDescriptor*)
                            ((byte*)ObjectUtilities.GetHandle(OutpointDescriptors) + Array.FieldsBytesSize);
                    if (RequestPtr->Outpoints != null)
                    {
                        SysCallResult = SystemCalls.GetPipeOutpoints(Class, Subclass, RequestPtr);
                        switch (SysCallResult)
                        {
                            case SystemCallResults.Unhandled:
                                //BasicConsole.WriteLine("BasicServerHelpers > GetPipeOutpoints: Unhandled!");
                                break;
                            case SystemCallResults.Fail:
                                //BasicConsole.WriteLine("BasicServerHelpers > GetPipeOutpoints: Failed!");
                                break;
                            case SystemCallResults.OK:
                                //BasicConsole.WriteLine("BasicServerHelpers > GetPipeOutpoints: Succeeded.");
                                break;
                            default:
                                //BasicConsole.WriteLine("BasicServerHelpers > GetPipeOutpoints: Unexpected system call result!");
                                break;
                        }
                    }
                    else
                    {
                        SysCallResult = SystemCallResults.Fail;
                        //BasicConsole.WriteLine("BasicServerHelpers > RequestPtr->Outpoints null! No memory allocated.");
                        ExceptionMethods.Throw(
                            new ArgumentException(
                                "BasicServerHelpers : Couldn't allocate memory outpoints list in outpoints request!"));
                    }
                }
                finally
                {
                    Heap.Free(RequestPtr);
                }
            }
            else
            {
                SysCallResult = SystemCallResults.Fail;
                //BasicConsole.WriteLine("BasicServerHelpers > RequestPtr null! No memory allocated.");
                ExceptionMethods.Throw(
                    new ArgumentException("BasicServerHelpers : Couldn't allocate memory get outpoints request!"));
            }
        }
    }
}