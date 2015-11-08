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
    public static unsafe class BasicServerHelpers
    {
        public static void GetNumPipeOutpoints(out int numOutpoints, out SystemCallResults SysCallResult, Pipes.PipeClasses Class, Pipes.PipeSubclasses Subclass)
        {
            SysCallResult = SystemCallMethods.GetNumPipeOutpoints(Class, Subclass, out numOutpoints);
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
        public static void GetOutpointDescriptors(int numOutpoints, ref SystemCallResults SysCallResult, out Pipes.PipeOutpointDescriptor[] OutpointDescriptors, Pipes.PipeClasses Class, Pipes.PipeSubclasses Subclass)
        {
            OutpointDescriptors = new Pipes.PipeOutpointDescriptor[numOutpoints];

            Pipes.PipeOutpointsRequest* RequestPtr = (Pipes.PipeOutpointsRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.PipeOutpointsRequest), "BasicServerHelpers : Alloc PipeOutpointsRequest");
            if (RequestPtr != null)
            {
                try
                {
                    RequestPtr->MaxDescriptors = numOutpoints;
                    RequestPtr->Outpoints = (Pipes.PipeOutpointDescriptor*)((byte*)Utilities.ObjectUtilities.GetHandle(OutpointDescriptors) + FOS_System.Array.FieldsBytesSize);
                    if (RequestPtr->Outpoints != null)
                    {
                        SysCallResult = SystemCallMethods.GetPipeOutpoints(Class, Subclass, RequestPtr);
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
                        ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicServerHelpers : Couldn't allocate memory outpoints list in outpoints request!"));
                        //BasicConsole.WriteLine("BasicServerHelpers > RequestPtr->Outpoints null! No memory allocated.");
                    }
                }
                finally
                {
                    Heap.Free(RequestPtr);
                }
            }
            else
            {
                //BasicConsole.WriteLine("BasicServerHelpers > RequestPtr null! No memory allocated.");
                ExceptionMethods.Throw(new FOS_System.Exceptions.ArgumentException("BasicServerHelpers : Couldn't allocate memory get outpoints request!"));
            }
        }
    }
}
