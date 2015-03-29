#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Types
{
    public class MethodInfo
    {
        public System.Reflection.MethodBase UnderlyingInfo;
        public Attributes.PluggedMethodAttribute PlugAttribute = null;

        public List<VariableInfo> ArgumentInfos = new List<VariableInfo>();
        public List<VariableInfo> LocalInfos = new List<VariableInfo>();

        public bool ApplyGC
        {
            get
            {
                return UnderlyingInfo.GetCustomAttributes(typeof(Attributes.NoGCAttribute), false).Length == 0;
            }
        }
        public bool ApplyDebug
        {
            get
            {
                return UnderlyingInfo.GetCustomAttributes(typeof(Attributes.NoDebugAttribute), false).Length == 0;
            }
        }
        public bool IsPlugged { get { return PlugAttribute != null; } }
        public bool IsConstructor { get { return UnderlyingInfo is System.Reflection.ConstructorInfo; } }
        public bool IsStatic { get { return UnderlyingInfo.IsStatic; } }

        private System.Reflection.MethodBody methodBody;
        public System.Reflection.MethodBody MethodBody
        {
            get
            {
                if (methodBody == null)
                {
                    methodBody = UnderlyingInfo.GetMethodBody();
                }
                return methodBody;
            }
        }

        public override string ToString()
        {
            return UnderlyingInfo.Name;
        }
    }
}
