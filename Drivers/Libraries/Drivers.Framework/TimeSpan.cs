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
    public class TimeSpan : Framework.Object
    {
        private System.Int64 seconds;
        
        //TODO: Remove cast downs when signed 64-bit division is supported

        public int Years
        {
            get
            {
                int castTime = (int)seconds;
                return (castTime / 31556926);
            }
        }
        public int Months
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= (castTime / 31556926) * 31556926;
                return (castTime / 2629743);
            }
        }
        public int Days
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= (castTime / 31556926) * 31556926;
                castTime -= (castTime / 2629743) * 2629743;
                return (castTime / 86400);
            }
        }
        public int Hours
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= (castTime / 31556926) * 31556926;
                castTime -= (castTime / 2629743) * 2629743;
                castTime -= (castTime / 86400) * 86400;
                return (castTime / 3600);
            }
        }
        public int Minutes
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= (castTime / 31556926) * 31556926;
                castTime -= (castTime / 2629743) * 2629743;
                castTime -= (castTime / 86400) * 86400;
                castTime -= (castTime / 3600) * 3600;
                return (castTime / 60);
            }
        }
        public int Seconds
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= (castTime / 31556926) * 31556926;
                castTime -= (castTime / 2629743) * 2629743;
                castTime -= (castTime / 86400) * 86400;
                castTime -= (castTime / 3600) * 3600;
                castTime -= (castTime / 60) * 60;
                return castTime;
            }
        }

        public TimeSpan(System.Int64 aSeconds)
        {
            seconds = aSeconds;
        }

        public Framework.String ToShortString()
        {
            return Framework.Int32.ToDecimalString(Years) + ":" +
                   Framework.Int32.ToDecimalString(Months) + ":" +
                   Framework.Int32.ToDecimalString(Days) + ":" +
                   Framework.Int32.ToDecimalString(Hours) + ":" +
                   Framework.Int32.ToDecimalString(Minutes) + ":" +
                   Framework.Int32.ToDecimalString(Seconds);
        }
        public Framework.String ToLongString()
        {
            return Framework.Int32.ToDecimalString(Years) + " years, " +
                   Framework.Int32.ToDecimalString(Months) + " months, " +
                   Framework.Int32.ToDecimalString(Days) + " days, " +
                   Framework.Int32.ToDecimalString(Hours) + " hours, " +
                   Framework.Int32.ToDecimalString(Minutes) + " minutes, " +
                   Framework.Int32.ToDecimalString(Seconds) + " seconds.";
        }
    }
}
