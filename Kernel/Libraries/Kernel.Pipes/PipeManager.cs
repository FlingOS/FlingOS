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

using Drivers.Compiler.Attributes;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Multiprocessing;

namespace Kernel.Pipes
{
    /// <summary>
    ///     The main manager for pipes. Used only in the core OS.
    /// </summary>
    public static unsafe class PipeManager
    {
        /// <summary>
        ///     Results of read or write requests.
        /// </summary>
        public enum RWResults
        {
            /// <summary>
            ///     An error occurred and the R/W request failed.
            /// </summary>
            Error,

            /// <summary>
            ///     The R/W request was successful.
            /// </summary>
            Complete,

            /// <summary>
            ///     For blocking calls, indicates the R/W request has been queued. For non-blocking calls,
            ///     indicates the R/W request failed because it couldn't be processed immediately.
            /// </summary>
            Queued
        }

        /// <summary>
        ///     The list of all registered outpoints.
        /// </summary>
        [Group(Name = "IsolatedKernel_Pipes")] public static List PipeOutpoints = new List(256, 256);

        /// <summary>
        ///     The list of all created pipes.
        /// </summary>
        [Group(Name = "IsolatedKernel_Pipes")] public static List Pipes = new List(20);

        /// <summary>
        ///     Number used to generate Ids for pipes.
        /// </summary>
        /// <remarks>
        ///     Might overflow back to 1 eventually but creating that number of pipes would be astounding. (The system would run
        ///     out of memory first and
        ///     there isn't currently a way to destroy pipes).
        /// </remarks>
        [Group(Name = "IsolatedKernel_Pipes")] private static int PipeIdGenerator = 1;

        /// <summary>
        ///     Attempts to register a pipe outpoint.
        /// </summary>
        /// <param name="OutProcessId">The Id of the process which should own the outpoint.</param>
        /// <param name="Class">The class of pipe the outpoint will create.</param>
        /// <param name="Subclass">The subclass of pipe the outpoint will create.</param>
        /// <param name="MaxConnections">
        ///     The maximum number of connections allowed to the outpoint. Also see
        ///     <see cref="PipeConstants.UnlimitedConnections" />.
        /// </param>
        /// <param name="outpoint">Out : The newly created outpoint (or null if the request fails).</param>
        /// <returns>True if the request was successful. Otherwise, false.</returns>
        /// <seealso cref="PipeConstants.UnlimitedConnections" />
        public static bool RegisterPipeOutpoint(uint OutProcessId, PipeClasses Class, PipeSubclasses Subclass,
            int MaxConnections, out PipeOutpoint outpoint)
        {
            // Validate inputs
            //  - Check process exists (if it doesn't then hmm...)
            //  - Check MaxConnections > 0 (0 or negative number of connections would be insane)
            if (ProcessManager.GetProcessById(OutProcessId) == null)
            {
                outpoint = null;
                return false;
            }
            if (MaxConnections <= 0 && MaxConnections != PipeConstants.UnlimitedConnections)
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

        /// <summary>
        ///     Attempts to get the number of outpoints of the specified class and subclass.
        /// </summary>
        /// <param name="Class">The class of outpoint to search for.</param>
        /// <param name="Subclass">The subclass of outpoint to search for.</param>
        /// <param name="numOutpoints">Out : The number of outpoints of the specified class and subclass.</param>
        /// <returns>True if the request was successful. Otherwise, false.</returns>
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
                    (anOutpoint.MaxConnections == PipeConstants.UnlimitedConnections ||
                     anOutpoint.NumConnections < anOutpoint.MaxConnections))
                {
                    numOutpoints++;
                }
            }

