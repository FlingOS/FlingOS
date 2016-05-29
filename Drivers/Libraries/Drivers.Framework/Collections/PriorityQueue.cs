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

using Drivers.Framework.Exceptions;

namespace Drivers.Framework.Collections
{
    /// <summary>
    ///     Implements a reasonably efficient binary heap.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implementation based on notes by Benjamin Sach at The University of Bristol from the COMS21103 Data Structures
    ///         and Algorithms module.
    ///     </para>
    ///     <para>
    ///         Uses the List and Comparable classes for the underlying data structure creating an implicit tree. The use of
    ///         List allows easy automatic
    ///         expansion and adding/removing items at the expense of some performance.
    ///     </para>
    /// </remarks>
    public class PriorityQueue : Object
    {
        private readonly List ImplicitHeap;
        public string Name = "[No Name]";

        public int Count
        {
            get { return ImplicitHeap.Count; }
        }

        public int Capacity
        {
            get { return ImplicitHeap.Capacity; }
            set { ImplicitHeap.Capacity = value; }
        }

        public PriorityQueue()
            : this(20)
        {
        }

        public PriorityQueue(int InitialCapacity)
        {
            ImplicitHeap = new List(InitialCapacity);
        }

        public void DecreaseKey(Comparable X, int NewKey)
        {
            if (NewKey > X.Key)
            {
                ExceptionMethods.Throw(new ArgumentException("New key is larger than old key!"));
            }

            X.Key = NewKey;

            Comparable parent = Parent(X);
            while (parent != null &&
                   X.Key < parent.Key)
            {
                Swap(parent, X);

                parent = Parent(X);
            }
        }

        public bool Insert(Comparable X)
        {
            //for (int i = 0; i < ImplicitHeap.Count; i++)
            //{
            //    if (ImplicitHeap[i] == X)
            //    {
            //        BasicConsole.Write(Name);
            //        BasicConsole.Write(" - ");
            //        BasicConsole.WriteLine("Priority queue : Error! Attempted to re-insert item.");
            //        return false;
            //    }
            //}

            X.Position = ImplicitHeap.Count + 1;
            ImplicitHeap.Add(X);
            DecreaseKey(X, X.Key);
            return true;
        }

        public Comparable ExtractMin()
        {
            Comparable result = null;

            if (ImplicitHeap.Count > 0)
            {
                result = (Comparable)ImplicitHeap[0];
                result.Position = 0;

                if (ImplicitHeap.Count > 1)
                {
                    Comparable Y = (Comparable)ImplicitHeap.Last();
                    ImplicitHeap.RemoveAt(Y.Position - 1);

                    Y.Position = 1;
                    ImplicitHeap[0] = Y;

                    Rebalance(Y);
                }
                else
                {
                    ImplicitHeap.RemoveAt(0);
                }
            }

            return result;
        }

        public Comparable PeekMin()
        {
            Comparable result = null;

            if (ImplicitHeap.Count > 0)
            {
                result = (Comparable)ImplicitHeap[0];
            }

            return result;
        }

        public void Delete(Comparable X)
        {
            //BasicConsole.WriteLine("Deleting");

            //bool OK = false;
            //for (int i = 0; i < ImplicitHeap.Count; i++)
            //{
            //    if (ImplicitHeap[i] == X)
            //    {
            //        OK = true;
            //        break;
            //    }
            //}
            //if (!OK)
            //{
            //    BasicConsole.WriteLine("Deleting when not present.");
            //}

            if (X.Position <= 0 || X.Position > ImplicitHeap.Count || ImplicitHeap[X.Position - 1] != X)
            {
                //BasicConsole.WriteLine("No delete");
                return;
            }

            //BasicConsole.WriteLine("Will delete");
            if (ImplicitHeap.Count > 1)
            {
                //BasicConsole.WriteLine("Using last item");
                Comparable Y = (Comparable)ImplicitHeap.Last();
                if (X != Y)
                {
                    //BasicConsole.WriteLine("X not equal to Y");
                    //BasicConsole.WriteLine("Removing Y");
                    ImplicitHeap.RemoveAt(Y.Position - 1);

                    //BasicConsole.WriteLine("Setting Y position");
                    Y.Position = X.Position;
                    //BasicConsole.WriteLine("Setting Y in array");
                    ImplicitHeap[Y.Position - 1] = Y;

                    //BasicConsole.WriteLine("Rebalancing");
                    Rebalance(Y);
                }
                else
                {
                    //BasicConsole.WriteLine("X is Y, removing X");
                    ImplicitHeap.RemoveAt(X.Position - 1);
                }
            }
            else
            {
                //BasicConsole.WriteLine("Removing at index 0");
                ImplicitHeap.RemoveAt(0);
            }

            //BasicConsole.WriteLine("Reset position");
            X.Position = 0;
        }

        public void DecreaseAllKeys(int amount, int min)
        {
            for (int i = 0; i < ImplicitHeap.Count; i++)
            {
                Comparable x = (Comparable)ImplicitHeap[i];
                int NewKey = x.Key - amount;
                DecreaseKey(x, NewKey < min ? min : NewKey);
            }
        }

        private void Rebalance(Comparable Y)
        {
            //BasicConsole.WriteLine("Rebalance (1)");

            Comparable ALeftChild = LeftChild(Y);
            Comparable ARightChild = RightChild(Y);

            while ((ALeftChild != null && Y.Key > ALeftChild.Key) ||
                   (ARightChild != null && Y.Key > ARightChild.Key))
            {
                //BasicConsole.WriteLine("Rebalance (2)");

                if (ALeftChild != null && Y.Key > ALeftChild.Key)
                {
                    if (ARightChild != null && ARightChild.Key < ALeftChild.Key)
                    {
                        Swap(ARightChild, Y);
                    }
                    else
                    {
                        Swap(ALeftChild, Y);
                    }
                }
                else if (ARightChild != null && Y.Key > ARightChild.Key)
                {
                    Swap(ARightChild, Y);
                }

                ALeftChild = LeftChild(Y);
                ARightChild = RightChild(Y);
            }
        }

        public String ToString()
        {
            String result = "";

            for (int i = 0; i < ImplicitHeap.Count; i++)
            {
                result += ((String)((Comparable)ImplicitHeap[i]).Key).PadRight(20, ' ');
            }

            return result;
        }

        private void Swap(Comparable X, Comparable Y, bool print = false)
        {
            int tempPos = X.Position;
            X.Position = Y.Position;
            Y.Position = tempPos;

            ImplicitHeap[X.Position - 1] = X;
            ImplicitHeap[Y.Position - 1] = Y;
        }

        private Comparable Parent(Comparable X)
        {
            int idx = ParentIndex(X) - 1;
            if (idx > -1)
            {
                return (Comparable)ImplicitHeap[idx];
            }
            return null;
        }

        private Comparable LeftChild(Comparable X)
        {
            int idx = LeftChildIndex(X) - 1;
            if (idx < ImplicitHeap.Count)
            {
                return (Comparable)ImplicitHeap[idx];
            }
            return null;
        }

        private Comparable RightChild(Comparable X)
        {
            int idx = RightChildIndex(X) - 1;
            if (idx < ImplicitHeap.Count)
            {
                return (Comparable)ImplicitHeap[idx];
            }
            return null;
        }

        private int ParentIndex(Comparable X)
        {
            if (X.Position > 1)
            {
                return X.Position/2;
            }
            return 0;
        }

        private int LeftChildIndex(Comparable X)
        {
            return X.Position*2;
        }

        private int RightChildIndex(Comparable X)
        {
            return X.Position*2 + 1;
        }
    }
}