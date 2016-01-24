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

namespace Drivers.Framework
{
    public class DateTime : Framework.Object
    {
        public byte Second;
        public byte Minute;
        public byte Hour;
        public byte Day;
        public byte Month;
        public uint Year;

        public DateTime(byte aSecond, byte aMinute, byte anHour, byte aDay, byte aMonth, uint aYear)
        {
            Second = aSecond;
            Minute = aMinute;
            Hour = anHour;
            Day = aDay;
            Month = aMonth;
            Year = aYear;
        }
        public DateTime(UInt64 utc)
        {
            //TODO: Decode UTC time from 64-bit value (when 64-bit division and modulo are supported)
            UInt32 castUTC = (UInt32)utc;
            Year = (castUTC / 31556926u) + 1970u;
            
            castUTC -= (Year-1970) * 31556926u;
            Month = (byte)(castUTC / 2629743u);

            castUTC -= Month * 2629743u;
            Day = (byte)(castUTC / 86400u);

            castUTC -= Day * 86400u;
            Hour = (byte)(castUTC / 3600u);

            castUTC -= Hour * 3600u;
            Minute = (byte)(castUTC / 60u);
            
            castUTC -= Minute * 60u;
            Second = (byte)(castUTC);
        }

        public Framework.String ToString()
        {
            return Framework.Int32.ToDecimalString((int)Year) + "-" +
                   Framework.Int32.ToDecimalString(Month).PadLeft(2, '0') + "-" +
                   Framework.Int32.ToDecimalString(Day).PadLeft(2, '0') + " " +
                   Framework.Int32.ToDecimalString(Hour).PadLeft(2, '0') + ":" +
                   Framework.Int32.ToDecimalString(Minute).PadLeft(2, '0') + ":" +
                   Framework.Int32.ToDecimalString(Second).PadLeft(2, '0');
        }
        public UInt64 ToUTC()
        {
            return DateTime.ToUTC(Second, Minute, Hour, Day, Month, Year);
        }
        public static UInt64 ToUTC(byte aSecond, byte aMinute, byte anHour, byte aDay, byte aMonth, uint aYear)
        {
            return ((aYear - 1970) * 31556926u) + (aMonth * 2629743u) + (aDay * 86400u) + (anHour * 3600u) + (aMinute * 60u) + aSecond;
        }
    }
}