            // This method will always succeed unless it throws an exception
            //  - A count result of zero is valid / success
            return true;
        }

        /// <summary>
        ///     Attempts to get descriptors of the outpoints of the specified class and subclass.
        /// </summary>
        /// <param name="CallerProcess">The process which owns the memory containing the <paramref cref="request" />.</param>
        /// <param name="Class">The class of outpoint to search for.</param>
        /// <param name="Subclass">The subclass of outpoint to search for.</param>
        /// <param name="request">A pointer to the request structure (Also used to store the result(s)).</param>
        /// <returns>True if the request was successful. Otherwise, false.</returns>
        public static bool GetPipeOutpoints(Process CallerProcess, PipeClasses Class, PipeSubclasses Subclass,
            PipeOutpointsRequest* request)
        {
            // Validate inputs & get caller process
            if (CallerProcess == null)
            {
                return false;
            }

            // Need access to the request structure
            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

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
                        (anOutpoint.MaxConnections == PipeConstants.UnlimitedConnections ||
                         anOutpoint.NumConnections < anOutpoint.MaxConnections))
                    {
                        // Set the resultant values
                        request->Outpoints[j++].ProcessId = anOutpoint.ProcessId;
                    }
                }
            }

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return OK;
        }

        /// <summary>
        ///     Attempts to create a new pipe.
        /// </summary>
        /// <param name="InProcessId">The Id of the target process which owns the inpoint.</param>
        /// <param name="OutProcessId">The Id of the target process which owns the outpoint.</param>
        /// <param name="request">A pointer to the request structure (Also used to store the result).</param>
        /// <returns>True if the request was successful. Otherwise, false.</returns>
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
            if (ProcessManager.GetProcessById(OutProcessId) == null)
            {
                return false;
            }
            if (request == null)
            {
                return false;
            }

            // Need access to the request structure
            bool ShouldDisableKernelAccessToProcessMemory = true;
            ProcessManager.EnableKernelAccessToProcessMemory(InProcessId);

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

                    ShouldDisableKernelAccessToProcessMemory = false;
                    ProcessManager.DisableKernelAccessToProcessMemory(InProcessId);

                    // Wake any threads (/processes) which were waiting on a pipe to be created
                    WakeWaitingThreads(outpoint, inpoint, pipe);
                }
            }

            if (ShouldDisableKernelAccessToProcessMemory)
            {
                ProcessManager.DisableKernelAccessToProcessMemory(InProcessId);
            }

            return OK;
        }

        /// <summary>
        ///     Wakes all threads waiting on a pipe to be created for the specified outpoint.
        /// </summary>
        /// <param name="outpoint">The outpoint to wake waiting threads of.</param>
        /// <param name="newPipeId">The Id of the newly created pipe.</param>
        private static void WakeWaitingThreads(PipeOutpoint outpoint, PipeInpoint inpoint, Pipe NewPipe)
        {
            while (outpoint.WaitingThreads.Count > 0)
            {
                ulong identifier = outpoint.WaitingThreads[0];
                outpoint.WaitingThreads.RemoveAt(0);

                uint processId = (uint)(identifier >> 32);
                uint threadId = (uint)identifier;

                Process process = ProcessManager.GetProcessById(processId);
                Thread thread = ProcessManager.GetThreadById(threadId, process);

                ProcessManager.EnableKernelAccessToProcessMemory(process);

                WaitOnPipeCreateRequest* request = (WaitOnPipeCreateRequest*)thread.Param1;
                request->Result.Id = NewPipe.Id;
                request->Result.BufferSize = NewPipe.BufferSize;
                request->Result.Class = outpoint.Class;
                request->Result.Subclass = outpoint.Subclass;
                request->Result.InpointProcessId = inpoint.ProcessId;
                request->Result.OutpointProcessId = outpoint.ProcessId;

                thread.Return1 = (uint)SystemCallResults.OK;
                thread.Return2 = 0;
                thread.Return3 = 0;
                thread.Return4 = 0;
                ProcessManager.DisableKernelAccessToProcessMemory(process);
                thread._Wake();
            }
        }

        /// <summary>
        ///     Attempts to add the specified thread to the list of threads waiting on a pipe to be created for the specified
        ///     outpoint.
        /// </summary>
        /// <param name="OutProcessId">The Id of the process which owns the outpoint and the thread to block.</param>
        /// <param name="OutThreadId">The Id of the thread to block.</param>
        /// <param name="request">The wait request.</param>
        /// <returns>True if the request was successful. Otherwise, false.</returns>
        public static bool WaitOnPipeCreate(uint OutProcessId, uint OutThreadId, WaitOnPipeCreateRequest* request)
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

            ProcessManager.EnableKernelAccessToProcessMemory(OutProcess);

            // Find the outpoint
            PipeOutpoint outpoint = GetOutpoint(OutProcessId, request->Class, request->Subclass);

            // Check that we actually found the outpoint
            if (outpoint == null)
            {
                return false;
            }

            // Mark the outpoint as being waited on by the specified process/thread
            outpoint.WaitingThreads.Add(((ulong)OutProcessId << 32) | OutThreadId);

            ProcessManager.DisableKernelAccessToProcessMemory(OutProcess);

            return true;
        }

        /// <summary>
        ///     Determines whether the specified process is allowed to read from the specified pipe.
        /// </summary>
        /// <param name="ThePipe">The pipe to check.</param>
        /// <param name="ProcessId">The Id of the process to check.</param>
        /// <returns>True if the process is allowed. Otherwise, false.</returns>
        private static bool AllowedToReadPipe(Pipe ThePipe, uint ProcessId)
        {
            return ThePipe.Inpoint.ProcessId == ProcessId;
        }

        /// <summary>
        ///     Determines whether the specified process is allowed to write to the specified pipe.
        /// </summary>
        /// <param name="ThePipe">The pipe to check.</param>
        /// <param name="ProcessId">The Id of the process to check.</param>
        /// <returns>True if the process is allowed. Otherwise, false.</returns>
        private static bool AllowedToWritePipe(Pipe ThePipe, uint ProcessId)
        {
            return ThePipe.Outpoint.ProcessId == ProcessId;
        }

        /// <summary>
        ///     Attempts to read from the specified pipe.
        /// </summary>
        /// <remarks>
        ///     Note that this function is non-blocking. It will, however, block a system caller thread by simply not returning it
        ///     from
        ///     the deferred system call.
        /// </remarks>
        /// <param name="PipeId">The Id of the pipe to read.</param>
        /// <param name="Blocking">Whether the read should be blocking or non-blocking.</param>
        /// <param name="CallerProcess">The process which made the call.</param>
        /// <param name="CallerThread">The thread which made the call.</param>
        /// <returns>See descriptions on <see cref="PipeManager.RWResults" /> values.</returns>
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
            BasicConsole.WriteLine("ReadPipe: Adding caller to read queue");
