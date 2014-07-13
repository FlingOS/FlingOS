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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    /// <summary>
    /// All objects (that are GC managed) should derive from this type.
    /// </summary>
    public class Object : ObjectWithType
    {
    }
    /// <summary>
    /// Represents an object with a type. You should use the <see cref="Kernel.FOS_System.Object"/> class.
    /// </summary>
    /// <remarks>
    /// We implement it like this so that _Type field is always the first
    /// field in memory of all objects.
    /// </remarks>
    public class ObjectWithType
    {
        /// <summary>
        /// The underlying, specific type of the object specified when it was created.
        /// </summary>
        public Type _Type;   
    }
}
