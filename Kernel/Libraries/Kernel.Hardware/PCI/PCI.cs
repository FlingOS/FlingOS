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
        /// List of all the PCI device found.
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
        /// <param name="xBus">The bus to enumerate.</param>
        /// <param name="step">The number of steps from the root bus.</param>
        [Compiler.NoDebug]
        private static void EnumerateBus(uint xBus, uint step)
        {
            for (uint xDevice = 0; xDevice < 32; xDevice++)
            {
                PCIDevice xPCIDevice = new PCIDevice(xBus, xDevice, 0x00);
                if (xPCIDevice.DeviceExists)
                {
                    if (xPCIDevice.HeaderType == PCIDevice.PCIHeaderType.Bridge)
                    {
                        for (uint xFunction = 0; xFunction < 8; xFunction++)
                        {
                            xPCIDevice = new PCIDevice(xBus, xDevice, xFunction);
                            if (xPCIDevice.DeviceExists)
                                AddDevice(new PCIDeviceBridge(xBus, xDevice, xFunction), step);
                        }
                    }
                    else if (xPCIDevice.HeaderType == PCIDevice.PCIHeaderType.Cardbus)
                    {
                        AddDevice(new PCIDeviceCardbus(xBus, xDevice, 0x00), step);
                    }
                    else
                    {
                        AddDevice(new PCIDeviceNormal(xBus, xDevice, 0x00), step);
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

            if (device._Type == (FOS_System.Type)(typeof(PCIDeviceBridge)))
            {
                EnumerateBus(((PCIDeviceBridge)device).SecondaryBusNumber, step + 1);
            }
        }
    }
}
