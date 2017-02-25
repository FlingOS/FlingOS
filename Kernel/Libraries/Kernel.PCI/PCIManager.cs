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

#define PCI_TRACE
#undef PCI_TRACE

using Drivers.Compiler.Attributes;
using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Pipes.PCI;
using Kernel.Utilities;

namespace Kernel.PCI
{
    /// <summary>
    ///     Provides methods for managing PCI access.
    /// </summary>
    public static class PCIManager
    {
        /// <summary>
        ///     The configuration address port.
        /// </summary>
        private static IOPort ConfigAddressPort;

        /// <summary>
        ///     The configuration data port.
        /// </summary>
        private static IOPort ConfigDataPort;

        /// <summary>
        ///     List of all the PCI devices found.
        /// </summary>
        public static List Devices;

        private static Pipes.PCI.PCIPortAccessOutpoint AccessOutpoint;
        private static uint WaitForAccessorsThreadId;
        private static int PortAccessSemaphoreId;

        public static bool Terminating;

        /// <summary>
        ///     Initialises the PCI bus by enumerating all connected devices.
        /// </summary>
        public static void Init()
        {
            Terminating = false;

            ConfigAddressPort = new IOPort(0xCF8);
            ConfigDataPort = new IOPort(0xCFC);
            Devices = new List();

            PortAccessSemaphoreId = -1;
        }

