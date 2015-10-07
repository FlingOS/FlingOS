using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Collections
{
    public class UInt32Queue : FOS_System.Object
    {
        private UInt32[] internalArray;
        /// <summary>
        /// Next index to pop item from
        /// </summary>
        private int BackIdx = 0;
        /// <summary>
        /// Next index to push item to
        /// </summary>
        private int FrontIdx = 0;

        public int Count
        {
            get
            {
                if (FrontIdx < BackIdx)
                {
                    return internalArray.Length - (BackIdx - FrontIdx);
                }
                else
                {
                    return FrontIdx - BackIdx;
                }
            }
        }

        public UInt32Queue()
        {
            internalArray = new UInt32[5];
        }

        public void Push(UInt32 val)
        {
            if ((BackIdx != 0 && FrontIdx == BackIdx - 1) ||
                (BackIdx == 0 && FrontIdx == internalArray.Length - 1))
            {
                // Queue full
                Expand(internalArray.Length);
            }

            internalArray[FrontIdx++] = val;

            if (FrontIdx >= internalArray.Length)
            {
                FrontIdx = 0;
            }
        }
        public UInt32 Pop()
        {
            if (Count > 0)
            {
                UInt32 result = internalArray[BackIdx++];

                if (BackIdx >= internalArray.Length)
                {
                    BackIdx = 0;
                }

                return result;
            }
            return 0;
        }
        public UInt32 Peek()
        {
            if (Count > 0)
            {
                return internalArray[BackIdx];
            }
            return 0;
        }

        public void Expand(int amount)
        {
            if (amount > 0)
            {
                UInt32[] newInternalArray = new UInt32[internalArray.Length + amount];
                int readIdx = BackIdx;
                int writeIdx = 0;

                while (readIdx != FrontIdx)
                {
                    newInternalArray[writeIdx++] = internalArray[readIdx++];

                    if (readIdx >= internalArray.Length)
                    {
                        readIdx = 0;
                    }
                }

                internalArray = newInternalArray;
                BackIdx = 0;
                FrontIdx = writeIdx;
            }
        }
    }
}
