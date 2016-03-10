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

        public static SystemCallResults RegisterDevice(DeviceDescriptor* DeviceDescriptor, out ulong DeviceId, Process CallerProcess)
        {
            DeviceId = IdGenerator++;
            return SystemCallResults.Fail;
        }
        public static SystemCallResults DeregisterDevice(ulong DeviceId, Process CallerProcess)
        {
            return SystemCallResults.Fail;
        }

        public static SystemCallResults GetNumDevices(out int NumDevices, Process CallerProcess)
        {
            NumDevices = 0;
            return SystemCallResults.Fail;
        }
        public static SystemCallResults GetDeviceList(DeviceDescriptor* DeviceList, int MaxDescriptors, Process CallerProcess)
        {
            return SystemCallResults.Fail;
        }

        public static SystemCallResults GetDeviceInfo(ulong DeviceId, DeviceDescriptor* DeviceDescriptor, Process CallerProcess)
        {
            return SystemCallResults.Fail;
        }
        
        public static SystemCallResults ClaimDevice(ulong DeviceId, Process CallerProcess)
        {
            return SystemCallResults.Fail;
        }
        public static SystemCallResults ReleaseDevice(ulong DeviceId, Process CallerProcess)
        {
            return SystemCallResults.Fail;
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
