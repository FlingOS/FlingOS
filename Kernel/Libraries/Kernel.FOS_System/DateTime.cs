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

namespace Kernel.FOS_System
{
    public class DateTime : FOS_System.Object
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

        public FOS_System.String ToString()
        {
            return FOS_System.Int32.ToDecimalString((int)Year) + "-" +
                   FOS_System.Int32.ToDecimalString(Month).PadLeft(2, '0') + "-" +
                   FOS_System.Int32.ToDecimalString(Day).PadLeft(2, '0') + " " +
                   FOS_System.Int32.ToDecimalString(Hour).PadLeft(2, '0') + ":" +
                   FOS_System.Int32.ToDecimalString(Minute).PadLeft(2, '0') + ":" +
                   FOS_System.Int32.ToDecimalString(Second).PadLeft(2, '0');
        }
    }
}
