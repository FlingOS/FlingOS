using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.USB.HCIs
{
    /// <summary>
    /// Represents a generic USB Host Controller Interface.
    /// </summary>
    public abstract class HCI : Device
    {
        /// <summary>
        /// The underlying PCI device for the host controller.
        /// </summary>
        protected PCI.PCIDeviceNormal pciDevice;

        /// <summary>
        /// Initializes a new generic host controller interface using the specified PCI device.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the HCI device.</param>
        public HCI(PCI.PCIDeviceNormal aPCIDevice)
            : base()
        {
            pciDevice = aPCIDevice;
        }
    }
}
