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

using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Devices;
using Kernel.Hardware.Processes;

namespace Kernel.Hardware.Devices
{
    /// <summary>
    /// The global device manager for the kernel.
    /// </summary>
    public static unsafe class DeviceManager
    {
        private static ulong IdGenerator;

        /// <summary>
        /// The list of all the devices detected.
        /// </summary>
        /// <remarks>
        /// Some items may be more specific instances of a device so duplicate references to one physical device may 
        /// exist. For example, a PCIDevice instance and a EHCI instance would both exist for one physical EHCI device.
        /// </remarks>
        public static List Devices;
        
        public static void Init()
        {
            IdGenerator = 1;
            Devices = new List(20);
        }

        public static SystemCallResults RegisterDevice(DeviceDescriptor* TheDescriptor, out ulong DeviceId, Process CallerProcess)
        {
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
            FOS_System.String Name = FOS_System.String.New(NameLength);
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

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return SystemCallResults.OK;
        }
        public static SystemCallResults DeregisterDevice(ulong DeviceId, Process CallerProcess)
        {
            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            else if (!TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }
            else if (TheDevice.OwnerProcessId != CallerProcess.Id)
            {
                return SystemCallResults.Fail;
            }

            Devices.Remove(TheDevice);

            return SystemCallResults.OK;
        }

        public static SystemCallResults GetNumDevices(out int NumDevices, Process CallerProcess)
        {
            int result = 0;

            for (int i = 0; i < Devices.Count; i++)
            {
                Device aDevice = (Device)Devices[i];
                if (!aDevice.Claimed || aDevice.OwnerProcessId == CallerProcess.Id)
                {
                    result++;
                }
            }

            NumDevices = result;
            return SystemCallResults.OK;
        }
        public static SystemCallResults GetDeviceList(DeviceDescriptor* DeviceList, int MaxDescriptors, Process CallerProcess)
        {
            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

            int pos = 0;
            for (int i = 0; i < Devices.Count && pos < MaxDescriptors; i++)
            {
                Device aDevice = (Device)Devices[i];
                bool OwnedByCaller = aDevice.Claimed && aDevice.OwnerProcessId == CallerProcess.Id;
                if (!aDevice.Claimed || OwnedByCaller)
                {
                    DeviceDescriptor* TheDescriptor = DeviceList + (pos++);
                    aDevice.FillDeviceDescriptor(TheDescriptor, OwnedByCaller);
                }
            }

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return SystemCallResults.OK;
        }
        
        public static SystemCallResults GetDeviceInfo(ulong DeviceId, DeviceDescriptor* TheDescriptor, Process CallerProcess)
        {
            ProcessManager.EnableKernelAccessToProcessMemory(CallerProcess);

            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            else if (!TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }
            else if (TheDevice.OwnerProcessId != CallerProcess.Id)
            {
                return SystemCallResults.Fail;
            }

            TheDevice.FillDeviceDescriptor(TheDescriptor, true);

            ProcessManager.DisableKernelAccessToProcessMemory(CallerProcess);

            return SystemCallResults.OK;
        }
        
        public static SystemCallResults ClaimDevice(ulong DeviceId, Process CallerProcess)
        {
            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            else if (TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }

            TheDevice.Claimed = true;
            TheDevice.OwnerProcessId = CallerProcess.Id;

            return SystemCallResults.OK;
        }
        public static SystemCallResults ReleaseDevice(ulong DeviceId, Process CallerProcess)
        {
            Device TheDevice = GetDevice(DeviceId);

            if (TheDevice == null)
            {
                return SystemCallResults.Fail;
            }
            else if (!TheDevice.Claimed)
            {
                return SystemCallResults.Fail;
            }
            else if (TheDevice.OwnerProcessId != CallerProcess.Id)
            {
                return SystemCallResults.Fail;
            }

            TheDevice.OwnerProcessId = 0;
            TheDevice.Claimed = false;

            return SystemCallResults.OK;
        }

        public static Device GetDevice(ulong Id)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                Device aDevice = (Device)Devices[i];
                if (aDevice.Id == Id)
                {
                    return aDevice;
                }
            }
            return null;
        }

        //public static Device FindDevice(FOS_System.Type DeviceType)
        //{
        //    for (int i = 0; i < Devices.Count; i++)
        //    {
        //        Device device = (Device)Devices[i];
        //        if (device._Type == DeviceType)
        //        {
        //            return device;
        //        }
        //    }
        //    return null;
        //}
    }
}
