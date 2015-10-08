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
            else if (MaxConnections <= 0 && MaxConnections != PipeConstants.UnlimitedConnections)
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
            //  - Check request exists (should've been pre-allocated by caller)
            //  - Check request->Outpoints exists (should've been pre-allocated by caller)
            //  - Check request->MaxDescriptors was set correctly
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
                    // Set the resultant values
                    request->Outpoints[j++].ProcessId = anOutpoint.ProcessId;
                }
            }
            return true;
        }
        
        public static bool CreatePipe(uint InProcessId, uint OutProcessId, CreatePipeRequest* request)
        {
            // Validate inputs
            //  - Check out process exists
            //  - Check in process exists
            //  - Check request isn't null (should've been pre-allocated)
            if (ProcessManager.GetProcessById(OutProcessId) == null)
            {
                return false;
            }
            else if (ProcessManager.GetProcessById(InProcessId) == null)
            {
                return false;
            }
            else if (request == null)
            {
                return false;
            }
            
            // Find the outpoint
            PipeOutpoint outpoint = GetOutpoint(OutProcessId, request->Class, request->Subclass);

            // Check that we actually found the outpoint
            if (outpoint == null)
            {
                return false;
            }

            // Check there are sufficient connections available
            if (outpoint.NumConnections >= outpoint.MaxConnections &&
                outpoint.MaxConnections != PipeConstants.UnlimitedConnections)
            {
                return false;
            }

            // Create new inpoint
            PipeInpoint inpoint = new PipeInpoint(InProcessId, request->Class, request->Subclass);

            // Create new pipe
            Pipe pipe = new Pipe(PipeIdGenerator++, outpoint, inpoint, request->BufferSize);
            // Add new pipe to list of pipes
            Pipes.Add(pipe);
            // Increment number of connections to the outpoint
            outpoint.NumConnections++;

            // Set result information
            request->Result.Id = pipe.Id;

            // Wake any threads (/processes) which were waiting on a pipe to be created
            WakeWaitingThreads(outpoint, pipe.Id);

            return true;
        }
        public static void WakeWaitingThreads(PipeOutpoint outpoint, int newPipeId)
        {
            while (outpoint.WaitingThreads.Count > 0)
            {
                UInt64 identifier = outpoint.WaitingThreads[0];
                outpoint.WaitingThreads.RemoveAt(0);

                UInt32 processId = (UInt32)(identifier >> 32);
                UInt32 threadId = (UInt32)(identifier);

                Process process = ProcessManager.GetProcessById(processId);
                Thread thread = ProcessManager.GetThreadById(threadId, process);

                thread.Return1 = (uint)Processes.SystemCallResults.OK;
                thread.Return2 = (uint)newPipeId;
                thread.Return3 = 0;
                thread.Return4 = 0;
                thread._Wake();
            }
        }
        public static bool WaitOnPipeCreate(uint OutProcessId, uint OutThreadId, PipeClasses Class, PipeSubclasses Subclass)
        {
            // Validate inputs
            //   - Check the process exists
            //   - Check the thread exists
            Process OutProcess = ProcessManager.GetProcessById(OutProcessId);
            if (OutProcess == null)
            {
                return false;
            }            
            Thread OutThread = ProcessManager.GetThreadById(OutThreadId, OutProcess);
            if (OutThread == null)
            {
                return false;
            }
            
            // Find the outpoint
            PipeOutpoint outpoint = GetOutpoint(OutProcessId, Class, Subclass);

            // Check that we actually found the outpoint
            if (outpoint == null)
            {
                return false;
            }

            // Mark the outpoint as being waited on by the specified process/thread
            outpoint.WaitingThreads.Add(((UInt64)OutProcessId << 32) | OutThreadId);

            return true;
        }

        public static bool AllowedToReadPipe(Pipe ThePipe, uint CallerProcessId)
        {
            return ThePipe.Inpoint.ProcessId == CallerProcessId;
        }
        public static bool AllowedToWritePipe(Pipe ThePipe, uint CallerProcessId)
        {
            return ThePipe.Outpoint.ProcessId == CallerProcessId;
        }
        public static bool ReadPipe(ReadPipeRequest* request, uint CallerProcessId, out int BytesRead)
        {
            // Vaildate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(request->PipeId);
            if (pipe == null)
            {
                // -1 = Failed to read
                BytesRead = -1;
                return false;
            }

            // Check the caller is allowed to access the pipe
            if (!AllowedToReadPipe(pipe, CallerProcessId))
            {
                BytesRead = -1;
                return false;
            }

            // Read the pipe
            BytesRead = pipe.Read(request->outBuffer, request->offset, request->length);

            // -1         = Failed to read
            //  0 or more = Read nothing or something, both are valid
            return BytesRead >= 0;
        }
        public static bool WritePipe(WritePipeRequest* request, uint CallerProcessId)
        {
            // Vaildate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(request->PipeId);
            if (pipe == null)
            {
                return false;
            }

            // Check the caller is allowed to access the pipe
            if (!AllowedToWritePipe(pipe, CallerProcessId))
            {
                return false;
            }

            return pipe.Write(request->inBuffer, request->offset, request->length);
        }

        private static PipeOutpoint GetOutpoint(uint OutProcessId, PipeClasses Class, PipeSubclasses Subclass)
        {
            PipeOutpoint outpoint = null;
            for (int i = 0; i < PipeOutpoints.Count; i++)
            {
                PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];

                if (anOutpoint.ProcessId == OutProcessId &&
                    anOutpoint.Class == Class &&
                    anOutpoint.Subclass == Subclass)
                {
                    outpoint = anOutpoint;
                    break;
                }
            }
            return outpoint;
        }
        private static Pipe GetPipe(int PipeId)
        {
            for (int i = 0; i < Pipes.Count; i++)
            {
                Pipe pipe = (Pipe)Pipes[i];
                if (pipe.Id == PipeId)
                {
                    return pipe;
                }
            }
            return null;
        }
    }
}
