using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler
{
    public static class Options
    {
        public enum BuildModes
        {
            Debug,
            Release
        }

        public static BuildModes BuildMode
        {
            get;
            set;
        }
        public static string TargetArchitecture
        {
            get;
            set;
        }
    }
}
