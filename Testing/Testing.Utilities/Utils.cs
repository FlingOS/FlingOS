using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Utilities
{
    /// <summary>
    /// Utility methods used throughout the testing frameworks.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Specialist method to create strings in the format the OS expects them. Strings created using this must not be altered later.
        /// </summary>
        /// <param name="content">The content of the string to create.</param>
        public static string CreateString(string content)
        {
            return content;
        }
    }
}
