using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.USB.HCIs
{
    public class HCI : Device
    {
        protected PCI.PCIDeviceNormal pciDevice;

        public HCI(PCI.PCIDeviceNormal aPCIDevice)
            : base()
        {
            pciDevice = aPCIDevice;
        }
    }
}