#endif
            // Add caller thread to the read queue
            pipe.QueueToRead(CallerThread.Id);

            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
            // Set up initial failure return value
            CallerThread.Return1 = (uint)SystemCallResults.Fail;
            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

#if PIPES_TRACE
            BasicConsole.WriteLine("ReadPipe: Processing pipe queue");
#endif
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);

            bool Completed = !pipe.AreThreadsWaitingToRead();
            if (!Blocking)
            {
                if (!Completed)
                {
                    uint temp;
                    bool removed = pipe.RemoveLastToRead(out temp);

                    if (!removed || temp != CallerThread.Id)
                    {
                        BasicConsole.WriteLine(
                            "PipeManager: Error! Async read failed and then removing last from queue resulted in thread Id mismatch!");
                    }

                    ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                    CallerThread.Return1 = (uint)SystemCallResults.Fail;
                    ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);
                    CallerThread._Wake();
                }
            }
            return Completed ? RWResults.Complete : RWResults.Queued;
        }

        /// <summary>
        ///     Attempts to write to the specified pipe.
        /// </summary>
        /// <remarks>
        ///     Note that this function is non-blocking. It will, however, block a system caller thread by simply not returning it
        ///     from
        ///     the deferred system call.
        /// </remarks>
        /// <param name="PipeId">The Id of the pipe to write.</param>
        /// <param name="Blocking">Whether the write should be blocking or non-blocking.</param>
        /// <param name="CallerProcess">The process which made the call.</param>
        /// <param name="CallerThread">The thread which made the call.</param>
        /// <returns>See descriptions on <see cref="PipeManager.RWResults" /> values.</returns>
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
            BasicConsole.WriteLine("WritePipe: Adding caller to write queue");
#endif
            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

            // Add caller thread to the write queue
            WritePipeRequest* Request = (WritePipeRequest*)CallerThread.Param1;
            pipe.QueueToWrite(CallerThread.Id, Request->Length);

            // Set up initial failure return value
            CallerThread.Return1 = (uint)SystemCallResults.Fail;

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

#if PIPES_TRACE
            BasicConsole.WriteLine("WritePipe: Processing pipe queue");
