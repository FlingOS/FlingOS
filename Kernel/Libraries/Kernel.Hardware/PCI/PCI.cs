#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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

using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.PCI
{
    /// <summary>
    /// Provides methods for managing PCI access.
    /// </summary>
    public static class PCI
    {
        /// <summary>
        /// The configuration address port.
        /// </summary>
        internal static IO.IOPort ConfigAddressPort = new IO.IOPort(0xCF8);
        /// <summary>
        /// The configuration data port.
        /// </summary>
        internal static IO.IOPort ConfigDataPort = new IO.IOPort(0xCFC);

        /// <summary>
        /// List of all the PCI devices found.
        /// </summary>
        public static List Devices = new List();

        /// <summary>
        /// Initialises the PCI bus by enumerating all connected devices.
        /// </summary>
        public static void Init()
        {
            EnumerateDevices();
        }
        
        /// <summary>
        /// Enumerates all connected PCI devices.
        /// </summary>
        [Compiler.NoDebug]
        private static void EnumerateDevices()
        {
            EnumerateBus(0, 0);
        }

        /// <summary>
        /// Enumerates a particular PCI bus for connected devices.
        /// </summary>
        /// <param name="bus">The bus to enumerate.</param>
        /// <param name="step">The number of steps from the root bus.</param>
        [Compiler.NoDebug]
        private static void EnumerateBus(uint bus, uint step)
        {
            for (uint device = 0; device < 32; device++)
            {
                PCIDevice zeroFuncDevice = new PCIDevice(bus, device, 0x00);
                if (zeroFuncDevice.DeviceExists)
                {
                    uint max = ((uint)zeroFuncDevice.HeaderType & 0x80) != 0 ? 8u : 1u;

                    for (uint function = 0; function < max; function++)
                    {
                        PCIDevice pciDevice = new PCIDevice(bus, device, function);
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
        /// Adds a PCI device to the list of devices. Enumerates the secondary bus if it is available.
        /// </summary>
        /// <param name="device">The device to add.</param>
        /// <param name="step">The number of steps from the root bus.</param>
        [Compiler.NoDebug]
        private static void AddDevice(PCIDevice device, uint step)
        {
            Devices.Add(device);
            DeviceManager.Devices.Add(device);

            if (device is PCIDeviceBridge)
            {
#if PCI_TRACE
                BasicConsole.WriteLine("Enumerating PCI Bridge Device...");
                BasicConsole.DelayOutput(5);
#endif
                 
                EnumerateBus(((PCIDeviceBridge)device).SecondaryBusNumber, step + 1);
            }
        }
    }
}
