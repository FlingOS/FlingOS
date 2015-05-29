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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Kernel;

namespace Testing._Kernel
{
    /// <summary>
    /// Test class for Kernel.BasicConsole class.
    /// </summary>
    [TestClass]
    public unsafe class BasicConsoleTest
    {
        /// <summary>
        /// Pointer to the memory to use for the simulated video memory for use by the console.
        /// </summary>
        private static char* vidMemory;
        /// <summary>
        /// Pointer to the end of the memory to use for the simulated video memory for use by the console.
        /// </summary>
        private static char* vidMemoryMax;

        /// <summary>
        /// Initialises the test class.
        /// </summary>
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            vidMemory = (char*)Marshal.AllocHGlobal(BasicConsole.rows * BasicConsole.cols * 2);
            vidMemoryMax = vidMemory + (BasicConsole.rows * BasicConsole.cols);
            BasicConsole.vidMemBasePtr = vidMemory;
        }
        /// <summary>
        /// Cleans up the test class.
        /// </summary>
        [ClassCleanup]
        public static void Cleanup()
        {
            Marshal.FreeHGlobal((IntPtr)vidMemory);
        }

        /// <summary>
        /// Tests the Clear() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Video - Text")]
        public void Clear_Test()
        {
            BasicConsole.Clear();
            
            for (int i = 0; i < (BasicConsole.rows * BasicConsole.cols); i++)
            {
                Assert.AreEqual(0x0000, vidMemory[i], "Memory not cleared to 0! Index: " + i);
            }
        }

        /// <summary>
        /// Tests the Write() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Video - Text")]
        public void Write_Test()
        {
            BasicConsole.Clear();

            for (int i = 0; i < 1000; i++)
            {
                Write_Test("Test");
                Write_Test("`1234567890-=qwertyuiop[]asdfghjkl;'#\\zxcvbnm,./QWERTYUIOPASDFGHJKLZXCVBNM!\"£$%^&*()_Activator+{}:@~<>?|¬¦");
            }
        }
        /// <summary>
        /// Tests the Write() method.
        /// </summary>
        /// <param name="StringToWrite">The string to write as the test.</param>
        private void Write_Test(string StringToWrite)
        {
            int offset = BasicConsole.Offset;
            int newOffset = offset + StringToWrite.Length;
            newOffset = Math.Min(BasicConsole.rows * BasicConsole.cols, newOffset);

            BasicConsole.Write(StringToWrite);
            Assert.AreEqual(newOffset, BasicConsole.Offset, string.Format("Video memory offset not shifted correctly. Offset: {0}; New offset: {1}", offset, newOffset));

            int stringIndex = 0;
            for (int i = offset; i < StringToWrite.Length; i++, stringIndex++)
            {
                int expected = ((StringToWrite[stringIndex] & 0x00FF) | BasicConsole.colour);
                int actual = vidMemory[i];
                Assert.AreEqual(expected, actual, "String not written correctly! String: " + StringToWrite + "; Index: " + i + "; Expected: 0x" + expected.ToString("X4") + "; Actual: 0x" + actual.ToString("X4"));
            }
        }

        /// <summary>
        /// Tests the WriteLine() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Video - Text")]
        public void WriteLine_Test()
        {
            BasicConsole.Clear();
            for (int i = 0; i < 1000; i++)
            {
                WriteLine_Test("Test");
                WriteLine_Test("`1234567890-=qwertyuiop[]asdfghjkl;'#\\zxcvbnm,./QWERTYUIOPASDFGHJKLZXCVBNM!\"£$%^&*()_Activator+{}:@~<>?|¬¦");
            }
        }
        /// <summary>
        /// Tests the WriteLine() method.
        /// </summary>
        /// <param name="StringToWrite">The string to write as the test.</param>
        private void WriteLine_Test(string StringToWrite)
        {
            int offset = BasicConsole.Offset;
            int newOffset = offset + StringToWrite.Length;
            newOffset += (BasicConsole.cols - (newOffset % BasicConsole.cols));
            newOffset = Math.Min(BasicConsole.rows * BasicConsole.cols, newOffset);

            BasicConsole.WriteLine(StringToWrite);
            Assert.AreEqual(newOffset, BasicConsole.Offset, string.Format("Video memory offset not shifted correctly. Offset: {0}; New offset: {1}", offset, newOffset));

            int stringIndex = 0;
            for (int i = offset; i < StringToWrite.Length; i++, stringIndex++)
            {
                char expected = (char)((StringToWrite[stringIndex] & 0x00FF) | BasicConsole.colour);
                char actual = vidMemory[i];
                Assert.AreEqual(expected, actual, "String not written correctly! String: " + StringToWrite + "; Index: " + i);
            }
            for (int i = offset + StringToWrite.Length; i < BasicConsole.Offset; i++)
            {
                Assert.AreEqual(0x0000, vidMemory[i], "String not written correctly! String: " + StringToWrite + "; Index: " + i);
            }
        }

        /// <summary>
        /// Tests the "colour" field is used properly.
        /// </summary>
        [TestMethod]
        [TestCategory("Video - Text")]
        public void Colour_Test()
        {
            BasicConsole.colour = (char)0x0100;
            Write_Test("Test");
            BasicConsole.colour = (char)0x0200;
            Write_Test("Test");
            BasicConsole.colour = (char)0x0300;
            Write_Test("Test");
        }
    }
}
