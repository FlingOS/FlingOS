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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     A node in a static constructor dependency tree.
    /// </summary>
    /// <remarks>
    ///     This class is used to create a tree structure of which static constructors depend on which other
    ///     static constructors. The tree is later flattened into a list where children appear before parents.
    ///     That list is then used to generate the IL code that calls all the static constructors in the
    ///     necessary order before any of the rest of the kernel executes.
    /// </remarks>
    public class StaticConstructorDependency
    {
        /// <summary>
        ///     A list of the child-nodes i.e. static constructors that TheConstructor depends upon.
        /// </summary>
        public List<StaticConstructorDependency> Children = new List<StaticConstructorDependency>();

        /// <summary>
        ///     The constructor represented by this node.
        /// </summary>
        public ConstructorInfo TheConstructor;

        /// <summary>
        ///     Returns the first node representing of the specified constructor - full-depth search.
        /// </summary>
        /// <value>Searches the complete tree looking for the node.</value>
        /// <param name="inf">The constructor to search for.</param>
        /// <returns>The first node representing of the specified constructor - full-depth search.</returns>
        public StaticConstructorDependency this[ConstructorInfo inf]
        {
            get
            {
                if (TheConstructor == inf)
                {
                    return this;
                }
                List<StaticConstructorDependency> posDeps = (from deps in Children
                    where deps.TheConstructor.Equals(inf)
                    select deps).ToList();
                if (posDeps.Count > 0)
                {
                    return posDeps.First();
                }
                foreach (StaticConstructorDependency child in Children)
                {
                    StaticConstructorDependency posDep = child[inf];
                    if (posDep != null)
                    {
                        return posDep;
                    }
                }
                return null;
            }
        }

        /// <summary>
        ///     Flattens the tree such that deepest-children appear first, shallowest-parents appear last.
        ///     Also removes any duplicates so that first instance only appears in the list.
        /// </summary>
        /// <returns>The flattened list.</returns>
        public List<ConstructorInfo> Flatten()
        {
            List<ConstructorInfo> result = new List<ConstructorInfo>();
            List<ConstructorInfo> finalResult = new List<ConstructorInfo>();

            foreach (StaticConstructorDependency child in Children)
            {
                result.AddRange(child.Flatten());
            }
            if (TheConstructor != null)
            {
                result.Add(TheConstructor);
            }

            //Remove duplicates
            for (int i = 0; i < result.Count; i++)
            {
                if (!finalResult.Contains(result[i]))
                {
                    finalResult.Add(result[i]);
                }
            }

            return finalResult;
        }
    }
}