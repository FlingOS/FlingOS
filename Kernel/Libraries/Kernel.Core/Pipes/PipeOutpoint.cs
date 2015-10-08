using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Pipes
{
    public class PipeOutpoint : FOS_System.Object
    {
        public uint ProcessId;
        public PipeClasses Class;
        public PipeSubclasses Subclass;
        public int MaxConnections;
        public int NumConnections;

        public PipeOutpoint(uint OwnerProcessId, PipeClasses pipeClass, PipeSubclasses pipeSubclass, int MaximumConnections)
        {
            ProcessId = OwnerProcessId;
            Class = pipeClass;
            Subclass = pipeSubclass;
            MaxConnections = MaximumConnections;
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
