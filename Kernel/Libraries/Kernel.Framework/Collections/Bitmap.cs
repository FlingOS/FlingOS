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

using Drivers.Compiler.Attributes;

namespace Kernel.Framework.Collections
{
    /// <summary>
    ///     A simple bitmap implementation.
    /// </summary>
    public class Bitmap : Object
    {
        /// <summary>
        ///     The underlying bitmap data.
        /// </summary>
        private readonly byte[] BitmapData;
        /// <summary>
        ///     The total number of bits set in the bitmap.
        /// </summary>
        private int SetCount;

        /// <summary>
        ///     The total number of bits set in the bitmap.
        /// </summary>
        public int Count
        {
            [NoDebug] get { return SetCount; }
        }

        /// <summary>
        ///     Initialises a new bitmap of the specified Size.
        /// </summary>
        /// <param name="Size"></param>
        [NoDebug]
        public Bitmap(int Size)
        {
            BitmapData = new byte[Size/8];
        }

        /// <summary>
        ///     Sets an entry in the bitmap.
        /// </summary>
        /// <param name="Entry">The index of the entry to set.</param>
        [NoDebug]
        [NoGC]
        public void Set(int Entry)
        {
            BitmapData[Entry/8] = (byte)(BitmapData[Entry/8] | (1 << (Entry%8)));
            SetCount++;
        }

        /// <summary>
        ///     Clears an entry in the bitmap.
        /// </summary>
        /// <param name="Entry">The index of the entry to clear.</param>
        [NoDebug]
        [NoGC]
        public void Clear(int Entry)
        {
            BitmapData[Entry/8] = (byte)(BitmapData[Entry/8] & ~(1 << (Entry%8)));
            SetCount--;
        }

        /// <summary>
        ///     Returns whether the specified entry in the bitmap is set or not.
        /// </summary>
        /// <param name="entry">The entry to look up.</param>
        /// <returns>True if the entry is set, otherwise false.</returns>
        [NoDebug]
        [NoGC]
        public bool IsSet(int entry)
        {
            return (BitmapData[entry/8] & (1 << (entry%8))) != 0;
        }

        /// <summary>
        ///     Finds the first clear entry in the bitmap.
        /// </summary>
        /// <returns>The index of the entry.</returns>
        [NoDebug]
        [NoGC]
        public int FindFirstClearEntry()
        {
            for (int i = 0; i < BitmapData.Length; i++)
            {
                for (int j = 1, x = 0; x < 8; j <<= 1, x++)
                {
                    if ((BitmapData[i] & j) == 0)
                    {
                        return i*8 + x;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        ///     Finds the last clear entry in the bitmap.
        /// </summary>
        /// <returns>The index of the entry.</returns>
        [NoDebug]
        [NoGC]
        public int FindLastClearEntry()
        {
            for (int i = BitmapData.Length - 1; i > -1; i--)
            {
                for (int j = 0x80, x = 7; x >= 0; j >>= 1, x--)
                {
                    if ((BitmapData[i] & j) == 0)
                    {
                        return i*8 + x;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        ///     Finds a contiguous set of size Num of clear entries.
        /// </summary>
        /// <param name="Num">The number of contiguous clear entries to find.</param>
        /// <returns>The index of the start of the set.</returns>
        [NoDebug]
        [NoGC]
        public int FindContiguousClearEntries(int Num)
        {
            int ContiguousEntries = 0;
            int TestPosition = 0;
            int StartPosition = 0;
            int Length = BitmapData.Length*8;
            while (ContiguousEntries != Num && TestPosition < Length)
            {
                if (!IsSet(TestPosition))
                {
                    if (ContiguousEntries == 0)
                    {
                        StartPosition = TestPosition;
                    }

                    ContiguousEntries++;
                }
                else
                {
                    ContiguousEntries = 0;
                }

                TestPosition++;
            }

            if (ContiguousEntries != Num)
            {
                return -1;
            }

            return StartPosition;
        }
    }
}