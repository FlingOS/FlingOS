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
    
using System;

using Kernel.FOS_System.Collections;

namespace Kernel.Hardware
{
    public delegate void DeviceAddedHandler(FOS_System.Object state, Device device);
    /// <summary>
    /// The global device manager for the kernel.
    /// </summary>
    public static class DeviceManager
    {
        /// <summary>
        /// The list of all the devices detected.
        /// </summary>
        /// <remarks>
        /// Some items may be more specific instances of a device so duplicate references to one physical device may 
        /// exist. For example, a PCIDevice instance and a EHCI instance would both exist for one physical EHCI device.
        /// </remarks>
        public static List Devices;

        public static List DeviceAddedListeners;

        public static void Init()
        {
            Devices = new List(20);
            DeviceAddedListeners = new List();
        }

        public static void AddDevice(Device aDevice)
        {
            if (Devices != null)
            {
                Devices.Add(aDevice);

                NotifyDevicesNeedUpdate();
            }
        }
        public static void AddDeviceAddedListener(DeviceAddedHandler aHandler, FOS_System.Object aState)
        {
            DeviceAddedListeners.Add(new DeviceAddedListener()
            {
                handler = aHandler,
                state = aState
            });
        }

        public static void NotifyDevicesNeedUpdate()
        {
            //TODO: Implement DeviceManager.NotifyDevicesNeedUpdate properly

            //if (Tasks.DeviceManagerTask.OwnerThread != null)
            //{
            //    Tasks.DeviceManagerTask.Awake = true;
            //    Tasks.DeviceManagerTask.OwnerThread._Wake();
            //}
        }

        public static void UpdateDevices()
        {
            //TODO: Any other classes of device which need updating?

            USB.USBManager.Update();

            for (int i = 0; i < Devices.Count; i++)
            {
                Device aDevice = (Device)Devices[i];
                if (aDevice.IsNew)
                {
                    aDevice.IsNew = false;

                    for (int j = 0; j < DeviceAddedListeners.Count; j++)
                    {
                        DeviceAddedListener listener = ((DeviceAddedListener)DeviceAddedListeners[j]);
                        listener.handler(listener.state, aDevice);
                    }
                }
            }
        }
        private class DeviceAddedListener : FOS_System.Object
        {
            internal DeviceAddedHandler handler;
            internal FOS_System.Object state;
        }

        public static Device FindDevice(FOS_System.Type DeviceType)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                Device device = (Device)Devices[i];
                if (device._Type == DeviceType)
                {
                    return device;
                }
            }
            return null;
        }
    }
}
