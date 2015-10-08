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
            // Vaildate inputs
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
            numOutpoints = 0;

            for (int i = 0; i < PipeOutpoints.Count; i++)
            {
                PipeOutpoint anOutpoint = (PipeOutpoint)PipeOutpoints[i];

                if (anOutpoint.Class == Class &&
                    anOutpoint.Subclass == Subclass)
                {
                    numOutpoints++;
                }
            }

            return true;
        }
        public static bool GetPipeOutpoints(PipeClasses Class, PipeSubclasses Subclass, out PipeOutpoint[] outpoints)
        {
            outpoints = new PipeOutpoint[PipeOutpoints.Count];
            for (int i = 0; i < PipeOutpoints.Count; i++)
            {
                outpoints[i] = (PipeOutpoint)PipeOutpoints[i];
            }
            return true;
        }
        
        public static bool CreatePipe(uint InProcessId, uint OutProcessId, PipeClasses Class, PipeSubclasses Subclass, out Pipe pipe)
        {
            pipe = null;
            return false;
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
