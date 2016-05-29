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

using Drivers.Compiler.Attributes;

namespace Kernel
{
    /// <summary>
    ///     Contains plugged methos that are pre-requisites for the kernel to boot.
    ///     For example, the Multiboot Signature.
    /// </summary>
    public static class Boot
    {
        //These methods are listed in the order they are included
        //Most of them are not callable methods - so don't touch/try!
        //Most of them, the hard-coded labels don't conform to compiler-standard labels
        //If they are public, you can actually call them 
        //      - Reset method will never return
        //      - Methods which are public must have a compiler-standard label in the 
        //          hard-coded asm so they can be called by the compiler generated code

        /// <summary>
        ///     Inserts the multiboot signature at the start of the file.
        /// </summary>
        [PluggedMethod(ASMFilePath = @"ASM\LoadSequence")]
        [SequencePriority(Priority = long.MinValue + 0)]
        private static void MultibootSignature()
        {
        }

        /// <summary>
        ///     Inserts the pre-entrypoint kernel start method plug.
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 1)]
        private static void Kernel_Start()
        {
        }

        /// <summary>
        ///     Initialises virtual memory (i.e. shifts to higher-half kernel).
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 2)]
        private static void VirtualMemInit()
        {
        }

        /// <summary>
        ///     Inserts the initialise stack code.
        ///     Kernel stack space is currently hard-coded into the
        ///     Multiboot Signature asm.
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 3)]
        private static void InitStack()
        {
        }

        /// <summary>
        ///     Initialises the Global Descriptor Table.
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 4)]
        private static void InitGDT()
        {
        }

        /// <summary>
        ///     Initialises the Interrupt Descriptor Table.
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 5)]
        private static void InitIDT()
        {
        }

        /// <summary>
        ///     Inserts the method that handles what happens when the Multiboot
        ///     Signature is invalid or undetected.
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 8)]
        private static void HandleNoMultiboot()
        {
        }

        /// <summary>
        ///     Inserts the stub that calls the main kernel entrypoint.
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 7)]
        private static void MainEntrypoint()
        {
        }

        /// <summary>
        ///     Resets the OS / CPU / etc. i.e. terminates the OS
        /// </summary>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 9)]
        public static void Reset()
        {
        }

        /// <summary>
        ///     Writes a piece of text to the first line of the screen. Note: Does not use same string memory structure as C# so
        ///     cannot be called
        ///     from C#. Uses format of: Length as DWORD, Characters as BYTE
        /// </summary>
        /// <param name="aText">
        ///     The text to write. First dword should be the length of the string. (Inserted by compiler for string
        ///     literals)
        /// </param>
        /// <param name="aColour">
        ///     The foreground/background (DOS) colour to write in - 0xXY where X is background colour and Y is
        ///     foreground colour.
        /// </param>
        [PluggedMethod(ASMFilePath = null)]
        [SequencePriority(Priority = long.MinValue + 10)]
        private static void WriteDebugVideo(string aText, uint aColour)
        {
        }
    }
}