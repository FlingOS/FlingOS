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

using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Multiprocessing;

namespace Kernel.Devices
{
    /// <summary>
    ///     The global device manager for the kernel.
    /// </summary>
    public static unsafe class DeviceManager
    {
        private static ulong IdGenerator;

        /// <summary>
        ///     The list of all the devices registered in the OS.
        /// </summary>
        /// <remarks>
        ///     Some items may be more specific instances of a device so duplicate references to one physical device may
        ///     exist. For example, a PCIDevice instance and a EHCI instance would both exist for one physical EHCI device.
        /// </remarks>
        private static List Devices;

        private static bool Initialised = false;
        private static bool ProcessInit = false;

        public static bool Ready
        {
            get { return ProcessInit; }
        }

        public static void Init()
        {
            IdGenerator = 0xFFFF + 2; // I/O POrts reserve id range 1 to (0xFFFF+1)
            Devices = new List(0x100 + 40, 0x100);
            Initialised = true;
            
            Device NewDevice = new Device();
            NewDevice.Id = IdGenerator++;
            NewDevice.Group = DeviceGroup.System;
            NewDevice.Class = DeviceClass.IO;
            NewDevice.SubClass = DeviceSubClass.Port;

            NewDevice.Name = "IOPort";

            NewDevice.Info = new uint[16];
            NewDevice.Info[0] = (uint)Serial.Serial.COMPorts.COM1;

            NewDevice.Claimed = true;
            NewDevice.OwnerProcessId = ProcessManager.KernelProcess.Id;

            Devices.Add(NewDevice);
        }

        public static void InitForProcess()
        {
            ProcessInit = true;
        }

        public static SystemCallResults RegisterDevice(Device TheDevice)
        {
            if (!ProcessInit)
            {
                ExceptionMethods.Throw(new NotSupportedException("RegisterDevice called without initialising the Device Manager for the current process."));
            }

            SystemCallResults result = SystemCallResults.Fail;
            DeviceDescriptor* descriptor =
                (DeviceDescriptor*)Heap.AllocZeroed((uint)sizeof(DeviceDescriptor), "DeviceManager : Register Device");

            try
            {
                TheDevice.FillDeviceDescriptor(descriptor, true);

                ulong DeviceId;
                result = SystemCalls.RegisterDevice(descriptor, out DeviceId);
                if (result == SystemCallResults.OK)
                {
                    TheDevice.Id = DeviceId;
                }
                else
                {
                    BasicConsole.WriteLine("DeviceManager: Error! Failed to register device! Device Id not updated.");
                }
            }
            finally
            {
                if (descriptor != null)
                {
                    Heap.Free(descriptor);
                }
            }

            return result;
        }

        public static int GetNumDevices()
        {
            if (!ProcessInit)
            {
                ExceptionMethods.Throw(new NotSupportedException("GetNumDevices called without initialising the Device Manager for the current process."));
            }

            int result;
            if (SystemCalls.GetNumDevices(out result) != SystemCallResults.OK)
            {
                result = -1;
            }
            return result;
        }

