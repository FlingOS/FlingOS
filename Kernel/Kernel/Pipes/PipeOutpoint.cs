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
using Kernel.FOS_System.Collections;

namespace Kernel.Pipes
{
    public class PipeOutpoint : FOS_System.Object
    {
        public uint ProcessId;
        public PipeClasses Class;
        public PipeSubclasses Subclass;
        public int MaxConnections;
        public int NumConnections;

        public UInt64List WaitingThreads;

        public PipeOutpoint(uint OwnerProcessId, PipeClasses pipeClass, PipeSubclasses pipeSubclass, int MaximumConnections)
        {
            ProcessId = OwnerProcessId;
            Class = pipeClass;
            Subclass = pipeSubclass;
            MaxConnections = MaximumConnections;

            WaitingThreads = new UInt64List();
        }
    }

    public struct PipeOutpointDescriptor
    {
        public uint ProcessId;
    }
    public unsafe struct PipeOutpointsRequest
    {
        public int MaxDescriptors;
        public PipeOutpointDescriptor* Outpoints;
    }
}
