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
            get
            {
                return setCount;
            }
        }

        public Bitmap(int size)
        {
            bitmap = new byte[size / 8];
        }

        public void Set(int entry)
        {
            bitmap[entry / 8] = (byte)(bitmap[entry / 8] | (1 << (entry % 8)));
            setCount++;
        }
        public void Clear(int entry)
        {
            bitmap[entry / 8] = (byte)(bitmap[entry / 8] & ~(1 << (entry % 8)));
            setCount--;
        }

        public bool IsSet(int entry)
        {
            return (bitmap[entry / 8] & ~(byte)(entry % 8)) > 0;
        }

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
