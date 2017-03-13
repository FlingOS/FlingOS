using Kernel.Consoles;
using Kernel.Devices;
using Kernel.Framework.Processes;
using Kernel.PCI;
using Kernel.Utilities;
using Kernel.VGA.VMWare;

namespace Kernel.Tasks.Driver
{
    public static class PCIDriverTask
    {
        private static VirtualConsole console;

        private static uint GCThreadId;

        private static bool Terminating = false;

        public static unsafe void Main()
        {
            Helpers.ProcessInit("PCI Driver", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("PCI Driver > Creating virtual console...");
                console = new VirtualConsole();

                BasicConsole.WriteLine("PCI Driver > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("PCI Driver > Executing.");

                DeviceManager.InitForProcess();

                try
                {
                    BasicConsole.WriteLine("PCI Driver > Initialising PCI Manager...");
                    PCIManager.Init();

                    BasicConsole.WriteLine("PCI Driver > Enumerating PCI devices...");
                    PCIManager.EnumerateDevices();

                    BasicConsole.WriteLine("PCI Driver > Starting accessors thread...");
                    PCIManager.StartAccessorsThread();

                    //BasicConsole.WriteLine("PCI Driver > Outputting PCI info...");
                    //OutputPCI();

                    SVGAII svga = null;
                    for (int i = 0; i < PCIManager.Devices.Count; i++)
                    {
                        PCIDevice aDevice = (PCIDevice)PCIManager.Devices[i];
                        if (aDevice.VendorID == SVGAII_Registers.PCI_VENDOR_ID && 
                            aDevice.DeviceID == SVGAII_Registers.PCI_DEVICE_ID)
                        {
                            BasicConsole.WriteLine("PCI Driver > Found an VMWare SVGA-II...");
                            svga = new SVGAII((PCIDeviceNormal)aDevice);
                            break;
                        }
                    }

                    if (svga != null)
                    {
                        GMR gmr = new GMR();
                        gmr.Init(svga);
                        
                        svga.SetMode(0, 0, 32);
                        
                        Screen.Init(svga);
                        
                        SVGAII_Registers.ScreenObject screenObject = new SVGAII_Registers.ScreenObject()
                        {
                            StructSize = (uint)sizeof(SVGAII_Registers.ScreenObject),
                            Id = 0,
                            Flags = (uint)(SVGAII_Registers.Screen.HAS_ROOT | SVGAII_Registers.Screen.IS_PRIMARY),
                            Size = new SVGAII_Registers.UnsignedDimensions()
                            {
                                Width = 960,
                                Height = 540
                            },
                            Root = new SVGAII_Registers.SignedPoint()
                            {
                                X = -500,
                                Y = 10000
                            }
                        };
                        
                        Screen.Create(svga, &screenObject);
                        
                        uint gmrId = 0;
                        byte bitsPerPixel = 32;
                        byte colourDepth = 24;

                        uint bytesPerPixel = (uint)bitsPerPixel >> 3;
                        uint fbBytesPerLine = screenObject.Size.Width * bytesPerPixel;
                        uint fbSizeInBytes = fbBytesPerLine * (screenObject.Size.Height + 10);
                        uint fbSizeInPages = (fbSizeInBytes + GMR.PAGE_MASK) / GMR.PAGE_SIZE;

                        uint fbFirstPage = gmr.DefineContiguous(svga, gmrId, fbSizeInPages);
                        uint* fbPointer = (uint*)GMR.PPN_POINTER(fbFirstPage);

                        SVGAII_Registers.GuestPointer fbGuestPtr = new SVGAII_Registers.GuestPointer()
                        {
                            GMRId = gmrId,
                            Offset = 0
                        };

                        SVGAII_Registers.GMRImageFormat fbFormat = new SVGAII_Registers.GMRImageFormat()
                        {
                            BitsPerPixel = bitsPerPixel,
                            ColourDepth = colourDepth
                        };
                        
                        Screen.DefineGMRFB(svga, fbGuestPtr, fbBytesPerLine, fbFormat);

                        SVGAII_Registers.SignedPoint blitOrigin = new SVGAII_Registers.SignedPoint()
                        {
                            X = 0,
                            Y = 0
                        };

                        SVGAII_Registers.SignedRectangle blitDest = new SVGAII_Registers.SignedRectangle()
                        {
                            Left = 0,
                            Top = 0,
                            Right = (int)screenObject.Size.Width,
                            Bottom = (int)screenObject.Size.Height
                        };

                        fbSizeInBytes = fbSizeInBytes - (fbBytesPerLine * 10);

                        byte* startPtr = (byte*)fbPointer;
                        byte* maxCPtr = startPtr + fbSizeInBytes;
                        
                        uint GreyLevels = 135;
                        uint Height = 270;
                        uint linesPerGreyIncrease = Height / GreyLevels;
                        uint bytesPerBand = fbBytesPerLine * linesPerGreyIncrease;

                        byte* cPtr = startPtr;
                        for (uint i = 40; i < GreyLevels + 40; i++, cPtr += bytesPerBand)
                        {
                            if (cPtr >= maxCPtr)
                            {
                                cPtr = (byte*)fbPointer;
                            }

                            MemoryUtils.MemSet(cPtr, (byte)i, bytesPerBand);
                        }

                        for (uint i = 40; i < GreyLevels + 40; i++, cPtr += bytesPerBand)
                        {
                            if (cPtr >= maxCPtr)
                            {
                                cPtr = (byte*)fbPointer;
                            }

                            MemoryUtils.MemSet(cPtr, (byte)((GreyLevels + 80) - i), bytesPerBand);
                        }

                        MemoryUtils.MemCpy(maxCPtr, maxCPtr - fbBytesPerLine, fbBytesPerLine);

                        while (!Terminating)
                        {
                            MemoryUtils.MemCpy(maxCPtr, startPtr, fbBytesPerLine * 10);
                            MemoryUtils.MemCpy(startPtr, startPtr + (fbBytesPerLine * 10), fbSizeInBytes);

                            Screen.BlitFromGMRFB(svga, &blitOrigin, &blitDest, screenObject.Id);

                            uint dmaFence = svga.InsertFence();

                            svga.SyncToFence(dmaFence);
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("PCI Driver > Error executing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }

                BasicConsole.WriteLine("PCI Driver > Execution complete.");
            }
            catch
            {
                BasicConsole.WriteLine("PCI Driver > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.WriteLine("PCI Driver > Exiting...");
        }

        /// <summary>
        ///     Outputs the PCI system information.
        /// </summary>
        private static void OutputPCI()
        {
            for (int i = 0; i < PCIManager.Devices.Count; i++)
            {
                PCIDevice aDevice = (PCIDevice)PCIManager.Devices[i];
                console.WriteLine(PCIDevice.DeviceClassInfo.GetString(aDevice));
                console.Write(" - Address: ");
                console.Write(aDevice.bus);
                console.Write(":");
                console.Write(aDevice.slot);
                console.Write(":");
                console.WriteLine(aDevice.function);

                console.Write(" - Vendor Id: ");
                console.WriteLine(aDevice.VendorID);

                console.Write(" - Device Id: ");
                console.WriteLine(aDevice.DeviceID);
            }
        }
    }
}