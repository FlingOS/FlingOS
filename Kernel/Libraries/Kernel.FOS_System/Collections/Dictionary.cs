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
    public class UInt32Dictionary : FOS_System.Object
    {
        public UInt32List Keys;
        public UInt32List Values;

        public UInt32Dictionary(int capacity = 5)
        {
            Keys = new UInt32List(capacity);
            Values = new UInt32List(capacity);
        }

        public int Capacity
        {
            get
            {
                return Keys.Capacity;
            }
            set
            {
                Keys.Capacity = value;
                Values.Capacity = value;
            }
        }

        public void Add(UInt32 key, UInt32 value)
        {
            if (Keys.IndexOf(key) > -1)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
            }

            Keys.Add(key);
            Values.Add(value);
        }
        public void AddRange(UInt32 keyStart, UInt32 keyStep, UInt32[] values)
        {
            if (Keys.ContainsItemInRange(keyStart, keyStart + ((uint)values.Length * keyStep)))
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
            }

            Capacity += values.Length;

            UInt32 keyVal = keyStart;
            for (uint i = 0; i < values.Length; i++)
            {
                Keys.Add(keyVal);
                Values.Add(values[i]);

                keyVal += keyStep;
            }
        }
        public void Remove(UInt32 key)
        {
            int index = Keys.IndexOf(key);
            if (index > -1)
            {
                Values.RemoveAt(index);
                Keys.RemoveAt(index);
            }
        }
        public void RemoveRange(UInt32 keyStart, UInt32 keyStep, UInt32 numKeys)
        {
            // Remove in reverse order - it's significantly faster
            for (int i = (int)(numKeys - 1); i >= 0; i--)
            {
                UInt32 currKey = (keyStart + ((UInt32)i * keyStep));
                Remove(currKey);
            }
        }

        public bool Contains(UInt32 key)
        {
            return Keys.IndexOf(key) > -1;
        }

        public UInt32 this[UInt32 key]
        {
            get
            {
                int keyIdx = Keys.IndexOf(key);
                if (keyIdx == -1)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("Key not found in dictionary!"));
                }
                return Values[keyIdx];
            }
            set
            {
                int keyIdx = Keys.IndexOf(key);
                if (keyIdx == -1)
                {
                    Add(key, value);
                }
                else
                {
                    Values[keyIdx] = value;
                }
            }
        }
    }

    public class UInt64Dictionary : FOS_System.Object
    {
        public UInt64List Keys;
        public UInt64List Values;

        public UInt64Dictionary(int capacity = 5)
        {
            Keys = new UInt64List(capacity);
            Values = new UInt64List(capacity);
        }

        public int Capacity
        {
            get
            {
                return Keys.Capacity;
            }
            set
            {
                Keys.Capacity = value;
                Values.Capacity = value;
            }
        }

        public void Add(UInt64 key, UInt64 value)
        {
            if (Keys.IndexOf(key) > -1)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
            }

            Keys.Add(key);
            Values.Add(value);
        }
        public void AddRange(UInt64 keyStart, UInt64 keyStep, UInt64[] values)
        {
            if (Keys.ContainsItemInRange(keyStart, keyStart + ((uint)values.Length * keyStep)))
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Cannot add duplicate key to the dictionary!"));
            }

            Capacity += values.Length;

            UInt64 keyVal = keyStart;
            for (uint i = 0; i < values.Length; i++)
            {
                Keys.Add(keyVal);
                Values.Add(values[i]);

                keyVal += keyStep;
            }
        }
        public void Remove(UInt64 key)
        {
            int index = Keys.IndexOf(key);
            if (index > -1)
            {
                Values.RemoveAt(index);
                Keys.RemoveAt(index);
            }
        }
        public void RemoveRange(UInt64 keyStart, UInt64 keyStep, UInt64 numKeys)
        {
            // Remove in reverse order - it's significantly faster
            for (int i = (int)(numKeys - 1); i >= 0; i--)
            {
                UInt64 currKey = (keyStart + ((UInt64)i * keyStep));
                Remove(currKey);
            }
        }

        public bool Contains(UInt64 key)
        {
            return Keys.IndexOf(key) > -1;
        }

        public UInt64 this[UInt64 key]
        {
            get
            {
                int keyIdx = Keys.IndexOf(key);
                if (keyIdx == -1)
                {
                    ExceptionMethods.Throw(new Exceptions.ArgumentException("Key not found in dictionary!"));
                }
                return Values[keyIdx];
            }
            set
            {
                int keyIdx = Keys.IndexOf(key);
                if (keyIdx == -1)
                {
                    Add(key, value);
                }
                else
                {
                    Values[keyIdx] = value;
                }
            }
        }
    }
}
