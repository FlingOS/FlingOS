using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Testing.Utilities;
using Kernel.FOS_System;

namespace Testing._Kernel.FOS_System
{
    /// <summary>
    /// Test class for Kernel.FOS_System.String class.
    /// </summary>
    [TestClass]
    public class StringTest
    {
        /// <summary>
        /// Tests the GetLength method.
        /// </summary>
        [TestMethod]
        public void GetLength_Test()
        {
            string TestStrContent = "Test";
            string TestStr = Utils.CreateString(TestStrContent);
            Assert.AreEqual(char.ConvertFromUtf32(0)[0], TestStr[0], "First length char not correct!");
            Assert.AreEqual(char.ConvertFromUtf32(4)[0], TestStr[1], "Second length char not correct!");
            Assert.AreEqual(TestStrContent.Length, Kernel.FOS_System.String.GetLength(TestStr), "Length not correct.");
        }
    }
}
