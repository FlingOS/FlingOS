#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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

namespace Kernel.Debug.Debugger
{
    /// <summary>
    /// An interface between the debugger and any virtual machine that the debugger can use to start and debug an instance of Fling OS on.
    /// </summary>
    public interface IVirtualMachine : IDisposable
    {
        /// <summary>
        /// Whether the VM is powered on or not (i.e. running or not).
        /// </summary>
        bool PoweredOn
        {
            get;
        }

        /// <summary>
        /// Powers on (i.e. starts) the VM.
        /// </summary>
        /// <returns>True if the VM starts succesfully, otherwise false.</returns>
        bool PowerOn();
        /// <summary>
        /// Powers off (i.e. stops) the VM.
        /// </summary>
        /// <returns>True if the VM stops, otherwise false.</returns>
        bool PowerOff();        
    }

    /// <summary>
    /// An implementation of the IVirtualMachine interface for VMWare virtual machines.
    /// </summary>
    public class VMWare_VM : IVirtualMachine
    {
        private VixCOM.VixLibClass VIX_Lib = new VixCOM.VixLibClass();
        private bool vmPoweredOn = false;
        /// <summary>
        /// Whether the VM is powered on or not (i.e. running or not).
        /// </summary>
        public bool PoweredOn
        {
            get
            {
                return vmPoweredOn;
            }
        }

        /// <summary>
        /// Powers on (i.e. starts) the VM.
        /// </summary>
        /// <returns>True if the VM starts succesfully, otherwise false.</returns>
        public bool PowerOn()
        {
            try
            {
                VixCOM.IHost VIX_host = null;
                VixCOM.IVM2 VIX_vm = null;

                VixCOM.IJob job =
                    VIX_Lib.Connect(VixCOM.Constants.VIX_API_VERSION,
                        VixCOM.Constants.VIX_SERVICEPROVIDER_VMWARE_PLAYER,
                        null, 0, null, null, 0, null, null);

                UInt64 err;
                object results = null;
                err = job.Wait(new int[] { VixCOM.Constants.VIX_PROPERTY_JOB_RESULT_HANDLE },
                     ref results);
                if (VIX_Lib.ErrorIndicatesFailure(err))
                {
                    // Handle the error... 
                    CloseVixObject(job);
                    throw new Exception("Unkown connection error.");
                }
                else
                {
                    VIX_host = (VixCOM.IHost)((object[])results)[0];
                    CloseVixObject(job);
                }

                job = VIX_host.OpenVM(Properties.Settings.Default.VMFilePath, null);

                err = job.Wait(new int[] { VixCOM.Constants.VIX_PROPERTY_JOB_RESULT_HANDLE },
                               ref results);
                if (VIX_Lib.ErrorIndicatesFailure(err))
                {
                    // Handle the error... 
                    CloseVixObject(job);
                    throw new Exception("Failed to open VM!");
                }
                else
                {
                    VIX_vm = (VixCOM.IVM2)((object[])results)[0];
                    CloseVixObject(job);
                }

                job = VIX_vm.PowerOn(VixCOM.Constants.VIX_VMPOWEROP_LAUNCH_GUI, null, null);
                job.WaitWithoutResults();
                if (VIX_Lib.ErrorIndicatesFailure(err))
                {
                    // Handle the error... 
                    CloseVixObject(job);
                    throw new Exception("Failed to power on VM!");
                }
                else
                {
                    vmPoweredOn = true;
                    CloseVixObject(job);
                    //AddText("VM started.");
                }

                CloseVixObject(VIX_vm);
                VIX_host.Disconnect();
            }
            catch //(Exception ex)
            {
                //AddText("Failed to auto start VMWare! Error : " + ex.Message);
            }

            return PoweredOn;
        }
        /// <summary>
        /// Powers off (i.e. stops) the VM.
        /// </summary>
        /// <returns>True if the VM stops, otherwise false.</returns>
        public bool PowerOff()
        {
            if(!PoweredOn)
            {
                return true;
            }

            try
            {
                VixCOM.IHost VIX_host = null;
                VixCOM.IVM2 VIX_vm = null;

                VixCOM.IJob job =
                    VIX_Lib.Connect(VixCOM.Constants.VIX_API_VERSION,
                        VixCOM.Constants.VIX_SERVICEPROVIDER_VMWARE_PLAYER,
                        null, 0, null, null, 0, null, null);

                UInt64 err;
                object results = null;
                err = job.Wait(new int[] { VixCOM.Constants.VIX_PROPERTY_JOB_RESULT_HANDLE },
                     ref results);
                if (VIX_Lib.ErrorIndicatesFailure(err))
                {
                    // Handle the error... 
                    CloseVixObject(job);
                    throw new Exception("Unkown connection error.");
                }
                else
                {
                    VIX_host = (VixCOM.IHost)((object[])results)[0];
                    CloseVixObject(job);
                }

                job = VIX_host.OpenVM(Properties.Settings.Default.VMFilePath, null);

                err = job.Wait(new int[] { VixCOM.Constants.VIX_PROPERTY_JOB_RESULT_HANDLE },
                               ref results);
                if (VIX_Lib.ErrorIndicatesFailure(err))
                {
                    // Handle the error... 
                    CloseVixObject(job);
                    throw new Exception("Failed to open VM!");
                }
                else
                {
                    VIX_vm = (VixCOM.IVM2)((object[])results)[0];
                    CloseVixObject(job);
                }

                job = VIX_vm.PowerOff(VixCOM.Constants.VIX_VMPOWEROP_LAUNCH_GUI, null);
                job.WaitWithoutResults();
                if (VIX_Lib.ErrorIndicatesFailure(err))
                {
                    // Handle the error... 
                    CloseVixObject(job);
                    throw new Exception("Failed to power off VM!");
                }
                else
                {
                    CloseVixObject(job);
                    vmPoweredOn = false;
                }

                CloseVixObject(VIX_vm);
                VIX_host.Disconnect();
            }
            catch //(Exception ex)
            {
                //AddText("Failed to auto start VMWare! Error : " + ex.Message);
            }

            return !PoweredOn;
        }

        private void CloseVixObject(Object vixObject)
        {
            try
            {
                ((VixCOM.IVixHandle2)vixObject).Close();
            }
            catch (Exception)
            {
                //Close is not supported in this version of Vix COM - Ignore
            }
        }

        /// <summary>
        /// The necessary dispose method. Calls PowerOff.
        /// </summary>
        public void Dispose()
        {
            PowerOff();
        }
    }
}
