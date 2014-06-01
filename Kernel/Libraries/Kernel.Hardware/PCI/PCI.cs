using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.PCI
{
    /// <summary>
    /// 
    /// </summary>
    public static class PCI
    {
        internal static IO.IOPort ConfigAddressPort = new IO.IOPort(0xCF8);
        internal static IO.IOPort ConfigDataPort = new IO.IOPort(0xCFC);

        public static List Devices = new List();

        public static void Init()
        {
            EnumerateDevices();
        }

        public static PCIDevice GetDevice(ushort VendorID, ushort DeviceID)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                if (((PCIDevice)Devices[i]).VendorID == VendorID && ((PCIDevice)Devices[i]).DeviceID == DeviceID)
                    return ((PCIDevice)Devices[i]);
            }
            return null;
        }

        [Compiler.NoDebug]
        private static void EnumerateDevices()
        {
            EnumerateBus(0, 0);
        }

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
