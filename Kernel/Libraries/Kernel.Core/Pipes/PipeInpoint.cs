using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Pipes
{
    public class PipeInpoint : FOS_System.Object
    {
        public uint ProcessId;
        public PipeClasses Class;
        public PipeSubclasses Subclass;

        public PipeInpoint(uint OwnerProcessId, PipeClasses pipeClass, PipeSubclasses pipeSubclass)
        {
            ProcessId = OwnerProcessId;
            Class = pipeClass;
            Subclass = pipeSubclass;
        }
    }
}
