namespace Kernel.Framework.Collections
{
    public class Queue : Object
    {
        private readonly bool CanExpand;

        /// <summary>
        ///     Next index to pop item from
        /// </summary>
        private int BackIdx;

        /// <summary>
        ///     Next index to push item to
        /// </summary>
        private int FrontIdx;

        private Object[] InternalArray;

        public int Count
        {
            get
            {
                if (FrontIdx < BackIdx)
                {
                    return InternalArray.Length - (BackIdx - FrontIdx);
                }
                return FrontIdx - BackIdx;
            }
        }

        public int Capacity => InternalArray.Length;

        public Queue(int capacity, bool canExpand)
        {
            InternalArray = new Object[capacity];
            CanExpand = canExpand;
        }

        public void Push(Object val)
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

        public Object Pop()
        {
            if (Count > 0)
            {
                Object result = InternalArray[BackIdx];
                InternalArray[BackIdx++] = null;

                if (BackIdx >= InternalArray.Length)
                {
                    BackIdx = 0;
                }

                return result;
            }
            return null;
        }

        public Object Peek()
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
                Object[] newInternalArray = new Object[InternalArray.Length + amount];
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

    public class UInt32Queue : Object
    {
        private readonly bool CanExpand;

        /// <summary>
        ///     Next index to pop item from
        /// </summary>
        private int BackIdx;

        /// <summary>
        ///     Next index to push item to
        /// </summary>
        private int FrontIdx;

        private uint[] InternalArray;

        public int Count
        {
            get
            {
                if (FrontIdx < BackIdx)
                {
                    return InternalArray.Length - (BackIdx - FrontIdx);
                }
                return FrontIdx - BackIdx;
            }
        }

        public int Capacity
        {
            get { return InternalArray.Length; }
        }

        public UInt32Queue(int capacity, bool canExpand)
        {
            InternalArray = new uint[capacity];
            CanExpand = canExpand;
        }

        public void Push(uint val)
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

        public uint Pop()
        {
            if (Count > 0)
            {
                uint result = InternalArray[BackIdx];
                InternalArray[BackIdx++] = 0;

                if (BackIdx >= InternalArray.Length)
                {
                    BackIdx = 0;
                }

                return result;
            }
            return 0;
        }

        public uint Peek()
        {
            if (Count > 0)
            {
                return InternalArray[BackIdx];
            }
            return 0;
        }

        public uint RemoveLast()
        {
            if (Count > 0)
            {
                if (FrontIdx == 0)
                {
                    FrontIdx = InternalArray.Length - 1;
                }
                else
                {
                    FrontIdx--;
                }

                return InternalArray[FrontIdx];
            }
            return 0;
        }

        public void Expand(int amount)
        {
            if (amount > 0)
            {
                uint[] newInternalArray = new uint[InternalArray.Length + amount];
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