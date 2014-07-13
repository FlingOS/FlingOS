#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;

namespace Kernel.FOS_System.Collections
{
    /// <summary>
    /// Represents a weakly typed list of objects (which must be derived from FOS_System.Object) that can be accessed by 
    /// index. Provides methods to search and manipulate lists.
    /// </summary>
    /// <remarks>
    /// The job of knowing which type of object is contained within the list is left to the developer. This is a 
    /// significant issue but one which we can't solve yet since generics aren't supported properly yet.
    /// </remarks>
    public class List : FOS_System.Object
    {
        /// <summary>
        /// The underlying object array.
        /// </summary>
        protected FOS_System.Object[] _array;
        /// <summary>
        /// The "currentIndex" is the index to insert the next new item.
        /// It is the index immediately after the last-set item in the array.
        /// It thus also acts as an item count.
        /// </summary>
        protected int currIndex = 0;

        /// <summary>
        /// The number of elements in the list.
        /// </summary>
        public int Count
        {
            [Compiler.NoDebug]
            get
            {
                return currIndex;
            }
        }

        /// <summary>
        /// Creates a new list with initial capacity of 5.
        /// </summary>
        [Compiler.NoDebug]
        public List()
        {
            _array = new FOS_System.Object[5];
        }
        /// <summary>
        /// Creates a new list with specified initial capacity. Use this to optimise memory usage.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        [Compiler.NoDebug]
        public List(int capacity)
        {
            _array = new FOS_System.Object[capacity];
        }

        /// <summary>
        /// Adds the specified object to the list.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        [Compiler.NoDebug]
        public void Add(FOS_System.Object obj)
        {
            if (currIndex >= _array.Length)
            {
                ExpandCapacity(5);
            }
            _array[currIndex++] = obj;
        }
        /// <summary>
        /// Removes the specified object from the list.
        /// </summary>
        /// <param name="obj">The object to remove.</param>
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
        /// <summary>
        /// The removes the object at the specified index from the list.
        /// </summary>
        /// <param name="index">The index of the object to remove.</param>
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

        /// <summary>
        /// Empties the list of all objects but does not alter the list capacity.
        /// </summary>
        [Compiler.NoDebug]
        public void Empty()
        {
            for (int i = 0; i < currIndex; i++)
            {
                _array[i] = null;
            }
            currIndex = 0;
        }

        /// <summary>
        /// Expands the capacity of the internel array that stores the objects.
        /// </summary>
        /// <param name="amount">The amount to expand the capacity by.</param>
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

        /// <summary>
        /// Gets the object at the specified index.
        /// </summary>
        /// <param name="index">The index of the object to get.</param>
        /// <returns>The object at the specified index.</returns>
        /// <exception cref="Kernel.FOS_System.Exceptions.IndexOutOfRangeException">
        /// Throws IndexOutOfRangeException if "index" is &lt; 0 or greater than the length of the list.
        /// </exception>
        public FOS_System.Object this[int index]
        {
            [Compiler.NoDebug]
            get
            {
                if (index >= currIndex)
                {
                    ExceptionMethods.Throw(new Exceptions.IndexOutOfRangeException());
                }

                return _array[index];
            }
            [Compiler.NoDebug]
            set
            {
                if (index >= currIndex)
                {
                    ExceptionMethods.Throw(new Exceptions.IndexOutOfRangeException());
                }

                _array[index] = value;
            }
        }
    }

    /// <summary>
    /// Represents a strongly typed list of UInt32s that can be accessed by 
    /// index. Provides methods to search and manipulate lists.
    /// </summary>
    public class UInt32List : FOS_System.Object
    {
        /// <summary>
        /// The underlying object array.
        /// </summary>
        protected UInt32[] _array;
        /// <summary>
        /// The "currentIndex" is the index to insert the next new item.
        /// It is the index immediately after the last-set item in the array.
        /// It thus also acts as an item count.
        /// </summary>
        protected int currIndex = 0;

        /// <summary>
        /// The number of elements in the list.
        /// </summary>
        public int Count
        {
            [Compiler.NoDebug]
            get
            {
                return currIndex;
            }
        }

        /// <summary>
        /// Creates a new list with initial capacity of 5.
        /// </summary>
        [Compiler.NoDebug]
        public UInt32List()
        {
            _array = new UInt32[5];
        }
        /// <summary>
        /// Creates a new list with specified initial capacity. Use this to optimise memory usage.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        [Compiler.NoDebug]
        public UInt32List(int capacity)
        {
            _array = new UInt32[capacity];
        }

        /// <summary>
        /// Adds the specified UInt32 to the list.
        /// </summary>
        /// <param name="obj">The UInt32 to add.</param>
        [Compiler.NoDebug]
        public void Add(UInt32 obj)
        {
            if (currIndex >= _array.Length)
            {
                ExpandCapacity(5);
            }
            _array[currIndex++] = obj;
        }
        /// <summary>
        /// Removes the first equal value of the specified UInt32 from the list.
        /// </summary>
        /// <param name="obj">The UInt32 to remove.</param>
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

        /// <summary>
        /// Expands the capacity of the internel array that stores the UInt32s.
        /// </summary>
        /// <param name="amount">The amount to expand the capacity by.</param>
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

        /// <summary>
        /// Gets the UInt32 at the specified index.
        /// </summary>
        /// <param name="index">The index of the UInt32 to get.</param>
        /// <returns>The UInt32 at the specified index.</returns>
        /// <exception cref="Kernel.FOS_System.Exceptions.IndexOutOfRangeException">
        /// Throws IndexOutOfRangeException if "index" is &lt; 0 or greater than the length of the list.
        /// </exception>
        public UInt32 this[int index]
        {
            [Compiler.NoDebug]
            get
            {
                if (index >= currIndex)
                {
                    ExceptionMethods.Throw(new Exceptions.IndexOutOfRangeException());
                }

                return _array[index];
            }
            [Compiler.NoDebug]
            set
            {
                if (index >= currIndex)
                {
                    ExceptionMethods.Throw(new Exceptions.IndexOutOfRangeException());
                }

                _array[index] = value;
            }
        }
    }
}
