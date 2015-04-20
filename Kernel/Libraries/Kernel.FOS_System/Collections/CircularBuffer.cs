using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Collections
{
    public class CircularBuffer : FOS_System.Object
    {
        private FOS_System.Object[] _array;
        private int ReadIdx = -1;
        private int WriteIdx = -1;

        public readonly bool ThrowExceptions;
        public int Count
        {
            get
            {
                if (WriteIdx == -1)
                {
                    return 0;
                }
                else if (WriteIdx <= ReadIdx)
                {
                    return (_array.Length - ReadIdx) + WriteIdx;
                }
                else
                {
                    return WriteIdx - ReadIdx;
                }
            }
        }
        public int Size
        {
            get
            {
                return _array.Length;
            }
        }

        private Processes.Synchronisation.SpinLock AccessLock = new Processes.Synchronisation.SpinLock(-1);

        public CircularBuffer(int size)
            : this(size, true)
        {
        }
        public CircularBuffer(int size, bool throwExceptions)
        {
            ThrowExceptions = throwExceptions;
            if (ThrowExceptions && size <= 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("Size of circular buffer cannot be less than or equal to zero!"));
            }
            _array = new FOS_System.Object[size];
        }

        public bool Push(FOS_System.Object obj)
        {
            AccessLock.Enter();

            try
            {
                if (WriteIdx == ReadIdx &&
                    ReadIdx != -1)
                {
                    if (ThrowExceptions)
                    {
                        ExceptionMethods.Throw(new Exceptions.OverflowException());
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
                            ExceptionMethods.Throw(new Exceptions.OverflowException());
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
        public FOS_System.Object Pop()
        {
            AccessLock.Enter();

            try
            {
                if (WriteIdx == -1)
                {
                    if (ThrowExceptions)
                    {
                        ExceptionMethods.Throw(new Exceptions.OverflowException());
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
        public FOS_System.Object Peek()
        {
            AccessLock.Enter();

            try
            {
                if (WriteIdx == -1)
                {
                    if (ThrowExceptions)
                    {
                        ExceptionMethods.Throw(new Exceptions.OverflowException());
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
