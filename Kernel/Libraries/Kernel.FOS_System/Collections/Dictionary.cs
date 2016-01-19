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

namespace Kernel.FOS_System.Collections
{
    public unsafe class UInt32Dictionary : Object
    {
        public struct KeyValuePair
        {
            public UInt32 Key;
            public UInt32 Value;
            internal KeyValuePair* Prev;
            internal KeyValuePair* Next;
            internal bool Empty;
        }

        public sealed class Iterator : Object
        {
            private KeyValuePair* list;
            private KeyValuePair* storedList;

            public Iterator(KeyValuePair* aList)
            {
                list = aList;
            }

            internal void Reset(KeyValuePair* aList)
            {
                list = aList;
            }

            public void StoreState()
            {
                storedList = list;
            }
            public void RestoreState()
            {
                list = storedList;
            }

            public bool HasNext()
            {
                return list != null && !list->Empty;
            }
            public KeyValuePair Next()
            {
                KeyValuePair result = *list;
                list = list->Prev;
                return result;
            }
        }

        protected KeyValuePair* list;
        protected Iterator iterator;

        public UInt32Dictionary()
        {
            list = null;
            iterator = new Iterator(null);

            Prefill(25);
        }
        private void Prefill(int capacity)
        {
            while (capacity-- > 0)
            {
                KeyValuePair* newItem = (KeyValuePair*)Heap.Alloc((uint)sizeof(KeyValuePair), "UInt32Dictionary.Prefill");
                newItem->Empty = true;
                newItem->Key = 0;
                newItem->Value = 0;
                newItem->Prev = null;
                if (list == null)
                {
                    newItem->Next = null;
                    list = newItem;
                }
                else
                {
                    newItem->Next = list;
                    list->Prev = newItem;
                    list = newItem;
                }
            }
        }
        
        public void Add(UInt32 key, UInt32 value, bool SkipCheck = false)
        {
            if (!SkipCheck)
            {
                if (ContainsKey(key))
                {
                    ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
                }
            }

            KeyValuePair* newItem = null;
            KeyValuePair* newNext = null;
            KeyValuePair* newPrev = null;
            KeyValuePair* newListNext = null;
            bool Alloc = true;
            if (list != null)
            {
                if (list->Empty)
                {
                    newItem = list;
                    newNext = newItem->Next;
                    newPrev = null;
                    newListNext = newItem->Next;
                    Alloc = false;
                }
                else if (list->Next != null)
                {
                    newItem = list->Next;
                    newNext = newItem->Next;
                    newPrev = newItem->Prev;
                    newListNext = newItem;
                    Alloc = false;
                }
                else
                {
                    Alloc = true;
                }
            }
            if(Alloc)
            {
                newItem = (KeyValuePair*)Heap.Alloc((uint)sizeof(KeyValuePair), "UInt32Dictionary.Add");
                newNext = null;
                newPrev = list;
                newListNext = newItem;
            }

            newItem->Key = key;
            newItem->Value = value;
            newItem->Next = newNext;
            newItem->Prev = newPrev;
            newItem->Empty = false;

            if (list != null && newItem != list)
            {
                list->Next = newListNext;
            }
            list = newItem;
        }
        public void AddRange(UInt32 keyStart, UInt32 keyStep, UInt32[] values)
        {
            if (ContainsKeyInRange(keyStart, keyStart + ((uint)values.Length * keyStep)))
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
            }
            
            UInt32 keyVal = keyStart;
            for (uint i = 0; i < values.Length; i++)
            {
                Add(keyVal, values[i], true);

                keyVal += keyStep;
            }
        }
        public void Remove(UInt32 key)
        {
            KeyValuePair* cPair = list;
            while(cPair != null)
            {
                if(cPair->Key == key)
                {
                    KeyValuePair* prev = cPair->Prev;
                    KeyValuePair* next = cPair->Next;

                    if (prev != null)
                    {
                        prev->Next = next;
                    }
                    if (next != null)
                    {
                        next->Prev = prev;
                    }

                    if (cPair == list)
                    {
                        if (prev == null)
                        {
                            list = next;
                        }
                        else
                        {
                            list = prev;
                        }
                    }

                    cPair->Empty = true;
                    cPair->Key = 0;
                    cPair->Value = 0;
                    cPair->Prev = list;
                    if (list != null)
                    {
                        cPair->Next = list->Next;
                        list->Next = cPair;
                        cPair->Next->Prev = cPair;
                    }
                    else
                    {
                        cPair->Next = null;
                        list = cPair;
                    }

                    break;
                }
                cPair = cPair->Prev;
            }
        }
        public void RemoveRange(UInt32 keyStart, UInt32 keyStep, UInt32 numKeys)
        {
            for (int i = (int)(numKeys - 1); i >= 0; i--)
            {
                UInt32 currKey = (keyStart + ((UInt32)i * keyStep));
                Remove(currKey);
            }
        }

