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
    public abstract class Type : System.Type
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
        /// Compares two types by ID to see if they represent the same type.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are equal, otherwise false.</returns>
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
        public static bool operator !=(Type x, Type y)
        {
            return x.Id != y.Id;
        }
    }
}
