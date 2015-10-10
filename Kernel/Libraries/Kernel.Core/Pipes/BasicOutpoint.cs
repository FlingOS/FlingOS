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
using Kernel.Core.Processes;
using Kernel.FOS_System;

namespace Kernel.Core.Pipes
{
    public unsafe class BasicOutpoint : FOS_System.Object
    {
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

        public BasicOutpoint(PipeClasses aClass, PipeSubclasses aSubclass, int MaxConnections)
        {
            Class = aClass;
            Subclass = aSubclass;

            SystemCallResults SysCallResult = SystemCallMethods.RegisterPipeOutpoint(Class, Subclass, MaxConnections);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Unhandled!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Register Pipe Outpoint system call unhandled!"));
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Failed!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Register Pipe Outpoint system call failed!"));
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("BasicOutPipe > RegisterPipeOutpoint: Unexpected system call result!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Register Pipe Outpoint system call unexpected result!"));
                    break;
            }            
        }

        public int WaitForConnect()
        {
            int aPipeId;
            SystemCallResults SysCallResult = SystemCallMethods.WaitOnPipeCreate(Class, Subclass, out aPipeId);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Unhandled!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Wait On Pipe Create unhandled!"));
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Failed!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Wait On Pipe Create failed!"));
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Succeeded.");
                    BasicConsole.Write("BasicOutPipe > New pipe id: ");
                    BasicConsole.WriteLine(aPipeId);
                    break;
                default:
                    BasicConsole.WriteLine("BasicOutPipe > WaitOnPipeCreate: Unexpected system call result!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Wait On Pipe Create unexpected result!"));
                    break;
            }
            return aPipeId;
        }

        public void Write(int PipeId, byte[] data, int offset, int length)
        {
            Pipes.WritePipeRequest* WritePipeRequestPtr = (Pipes.WritePipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.WritePipeRequest), "BasicOutPipe : Alloc WritePipeRequest");
            try
            {
                if (WritePipeRequestPtr != null)
                {
                    WritePipeRequestPtr->offset = offset;
                    WritePipeRequestPtr->PipeId = PipeId;
                    WritePipeRequestPtr->length = FOS_System.Math.Min(data.Length - offset, length);
                    WritePipeRequestPtr->inBuffer = (byte*)Utilities.ObjectUtilities.GetHandle(data) + FOS_System.Array.FieldsBytesSize;
                    SystemCallResults SysCallResult = SystemCallMethods.WritePipe(WritePipeRequestPtr);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            BasicConsole.WriteLine("BasicOutPipe > WritePipe: Unhandled!");
                            ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Write Pipe unexpected unhandled!"));
                            break;
                        case SystemCallResults.Fail:
                            BasicConsole.WriteLine("BasicOutPipe > WritePipe: Failed!");
                            ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Write Pipe unexpected failed!"));
                            break;
                        case SystemCallResults.OK:
                            BasicConsole.WriteLine("BasicOutPipe > WritePipe: Succeeded.");
                            break;
                        default:
                            BasicConsole.WriteLine("BasicOutPipe > WritePipe: Unexpected system call result!");
                            ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Write Pipe unexpected result!"));
                            break;
                    }
                }
                else
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicOutPipe : Couldn't allocate memory to write to pipe!"));
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
    }
}
