using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes;
using Kernel.Pipes.PCI;
using NotSupportedException = Kernel.Framework.Exceptions.NotSupportedException;

namespace Kernel.PCI
{
    public static class PCIAccessor
    {
        private static PCIPortAccessInpoint Inpoint;
        private static PCIPortAccessOutpoint Outpoint;
        private static int OutPipeId;

        public static void Init()
        {
            if (Outpoint == null)
            {
                Outpoint = new PCIPortAccessOutpoint(1, true);
            }

            if (Inpoint == null)
            { 
                int numOutpoints;
                SystemCallResults SysCallResult;
                BasicOutpoint.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, PipeClasses.PCI,
                    PipeSubclasses.PCI_PortAccess_Out);

                if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
                {
                    PipeOutpointDescriptor[] OutpointDescriptors;
                    BasicOutpoint.GetOutpointDescriptors(numOutpoints, out SysCallResult, out OutpointDescriptors,
                        PipeClasses.PCI, PipeSubclasses.PCI_PortAccess_Out);

                    if (SysCallResult == SystemCallResults.OK)
                    {
                        PipeOutpointDescriptor Descriptor = OutpointDescriptors[0];
                        
                        Inpoint = new PCIPortAccessInpoint(Descriptor.ProcessId, false);

                        uint InProcessId;
                        //BasicConsole.WriteLine("PCIAccessor: Init: Waiting for connection...");
                        OutPipeId = Outpoint.WaitForConnect(out InProcessId);
                        //BasicConsole.WriteLine("PCIAccessor: Init: Connected.");
                        if (InProcessId != Descriptor.ProcessId)
                        {
                            BasicConsole.WriteLine("PCIAccessor: Init: Wrong process connected.");
                            ExceptionMethods.Throw(
                                new ArgumentException(
                                    "PCIAccessor: Process that connected wasn't the expected PCI driver process."));
                        }
                    }
                    else
                    {
                        ExceptionMethods.Throw(new NotSupportedException("PCIAccessor: No PCI driver to connect to!"));
                    }
                }
                else
                {
                    ExceptionMethods.Throw(new NotSupportedException("PCIAccessor: No PCI driver to connect to!"));
                }
            }
        }

        public static uint AccessPorts(uint Address, bool Read, byte DataSize, uint Data)
        {
            if (Outpoint != null)
            {
                //BasicConsole.WriteLine("PCIAccessor: AccessPorts: Sending command...");
                Outpoint.SendCommand(OutPipeId, Address, Read, DataSize, Data);
                //BasicConsole.WriteLine("PCIAccessor: AccessPorts: Command sent.");
                if (Read)
                {
                    //BasicConsole.WriteLine("PCIAccessor: AccessPorts: Reading result...");
                    uint Result;
                    if (Inpoint.ReadData(out Result))
                    {
                        //BasicConsole.WriteLine("PCIAccessor: AccessPorts: Result read. Returning...");
                        //BasicConsole.WriteLine("PCIAccessor: AccessPorts: Result read. Value: " + (String)Result);
                        return Result;
                    }
                    else
                    {
                        BasicConsole.WriteLine("PCIAccessor: AccessPorts: Couldn't read from register.");
                        ExceptionMethods.Throw(new Exception("PCIAccessor: Could not read from register."));
                    }
                }
                return 0;
            }
            return 0;
        }
    }
}
