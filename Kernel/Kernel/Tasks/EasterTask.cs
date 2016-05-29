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
using Kernel.Framework.Processes;

namespace Kernel.Tasks
{
    public static unsafe class EasterTask
    {
        private static readonly String ImageMap =
            "                                                                                                                                                                                                                                                                  HH    HH                                                                        HH    HH    HHHH      HH        HH    HH    HH                                  HH    HH  HH    HH  HH  HH   HH  HH     HH  HH                                  HHHHHHHH  HH    HH  HH   HH  HH   HH      HH                                    HH    HH  HHHHHHHH  HHHHH    HHHHH       HH                                     HH    HH  HH    HH  HH       HH         HH                                      HH    HH  HH    HH  HH       HH        HH                                                                                                                                                                                                                                                                                   HHHHHHHH                                                                        HH                                                                              HH          HHHH       HHHHHH  HH      HHHHHHHH   HHHHH                         HHHHHH    HH    HH   HH        HH      HH        HH   HH                        HH        HHHHHHHH     HHHH    HHHH    HHHHHH    HHHHH                          HH        HH    HH         HH  HH      HH        HH  HH                         HHHHHHHH  HH    HH   HHHHHH     HHHHH  HHHHHHHH  HH    HH                                                                                                                                                                                                                                                                                                                                                                                                                         ";

        public static void Main()
        {
            for (int i = 0; i < ImageMap.Length; i++)
            {
                if (ImageMap[i] == 'H')
                {
                    ImageMap[i] = (char)(' ' | 0xFF00);
                }
                else
                {
                    ImageMap[i] = (char)(' ' | 0x4400);
                }
            }

            while (true)
            {
                char* VidMemPtr = (char*)0xB8000;
                for (int i = 0; i < ImageMap.Length; i++)
                {
                    VidMemPtr[i] = ImageMap[i];
                }

                SystemCalls.SleepThread(1000);

                /*
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * ------------------  ----  ------------------------------------------------------
                 * ------------------  ----  ----    ------  --------  ----  ----  ----------------
                 * ------------------  ----  --  ----  --  --  ---  --  -----  --  ----------------
                 * ------------------        --  ----  --  ---  --  ---  ------  ------------------
                 * ------------------  ----  --        --     ----     -------  -------------------
                 * ------------------  ----  --  ----  --  -------  ---------  --------------------
                 * ------------------  ----  --  ----  --  -------  --------  ---------------------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * --------------        ----------------------------------------------------------
                 * --------------  ----------------------------------------------------------------
                 * --------------  ----------    -------      --  ------        ---     -----------
                 * --------------      ----  ----  ---  --------    ----  --------  ---  ----------
                 * --------------  --------        -----    ----  ------      ----     ------------
                 * --------------  --------  ----  ---------  --  ------  --------  --  -----------
                 * --------------        --  ----  ---      -----     --        --  ----  ---------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 * --------------------------------------------------------------------------------
                 */
            }
        }
    }
}