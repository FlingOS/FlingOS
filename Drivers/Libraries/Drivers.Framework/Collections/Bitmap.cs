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

namespace Drivers.Framework.Collections
{
    public class Bitmap : Object
    {
        private readonly byte[] bitmap;
        private int setCount;

        public int Count
        {
            [NoDebug] get { return setCount; }
        }

        [NoDebug]
        public Bitmap(int size)
        {
            bitmap = new byte[size/8];
        }

        [NoDebug]
        public void Set(int entry)
        {
            bitmap[entry/8] = (byte)(bitmap[entry/8] | (1 << (entry%8)));
            setCount++;
        }

        [NoDebug]
        public void Clear(int entry)
        {
            bitmap[entry/8] = (byte)(bitmap[entry/8] & ~(1 << (entry%8)));
            setCount--;
        }

        [NoDebug]
        public bool IsSet(int entry)
        {
            return (bitmap[entry/8] & (1 << (entry%8))) != 0;
        }

        [NoDebug]
        public int FindFirstClearEntry()
        {
            for (int i = 0; i < bitmap.Length; i++)
            {
                for (int j = 1, x = 0; x < 8; j <<= 1, x++)
                {
                    if ((bitmap[i] & j) == 0)
                    {
                        return i*8 + x;
                    }
                }
            }
            return -1;
        }

        [NoDebug]
        public int FindLastClearEntry()
        {
            for (int i = bitmap.Length - 1; i > -1; i--)
            {
                for (int j = 0x80, x = 7; x >= 0; j >>= 1, x--)
                {
                    if ((bitmap[i] & j) == 0)
                    {
                        return i*8 + x;
                    }
                }
            }
            return -1;
        }

        [NoDebug]
        public int FindContiguousClearEntries(int num)
        {
            int contiguousEntries = 0;
            int testPos = 0;
            int startPos = 0;
            int length = bitmap.Length*8;
            while (contiguousEntries != num && testPos < length)
            {
                if (!IsSet(testPos))
                {
                    if (contiguousEntries == 0)
                    {
                        startPos = testPos;
                    }

                    contiguousEntries++;
                }
                else
                {
                    contiguousEntries = 0;
                }

                testPos++;
            }

            if (contiguousEntries != num)
            {
                return -1;
            }

            return startPos;
        }
    }
}