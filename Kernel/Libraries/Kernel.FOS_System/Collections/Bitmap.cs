#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Collections
{
    public class Bitmap : FOS_System.Object
    {
        private byte[] bitmap;
        private int setCount = 0;

        public int Count
        {
            [Compiler.NoDebug]
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return setCount;
            }
        }

        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public Bitmap(int size)
        {
            bitmap = new byte[size / 8];
        }

        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public void Set(int entry)
        {
            bitmap[entry / 8] = (byte)(bitmap[entry / 8] | (1 << (entry % 8)));
            setCount++;
        }
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public void Clear(int entry)
        {
            bitmap[entry / 8] = (byte)(bitmap[entry / 8] & ~(1 << (entry % 8)));
            setCount--;
        }

        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public bool IsSet(int entry)
        {
            return (bitmap[entry / 8] & ~(byte)(entry % 8)) > 0;
        }

        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public int FindFirstClearEntry()
        {
            for (int i = 0; i < bitmap.Length; i++)
            {
                for (int j = 1, x = 0; x < 8; j <<= 1, x++)
                {
                    if ((bitmap[i] & j) == 0)
                    {
                        return (i * 8) + x;
                    }
                }
            }
            return -1;
        }
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public int FindLastClearEntry()
        {
            for (int i = bitmap.Length - 1; i > -1; i--)
            {
                for (int j = 0x80, x = 7; x >= 0; j >>= 1, x--)
                {
                    if ((bitmap[i] & j) == 0)
                    {
                        return (i * 8) + x;
                    }
                }
            }
            return -1;
        }
    }
}
