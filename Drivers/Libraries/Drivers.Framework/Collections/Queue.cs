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

namespace Drivers.Framework.Collections
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

        public int Capacity
        {
            get { return InternalArray.Length; }
        }

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