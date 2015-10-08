using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Pipes
{
    public unsafe static class PipeManager
    {
        public static List PipeOutpoints = new List(256, 256);
        public static List Pipes = new List(20);
        
        private static int PipeIdGenerator = 1;

        public static bool RegisterPipeOutpoint(uint OutProcessId, PipeClasses Class, PipeSubclasses Subclass, int MaxConnections, out PipeOutpoint outpoint)
        {
            // Validate inputs
            //  - Check process exists (if it doesn't then hmm...)
            //  - Check MaxConnections > 0 (0 or negative number of connections would be insane)
            if (ProcessManager.GetProcessById(OutProcessId) == null)
            {
                outpoint = null;
                return false;
            }
            else if (MaxConnections <= 0)
            {
                outpoint = null;
                return false;
            }

            // Check no existing outpoints of the same type exist for the specified process
            for (int i = 0; i < PipeOutpoints.Count; i++)
            {
                PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];
                if (anOutpoint.ProcessId == OutProcessId &&
                    anOutpoint.Class == Class &&
                    anOutpoint.Subclass == Subclass)
                {
                    // Set the resultant outpoint to the existing outpoint
                    outpoint = anOutpoint;
                    // Return true because the outpoint exists (even though we didn't create a new one)
                    return true;
                }
            }

            // None exists? Create a new one
            outpoint = new PipeOutpoint(OutProcessId, Class, Subclass, MaxConnections);
            // Add it to our complete list outpoints
            PipeOutpoints.Add(outpoint);

            // Return true because the outpoint exists (a new one was created)
            return true;
        }

        public static bool GetNumPipeOutpoints(PipeClasses Class, PipeSubclasses Subclass, out int numOutpoints)
        {
            // Initialise count to zero
            numOutpoints = 0;

            // Search for outpoints of correct class and subclass, incrementing count as we go
            for (int i = 0; i < PipeOutpoints.Count; i++)
            {
                PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];

                if (anOutpoint.Class == Class &&
                    anOutpoint.Subclass == Subclass)
                {
                    numOutpoints++;
                }
            }

            // This method will always succeed unless it throws an exception
            //  - A count result of zero is valid / success
            return true;
        }
        public static bool GetPipeOutpoints(PipeClasses Class, PipeSubclasses Subclass, PipeOutpointsRequest* request)
        {
            // Validate inputs
            if (request == null)
            {
                // Should have been pre-allocated by the calling thread (/process)
                return false;
            }
            else if (request->Outpoints == null)
            {
                // Should have been pre-allocated by the calling thread (/process)
                return false;
            }
            else if (request->MaxDescriptors == 0)
            {
                // Not technically an error but let's not waste time processing 0 descriptors
                return true;
            }

            // Search for all outpoints of correct class and subclass
            int maxDescriptors = request->MaxDescriptors;
            for (int i = 0, j = 0; i < PipeOutpoints.Count && j < maxDescriptors; i++)
            {
                PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];

                if (anOutpoint.Class == Class &&
                    anOutpoint.Subclass == Subclass)
                {
                    request->Outpoints[j++].ProcessId = anOutpoint.ProcessId;
                }
            }
            return true;
        }
        
        public static bool CreatePipe(uint InProcessId, uint OutProcessId, CreatePipeRequest* request)
        {
            // Validate inputs
            if (ProcessManager.GetProcessById(InProcessId) == null)
            {
                return false;
            }
            else if (ProcessManager.GetProcessById(InProcessId) == null)
            {
                return false;
            }
            
            // Find the outpoint
            PipeOutpoint outpoint = null;
            for (int i = 0; i < PipeOutpoints.Count; i++)
            {
                PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];

                if (anOutpoint.ProcessId == OutProcessId &&
                    anOutpoint.Class == request->Class &&
                    anOutpoint.Subclass == request->Subclass)
                {
                    outpoint = anOutpoint;
                    break;
                }
            }

            // Check that we actually found the outpoint
            if (outpoint == null)
            {
                return false;
            }

            // Create new inpoint
            PipeInpoint inpoint = new PipeInpoint(InProcessId, request->Class, request->Subclass);

            // Create new pipe
            Pipe pipe = new Pipe(PipeIdGenerator++, outpoint, inpoint, request->BufferSize);
            // Add new pipe to list of pipes
            Pipes.Add(pipe);

            // Set result information
            request->Result.Id = pipe.Id;

            return true;
        }
        public static bool WaitOnPipeCreate(uint OutProcessId, PipeClasses Class, PipeSubclasses Subclass)
        {
            return false;
        }

        public static int ReadPipe(int PipeId, byte* outBuffer, int offset, int length)
        {
            return 0;
        }
        public static bool WritePipe(int PipeId, byte* inBuffer, int offset, int length)
        {
            return false;
        }
    }
}
