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
using Kernel.Processes;
using Kernel.FOS_System;

namespace Kernel.Pipes
{
    public unsafe class BasicInpoint : FOS_System.Object
    {
        public uint OutProcessId
        {
            get;
            protected set;
        }
        public int PipeId
        {
            get;
            protected set;
        }
        public PipeClasses Class
        {
            get;
            protected set;
        }
        public PipeSubclasses Subclass
        {
            get;
            protected set;
        }
        public int BufferSize
        {
            get;
            protected set;
        }

        public BasicInpoint(uint anOutProcessId, PipeClasses aClass, PipeSubclasses aSubclass, int aBufferSize)
        {
            OutProcessId = anOutProcessId;
            Class = aClass;
            Subclass = aSubclass;
            BufferSize = aBufferSize;

            Pipes.CreatePipeRequest* RequestPtr = (Pipes.CreatePipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.CreatePipeRequest), "BasicInPipe : Alloc CreatePipeRequest");
            if (RequestPtr != null)
            {
                try
                {
                    RequestPtr->BufferSize = aBufferSize;
                    RequestPtr->Class = aClass;
                    RequestPtr->Subclass = aSubclass;
                                        
                    SystemCallResults SysCallResult = SystemCallMethods.CreatePipe(anOutProcessId, RequestPtr);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            BasicConsole.WriteLine("BasicInPipe > CreatePipe: Unhandled!");
                            break;
                        case SystemCallResults.Fail:
                            BasicConsole.WriteLine("BasicInPipe > CreatePipe: Failed!");
                            break;
                        case SystemCallResults.OK:
                            BasicConsole.WriteLine("BasicInPipe > CreatePipe: Succeeded.");
                            PipeId = RequestPtr->Result.Id;

                            BasicConsole.Write("BasicInPipe > CreatePipe: New pipe id = ");
                            BasicConsole.WriteLine(PipeId);
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
                BasicConsole.WriteLine("BasicInPipe > RequestPtr null! No memory allocated.");
            }
        }

        public int Read(byte[] data, int offset, int length, bool blocking)
        {
            int BytesRead = 0;

            Pipes.ReadPipeRequest* ReadPipeRequestPtr = (Pipes.ReadPipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.ReadPipeRequest), "BasicInPipe : Alloc ReadPipeRequest");
            try
            {
                if (ReadPipeRequestPtr != null)
                {
                    ReadPipeRequestPtr->PipeId = PipeId;
                    ReadPipeRequestPtr->offset = offset;
                    ReadPipeRequestPtr->length = FOS_System.Math.Min(data.Length - offset, length);
                    ReadPipeRequestPtr->outBuffer = (byte*)Utilities.ObjectUtilities.GetHandle(data) + FOS_System.Array.FieldsBytesSize;
                    ReadPipeRequestPtr->blocking = blocking;

                    SystemCallResults SysCallResult = SystemCallMethods.ReadPipe(ReadPipeRequestPtr, out BytesRead);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Unhandled!");
                            ExceptionMethods.Throw(new Exceptions.RWUnhandledException("BasicInPipe : Read Pipe unexpected unhandled!"));
                            break;
                        case SystemCallResults.Fail:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Failed!");
                            if (blocking)
                            {
                                ExceptionMethods.Throw(new Exceptions.RWFailedException("BasicInPipe : Write Pipe unexpected failed! (Blocking call)"));
                            }
                            else
                            {
                                ExceptionMethods.Throw(new Exceptions.RWFailedException("BasicInPipe : Write Pipe failed. (Non-blocking call)"));
                            }
                            break;
                        case SystemCallResults.OK:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Succeeded.");
                            break;
                        default:
                            //BasicConsole.WriteLine("BasicInPipe > ReadPipe: Unexpected system call result!");
                            ExceptionMethods.Throw(new Exceptions.RWUnhandledException("BasicInPipe : Read Pipe unexpected result!"));
                            break;
                    }
                }
                else
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicInPipe : Couldn't allocate memory to read from pipe!"));
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