        public static List GetDeviceList()
        {
            if (!ProcessInit)
            {
                ExceptionMethods.Throw(new NotSupportedException("GetDeviceList called without initialising the Device Manager for the current process."));
            }

            List result = null;

            int NumDevices = GetNumDevices();
            if (NumDevices > -1)
            {
                result = new List(NumDevices);

                if (NumDevices > 0)
                {
                    DeviceDescriptor* DeviceList =
                        (DeviceDescriptor*)
                            Heap.AllocZeroed((uint)(sizeof(DeviceDescriptor)*NumDevices),
                                "DeviceManager : GetDeviceList");

                    try
                    {
                        if (SystemCalls.GetDeviceList(DeviceList, NumDevices) == SystemCallResults.OK)
                        {
                            for (int i = 0; i < NumDevices; i++)
                            {
                                DeviceDescriptor* descriptor = DeviceList + i;
                                result.Add(new Device(descriptor));
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("GetDeviceList failed!");
                            result = null;
                        }
                    }
                    finally
                    {
                        if (DeviceList != null)
                        {
                            Heap.Free(DeviceList);
                        }
                    }
                }
                else
                {
                    BasicConsole.WriteLine("GetNumDevices returned 0!");
                }
            }
            else
            {
                BasicConsole.WriteLine("GetNumDevices returned -1!");
            }

            return result;
        }

        public static bool FillDeviceInfo(Device TheDevice)
        {
            if (!ProcessInit)
            {
                ExceptionMethods.Throw(new NotSupportedException("FillDeviceInfo called without initialising the Device Manager for the current process."));
            }

            bool result = false;
            DeviceDescriptor* descriptor =
                (DeviceDescriptor*)Heap.AllocZeroed((uint)sizeof(DeviceDescriptor), "DeviceManager : FillDeviceInfo");

            try
            {
                if (SystemCalls.GetDeviceInfo(TheDevice.Id, descriptor) == SystemCallResults.OK)
                {
                    TheDevice.FillDevice(descriptor);
                    result = true;
                }
                else
                {
                    BasicConsole.WriteLine("FillDeviceInfo failed!");
                    result = false;
                }
            }
            finally
            {
                if (descriptor != null)
                {
                    Heap.Free(descriptor);
                }
            }
            return result;
        }

        public static bool ClaimDevice(Device TheDevice)
        {
            if (!ProcessInit)
            {
                ExceptionMethods.Throw(new NotSupportedException("ClaimDevice called without initialising the Device Manager for the current process."));
            }

            return SystemCalls.ClaimDevice(TheDevice.Id) == SystemCallResults.OK;
        }

        public static bool ReleaseDevice(Device TheDevice)
        {
            if (!ProcessInit)
            {
                ExceptionMethods.Throw(new NotSupportedException("ReleaseDevice called without initialising the Device Manager for the current process."));
            }

            return SystemCalls.ReleaseDevice(TheDevice.Id) == SystemCallResults.OK;
        }


        public static SystemCallResults RegisterDevice(DeviceDescriptor* TheDescriptor, out ulong DeviceId,
            Process CallerProcess)
        {
            if (!Initialised)
            {
                DeviceId = 0;
                return SystemCallResults.Unhandled;
            }

            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

            Device NewDevice = new Device();
            NewDevice.Id = DeviceId = IdGenerator++;
            NewDevice.Group = TheDescriptor->Group;
            NewDevice.Class = TheDescriptor->Class;
            NewDevice.SubClass = TheDescriptor->SubClass;

            int NameLength = 0;
            for (int i = 0; i < 64; i++)
            {
                if (TheDescriptor->Name[i] == '\0')
                {
                    NameLength = i;
                    break;
                }
            }
            String Name = String.New(NameLength);
            for (int i = 0; i < NameLength; i++)
            {
                Name[i] = TheDescriptor->Name[i];
            }
            NewDevice.Name = Name;

            NewDevice.Info = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                NewDevice.Info[i] = TheDescriptor->Info[i];
            }

            NewDevice.Claimed = TheDescriptor->Claimed;
            NewDevice.OwnerProcessId = NewDevice.Claimed ? CallerProcess.Id : 0;

            Devices.Add(NewDevice);

            //BasicConsole.Write("Registering new device:");
            //BasicConsole.WriteLine(NewDevice.Id);
            //BasicConsole.WriteLine((uint)NewDevice.Group);
            //BasicConsole.WriteLine((uint)NewDevice.Class);
            //BasicConsole.WriteLine((uint)NewDevice.SubClass);
            //BasicConsole.WriteLine(NewDevice.Name);
            //for (int i = 0; i < NewDevice.Info.Length; i++)
            //{
            //    BasicConsole.Write(NewDevice.Info[i]);
            //    if (i < NewDevice.Info.Length - 1)
            //    {
            //        BasicConsole.Write(", ");
            //    }
            //}
            //BasicConsole.WriteLine();
            //BasicConsole.WriteLine(NewDevice.Claimed);
            //BasicConsole.WriteLine(NewDevice.OwnerProcessId);
            //BasicConsole.WriteLine("------------------------");

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return SystemCallResults.OK;
        }

