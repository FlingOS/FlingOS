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

namespace Kernel.Framework
{
    /// <summary>
    ///     The delegate type for a timer event handler method. An example of a timer event
    ///     is a one-shot countdown expiring or a periodic/recurring countdown ticking.
    /// </summary>
    /// <param name="State">The state object that was provided when the handler was registered.</param>
    public delegate void TimerHandler(IObject State);

    public class TimeSpan : Object
    {
        private readonly long seconds;

        //TODO: Remove cast downs when signed 64-bit division is supported

        public int Years
        {
            get
            {
                int castTime = (int)seconds;
                return castTime/31556926;
            }
        }

        public int Months
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= castTime/31556926*31556926;
                return castTime/2629743;
            }
        }

        public int Days
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= castTime/31556926*31556926;
                castTime -= castTime/2629743*2629743;
                return castTime/86400;
            }
        }

        public int Hours
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= castTime/31556926*31556926;
                castTime -= castTime/2629743*2629743;
                castTime -= castTime/86400*86400;
                return castTime/3600;
            }
        }

        public int Minutes
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= castTime/31556926*31556926;
                castTime -= castTime/2629743*2629743;
                castTime -= castTime/86400*86400;
                castTime -= castTime/3600*3600;
                return castTime/60;
            }
        }

        public int Seconds
        {
            get
            {
                int castTime = (int)seconds;
                castTime -= castTime/31556926*31556926;
                castTime -= castTime/2629743*2629743;
                castTime -= castTime/86400*86400;
                castTime -= castTime/3600*3600;
                castTime -= castTime/60*60;
                return castTime;
            }
        }

        public TimeSpan(long aSeconds)
        {
            seconds = aSeconds;
        }

        public String ToShortString()
        {
            return Int32.ToDecimalString(Years) + ":" +
                   Int32.ToDecimalString(Months) + ":" +
                   Int32.ToDecimalString(Days) + ":" +
                   Int32.ToDecimalString(Hours) + ":" +
                   Int32.ToDecimalString(Minutes) + ":" +
                   Int32.ToDecimalString(Seconds);
        }

        public String ToLongString()
        {
            return Int32.ToDecimalString(Years) + " years, " +
                   Int32.ToDecimalString(Months) + " months, " +
                   Int32.ToDecimalString(Days) + " days, " +
                   Int32.ToDecimalString(Hours) + " hours, " +
                   Int32.ToDecimalString(Minutes) + " minutes, " +
                   Int32.ToDecimalString(Seconds) + " seconds.";
        }
    }
}