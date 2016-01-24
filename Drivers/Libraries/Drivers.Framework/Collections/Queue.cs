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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Framework.Collections
{
    public class Queue : Framework.Object
    {
        private Framework.Object[] InternalArray;
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
            InternalArray = new Framework.Object[capacity];
            CanExpand = canExpand;
        }

        public void Push(Framework.Object val)
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
        public Framework.Object Pop()
        {
            if (Count > 0)
            {
                Framework.Object result = InternalArray[BackIdx];
                InternalArray[BackIdx++] = null;

                if (BackIdx >= InternalArray.Length)
                {
                    BackIdx = 0;
                }

                return result;
            }
            return null;
        }
        public Framework.Object Peek()
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
                Framework.Object[] newInternalArray = new Framework.Object[InternalArray.Length + amount];
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
    public class UInt32Queue : Framework.Object
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
        public UInt32 RemoveLast()
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
