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
    ///     <para>
    ///         Indicates a method is plugged. Use the ASMFilePath property to specify the path to the
    ///         ASM plug file.
    ///     </para>
    ///     <para>
    ///         The path should be relative to the build directory and not include an
    ///         extension.
    ///     </para>
    ///     <para>
    ///         e.g. @"ASM\ExampleFolder\Example" would result in the following path being
    ///         used: @"{ProjectDirectory}\bin\{BuildMode}\ASM\ExampleFolder\Example.{TargetArch}.asm"
    ///         so for an x86 debug build that would be:
    ///         @"{ProjectDirectory}\bin\Debug\ASM\ExampleFolder\Example.x86.asm"
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For backwards compatibility with the Kernel Compiler, for x86 32-bit builds, the file
    ///         extension ".x86_32.asm" is also recognised along with the new ".x86.asm" extension.
    ///     </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class PluggedMethodAttribute : Attribute
    {
        /// <summary>
        ///     The path (relative to the build directory, excluding extension - see attribute summary) to the plug file.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        public string ASMFilePath { get; set; }
    }
}