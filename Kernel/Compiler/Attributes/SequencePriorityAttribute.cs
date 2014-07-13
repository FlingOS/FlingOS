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

namespace Kernel.Compiler
{
    /// <summary>
    /// Specifies the priority of a method when it is sequenced in the final 
    /// assembly file.
    /// <para>Value meanings:</para>
    /// <list type="bullet">
    /// <item>
    /// <term>long.MinValue</term>
    /// <description>The lowest possible priority. Indicates the method should 
    /// be the first bit of ASM in the final file.
    /// </description>
    /// </item>
    /// <item>
    /// <term>0</term>
    /// <description>The default priority. Indicates it doesn't matter where 
    /// the method appears in the final ASM file.
    /// </description>
    /// </item>
    /// <item>
    /// <term>long.MaxValue</term>
    /// <description>The highest possible priority. Indicates the method should 
    /// be the last bit of ASM in the final file.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Low priority = close to start of file
    /// High priority = close to end of file
    /// </para>
    /// <para>
    /// This is expected to be used for:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>Plugged methods</term>
    /// <description>
    /// Plugs such as MultibootSignature or entry points which must appear at 
    /// the start of the file for the OS to be bootable.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Data blocks</term>
    /// <description>
    /// Data blocks such as StringLiterals which are nice to have at 
    /// the end of the file for debugging etc.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SequencePriorityAttribute : Attribute
    {
        /// <summary>
        /// The priority of the targetted method.
        /// </summary>
        /// <remarks>
        /// Please see class remarks.
        /// </remarks>
        public long Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Initialises a new SequencePriorityAttribute with priority 0.
        /// </summary>
        /// <remarks>
        /// 0 is the default (/standard) priority. 
        /// </remarks>
        public SequencePriorityAttribute() 
            : base()
        {
            Priority = 0;
        }
    }
}