#endif
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);

            bool Completed = !pipe.AreThreadsWaitingToWrite();
            if (!Blocking)
            {
                if (!Completed)
                {
                    uint temp;
                    bool removed = pipe.RemoveLastToWrite(out temp);

                    if (!removed || temp != CallerThread.Id)
                    {
                        BasicConsole.WriteLine(
                            "PipeManager: Error! Async write failed and then removing last from queue resulted in thread Id mismatch!");
                    }

                    ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);
                    CallerThread.Return1 = (uint)SystemCallResults.Fail;
                    ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

                    CallerThread._Wake();
                }
            }
            return Completed ? RWResults.Complete : RWResults.Queued;
        }

        public static bool AbortPipeReadWrite(int PipeId, Process CallerProcess)
        {
            // Validate inputs
            //  - Check pipe exists
            Pipe pipe = GetPipe(PipeId);
            if (pipe == null)
            {
                return false;
            }

            // Check the caller is allowed to access the pipe
            if (!AllowedToWritePipe(pipe, CallerProcess.Id) && !AllowedToReadPipe(pipe, CallerProcess.Id))
            {
                return false;
            }

            // Get outpoint process
            Process OutProcess = ProcessManager.GetProcessById(pipe.Outpoint.ProcessId);
            if (OutProcess == null)
            {
                return false;
            }

            // Get inpoint process
            Process InProcess = ProcessManager.GetProcessById(pipe.Inpoint.ProcessId);
            if (InProcess == null)
            {
                return false;
            }

            if (CallerProcess.Id == InProcess.Id)
            {
                // Read Request
                if (pipe.AreThreadsWaitingToRead())
                {
                    uint ThreadId;
                    if (!pipe.PeekToRead(out ThreadId))
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("Pipe Manager > Couldn't identify waiting read thread.");
#endif
                        return false;
                    }

                    Thread ReadThread = ProcessManager.GetThreadById(ThreadId, InProcess);
                    if (ReadThread == null)
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("Pipe Manager > Couldn't get waiting read thread.");
#endif
                        return false;
                    }

                    ProcessManager.EnableKernelAccessToProcessMemory(InProcess);

                    ReadPipeRequest* Request = (ReadPipeRequest*)ReadThread.Param1;
                    Request->Aborted = true;
#if PIPES_TRACE
                    BasicConsole.WriteLine("Pipe Manager > Set waiting read request to aborted.");
#endif

                    ProcessManager.DisableKernelAccessToProcessMemory(InProcess);
                }
                else
                {
#if PIPES_TRACE
                    BasicConsole.WriteLine("Pipe Manager > No read to abort.");
#endif
                    // Clean return if nothing was waiting
                    return true;
                }
            }
            else
            {
                // Write Request

                if (pipe.AreThreadsWaitingToWrite())
                {
                    uint ThreadId;
                    if (!pipe.PeekToWrite(out ThreadId))
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("Pipe Manager > Couldn't identify waiting write thread.");
#endif
                        return false;
                    }

                    Thread WriteThread = ProcessManager.GetThreadById(ThreadId, OutProcess);
                    if (WriteThread == null)
                    {
#if PIPES_TRACE
                        BasicConsole.WriteLine("Pipe Manager > Couldn't get waiting write thread.");
#endif
                        return false;
                    }

                    ProcessManager.EnableKernelAccessToProcessMemory(OutProcess);

                    WritePipeRequest* Request = (WritePipeRequest*)WriteThread.Param1;
                    Request->Aborted = true;
#if PIPES_TRACE
                    BasicConsole.WriteLine("Pipe Manager > Set waiting write request to aborted.");
#endif

                    ProcessManager.DisableKernelAccessToProcessMemory(OutProcess);
                }
                else
                {
#if PIPES_TRACE
                    BasicConsole.WriteLine("Pipe Manager > No write to abort.");
#endif
                    // Clean return if nothing was waiting
                    return true;
                }
            }

#if PIPES_TRACE
            BasicConsole.WriteLine("Pipe Manager > Processing pipe queue (after abort)...");
#endif
            // Process the pipe queue
            ProcessPipeQueue(pipe, OutProcess, InProcess);
#if PIPES_TRACE
            BasicConsole.WriteLine("Pipe Manager > Pipe queue processed (after abort).");
