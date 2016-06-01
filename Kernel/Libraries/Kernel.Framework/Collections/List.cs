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

using Drivers.Compiler.Attributes;
using Kernel.Framework.Exceptions;

namespace Kernel.Framework.Collections
{
    /// <summary>
    ///     Represents a weakly typed list of objects (which must be derived from Framework.Object) that can be accessed by
    ///     index. Provides methods to search and manipulate lists.
    /// </summary>
    /// <remarks>
    ///     The job of knowing which type of object is contained within the list is left to the developer. This is a
    ///     significant issue but one which we can't solve yet since generics aren't supported properly yet.
    /// </remarks>
    public class List : Object
    {
        //Note: The "capacity" of the list is the length of the internal array.

        /// <summary>
        ///     The underlying object array.
        /// </summary>
        /// <remarks>
        ///     When describing entries in the internal, the internal array should be seen
        ///     as a list which grows downwards. So the first entry (index 0) is the top of
        ///     the list. Adding an entry extends the list downwards by one. Removing an
        ///     entry shifts the remaining items up by one.
        /// </remarks>
        protected Object[] _array;

        public int ExpandAmount;

        /// <summary>
        ///     The "nextIndex" is the index to insert the next new item.
        ///     It is the index immediately after the last-set item in the array.
        ///     It thus also acts as an item count.
        /// </summary>
        protected int nextIndex;

        /// <summary>
        ///     The number of elements in the list.
        /// </summary>
        public int Count
        {
            [NoGC] [NoDebug] get { return nextIndex; }
        }

        public int Capacity
        {
            [NoGC] [NoDebug] get { return _array.Length; }
            [NoDebug]
            set
            {
                if (value > _array.Length)
                {
                    ExpandCapacity(value - _array.Length);
                }
            }
        }

        /// <summary>
        ///     Gets the object at the specified index.
        /// </summary>
        /// <param name="index">The index of the object to get.</param>
        /// <returns>The object at the specified index.</returns>
        /// <exception cref="Kernel.Framework.Exceptions.IndexOutOfRangeException">
        ///     Throws IndexOutOfRangeException if "index" is &lt; 0 or greater than the length of the list.
        /// </exception>
        public Object this[int index]
        {
            [NoDebug]
            get
            {
                //Throw an exception if the index to get is beyond the length of
                //  the list.
                //Note: Beyond the length of the list not the capacity!
                if (index >= nextIndex)
                {
                    ExceptionMethods.Throw(new IndexOutOfRangeException(index, nextIndex));
                }
                else if (index < 0)
                {
                    ExceptionMethods.Throw(new IndexOutOfRangeException(index, nextIndex));
                }

                return _array[index];
            }
            [NoDebug]
            set
            {
                //Throw an exception if the index to set is beyond the length of
                //  the list.
                //Note: Beyond the length of the list not the capacity!
                if (index >= nextIndex)
                {
                    ExceptionMethods.Throw(new IndexOutOfRangeException(index, nextIndex));
                }
                else if (index < 0)
                {
                    ExceptionMethods.Throw(new IndexOutOfRangeException(index, nextIndex));
                }

                _array[index] = value;
            }
        }

        /// <summary>
        ///     Creates a new list with initial capacity of 5.
        /// </summary>
        [NoDebug]
        public List()
            : this(5)
        {
        }

        /// <summary>
        ///     Creates a new list with specified initial capacity. Use this to optimise memory usage.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        [NoDebug]
        public List(int capacity)
            : this(capacity, 5)
        {
        }

        /// <summary>
        ///     Creates a new list with specified initial capacity. Use this to optimise memory usage.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        /// <param name="expandAmount">The amount to expand the list capacity by each time the capacity must be increased.</param>
        [NoDebug]
        public List(int capacity, int expandAmount)
        {
            //Create the internal array with specified capacity.
            _array = new Object[capacity];
            ExpandAmount = expandAmount;
        }

        /// <summary>
        ///     Adds the specified object to the list.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        [NoDebug]
        public void Add(Object obj)
        {
            //If the next index to insert an item at is beyond the capacity of
            //  the array, we need to expand the array.
            if (nextIndex >= _array.Length)
            {
                ExpandCapacity(ExpandAmount);
            }
            //Insert the object at the next index in the internal array then increment
            //  next index 
            _array[nextIndex] = obj;
            nextIndex++;
        }

