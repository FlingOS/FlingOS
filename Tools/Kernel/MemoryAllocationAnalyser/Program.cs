using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MemoryAllocationAnalyser
{
    internal class Program
    {
        private static readonly List<Tuple<uint, uint>> AllocatedRanges = new List<Tuple<uint, uint>>();

        private static void Main(string[] args)
        {
            string[] Lines = File.ReadAllLines(args[0]);
            List<string> NewLines = new List<string>();

            int i = 0;
            foreach (string aLine in Lines)
            {
                i++;

                if (!string.IsNullOrEmpty(aLine))
                {
                    string[] lineParts = aLine.Split(' ');
                    if (lineParts[0] == "Allocating")
                    {
                        uint address = Convert.ToUInt32(lineParts[1].Substring(2, 8), 16);
                        uint size = Convert.ToUInt32(lineParts[3].Substring(2, 8), 16);

                        List<Tuple<uint, uint>> Clashes = (from ranges in AllocatedRanges
                            where Overlaps(ranges.Item1, ranges.Item2, address, size)
                            select ranges).ToList();

                        string NewLine = "Allocating 0x" + address.ToString("X2") + ", Size: 0x" + size.ToString("X8");
                        if (Clashes.Count > 0)
                        {
                            if (Clashes[0].Item1 == address)
                            {
                                NewLine += " - Reallocated";
                                Console.WriteLine("Same address reallocated! Address: " + address.ToString("X8") +
                                                  ", Line: " + i);
                            }
                            else
                            {
                                NewLine += " - Reallocated within block";
                                Console.WriteLine("Address reallocated within block! Address: " + address.ToString("X8") +
                                                  ", Line: " + i);
                            }
                        }
                        else
                        {
                            AllocatedRanges.Add(new Tuple<uint, uint>(address, size));
                        }

                        NewLines.Add(NewLine);
                    }
                    else if (lineParts[0] == "Freeing")
                    {
                        uint address = Convert.ToUInt32(lineParts[1].Substring(2, 8), 16);
                        List<Tuple<uint, uint>> Allocations = (from ranges in AllocatedRanges
                            where Overlaps(ranges.Item1, ranges.Item2, address, 1)
                            select ranges).ToList();
                        string NewLine = "Freeing " + address.ToString("X8");
                        if (Allocations.Count == 0)
                        {
                            NewLine += " - Unallocated";
                            //Console.WriteLine("Freeing unallocated address! Address: " + address.ToString("X2") + ", Line " + i);
                        }
                        else
                        {
                            AllocatedRanges.Remove(Allocations[0]);
                        }

                        NewLines.Add(NewLine);
                    }
                    else
                    {
                        NewLines.Add(aLine);
                    }
                }
            }

            File.WriteAllLines(args[0], NewLines);
        }

        private static bool Overlaps(uint start1, uint length1,
            uint start2, uint length2)
        {
            bool result = false;

            result |= (start1 >= start2) && (start1 < start2 + length2);
            result |= (start1 + length1 >= start2) && (start1 + length1 < start2 + length2);

            return result;
        }
    }
}