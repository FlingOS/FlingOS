using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Collections
{
    public class UInt32Dictionary : FOS_System.Object
    {
        public UInt32List Keys = new UInt32List();
        public UInt32List Values = new UInt32List();

        public UInt32Dictionary()
        {
        }

        public void Add(UInt32 key, UInt32 value)
        {
            if (Keys.IndexOf(key) > -1)
            {
                ExceptionMethods.Throw(new FOS_System.Exception(
                    ""));
            }

            Keys.Add(key);
            Values.Add(value);
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

        public bool Contains(UInt32 key)
        {
            return Keys.IndexOf(key) > -1;
        }

        public UInt32 this[UInt32 key]
        {
            get
            {
                return Values[Keys.IndexOf(key)];
            }
            set
            {
                if (Keys.IndexOf(key) == -1)
                {
                    Add(key, value);
                }
                else
                {
                    Values[Keys.IndexOf(key)] = value;
                }
            }
        }
    }
}