        public static void StartAccessorsThread()
        {
            AccessOutpoint = new PCIPortAccessOutpoint(-1, false);

            if (SystemCalls.CreateSemaphore(1, out PortAccessSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("PCI Manager > Failed to create semaphore!");
                ExceptionMethods.Throw(new NullReferenceException());
            }

            if (SystemCalls.StartThread(WaitForAccessors, out WaitForAccessorsThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("PCI Manager > Failed to create accessor listener thread!");
            }
        }

        /// <summary>
        ///     Enumerates all connected PCI devices.
        /// </summary>
        [NoDebug]
        public static void EnumerateDevices()
        {
            EnumerateBus(0, 0);
        }

        /// <summary>
        ///     Enumerates a particular PCI bus for connected devices.
        /// </summary>
        /// <param name="bus">The bus to enumerate.</param>
        /// <param name="step">The number of steps from the root bus.</param>
        [NoDebug]
        private static void EnumerateBus(uint bus, uint step)
        {
            for (uint device = 0; device < 32; device++)
            {
                PCIDevice zeroFuncDevice = new PCIDevice(bus, device, 0x00, "Generic PCI Device");
                if (zeroFuncDevice.DeviceExists)
                {
                    uint max = ((uint)zeroFuncDevice.HeaderType & 0x80) != 0 ? 8u : 1u;

                    for (uint function = 0; function < max; function++)
                    {
                        PCIDevice pciDevice = new PCIDevice(bus, device, function, "Generic PCI Device");
                        if (pciDevice.DeviceExists)
                        {
                            if (pciDevice.HeaderType == PCIDevice.PCIHeaderType.Bridge)
                            {
                                AddDevice(new PCIDeviceBridge(bus, device, function), step);
                            }
                            else if (pciDevice.HeaderType == PCIDevice.PCIHeaderType.Cardbus)
                            {
                                AddDevice(new PCIDeviceCardbus(bus, device, function), step);
                            }
                            else
                            {
                                AddDevice(new PCIDeviceNormal(bus, device, function), step);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Adds a PCI device to the list of devices. Enumerates the secondary bus if it is available.
        /// </summary>
        /// <param name="device">The device to add.</param>
        /// <param name="step">The number of steps from the root bus.</param>
        [NoDebug]
        private static void AddDevice(PCIDevice device, uint step)
        {
            Devices.Add(device);
            DeviceManager.RegisterDevice(device);

            if (device is PCIDeviceBridge)
            {
#if PCI_TRACE
                BasicConsole.WriteLine("Enumerating PCI Bridge Device...");
                BasicConsole.DelayOutput(5);
#endif

                EnumerateBus(((PCIDeviceBridge)device).SecondaryBusNumber, step + 1);
            }
        }

        private static unsafe void WaitForAccessors()
        {
            while (!Terminating)
            {
                uint RemoteProcessId;
                int NewPipeId = AccessOutpoint.WaitForConnect(out RemoteProcessId);

                uint NewThreadId;
                if (SystemCalls.StartThread(ObjectUtilities.GetHandle((ManageAccessorDelegate)ManageAccessor), out NewThreadId, new uint[] { RemoteProcessId, (uint)NewPipeId }) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("PCI Manager > Failed to create accessor management thread!");
                }
            }
        }

        private delegate void ManageAccessorDelegate(uint RemoteProcessId, int OutPipeId, uint CS);
        private static void ManageAccessor(uint RemoteProcessId, int OutPipeId, uint CS)
        {
            //BasicConsole.WriteLine("PCIManager: ManageAccessor: Connecting inpoint...");
            PCIPortAccessInpoint Inpoint = new PCIPortAccessInpoint(RemoteProcessId, true);
            //BasicConsole.WriteLine("PCIManager: ManageAccessor: Connected.");

            while (!Terminating)
            {
                try
                {
                    uint Address;
                    bool Read;
                    byte DataSize;
                    uint Data;
                    //BasicConsole.WriteLine("PCIManager: ManageAccessor: Waiting for command...");
                    if (Inpoint.ReadCommand(out Address, out Read, out DataSize, out Data))
                    {
                        //BasicConsole.WriteLine("PCIManager: ManageAccessor: Read command. Accessing ports...");
                        Data = AccessPorts(Address, Read, DataSize, Data);
                        //BasicConsole.WriteLine("PCIManager: ManageAccessor: Accessed ports.");
                        if (Read)
                        {
                            //BasicConsole.WriteLine("PCIManager: ManageAccessor: Sending data result..." + (String)Data);
                            AccessOutpoint.SendData(OutPipeId, Data);
                            //BasicConsole.WriteLine("PCIManager: ManageAccessor: Result sent.");
                        }
                    }
                    else
                    {
                        BasicConsole.WriteLine("PCIManager: ManageAccessor: Couldn't read command.");
                        //TODO: Error handling
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("PCIManager: ManageAccessor: Error handling command:");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        public static uint AccessPorts(uint Address, bool Read, byte DataSize, uint Data)
        {
            uint Result = 0;

            if (PortAccessSemaphoreId != -1)
            {
                //BasicConsole.WriteLine("PCIManager: AccessPorts: Wait semaphore...");
                SystemCallResults WaitResult = SystemCalls.WaitSemaphore(PortAccessSemaphoreId);
                //BasicConsole.WriteLine("PCIManager: AccessPorts: Wait semaphore done.");

                if (WaitResult == SystemCallResults.OK)
                {
                    try
                    {
                        Result = DoAccessPorts(Address, Read, DataSize, Data, Result);
                    }
                    finally
                    {
                        if (SystemCalls.SignalSemaphore(PortAccessSemaphoreId) != SystemCallResults.OK)
                        {
                            BasicConsole.WriteLine("PCIManager: AccessPorts: Couldn't signal lock.");
                            ExceptionMethods.Throw(
                                new Exception("Could not release lock of PCI register ports."));
                        }
                    }
                }
                else
                {
                    BasicConsole.WriteLine("PCIManager: AccessPorts: Couldn't obtain lock. SemaphoreId=" +
                                           (String)PortAccessSemaphoreId + ", WaitResult=" + (String)(int)WaitResult);
                    ExceptionMethods.Throw(
                        new Exception("Could not obtain lock to access PCI register ports. SemaphoreId=" +
                                      (String)PortAccessSemaphoreId + ", WaitResult=" + (String)(int)WaitResult));
                }
            }
            else
            {
                Result = DoAccessPorts(Address, Read, DataSize, Data, Result);
            }

            return Result;
        }

        private static uint DoAccessPorts(uint Address, bool Read, byte DataSize, uint Data, uint Result)
        {
            ConfigAddressPort.Write_UInt32(Address);

            if (Read)
            {
                switch (DataSize)
                {
                    case 1:
                        Result = ConfigDataPort.Read_UInt32();
                        break;
                    case 2:
                        Result = ConfigDataPort.Read_UInt32();
                        break;
                    case 4:
                        Result = ConfigDataPort.Read_UInt32();
                        break;
                    default:
                        BasicConsole.WriteLine("PCIManager: AccessPorts: Bad data size.");
                        ExceptionMethods.Throw(
                            new NotSupportedException("Can only read 1, 2, 4 bytes from PCI register ports."));
                        Result = 0;
                        break;
                }
            }
            else
            {
                switch (DataSize)
                {
                    case 1:
                        ConfigDataPort.Write_Byte((byte)Data);
                        break;
                    case 2:
                        ConfigDataPort.Write_UInt16((ushort)Data);
                        break;
                    case 4:
                        ConfigDataPort.Write_UInt32(Data);
                        break;
                    default:
                        BasicConsole.WriteLine("PCIManager: AccessPorts: Bad data size.");
                        ExceptionMethods.Throw(
                            new NotSupportedException("Can only read 1, 2, 4 bytes from PCI register ports."));
                        break;
                }
            }
            return Result;
        }
    }
}