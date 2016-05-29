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

namespace Kernel.Framework.Stubs
{
    /// <summary>
    ///     Replacement class for methods, properties and fields usually found on standard System.Int32 type.
    /// </summary>
    public static class UInt32
    {
        /// <summary>
        ///     Returns the maximum value of an Int32.
        /// </summary>
        public static uint MaxValue
        {
            get { return 4294967295; }
        }

        public static String ToDecimalString(uint num)
        {
            String result = "";
            //If the number is already 0, just output 0
            //  straight off. The algorithm below does not
            //  work if num is 0.
            if (num != 0)
            {
                //Loop through outputting the units value (base 10)
                //  and then dividing by 10 to move to the next digit.
                while (num > 0)
                {
                    //Get the units
                    uint rem = num%10;
                    //Output the units character
                    switch (rem)
                    {
                        case 0:
                            result = "0" + result;
                            break;
                        case 1:
                            result = "1" + result;
                            break;
                        case 2:
                            result = "2" + result;
                            break;
                        case 3:
                            result = "3" + result;
                            break;
                        case 4:
                            result = "4" + result;
                            break;
                        case 5:
                            result = "5" + result;
                            break;
                        case 6:
                            result = "6" + result;
                            break;
                        case 7:
                            result = "7" + result;
                            break;
                        case 8:
                            result = "8" + result;
                            break;
                        case 9:
                            result = "9" + result;
                            break;
                    }
                    //Divide by 10 to move to the next digit.
                    num /= 10;
                }
            }
            else
            {
                result = "0";
            }
            return result;
        }
    }
}