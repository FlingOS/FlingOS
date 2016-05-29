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
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.IO;

namespace Kernel.Devices.Timers
{
    //TODO: RTC should inherit from Devices.Timer and implement the necessary functions

    /// <remarks>
    ///     Code (lightly) adapted from CMOS sample on OSDev.org : http://wiki.osdev.org/CMOS
    /// </remarks>
    public class RTC : Device
    {
        private const uint CURRENT_YEAR = 2016;

        private readonly IOPort AddressPort = new IOPort(0x70);
        private readonly IOPort DataPort = new IOPort(0x71);
        // TODO: Set by ACPI table parsing code if the century register is available
        internal byte century_register = 0x00;
        private byte day;
        private byte hour;
        private byte minute;
        private byte month;

        private byte second;
        private uint year;

        public RTC()
            : base(DeviceGroup.System, DeviceClass.Timer, DeviceSubClass.Clock, "Real Time Clock", new uint[0], true)
        {
        }

        private bool GetUpdateInProgressFlag()
        {
            AddressPort.Write_Byte(0x0A);
            return (DataPort.Read_Byte() & 0x80) != 0;
        }

        private byte GetRTCRegister(byte reg)
        {
            AddressPort.Write_Byte(reg);
            return DataPort.Read_Byte();
        }

        private void UpdateTime()
        {
            byte century = 0x0;
            byte last_second;
            byte last_minute;
            byte last_hour;
            byte last_day;
            byte last_month;
            uint last_year;
            byte last_century;
            byte registerB;

            // Note: This uses the "read registers until you get the same values twice in a row" technique
            //       to avoid getting dodgy/inconsistent values due to RTC updates

            while (GetUpdateInProgressFlag()) ; // Make sure an update isn't in progress
            second = GetRTCRegister(0x00);
            minute = GetRTCRegister(0x02);
            hour = GetRTCRegister(0x04);
            day = GetRTCRegister(0x07);
            month = GetRTCRegister(0x08);
            year = GetRTCRegister(0x09);
            if (century_register != 0)
            {
                century = GetRTCRegister(century_register);
            }

            do
            {
                last_second = second;
                last_minute = minute;
                last_hour = hour;
                last_day = day;
                last_month = month;
                last_year = year;
                last_century = century;

                while (GetUpdateInProgressFlag()) ; // Make sure an update isn't in progress
                second = GetRTCRegister(0x00);
                minute = GetRTCRegister(0x02);
                hour = GetRTCRegister(0x04);
                day = GetRTCRegister(0x07);
                month = GetRTCRegister(0x08);
                year = GetRTCRegister(0x09);
                if (century_register != 0)
                {
                    century = GetRTCRegister(century_register);
                }
            } while ((last_second != second) || (last_minute != minute) || (last_hour != hour) ||
                     (last_day != day) || (last_month != month) || (last_year != year) ||
                     (last_century != century));

            registerB = GetRTCRegister(0x0B);

            // Convert BCD to binary values if necessary

            if ((registerB & 0x04) == 0)
            {
                second = (byte)((second & 0x0F) + second/16*10);
                minute = (byte)((minute & 0x0F) + minute/16*10);
                hour = (byte)(((hour & 0x0F) + (hour & 0x70)/16*10) | (hour & 0x80));
                day = (byte)((day & 0x0F) + day/16*10);
                month = (byte)((month & 0x0F) + month/16*10);
                year = (byte)((year & 0x0F) + year/16*10);
                if (century_register != 0)
                {
                    century = (byte)((century & 0x0F) + century/16*10);
                }
            }

            // Convert 12 hour clock to 24 hour clock if necessary

            if ((registerB & 0x02) == 0 && (hour & 0x80) != 0)
            {
                hour = (byte)(((hour & 0x7F) + 12)%24);
            }

            // Calculate the full (4-digit) year

            if (century_register != 0)
            {
                year += (uint)(century*100);
            }
            else
            {
                year += CURRENT_YEAR/100*100;
                if (year < CURRENT_YEAR) year += 100;
            }
        }

        public DateTime GetDateTime()
        {
            UpdateTime();
            return new DateTime(second, minute, hour, day, month, year);
        }

        public ulong GetUTCTime()
        {
            UpdateTime();
            return DateTime.ToUTC(second, minute, hour, day, month, year);
        }
    }
}