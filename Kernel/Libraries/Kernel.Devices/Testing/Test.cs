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

using Kernel.Framework;

namespace Kernel.Devices.Testing
{
    /// <summary>
    ///     Delegate for outputting informational messages during a test.
    /// </summary>
    /// <param name="TestName">The (human-readable) name of the current test.</param>
    /// <param name="Message">The message to output.</param>
    public delegate void OutputMessageDelegate(String TestName, String Message);

    /// <summary>
    ///     Delegate for outputting warning messages during a test.
    /// </summary>
    /// <param name="TestName">The (human-readable) name of the current test.</param>
    /// <param name="Message">The message to output.</param>
    public delegate void OutputWarningDelegate(String TestName, String Message);

    /// <summary>
    ///     Delegate for outputting error messages during a test.
    /// </summary>
    /// <param name="TestName">The (human-readable) name of the current test.</param>
    /// <param name="Message">The message to output.</param>
    public delegate void OutputErrorDelegate(String TestName, String Message);

    /// <summary>
    ///     Stub base class for implementing device tests. 
    /// </summary>
    /// <remarks>
    ///     FlingOS basically has no automated testing at the moment. The FlingOS Drivers Compiler is behaviourally tested 
    ///     with around 70% coverage using FlingOops - it catches the biggest compielr bugs along with lots of the corner
    ///     cases found during development.
    ///     TODO: Testing should be moved to a new project and be expanded into a fuller framework.
    /// </remarks>
    public abstract class Test : Object
    {
    }
}