        /// <summary>
        ///     Removes the specified object from the list.
        /// </summary>
        /// <param name="obj">The object to remove.</param>
        [NoDebug]
        public void Remove(Object obj)
        {
            //Determines whether we should be setting the array entries
            //  to null or not. After we have removed an item and shifted
            //  existing items up in the array, all remaining unused entries
            //  must be set to null.
            bool setObjectToNull = false;
            //Store the current index. There is no point looping through the whole capacity
            //  of the array, but we must at least loop through everything that has been set
            //  including the last entry even after higher entries have been shifted or removed.
            //Note: It would be invalid to use nextIndex+1 because that assumes an entry will 
            //      be removed but if the object to remove is not in the list, it will not be
            //      found and so nextIndex+1 would over-run the capacity of the list.
            int origNextIndex = nextIndex;
            //Loop through all items in the array that have had a value set.
            for (int i = 0; i < origNextIndex; i++)
            {
                //There are two scenarios here:
                //  1) We are searching from the start of the list until we find
                //     the item to be removed. Until we find the item, we don't need
                //     to make any changes to the internal array.
                //  2) Or, we have found the item to be removed and are in the process
                //     of shifting entries up 1 in the array to remove the item.
                if (_array[i] == obj || setObjectToNull)
                {
                    //If we are not setting objects to null, then "_array[i] == obj" must
                    //  have been true i.e. the current search index is the index of the
                    //  object to be removed. 
                    //Note: This if block is just a more efficient way of writing:
                    //                      if (_array[i] == obj)
                    if (!setObjectToNull)
                    {
                        //Removing the object reduces the total count of objects by 1
                        nextIndex--;
                    }

                    //We should now start shifting objects and setting entries to null
                    setObjectToNull = true;

                    //If we are still within the (new) count of objects then simply shift the 
                    //  next object up one in the list
                    if (i < nextIndex)
                    {
                        //Set current index to next value in the list.
                        _array[i] = _array[i + 1];
                    }
                    else
                    {
                        //Otherwise, just set the entry to null.
                        //  This ensures values aren't randomly left with entries 
                        //  in the top of the list.
                        _array[i] = null;
                    }
                }
            }
        }

        /// <summary>
        ///     The removes the object at the specified index from the list.
        /// </summary>
        /// <param name="index">The index of the object to remove.</param>
        [NoDebug]
        public void RemoveAt(int index)
        {
            //Throw and exception if the index to remove
            //  at is beyond the length of the list
            //Note: Beyond the length of the list not the capacity of the list.
            if (index >= nextIndex)
            {
                BasicConsole.WriteLine("Error! List.RemoveAt : index >= nextIndex");
                ExceptionMethods.Throw(new IndexOutOfRangeException(index, nextIndex));
            }
            else if (index < 0)
            {
                BasicConsole.WriteLine("Error! List.RemoveAt : index < 0");
                ExceptionMethods.Throw(new IndexOutOfRangeException(index, nextIndex));
            }

            //Note: Because we know our starting index, this algorithm is both different
            //      and more efficient than the one in "Remove(obj)"

            //Loop through all items that have had a value set, starting at index
            //  through to the end of the list.
            //Note: Because we decrement nextIndex after the loop has completed, this
            //      loop will also cover the very last entry that had a value set which
            //      must be set to null.
            for (int i = index; i < nextIndex; i++)
            {
                //While there is an item after the current one
                if (i < nextIndex - 1)
                {
                    //Shift the next item into the current one.
                    //  Note: The first iteration of the loop thus removes the entry for the
                    //        index to be removed. Subsequent iterations have the effect of
                    //        moving all the items up the list by one.
                    _array[i] = _array[i + 1];
                }
                else
                {
                    //The last entry that was set must now be set to null.
                    _array[i] = null;
                }
            }

            //Now decrement the count (length of the list) by one
            nextIndex--;
        }

        /// <summary>
        ///     Returns the index of the first instance of the specified object or -1
        ///     if it is not found.
        /// </summary>
        /// <param name="obj">The object to search for.</param>
        /// <returns>The index or -1 if not found.</returns>
        [NoDebug]
        public int IndexOf(Object obj)
        {
            //This is a straight forward search. Other search algorithms
            //  may be faster but quite frankly who cares. Optimising the compiler
            //  ASM output would have a far greater effect than changing this
            //  nice, simple algorithm.
            for (int i = 0; i < nextIndex; i++)
            {
                if (_array[i] == obj)
                {
                    return i;
                }
            }

            //As per C# (perhaps C?) standard (convention?)
            return -1;
        }

        [NoDebug]
        public Object Last()
        {
            if (nextIndex == 0)
            {
                return null;
            }

            return _array[nextIndex - 1];
        }

        /// <summary>
        ///     Empties the list of all objects but does not alter the list capacity.
        /// </summary>
        [NoDebug]
        public void Empty()
        {
            //Reset the count to 0
            nextIndex = 0;
            //Nice and simple again - just set everything to null :)
            for (int i = 0; i < nextIndex; i++)
            {
                _array[i] = null;
            }
        }

        /// <summary>
        ///     Expands the capacity of the internel array that stores the objects.
        /// </summary>
        /// <param name="amount">The amount to expand the capacity by.</param>
        [NoDebug]
        private void ExpandCapacity(int amount)
        {
            //We need to expand the size of the internal array. Unfortunately, dynamic
            //  expansion of an array to non-contiguous memory is not supported by my OS
            //  or compiler because it's just too darn complicated. So, we must allocate
            //  a new array and copy everything across.

            //Allocate the new, larger array
            Object[] newArray = new Object[_array.Length + amount];
            //Copy all the values across
            for (int i = 0; i < _array.Length; i++)
            {
                newArray[i] = _array[i];
            }
            //And set the internal array to the new, large array
            _array = newArray;
        }
    }
}