        public bool ContainsKey(UInt32 key)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (!cPair->Empty && cPair->Key == key)
                {
                    return true;
                }
                cPair = cPair->Prev;
            }
            return false;
        }
        public bool ContainsValue(UInt32 value)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (!cPair->Empty && cPair->Value == value)
                {
                    return true;
                }
                cPair = cPair->Prev;
            }
            return false;
        }
        public bool ContainsKeyInRange(UInt32 startKey, UInt32 endKey)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (!cPair->Empty && cPair->Key >= startKey && cPair->Key < endKey)
                {
                    return true;
                }
                cPair = cPair->Prev;
            }
            return false;
        }
        public bool ContainsValueInRange(UInt32 startValue, UInt32 endValue)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (!cPair->Empty && cPair->Value >= startValue && cPair->Value < endValue)
                {
                    return true;
                }
                cPair = cPair->Prev;
            }
            return false;
        }

        public UInt32 GetFirstKeyOfValue(UInt32 value)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (!cPair->Empty && cPair->Value == value)
                {
                    return cPair->Key;
                }
                cPair = cPair->Prev;
            }
            return 0;
        }

        public UInt32 this[UInt32 key]
        {
            get
            {
                KeyValuePair* cPair = list;
                while (cPair != null)
                {
                    if (cPair->Key == key)
                    {
                        return cPair->Value;
                    }
                    cPair = cPair->Prev;
                }
                ExceptionMethods.Throw(new Exceptions.ArgumentException("Key not found in dictionary!"));
                return 0;
            }
            set
            {
                KeyValuePair* cPair = list;
                while (cPair != null)
                {
                    if (cPair->Key == key)
                    {
                        cPair->Value = value;
                        break;
                    }
                    cPair = cPair->Prev;
                }
                if(cPair == null)
                {
                    Add(key, value);
                }
            }
        }

        public Iterator GetIterator()
        {
            iterator.StoreState();
            iterator.Reset(list);
            return iterator;
        }
        public Iterator GetNewIterator()
        {
            return new Iterator(list);
        }
    }

    public unsafe class UInt64Dictionary : Object
    {
        public struct KeyValuePair
        {
            public UInt64 Key;
            public UInt64 Value;
            internal KeyValuePair* Prev;
            internal KeyValuePair* Next;
        }

        public sealed class Iterator : Object
        {
            private KeyValuePair* list;
            private KeyValuePair* storedList;

            public Iterator(KeyValuePair* aList)
            {
                list = aList;
            }

            internal void Reset(KeyValuePair* aList)
            {
                list = aList;
            }

            public void StoreState()
            {
                storedList = list;
            }
            public void RestoreState()
            {
                list = storedList;
            }

            public bool HasNext()
            {
                return list != null;
            }
            public KeyValuePair Next()
            {
                KeyValuePair result = *list;
                list = list->Prev;
                return result;
            }
        }

        protected KeyValuePair* list;
        protected Iterator iterator;

        public UInt64Dictionary()
        {
            list = null;
            iterator = new Iterator(list);
        }

        public void Add(UInt64 key, UInt64 value, bool SkipCheck = false)
        {
            if (!SkipCheck)
            {
                if (Contains(key))
                {
                    ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
                }
            }

            KeyValuePair* newItem = (KeyValuePair*)Heap.Alloc((uint)sizeof(KeyValuePair), "UInt64Dictionary.Add");
            newItem->Key = key;
            newItem->Value = value;
            newItem->Next = null;
            newItem->Prev = list; 
            if (list != null)
            {
                list->Next = newItem;
            }
            list = newItem;
        }
        public void AddRange(UInt64 keyStart, UInt64 keyStep, UInt64[] values)
        {
            if (ContainsItemInRange(keyStart, keyStart + ((uint)values.Length * keyStep)))
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
            }

            UInt64 keyVal = keyStart;
            for (uint i = 0; i < values.Length; i++)
            {
                Add(keyVal, values[i], true);

                keyVal += keyStep;
            }
        }
        public void Remove(UInt64 key)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (cPair->Key == key)
                {
                    KeyValuePair* prev = cPair->Prev;
                    KeyValuePair* next = cPair->Next;
                    
                    if (prev != null)
                    {
                        prev->Next = next;
                    }
                    if (next != null)
                    {
                        next->Prev = prev;
                    }

                    Heap.Free(cPair);
                }
                cPair = cPair->Prev;
            }
        }
        public void RemoveRange(UInt64 keyStart, UInt64 keyStep, UInt64 numKeys)
        {
            for (int i = (int)(numKeys - 1); i >= 0; i--)
            {
                UInt64 currKey = (keyStart + ((UInt64)i * keyStep));
                Remove(currKey);
            }
        }

        public bool Contains(UInt64 key)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (cPair->Key == key)
                {
                    return true;
                }
                cPair = cPair->Prev;
            }
            return false;
        }
        public bool ContainsItemInRange(UInt64 startKey, UInt64 endKey)
        {
            KeyValuePair* cPair = list;
            while (cPair != null)
            {
                if (cPair->Key >= startKey && cPair->Key < endKey)
                {
                    return true;
                }
                cPair = cPair->Prev;
            }
            return false;
        }

        public UInt64 this[UInt64 key]
        {
            get
            {
                KeyValuePair* cPair = list;
                while (cPair != null)
                {
                    if (cPair->Key == key)
                    {
                        return cPair->Value;
                    }
                    cPair = cPair->Prev;
                }
                ExceptionMethods.Throw(new Exceptions.ArgumentException("Key not found in dictionary!"));
                return 0;
            }
            set
            {
                KeyValuePair* cPair = list;
                while (cPair != null)
                {
                    if (cPair->Key == key)
                    {
                        cPair->Value = value;
                        break;
                    }
                    cPair = cPair->Prev;
                }
                if (cPair == null)
                {
                    Add(key, value);
                }
            }
        }

        public Iterator GetIterator()
        {
            iterator.Reset(list);
            return iterator;
        }
        public Iterator GetNewIterator()
        {
            return new Iterator(list);
        }
    }
}
