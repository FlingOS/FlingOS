#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    /// <summary>
    /// Represents an object type specification.
    /// </summary>
    [Compiler.TypeClass]
    public unsafe abstract class Type : System.Type
    {
        /// <summary>
        /// The size of the object in memory.
        /// </summary>
        public uint Size;
        /// <summary>
        /// The type ID.
        /// </summary>
        public uint Id;
        /// <summary>
        /// The size of the type when on the stack or in an array.
        /// </summary>
        public uint StackSize;
        /// <summary>
        /// Whether the type is a value type or not.
        /// </summary>
        public new bool IsValueType;

        /// <summary>
        /// A pointer to the start of the method table.
        /// </summary>
        public byte* MethodTablePtr;

        /// <summary>
        /// Compares two types by ID to see if they represent the same type.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are equal, otherwise false.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static bool operator ==(Type x, Type y)
        {
            return x.Id == y.Id;
        }
        /// <summary>
        /// Compares two types by ID to see if they represent the different types.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are not equal, otherwise false.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static bool operator !=(Type x, Type y)
        {
            return x.Id != y.Id;
        }
    }
}
