using System;
using System.Linq;
using System.Text;
using ELFSharp.ELF;

namespace CI20Booter
{
    /// <summary>
    ///     Adapted from Lardcave.net's usbloader.py (my adaptation for Windows fork: https://github.com/EdNutting/ci20-os)
    /// </summary>
    /// <remarks>
    ///     Requires LibUSBDotNet (and its dependency libusb-win32) to be installed. Also requires ELFSharp.
    ///     CI20 device driver must be installed via libusb-win32's ".Inf Install Wizard".
    ///     http://sourceforge.net/projects/libusbdotnet/
    ///     http://sourceforge.net/projects/libusb-win32/
    ///     http://elfsharp.hellsgate.pl/index.shtml
    /// </remarks>
    internal class Program
    {
        private const int VendorID = 0xA108;
        private const int ProductID = 0x4780;

        private static void Main(string[] args)
        {
            var devices = LibUsbDotNet.UsbDevice.AllDevices;
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];

                Console.WriteLine(devices[i].FullName);
            }

            try
            {
                ELF<uint> elfFile = ELFReader.Load<uint>(args[0]);

                USBLoader loader = new USBLoader(VendorID, ProductID);
                try
                {
                    loader.BootStage1(elfFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    loader.Cleanup();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }

    public class USBLoader
    {
        private const byte VR_GET_CPU_INFO = 0;
        private const byte VR_SET_DATA_ADDRESS = 1;
        private const byte VR_SET_DATA_LENGTH = 2;
        private const byte VR_FLUSH_CACHES = 3;
        private const byte VR_PROGRAM_START1 = 4;
        private const byte VR_PROGRAM_START2 = 5;

        private const byte BM_REQUEST_DEVICE_TO_HOST = 1 << 7;

        private const uint JZ4780_TCSM_START = 0xf4000800;
        private const uint JZ4780_TCSM_END = 0xf4004000;
        private const int JZ4780_TCSM_BANK_SIZE = 0x800;

        private static readonly string[] KNOWN_CPUS = {"JZ4780V1"};
        private readonly string CPUInfo;

        private IUsbDevice dev;

        public USBLoader(int VendorID, int ProductID)
        {
            InitUSB(VendorID, ProductID);
            CPUInfo = GetCPUInfo();

            if (KNOWN_CPUS.Contains(CPUInfo))
            {
                Console.WriteLine("Found CPU: {0}", CPUInfo);
            }
            else
            {
                Console.WriteLine("Unknown CPU: {0}", CPUInfo);
            }
        }

        public void BootStage1(ELF<uint> elfFile)
        {
            foreach (var segment in elfFile.Segments)
            {
                byte[] data = segment.GetContents();

                if (IsAddressInTCSM(segment.Address))
                {
                    data = PadData(data, JZ4780_TCSM_BANK_SIZE);
                }

                Console.WriteLine("Writing from {0:X8} to {1:X8}", segment.Address, segment.Address + data.Length);
                SetDataAddress(segment.Address);
                SendData(data);
            }

            FlushCaches();
            Console.WriteLine("Executing at {0:X8}", elfFile.EntryPoint);
            Start2(elfFile.EntryPoint);
        }

        private bool IsAddressInTCSM(uint address)
        {
            return address >= JZ4780_TCSM_START && address < JZ4780_TCSM_END;
        }

        private byte[] PadData(byte[] data, int PadLength)
        {
            if (data.Length%PadLength != 0)
            {
                return data.Concat(new byte[PadLength - data.Length%PadLength]).ToArray();
            }
            return data;
        }

        private void InitUSB(int VendorID, int ProductID)
        {
            dev = (IUsbDevice)UsbDevice.OpenUsbDevice(x => x.Vid == VendorID && x.Pid == ProductID);

            if (dev == null)
            {
                throw new NullReferenceException(
                    "Cannot find USB device for JZ4780. Check Vendor/Product IDs and that LibUSB-Win32 \".inf\" installed.");
            }

            dev.SetConfiguration(1);
        }

        private string GetCPUInfo()
        {
            byte[] buffer = new byte[8];
            int bytesRead = SendControlTransfer(buffer, VR_GET_CPU_INFO, 0, 0, 0);
            return Encoding.Default.GetString(buffer, 0, bytesRead);
        }

        private void SetDataAddress(uint address)
        {
            SendControlTransfer(VR_SET_DATA_ADDRESS, address);
        }

        private void SetDataLength(uint length)
        {
            SendControlTransfer(VR_SET_DATA_LENGTH, length);
        }

        private void FlushCaches()
        {
            SendControlTransfer(VR_FLUSH_CACHES);
        }

        private void Start1(uint entrypoint)
        {
            SendControlTransfer(VR_PROGRAM_START1, entrypoint);
        }

        private void Start2(uint entrypoint)
        {
            SendControlTransfer(VR_PROGRAM_START2, entrypoint);
        }

        private void SendData(byte[] data)
        {
            using (var writer = dev.OpenEndpointWriter(WriteEndpointID.Ep01))
            {
                int transferred;
                ErrorCode ec = writer.Write(data, 10000, out transferred);
                if (ec != ErrorCode.None)
                {
                    throw new Exception("Data not transferred! (Send) Error: " + ec.ToString() + " : " +
                                        LibUsbDotNet.UsbDevice.LastErrorString);
                }
                if (transferred != data.Length)
                {
                    throw new Exception("Data not fully transferred! (Send)");
                }
            }
        }

        private void ReadData(byte[] data)
        {
            using (var reader = dev.OpenEndpointReader(ReadEndpointID.Ep01))
            {
                int transferred;
                ErrorCode ec = reader.Read(data, 10000, out transferred);
                if (ec != ErrorCode.None)
                {
                    throw new Exception("Data not transferred! (Read) Error: " + ec.ToString() + " : " +
                                        LibUsbDotNet.UsbDevice.LastErrorString);
                }
                if (transferred != data.Length)
                {
                    throw new Exception("Data not fully transferred! (Read)");
                }
            }
        }

        private void SendControlTransfer(byte request)
        {
            SendControlTransfer(request, 0, 0, 0);
        }

        private void SendControlTransfer(byte request, uint value)
        {
            SendControlTransfer(request, (short)(value >> 16), (short)(value & 0xFFFF), 0);
        }

        private void SendControlTransfer(byte request, short value, short index, short outLength)
        {
            SendControlTransfer(null, request, value, index, outLength);
        }

        private int SendControlTransfer(byte[] buffer, byte request, short value, short index, short outLength)
        {
            LibUsbDotNet.Main.UsbSetupPacket setupPacket =
                new LibUsbDotNet.Main.UsbSetupPacket((byte)UsbRequestType.TypeVendor | BM_REQUEST_DEVICE_TO_HOST,
                    request, value, index, outLength);
            int bytesRead = 0;
            dev.ControlTransfer(ref setupPacket, buffer, 8, out bytesRead);
            return bytesRead;
        }

        public void Cleanup()
        {
            dev.ReleaseInterface(0);
            dev.Close();
            dev = null;
        }
    }
}