        public static SystemCallResults DeregisterDevice(ulong DeviceId, Process CallerProcess)
        {
            if (!Initialised)
            {
                return SystemCallResults.Unhandled;
            }

            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            if (!TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }
            if (TheDevice.OwnerProcessId != CallerProcess.Id)
            {
                return SystemCallResults.Fail;
            }

            Devices.Remove(TheDevice);

            return SystemCallResults.OK;
        }

        public static SystemCallResults GetNumDevices(out int NumDevices, Process CallerProcess)
        {
            if (!Initialised)
            {
                NumDevices = 0;
                return SystemCallResults.Unhandled;
            }

            NumDevices = Devices.Count;
            return SystemCallResults.OK;
        }

        public static SystemCallResults GetDeviceList(DeviceDescriptor* DeviceList, int MaxDescriptors,
            Process CallerProcess)
        {
            if (!Initialised)
            {
                return SystemCallResults.Unhandled;
            }

            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

            int pos = 0;
            for (int i = 0; i < Devices.Count && pos < MaxDescriptors; i++)
            {
                Device aDevice = (Device)Devices[i];
                DeviceDescriptor* TheDescriptor = DeviceList + pos++;
                aDevice.FillDeviceDescriptor(TheDescriptor, aDevice.Claimed && aDevice.OwnerProcessId == CallerProcess.Id);
            }

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return SystemCallResults.OK;
        }

        public static SystemCallResults GetDeviceInfo(ulong DeviceId, DeviceDescriptor* TheDescriptor,
            Process CallerProcess)
        {
            if (!Initialised)
            {
                return SystemCallResults.Unhandled;
            }

            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            if (!TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }
            if (TheDevice.OwnerProcessId != CallerProcess.Id)
            {
                return SystemCallResults.Fail;
            }

            TheDevice.FillDeviceDescriptor(TheDescriptor, true);

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return SystemCallResults.OK;
        }

        public static SystemCallResults ClaimDevice(ulong DeviceId, Process CallerProcess)
        {
            if (!Initialised)
            {
                return SystemCallResults.Unhandled;
            }

            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            if (TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }

            TheDevice.Claimed = true;
            TheDevice.OwnerProcessId = CallerProcess.Id;

            return SystemCallResults.OK;
        }

        public static SystemCallResults ReleaseDevice(ulong DeviceId, Process CallerProcess)
        {
            if (!Initialised)
            {
                return SystemCallResults.Unhandled;
            }

            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            if (!TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }
            if (TheDevice.OwnerProcessId != CallerProcess.Id)
            {
                return SystemCallResults.Fail;
            }

            TheDevice.OwnerProcessId = 0;
            TheDevice.Claimed = false;

            return SystemCallResults.OK;
        }

        public static Device GetDevice(ulong Id)
        {
            if (!Initialised)
            {
                return null;
            }

            for (int i = 0; i < Devices.Count; i++)
            {
                Device aDevice = (Device)Devices[i];
                if (aDevice.Id == Id)
                {
                    return aDevice;
                }
            }

            if (Id > 0 && Id <= 0xFFFF)
            {
                // Allocate I/O Port
                Device NewDevice = new Device();
                NewDevice.Id = Id;
                NewDevice.Group = DeviceGroup.System;
                NewDevice.Class = DeviceClass.IO;
                NewDevice.SubClass = DeviceSubClass.Port;

                NewDevice.Name = "IOPort";

                NewDevice.Info = new uint[16];
                NewDevice.Info[0] = (uint)(Id - 1);

                NewDevice.Claimed = false;
                NewDevice.OwnerProcessId = 0;

                Devices.Add(NewDevice);

                return NewDevice;
            }

            return null;
        }

        //}
        //    return null;
        //    }
        //        }
        //            return device;
        //        {
        //        if (device._Type == DeviceType)
        //        Device device = (Device)Devices[i];
        //    {
        //    for (int i = 0; i < Devices.Count; i++)
        //{

        //public static Device FindDevice(Framework.Type DeviceType)
    }
}
