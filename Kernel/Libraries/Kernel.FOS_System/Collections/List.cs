using System;

namespace Kernel.FOS_System.Collections
{
    public class List : FOS_System.Object
    {
        protected FOS_System.Object[] _array;
        protected int currIndex = 0;

        public int Count
        {
            [Compiler.NoDebug]
            get
            {
                return currIndex;
            }
        }

        [Compiler.NoDebug]
        public List()
        {
            _array = new FOS_System.Object[5];
        }
        [Compiler.NoDebug]
        public List(int capacity)
        {
            _array = new FOS_System.Object[capacity];
        }

        [Compiler.NoDebug]
        public void Add(FOS_System.Object obj)
        {
            if (currIndex >= _array.Length)
            {
                ExpandCapacity(5);
            }
            _array[currIndex++] = obj;
        }
        [Compiler.NoDebug]
        public void Remove(FOS_System.Object obj)
        {
            bool setObjectToNull = false;
            for (int i = 0; i < currIndex; i++)
            {
                if (setObjectToNull || _array[i] == obj)
                {
                    if (!setObjectToNull)
                    {
                        currIndex--;
                    }

                    setObjectToNull = true;
                    if (i < currIndex - 1)
                    {
                        _array[i] = _array[i + 1];
                    }
                    else
                    {
                        _array[i] = null;
                    }
                }
            }
        }
        [Compiler.NoDebug]
        public void RemoveAt(int index)
        {
            if (index >= currIndex)
            {
                ExceptionMethods.Throw(new Exceptions.OverflowException());
            }

            for (int i = index; i < currIndex; i++)
            {
                if (i < currIndex - 1)
                {
                    _array[i] = _array[i + 1];
                }
                else
                {
                    _array[i] = null;
                }
            }

            currIndex--;
        }

        [Compiler.NoDebug]
        public void Empty()
        {
            for (int i = 0; i < currIndex; i++)
            {
                _array[i] = null;
            }
            currIndex = 0;
        }

        [Compiler.NoDebug]
        private void ExpandCapacity(int amount)
        {
            FOS_System.Object[] newArray = new FOS_System.Object[_array.Length + amount];
            for (int i = 0; i < _array.Length; i++)
            {
                newArray[i] = _array[i];
            }
            _array = newArray;
        }

        public FOS_System.Object this[int index]
        {
            [Compiler.NoDebug]
            get
            {
                return _array[index];
            }
            [Compiler.NoDebug]
            set
            {
                _array[index] = value;
            }
        }
    }

    public class UInt32List : FOS_System.Object
    {
        protected UInt32[] _array;
        protected int currIndex = 0;

        public int Count
        {
            [Compiler.NoDebug]
            get
            {
                return currIndex;
            }
        }

        [Compiler.NoDebug]
        public UInt32List()
        {
            _array = new UInt32[5];
        }
        [Compiler.NoDebug]
        public UInt32List(int capacity)
        {
            _array = new UInt32[capacity];
        }

        [Compiler.NoDebug]
        public void Add(UInt32 obj)
        {
            if (currIndex >= _array.Length)
            {
                ExpandCapacity(5);
            }
            _array[currIndex++] = obj;
        }
        [Compiler.NoDebug]
        public void Remove(UInt32 obj)
        {
            bool setObjectToNull = false;
            for (int i = 0; i < currIndex; i++)
            {
                if (setObjectToNull || _array[i] == obj)
                {
                    setObjectToNull = true;
                    if (i < currIndex - 1)
                    {
                        _array[i] = _array[i + 1];
                    }
                    else
                    {
                        _array[i] = 0;
                    }
                }
            }
        }

        [Compiler.NoDebug]
        private void ExpandCapacity(int amount)
        {
            UInt32[] newArray = new UInt32[_array.Length + amount];
            for (int i = 0; i < _array.Length; i++)
            {
                newArray[i] = _array[i];
            }
            _array = newArray;
        }

        public UInt32 this[int index]
        {
            [Compiler.NoDebug]
            get
            {
                return _array[index];
            }
            [Compiler.NoDebug]
            set
            {
                _array[index] = value;
            }
        }
    }
}
