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


#if MIPS
using System;

namespace FlingOops.MIPS.CI20
{
    public static unsafe class LED
    {
        public const uint GPIO_F_SET = 0xb0010544;
        public const uint GPIO_F_CLEAR = 0xb0010548;

        public const uint GPIO_F_LED_PIN = (1 << 15);

        public static void Red()
        {
            *(uint*)GPIO_F_SET = GPIO_F_LED_PIN;
        }
        public static void Blue()
        {
            *(uint*)GPIO_F_CLEAR = GPIO_F_LED_PIN;
        }
    }
}
#endif