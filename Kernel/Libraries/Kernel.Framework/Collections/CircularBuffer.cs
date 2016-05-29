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

using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes.Synchronisation;

namespace Kernel.Framework.Collections
{
    public class CircularBuffer : Object
    {
        private readonly Object[] _array;

        private readonly SpinLock AccessLock = new SpinLock();
        public readonly bool ThrowExceptions;
        private int ReadIdx = -1;
        private int WriteIdx = -1;

        public int Count
        {
            get
            {
                if (WriteIdx == -1)
                {
                    return 0;
                }
                if (WriteIdx <= ReadIdx)
                {
                    return _array.Length - ReadIdx + WriteIdx;
                }
                return WriteIdx - ReadIdx;
            }
        }

        public int Size
        {
            get { return _array.Length; }
        }

        public CircularBuffer(int size)
            : this(size, true)
        {
        }

        public CircularBuffer(int size, bool throwExceptions)
        {
            ThrowExceptions = throwExceptions;
            if (ThrowExceptions && size <= 0)
            {
                ExceptionMethods.Throw(
                    new ArgumentException("Size of circular buffer cannot be less than or equal to zero!"));
            }
            _array = new Object[size];
        }

        public bool Push(Object obj)
        {
            AccessLock.Enter();

            try
            {
                if (WriteIdx == ReadIdx &&
                    ReadIdx != -1)
                {
                    if (ThrowExceptions)
                    {
                        ExceptionMethods.Throw(
                            new OverflowException("Circular buffer cannot Push because the buffer is full."));
                    }
                    return false;
                }

                WriteIdx++;

                if (WriteIdx == _array.Length)
                {
                    if (ReadIdx == -1)
                    {
                        WriteIdx--;

                        if (ThrowExceptions)
                        {
                            ExceptionMethods.Throw(
                                new OverflowException("Circular buffer cannot Push because the buffer is full."));
                        }
                        return false;
                    }

                    WriteIdx = 0;
                }

                _array[WriteIdx] = obj;

                return true;
            }
            finally
            {
                AccessLock.Exit();
            }
        }

        public Object Pop()
        {
            AccessLock.Enter();

            try
            {
                if (WriteIdx == -1)
                {
                    if (ThrowExceptions)
                    {
                        ExceptionMethods.Throw(
                            new OverflowException("Circular buffer cannot Pop because the buffer is empty."));
                    }
                    return null;
                }

                ReadIdx++;
                if (ReadIdx == _array.Length)
                {
                    ReadIdx = 0;
                }

                int tmpReadIdx = ReadIdx;
                if (ReadIdx == WriteIdx)
                {
                    ReadIdx = -1;
                    WriteIdx = -1;
                }

                return _array[tmpReadIdx];
            }
            finally
            {
                AccessLock.Exit();
            }
        }

        public Object Peek()
        {
            AccessLock.Enter();

            try
            {
                if (WriteIdx == -1)
                {
                    if (ThrowExceptions)
                    {
                        ExceptionMethods.Throw(
                            new OverflowException("Circular buffer cannot Peek because the buffer is empty."));
                    }
                    return null;
                }

                int tmpReadIdx = ReadIdx;

                tmpReadIdx++;
                if (tmpReadIdx == _array.Length)
                {
                    tmpReadIdx = 0;
                }

                return _array[tmpReadIdx];
            }
            finally
            {
                AccessLock.Exit();
            }
        }

        public void Empty()
        {
            AccessLock.Enter();
            try
            {
                for (int i = 0; i < _array.Length; i++)
                {
                    _array[i] = null;
                }
                ReadIdx = -1;
                WriteIdx = -1;
            }
            finally
            {
                AccessLock.Exit();
            }
        }
    }
}