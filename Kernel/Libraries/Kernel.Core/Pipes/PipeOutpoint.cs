using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Core.Pipes
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
