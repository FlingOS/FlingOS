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

namespace Kernel.Core.Pipes.Standard
{
    public class StandardOutPipe : FOS_System.Object
    {
        private int PipeId;

        public StandardOutPipe()
        {
            SystemCallResults SysCallResult = SystemCallMethods.RegisterPipeOutpoint(Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out, 1);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("StandardOutPipe > RegisterPipeOutpoint: Unhandled!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Register Pipe Outpoint system call unhandled!"));
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("StandardOutPipe > RegisterPipeOutpoint: Failed!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Register Pipe Outpoint system call failed!"));
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("StandardOutPipe > RegisterPipeOutpoint: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("StandardOutPipe > RegisterPipeOutpoint: Unexpected system call result!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Register Pipe Outpoint system call unexpected result!"));
                    break;
            }            
        }

        public int WaitForConnect()
        {
            SystemCallResults SysCallResult = SystemCallMethods.WaitOnPipeCreate(Pipes.PipeClasses.Standard, Pipes.PipeSubclasses.Standard_Out, out PipeId);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("StandardOutPipe > WaitOnPipeCreate: Unhandled!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Wait On Pipe Create unhandled!"));
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("StandardOutPipe > WaitOnPipeCreate: Failed!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Wait On Pipe Create failed!"));
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("StandardOutPipe > WaitOnPipeCreate: Succeeded.");
                    BasicConsole.Write("StandardOutPipe > New pipe id: ");
                    BasicConsole.WriteLine(PipeId);
                    break;
                default:
                    BasicConsole.WriteLine("StandardOutPipe > WaitOnPipeCreate: Unexpected system call result!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Wait On Pipe Create unexpected result!"));
                    break;
            }
            return PipeId;
        }

        public unsafe void Write(FOS_System.String message)
        {
            Pipes.WritePipeRequest* WritePipeRequestPtr = (Pipes.WritePipeRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.WritePipeRequest), "Window Manager : Alloc WritePipeRequest");
            try
            {
                if (WritePipeRequestPtr != null)
                {
                    WritePipeRequestPtr->offset = 0;
                    WritePipeRequestPtr->PipeId = PipeId;
                    byte[] messageBytes = ByteConverter.GetASCIIBytes(message);
                    WritePipeRequestPtr->length = messageBytes.Length;
                    WritePipeRequestPtr->inBuffer = (byte*)Utilities.ObjectUtilities.GetHandle(messageBytes) + FOS_System.Array.FieldsBytesSize;
                    SystemCallResults SysCallResult = SystemCallMethods.WritePipe(WritePipeRequestPtr);
                    switch (SysCallResult)
                    {
                        case SystemCallResults.Unhandled:
                            BasicConsole.WriteLine("StandardOutPipe > WritePipe: Unhandled!");
                            ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Write Pipe unexpected unhandled!"));
                            break;
                        case SystemCallResults.Fail:
                            BasicConsole.WriteLine("StandardOutPipe > WritePipe: Failed!");
                            ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Write Pipe unexpected failed!"));
                            break;
                        case SystemCallResults.OK:
                            BasicConsole.WriteLine("StandardOutPipe > WritePipe: Succeeded.");
                            break;
                        default:
                            BasicConsole.WriteLine("StandardOutPipe > WritePipe: Unexpected system call result!");
                            ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Write Pipe unexpected result!"));
                            break;
                    }
                }
                else
                {
                    ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("StandardOutPipe : Couldn't allocate memory to write to pipe!"));
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