#endif

            bool Completed;
            if (CallerProcess.Id == InProcess.Id)
            {
                // Read Request
                Completed = !pipe.AreThreadsWaitingToRead();
            }
            else
            {
                // Write Request
                Completed = !pipe.AreThreadsWaitingToWrite();
            }

#if PIPES_TRACE
            if (Completed)
            {
                BasicConsole.WriteLine("Pipe Manager > Abort completed.");
            }
            else
            {
                BasicConsole.WriteLine("Pipe Manager > Abort failed.");
            }
#endif

            return Completed;
        }

        /// <summary>
        ///     Process the read/write queues of the specified pipe.
        /// </summary>
        /// <param name="pipe">The pipe to process.</param>
        /// <param name="OutProcess">The process which owns the pipe's outpoint.</param>
        /// <param name="InProcess">The process which owns the pipe's inpoint.</param>
        private static void ProcessPipeQueue(Pipe pipe, Process OutProcess, Process InProcess)
        {
#if PIPES_TRACE
            BasicConsole.WriteLine("ProcessPipeQueue: Checking first loop condition");
#endif
            bool AbortWritePossible = true;
            bool AbortReadPossible = true;
            bool WritePossible = true;
            bool ReadPossible = true;

            while ((pipe.AreThreadsWaitingToWrite() && (AbortWritePossible || WritePossible)) ||
                   (pipe.AreThreadsWaitingToRead() && (AbortReadPossible || ReadPossible)))
            {
#if PIPES_TRACE
                BasicConsole.WriteLine("ProcessPipeQueue: Loop start");
#endif
                if (pipe.AreThreadsWaitingToWrite())
                {
#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Pipe waiting to write");
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
                    uint ThreadId;
                    if (pipe.PeekToWrite(out ThreadId))
                    {

#if PIPES_TRACE
                        BasicConsole.WriteLine("ProcessPipeQueue: Getting out thread");
#endif
                        Thread WriteThread = ProcessManager.GetThreadById(ThreadId, OutProcess);
                        if (WriteThread != null)
                        {

#if PIPES_TRACE
                            BasicConsole.WriteLine("ProcessPipeQueue: Writing pipe");
#endif
                            ProcessManager.EnableKernelAccessToProcessMemory(OutProcess);

                            WritePipeRequest* Request = (WritePipeRequest*)WriteThread.Param1;
                            bool Successful = false;
                            bool CanWrite = WritePossible = pipe.CanWrite();
                            bool ReachedResult = Request->Aborted || CanWrite;
                            if (!Request->Aborted)
                            {
                                if (CanWrite)
                                {
                                    pipe.DequeueToWrite(out ThreadId); // Will return the same thread id as peek did
                                    Successful = pipe.Write(Request->InBuffer, Request->Offset, Request->Length);
                                }
                                else
                                {
                                    AbortWritePossible = false;
                                }
                            }
                            else
                            {
#if PIPES_TRACE
                                BasicConsole.WriteLine("ProcessPipeQueue: Write request aborted.");
#endif

                                // Dequeue the aborted request
                                pipe.DequeueToWrite(out ThreadId); // Will return the same thread id as peek did
                            }

                            if (ReachedResult)
                            {
                                if (Successful)
                                {
#if PIPES_TRACE
                                    BasicConsole.WriteLine("ProcessPipeQueue: Write successful");
#endif
                                    WriteThread.Return1 = (uint)SystemCallResults.OK;
                                }
                                else
                                {
#if PIPES_TRACE
                                    BasicConsole.WriteLine("ProcessPipeQueue: Write failed");
#endif
                                    WriteThread.Return1 = (uint)SystemCallResults.Fail;
                                }

                                ProcessManager.DisableKernelAccessToProcessMemory(OutProcess);

                                WriteThread._Wake();
                            }
                            else
                            {
                                ProcessManager.DisableKernelAccessToProcessMemory(OutProcess);
                            }
                        }
                    }
                }

                if (!(pipe.AreThreadsWaitingToWrite() && WritePossible) && pipe.AreThreadsWaitingToRead())
                {
#if PIPES_TRACE
                    BasicConsole.WriteLine("ProcessPipeQueue: Pipe waiting to read");
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
                    uint ThreadId;
                    if (pipe.PeekToRead(out ThreadId))
                    {

#if PIPES_TRACE
                        BasicConsole.WriteLine("ProcessPipeQueue: Getting in thread");
#endif
                        Thread ReadThread = ProcessManager.GetThreadById(ThreadId, InProcess);
                        if (ReadThread != null)
                        {

#if PIPES_TRACE
                            BasicConsole.WriteLine("ProcessPipeQueue: Reading pipe");
#endif
                            ProcessManager.EnableKernelAccessToProcessMemory(InProcess);

                            ReadPipeRequest* Request = (ReadPipeRequest*)ReadThread.Param1;
                            int BytesRead = 0;
                            bool Successful = false;
                            bool CanRead = ReadPossible = pipe.CanRead();
                            bool ReachedResult = Request->Aborted || CanRead;
                            if (!Request->Aborted)
                            {
#if PIPES_TRACE
                                BasicConsole.WriteLine("ProcessPipeQueue: Read request not aborted");
#endif

                                if (CanRead)
                                {
#if PIPES_TRACE
                                    BasicConsole.WriteLine("ProcessPipeQueue: Can read");
#endif

                                    pipe.DequeueToRead(out ThreadId); // Will return the same thread id as peek did
                                    Successful = pipe.Read(Request->OutBuffer, Request->Offset, Request->Length,
                                        out BytesRead);
                                }
                                else
                                {
#if PIPES_TRACE
                                    BasicConsole.WriteLine("ProcessPipeQueue: AbortReadPossible = false");
#endif

                                    AbortReadPossible = false;
                                }
                            }
                            else
                            {
#if PIPES_TRACE
                                BasicConsole.WriteLine("ProcessPipeQueue: Read request aborted.");
#endif

                                // Dequeue the aborted request
                                pipe.DequeueToRead(out ThreadId); // Will return the same thread id as peek did
                            }

                            if (ReachedResult)
                            {
#if PIPES_TRACE
                                BasicConsole.WriteLine("ProcessPipeQueue: Reached read result");
#endif

                                if (Successful)
                                {
#if PIPES_TRACE
                                    BasicConsole.WriteLine("ProcessPipeQueue: Read successful");
#endif
                                    ReadThread.Return1 = (uint)SystemCallResults.OK;
                                    ReadThread.Return2 = (uint)BytesRead;
                                }
                                else
                                {
#if PIPES_TRACE
                                    BasicConsole.WriteLine("ProcessPipeQueue: Read failed");
#endif
                                    ReadThread.Return1 = (uint)SystemCallResults.Fail;
                                }

                                ProcessManager.DisableKernelAccessToProcessMemory(InProcess);

                                ReadThread._Wake();
                            }
                            else
                            {
#if PIPES_TRACE
                                BasicConsole.WriteLine("ProcessPipeQueue: Not reached read result");
#endif

                                ProcessManager.DisableKernelAccessToProcessMemory(InProcess);
                            }
                        }
                    }
                }

#if PIPES_TRACE
                BasicConsole.WriteLine("ProcessPipeQueue: Looping...");
                BasicConsole.Write(pipe.AreThreadsWaitingToWrite()); BasicConsole.Write(" ");
                BasicConsole.Write(AbortWritePossible); BasicConsole.Write(" ");
                BasicConsole.Write(WritePossible); BasicConsole.Write(" ");
                BasicConsole.Write(pipe.AreThreadsWaitingToRead()); BasicConsole.Write(" ");
                BasicConsole.Write(AbortReadPossible); BasicConsole.Write(" ");
                BasicConsole.WriteLine(ReadPossible);
#endif
            }
        }

        /// <summary>
        ///     Gets the outpoint from the specified process of the desired class and subclass.
        /// </summary>
        /// <param name="OutProcessId">The Id of the process which owns the outpoint.</param>
        /// <param name="Class">The class of the outpoint.</param>
        /// <param name="Subclass">The subclass of the outpoint.</param>
        /// <returns>The outpoint or null if not found.</returns>
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

        /// <summary>
        ///     Gets the pipe with the specified Id.
        /// </summary>
        /// <param name="PipeId">The Id of the pipe to get.</param>
        /// <returns>The pipe or null if it is not found.</returns>
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