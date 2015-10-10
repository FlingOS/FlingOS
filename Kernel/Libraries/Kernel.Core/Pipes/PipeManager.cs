using System;
using Kernel.Core.Processes;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;
using Kernel.Hardware.VirtMem;

namespace Kernel.Core.Pipes
{
    public unsafe static class PipeManager
    {
        public enum RWResults
        {
            Error,
            Complete,
            Queued
        }

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
                    anOutpoint.Subclass == Subclass &&
                    (   anOutpoint.MaxConnections == PipeConstants.UnlimitedConnections || 
                        anOutpoint.NumConnections < anOutpoint.MaxConnections))
                {
                    numOutpoints++;
                }
            }

            // This method will always succeed unless it throws an exception
            //  - A count result of zero is valid / success
            return true;
        }
        public static bool GetPipeOutpoints(Process CallerProcess, PipeClasses Class, PipeSubclasses Subclass, PipeOutpointsRequest* request)
        {
            // Validate inputs & get caller process
            if (CallerProcess == null)
            {
                return false;
            }

            // Merge memory layouts 
            //  so we can access the request structure
            MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(CallerProcess);
            bool OK = true;

            // More validate inputs
            //  - Check request exists (should've been pre-allocated by caller)
            //  - Check request->Outpoints exists (should've been pre-allocated by caller)
            //  - Check request->MaxDescriptors was set correctly
            if (request == null)
            {
                // Should have been pre-allocated by the calling thread (/process)
                OK = false;
            }
            else if (request->Outpoints == null)
            {
                // Should have been pre-allocated by the calling thread (/process)
                OK = false;
            }
            else if (request->MaxDescriptors == 0)
            {
                // Not technically an error but let's not waste time processing 0 descriptors
                OK = true;
            }

            if (OK)
            {
                // Search for all outpoints of correct class and subclass
                int maxDescriptors = request->MaxDescriptors;
                for (int i = 0, j = 0; i < PipeOutpoints.Count && j < maxDescriptors; i++)
                {
                    PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];

                    if (anOutpoint.Class == Class &&
                        anOutpoint.Subclass == Subclass &&
                        (   anOutpoint.MaxConnections == PipeConstants.UnlimitedConnections ||
                            anOutpoint.NumConnections < anOutpoint.MaxConnections))
                    {
                        // Set the resultant values
                        request->Outpoints[j++].ProcessId = anOutpoint.ProcessId;
                    }
                }
            }

            SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

            return OK;
        }
        
        public static bool CreatePipe(uint InProcessId, uint OutProcessId, CreatePipeRequest* request)
        {
            // Validate inputs
            //  - Check out process exists
            //  - Check in process exists
            //  - Check request isn't null (should've been pre-allocated)
            Process InProcess = ProcessManager.GetProcessById(InProcessId);
            if (InProcess == null)
            {
                return false;
            }
            else if (ProcessManager.GetProcessById(OutProcessId) == null)
            {
                return false;
            }
            else if (request == null)
            {
                return false;
            }

            // Merge memory layouts 
            //  so we can access the request structure
            MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(InProcess);
            bool OK = true;

            // Find the outpoint
            PipeOutpoint outpoint = GetOutpoint(OutProcessId, request->Class, request->Subclass);

            // Check that we actually found the outpoint
            if (outpoint == null)
            {
                OK = false;
            }

            if (OK)
            {
                // Check there are sufficient connections available
                if (outpoint.NumConnections >= outpoint.MaxConnections &&
                    outpoint.MaxConnections != PipeConstants.UnlimitedConnections)
                {
                    OK = false;
                }

                if (OK)
                {
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
                }
            }

            SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

            return OK;
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

                thread.Return1 = (uint)SystemCallResults.OK;
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
        public static RWResults ReadPipe(int PipeId, bool Blocking, Process CallerProcess, Thread CallerThread)
        {
            BasicConsole.WriteLine("ReadPipe: Validating inputs");
            // Validate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(PipeId);
            if (pipe == null)
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("ReadPipe: Checking caller allowed to write");
            // Check the caller is allowed to access the pipe
            if (!AllowedToReadPipe(pipe, CallerProcess.Id))
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("ReadPipe: Getting out process");
            // Get outpoint process
            Process OutProcess = ProcessManager.GetProcessById(pipe.Outpoint.ProcessId);
            if (OutProcess == null)
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("ReadPipe: Getting in process");
            // Get inpoint process
            Process InProcess = ProcessManager.GetProcessById(pipe.Inpoint.ProcessId);
            if (InProcess == null)
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("ReadPipe: Merging memory layouts");
            // Merge memory layouts of out process (in process should have already been done by caller)
            //  so we can access the request structure(s) and buffers
            MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(OutProcess);

            BasicConsole.WriteLine("ReadPipe: Adding caller to read queue");
            // Add caller thread to the read queue
            pipe.QueueToRead(CallerThread.Id);

            // Set up initial failure return value
            CallerThread.Return1 = (uint)SystemCallResults.Fail;

            BasicConsole.WriteLine("ReadPipe: Processing pipe queue");
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);
            
            BasicConsole.WriteLine("ReadPipe: Unmerging memory layouts");
            // Unmerge memory layouts
            SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

            bool Completed = pipe.AreThreadsWaitingToRead();
            if (!Blocking)
            {
                if (!Completed)
                {
                    uint temp;
                    pipe.DequeueToRead(out temp);
                }
            }
            return Completed ? RWResults.Queued : RWResults.Complete;
        }
        public static RWResults WritePipe(int PipeId, bool Blocking, Process CallerProcess, Thread CallerThread)
        {
            BasicConsole.WriteLine("WritePipe: Validating inputs");
            // Validate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(PipeId);
            if (pipe == null)
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("WritePipe: Checking caller allowed to write");
            // Check the caller is allowed to access the pipe
            if (!AllowedToWritePipe(pipe, CallerProcess.Id))
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("WritePipe: Getting out process");
            // Get outpoint process
            Process OutProcess = ProcessManager.GetProcessById(pipe.Outpoint.ProcessId);
            if (OutProcess == null)
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("WritePipe: Getting in process");
            // Get inpoint process
            Process InProcess = ProcessManager.GetProcessById(pipe.Inpoint.ProcessId);
            if (InProcess == null)
            {
                return RWResults.Error;
            }

            BasicConsole.WriteLine("WritePipe: Merging memory layouts");
            // Merge memory layouts of in process (out process should already have been done by caller)
            //  so we can access the request structure(s) and buffers
            MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(InProcess);

            BasicConsole.WriteLine("WritePipe: Adding caller to write queue");
            // Add caller thread to the write queue
            pipe.QueueToWrite(CallerThread.Id);

            // Set up initial failure return value
            CallerThread.Return1 = (uint)SystemCallResults.Fail;

            BasicConsole.WriteLine("WritePipe: Processing pipe queue");
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);

            BasicConsole.WriteLine("WritePipe: Unmerging memory layouts");
            // Unmerge memory layouts
            SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

            bool Completed = pipe.AreThreadsWaitingToWrite();
            if (!Blocking)
            {
                if (!Completed)
                {
                    uint temp;
                    pipe.DequeueToWrite(out temp);
                }
            }
            return Completed ? RWResults.Queued : RWResults.Complete;
        }
        private static void ProcessPipeQueue(Pipe pipe, Process OutProcess, Process InProcess)
        {
            BasicConsole.WriteLine("ProcessPipeQueue: Checking first loop condition");
            while ((pipe.AreThreadsWaitingToWrite() && pipe.CanWrite()) || (pipe.AreThreadsWaitingToRead() && pipe.CanRead()))
            {
                BasicConsole.WriteLine("ProcessPipeQueue: Loop start");
                if (pipe.CanWrite())
                {
                    BasicConsole.WriteLine("ProcessPipeQueue: Pipe can write");

                    /*  - Dequeue thread to write
                     *  - Find thread to write
                     *  - Load pointer to request structure from thread's stack
                     *  - Write pipe
                     *  - Setup return values for thread
                     *  - Wake thread
                     *  - Loop back
                     */

                    BasicConsole.WriteLine("ProcessPipeQueue: Dequeuing out thread id");
                    UInt32 ThreadId;
                    if (!pipe.DequeueToWrite(out ThreadId))
                    {
                        break;
                    }

                    BasicConsole.WriteLine("ProcessPipeQueue: Getting out thread");
                    Thread WriteThread = ProcessManager.GetThreadById(ThreadId, OutProcess);
                    if (WriteThread == null)
                    {
                        break;
                    }

                    BasicConsole.WriteLine("ProcessPipeQueue: Writing pipe");
                    WritePipeRequest* Request = (WritePipeRequest*)WriteThread.Param1;
                    bool Successful = pipe.Write(Request->inBuffer, Request->offset, Request->length);
                    if (Successful)
                    {
                        BasicConsole.WriteLine("ProcessPipeQueue: Write successful");
                        WriteThread.Return1 = (uint)SystemCallResults.OK;
                        WriteThread._Wake();
                    }
                    else
                    {
                        BasicConsole.WriteLine("ProcessPipeQueue: Write failed");
                        WriteThread.Return1 = (uint)SystemCallResults.Fail;
                        WriteThread._Wake();
                    }
                }
                else if (pipe.CanRead())
                {
                    BasicConsole.WriteLine("ProcessPipeQueue: Pipe can read");

                    /*  - Dequeue thread to read
                     *  - Find thread to read
                     *  - Load pointer to request structure from thread's stack
                     *  - Read pipe
                     *  - Setup return values for thread
                     *  - Wake thread
                     *  - Loop back
                    */

                    BasicConsole.WriteLine("ProcessPipeQueue: Dequeuing in thread id");
                    UInt32 ThreadId;
                    if (!pipe.DequeueToRead(out ThreadId))
                    {
                        break;
                    }

                    BasicConsole.WriteLine("ProcessPipeQueue: Getting in thread");
                    Thread ReadThread = ProcessManager.GetThreadById(ThreadId, InProcess);
                    if (ReadThread == null)
                    {
                        break;
                    }

                    BasicConsole.WriteLine("ProcessPipeQueue: Reading pipe");
                    ReadPipeRequest* Request = (ReadPipeRequest*)ReadThread.Param1;
                    int BytesRead;
                    bool Successful = pipe.Read(Request->outBuffer, Request->offset, Request->length, out BytesRead);
                    if (Successful)
                    {
                        BasicConsole.WriteLine("ProcessPipeQueue: Read successful");
                        ReadThread.Return1 = (uint)SystemCallResults.OK;
                        ReadThread.Return2 = (uint)BytesRead;
                        ReadThread._Wake();
                    }
                    else
                    {
                        BasicConsole.WriteLine("ProcessPipeQueue: Read failed");
                        ReadThread.Return1 = (uint)SystemCallResults.Fail;
                        ReadThread._Wake();
                    }
                }

                BasicConsole.WriteLine("ProcessPipeQueue: Looping...");
            }
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
