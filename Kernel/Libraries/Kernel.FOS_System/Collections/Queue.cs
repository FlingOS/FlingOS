using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Collections
{
    public class Queue : FOS_System.Object
    {
        private FOS_System.Object[] InternalArray;
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
                    return InternalArray.Length - (BackIdx - FrontIdx);
                }
                else
                {
                    return FrontIdx - BackIdx;
                }
            }
        }
        public int Capacity
        {
            get
            {
                return InternalArray.Length;
            }
        }

        private bool CanExpand;

        public Queue(int capacity, bool canExpand)
        {
            InternalArray = new FOS_System.Object[capacity];
            CanExpand = canExpand;
        }

        public void Push(FOS_System.Object val)
        {
            if ((BackIdx != 0 && FrontIdx == BackIdx - 1) ||
                (BackIdx == 0 && FrontIdx == InternalArray.Length - 1))
            {
                // Queue full
                if (CanExpand)
                {
                    Expand(InternalArray.Length);
                }
                else
                {
                    return;
                }
            }

            InternalArray[FrontIdx++] = val;

            if (FrontIdx >= InternalArray.Length)
            {
                FrontIdx = 0;
            }
        }
        public FOS_System.Object Pop()
        {
            if (Count > 0)
            {
                FOS_System.Object result = InternalArray[BackIdx];
                InternalArray[BackIdx++] = null;

                if (BackIdx >= InternalArray.Length)
                {
                    BackIdx = 0;
                }

                return result;
            }
            return null;
        }
        public FOS_System.Object Peek()
        {
            if (Count > 0)
            {
                return InternalArray[BackIdx];
            }
            return null;
        }

        public void Expand(int amount)
        {
            if (amount > 0)
            {
                FOS_System.Object[] newInternalArray = new FOS_System.Object[InternalArray.Length + amount];
                int readIdx = BackIdx;
                int writeIdx = 0;

                while (readIdx != FrontIdx)
                {
                    newInternalArray[writeIdx++] = InternalArray[readIdx++];

                    if (readIdx >= InternalArray.Length)
                    {
                        readIdx = 0;
                    }
                }

                InternalArray = newInternalArray;
                BackIdx = 0;
                FrontIdx = writeIdx;
            }
        }
    }
    public class UInt32Queue : FOS_System.Object
    {
        private UInt32[] InternalArray;
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
                    return InternalArray.Length - (BackIdx - FrontIdx);
                }
                else
                {
                    return FrontIdx - BackIdx;
                }
            }
        }
        public int Capacity
        {
            get
            {
                return InternalArray.Length;
            }
        }
        
        private bool CanExpand;

        public UInt32Queue(int capacity, bool canExpand)
        {
            InternalArray = new UInt32[capacity];
            CanExpand = canExpand;
        }

        public void Push(UInt32 val)
        {
            if ((BackIdx != 0 && FrontIdx == BackIdx - 1) ||
                (BackIdx == 0 && FrontIdx == InternalArray.Length - 1))
            {
                // Queue full
                if (CanExpand)
                {
                    Expand(InternalArray.Length);
                }
                else
                {
                    return;
                }
            }

            InternalArray[FrontIdx++] = val;

            if (FrontIdx >= InternalArray.Length)
            {
                FrontIdx = 0;
            }
        }
        public UInt32 Pop()
        {
            if (Count > 0)
            {
                UInt32 result = InternalArray[BackIdx];
                InternalArray[BackIdx++] = 0;

                if (BackIdx >= InternalArray.Length)
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
                return InternalArray[BackIdx];
            }
            return 0;
        }

        public void Expand(int amount)
        {
            if (amount > 0)
            {
                UInt32[] newInternalArray = new UInt32[InternalArray.Length + amount];
                int readIdx = BackIdx;
                int writeIdx = 0;

                while (readIdx != FrontIdx)
                {
                    newInternalArray[writeIdx++] = InternalArray[readIdx++];

                    if (readIdx >= InternalArray.Length)
                    {
                        readIdx = 0;
                    }
                }

                InternalArray = newInternalArray;
                BackIdx = 0;
                FrontIdx = writeIdx;
            }
        }
    }
}
