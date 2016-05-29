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

namespace Drivers.Compiler.Attributes
{
    /// <summary>
    ///     Specifies the priority of a method when it is sequenced in the final
    ///     assembly file.
    ///     <para>Value meanings:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <term>long.MinValue</term>
    ///             <description>
    ///                 The lowest possible priority. Indicates the method should
    ///                 be the first bit of ASM in the final file.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>0</term>
    ///             <description>
    ///                 The default priority. Indicates it doesn't matter where
    ///                 the method appears in the final ASM file.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>long.MaxValue</term>
    ///             <description>
    ///                 The highest possible priority. Indicates the method should
    ///                 be the last bit of ASM in the final file.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Low priority = close to start of file
    ///         High priority = close to end of file
    ///     </para>
    ///     <para>
    ///         This is expected to be used for:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <term>Plugged methods</term>
    ///             <description>
    ///                 Plugs such as MultibootSignature or entry points which must appear at
    ///                 the start of the file for the OS to be bootable.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Data blocks</term>
    ///             <description>
    ///                 Data blocks such as StringLiterals which are nice to have at
    ///                 the end of the file for debugging etc.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SequencePriorityAttribute : Attribute
    {
        /// <summary>
        ///     The priority of the targeted method.
        /// </summary>
        /// <remarks>
        ///     Please see class remarks.
        /// </remarks>
        /// <value>Gets/sets an implicitly defined field.</value>
        public long Priority { get; set; }

        /// <summary>
        ///     Initialises a new SequencePriorityAttribute with priority 0.
        /// </summary>
        /// <remarks>
        ///     0 is the default (/standard) priority.
        /// </remarks>
        public SequencePriorityAttribute()
        {
            Priority = 0;
        }
    }
}