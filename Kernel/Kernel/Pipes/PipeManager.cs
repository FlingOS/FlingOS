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

#define PIPES_TRACE
#undef PIPES_TRACE

using System;
using Kernel.Processes;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;
using Kernel.Hardware.VirtMem;

namespace Kernel.Pipes
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
#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Validating inputs");
#endif
            // Validate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(PipeId);
            if (pipe == null)
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Checking caller allowed to write");
#endif
            // Check the caller is allowed to access the pipe
            if (!AllowedToReadPipe(pipe, CallerProcess.Id))
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Getting out process");
#endif
            // Get outpoint process
            Process OutProcess = ProcessManager.GetProcessById(pipe.Outpoint.ProcessId);
            if (OutProcess == null)
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Getting in process");
#endif
            // Get inpoint process
            Process InProcess = ProcessManager.GetProcessById(pipe.Inpoint.ProcessId);
            if (InProcess == null)
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Merging memory layouts");
#endif
            // Merge memory layouts of out process (in process should have already been done by caller)
            //  so we can access the request structure(s) and buffers
            MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(OutProcess);

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Adding caller to read queue");
#endif
            // Add caller thread to the read queue
            pipe.QueueToRead(CallerThread.Id);

            // Set up initial failure return value
            CallerThread.Return1 = (uint)SystemCallResults.Fail;

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Processing pipe queue");
#endif
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);
            
#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Unmerging memory layouts");
#endif
            // Unmerge memory layouts
            SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

            bool Completed = !pipe.AreThreadsWaitingToRead();
            if (!Blocking)
            {
                if (!Completed)
                {
                    uint temp;
                    pipe.DequeueToRead(out temp);
                }
            }
            return Completed ? RWResults.Complete : RWResults.Queued;
        }
        public static RWResults WritePipe(int PipeId, bool Blocking, Process CallerProcess, Thread CallerThread)
        {
#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Validating inputs");
#endif
            // Validate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(PipeId);
            if (pipe == null)
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Checking caller allowed to write");
#endif
            // Check the caller is allowed to access the pipe
            if (!AllowedToWritePipe(pipe, CallerProcess.Id))
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Getting out process");
#endif
            // Get outpoint process
            Process OutProcess = ProcessManager.GetProcessById(pipe.Outpoint.ProcessId);
            if (OutProcess == null)
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Getting in process");
#endif
            // Get inpoint process
            Process InProcess = ProcessManager.GetProcessById(pipe.Inpoint.ProcessId);
            if (InProcess == null)
            {
                return RWResults.Error;
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Merging memory layouts");
#endif
            // Merge memory layouts of in process (out process should already have been done by caller)
            //  so we can access the request structure(s) and buffers
            MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(InProcess);

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Adding caller to write queue");
#endif
            // Add caller thread to the write queue
            WritePipeRequest* Request = (WritePipeRequest*)CallerThread.Param1;
            pipe.QueueToWrite(CallerThread.Id, Request->length);

            // Set up initial failure return value
            CallerThread.Return1 = (uint)SystemCallResults.Fail;

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Processing pipe queue");
#endif
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Unmerging memory layouts");
#endif
            // Unmerge memory layouts
            SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

            bool Completed = !pipe.AreThreadsWaitingToWrite();
            if (!Blocking)
            {
                if (!Completed)
                {
                    uint temp;
                    pipe.DequeueToWrite(out temp);
                }
            }
            return Completed ? RWResults.Complete : RWResults.Queued;
        }
        private static void ProcessPipeQueue(Pipe pipe, Process OutProcess, Process InProcess)
        {
#if PIPES_TRACE
            BasicConsole.WriteLine("ProcessPipeQueue: Checking first loop condition");
#endif
            while ((pipe.AreThreadsWaitingToWrite() && pipe.CanWrite()) || (pipe.AreThreadsWaitingToRead() && pipe.CanRead()))
            {
#if PIPES_TRACE
                BasicConsole.WriteLine("ProcessPipeQueue: Loop start");
#endif
                if (pipe.AreThreadsWaitingToWrite() && pipe.CanWrite())
                {
#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Pipe can write");
#endif

                    /*  - Dequeue thread to write
                     *  - Find thread to write
                     *  - Load pointer to request structure from thread's stack
                     *  - Write pipe
                     *  - Setup return values for thread
                     *  - Wake thread
                     *  - Loop back
                     */

#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Dequeuing out thread id");
#endif
                    UInt32 ThreadId;
                    if (!pipe.DequeueToWrite(out ThreadId))
                    {
                        break;
                    }

#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Getting out thread");
#endif
                    Thread WriteThread = ProcessManager.GetThreadById(ThreadId, OutProcess);
                    if (WriteThread == null)
                    {
                        break;
                    }

#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Writing pipe");
#endif
                    WritePipeRequest* Request = (WritePipeRequest*)WriteThread.Param1;
                    bool Successful = pipe.Write(Request->inBuffer, Request->offset, Request->length);
                    if (Successful)
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("ProcessPipeQueue: Write successful");
#endif
                        WriteThread.Return1 = (uint)SystemCallResults.OK;
                        WriteThread._Wake();
                    }
                    else
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("ProcessPipeQueue: Write failed");
#endif
                        WriteThread.Return1 = (uint)SystemCallResults.Fail;
                        WriteThread._Wake();
                    }
                }
                else if (pipe.AreThreadsWaitingToRead() && pipe.CanRead())
                {
#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Pipe can read");
#endif

                    /*  - Dequeue thread to read
                     *  - Find thread to read
                     *  - Load pointer to request structure from thread's stack
                     *  - Read pipe
                     *  - Setup return values for thread
                     *  - Wake thread
                     *  - Loop back
                    */

#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Dequeuing in thread id");
#endif
                    UInt32 ThreadId;
                    if (!pipe.DequeueToRead(out ThreadId))
                    {
                        break;
                    }

#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Getting in thread");
#endif
                    Thread ReadThread = ProcessManager.GetThreadById(ThreadId, InProcess);
                    if (ReadThread == null)
                    {
                        break;
                    }

#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Reading pipe");
#endif
                    ReadPipeRequest* Request = (ReadPipeRequest*)ReadThread.Param1;
                    int BytesRead;
                    bool Successful = pipe.Read(Request->outBuffer, Request->offset, Request->length, out BytesRead);
                    if (Successful)
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("ProcessPipeQueue: Read successful");
#endif
                        ReadThread.Return1 = (uint)SystemCallResults.OK;
                        ReadThread.Return2 = (uint)BytesRead;
                        ReadThread._Wake();
                    }
                    else
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("ProcessPipeQueue: Read failed");
#endif
                        ReadThread.Return1 = (uint)SystemCallResults.Fail;
                        ReadThread._Wake();
                    }
                }

#if PIPES_TRACE
                BasicConsole.WriteLine("ProcessPipeQueue: Looping...");
#endif
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
