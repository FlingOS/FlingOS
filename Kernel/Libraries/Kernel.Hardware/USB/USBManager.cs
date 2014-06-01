using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.PCI;
using Kernel.Hardware.USB.HCIs;

namespace Kernel.Hardware.USB
{
    /// <summary>
    /// Provides methods for managing USB access.
    /// </summary>
    public static class USBManager
    {
        /// <summary>
        /// The number of UHCI devices detected.
        /// </summary>
        public static int NumUHCIDevices
        {
            get;
            private set;
        }
        /// <summary>
        /// The number of OHCI devices detected.
        /// </summary>
        public static int NumOHCIDevices
        {
            get;
            private set;
        }

        /// <summary>
        /// List of all the HCI device instances.
        /// </summary>
        public static List HCIDevices = new List(1);
        
        /// <summary>
        /// Initialises USB management. Scans the PCI bus for HCIs and initialises any supported HCIs that are found.
        /// </summary>
        public static void Init()
        {
            //Enumerate PCI devices looking for (unclaimed) USB host controllers

            //      UHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x00
            //      OHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x10
            //      EHCI:  Class ID: 0x0C, Sub-class: 0x03, Prog(ramming) Interface: 0x20
            
            for (int i = 0; i < PCI.PCI.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice)(PCI.PCI.Devices[i]);
                //0x0C = Serial bus controllers
                if (aDevice.ClassCode == 0x0C)
                {
                    //0x03 = USB controllers
                    if (aDevice.Subclass == 0x03)
                    {
                        //UHCI = 0x00
                        if (aDevice.ProgIF == 0x00)
                        {
                            //UHCI detected
                            //BasicConsole.WriteLine("UHCI detected.");

                            //TODO - Add UHCI support
                            //Supported by VMWare and my laptop 
                            //  - This is USB 1.0/1.1.

                            NumUHCIDevices++;
                        }
                        //OHCI = 0x10
                        else if (aDevice.ProgIF == 0x10)
                        {
                            //OHCI detected
                            //BasicConsole.WriteLine("OHCI detected.");

                            //TODO - Add OHCI support
                            //Not supported by VMWare or my laptop 
                            //  so we aren't going to program this any further for now.

                            NumOHCIDevices++;
                        }
                        //EHCI = 0x20
                        else if (aDevice.ProgIF == 0x20)
                        {
                            //EHCI detected
                            //BasicConsole.WriteLine("EHCI detected.");

                            PCIDeviceNormal EHCI_PCIDevice = (PCIDeviceNormal)aDevice;
                            EHCI_PCIDevice.Claimed = true;

                            EHCI newEHCI = new EHCI(EHCI_PCIDevice);
                            
                            HCIDevices.Add(newEHCI);
                            DeviceManager.Devices.Add(newEHCI);
                        }
                    }
                }
            }
        }
    }
}
