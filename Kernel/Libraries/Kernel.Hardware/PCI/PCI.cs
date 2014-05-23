using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.PCI
{
    public class PCI : FOS_System.Object
    {
        public IO.IOPort ConfigAddressPort = new IO.IOPort(0xCF8);
        public IO.IOPort ConfigDataPort = new IO.IOPort(0xCFC);

        private static PCIDevice[] devices;
        private static int currDeviceIndex = 0;

        public static int NumDevices
        {
            get
            {
                return currDeviceIndex + 1;
            }
        }

        public static void Init()
        {
            EnumerateDevices();
        }

        public static PCIDevice GetDevice(ushort VendorID, ushort DeviceID)
        {
            for (int i = 0; i < ((FOS_System.Array)devices).length; i++)
            {
                if (devices[i].VendorID == VendorID && devices[i].DeviceID == DeviceID)
                    return devices[i];
            }
            return null;
        }

        [Compiler.NoDebug]
        private static void EnumerateDevices()
        {
            devices = new PCIDevice[1024];
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
            devices[currDeviceIndex++] = device;

            if (device._Type == (FOS_System.Type)(typeof(PCIDeviceBridge)))
            {
                EnumerateBus(((PCIDeviceBridge)device).SecondaryBusNumber, step + 1);
            }
        }
    }
}
