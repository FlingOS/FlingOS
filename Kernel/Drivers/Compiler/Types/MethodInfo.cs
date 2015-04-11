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

        private string id = null;
        public string ID
        {
            get
            {
                if (id == null)
                {
                    id = CreateMethodID(Signature);
                }
                return id;
            }
        }

        public string signature = null;
        public string Signature
        {
            get
            {
                if (signature == null)
                {
                    signature = GetMethodSignature(UnderlyingInfo);
                }
                return signature;
            }
        }

        public int IDValue = -1;

        private long? priority = null;
        public long Priority
        {
            get
            {
                if (!priority.HasValue)
                {
                    object[] priorAttrs = UnderlyingInfo.GetCustomAttributes(typeof(Attributes.SequencePriorityAttribute), false);
                    if (priorAttrs.Length > 0)
                    {
                        Attributes.SequencePriorityAttribute priorAttr = (Attributes.SequencePriorityAttribute)priorAttrs[0];
                        priority = priorAttr.Priority;
                    }
                    else
                    {
                        priority = 0;
                    }
                }
                return priority.Value;
            }
        }

        public bool Preprocessed = false;

        public override string ToString()
        {
            string result = UnderlyingInfo.DeclaringType.FullName + "." + UnderlyingInfo.Name + "(";

            if (ArgumentInfos.Count-1 > 0)
            {
                result += string.Join(", ", ArgumentInfos.ConvertAll(x => x.ToString()).ToArray(), 0, ArgumentInfos.Count - 1);
            }

            result += ")";

            return result;
        }

        private static string GetMethodSignature(System.Reflection.MethodBase aMethod)
        {
            string[] paramTypes = aMethod.GetParameters().Select(x => x.ParameterType).Select(x => x.FullName).ToArray();
            string returnType = "";
            string declaringType = "";
            string methodName = "";
            if (aMethod.IsConstructor || aMethod is System.Reflection.ConstructorInfo)
            {
                returnType = typeof(void).FullName;
                declaringType = aMethod.DeclaringType.FullName;
                methodName = aMethod.Name;
            }
            else
            {
                returnType = ((System.Reflection.MethodInfo)aMethod).ReturnType.FullName;
                declaringType = aMethod.DeclaringType.FullName;
                methodName = aMethod.Name;
            }

            return GetMethodSignature(returnType, declaringType, methodName, paramTypes);
        }
        private static string GetMethodSignature(string returnType, string declaringType, string methodName, string[] paramTypes)
        {
            string aMethodSignature = "";
            aMethodSignature = returnType + "_RETEND_" + declaringType + "_DECLEND_" + methodName + "_NAMEEND_(";
            bool firstParam = true;
            foreach (string aParam in paramTypes)
            {
                if (!firstParam)
                {
                    aMethodSignature += ",";
                }
                firstParam = false;

                aMethodSignature += aParam;
            }
            aMethodSignature += ")";
            return aMethodSignature;
        }
        private static string CreateMethodID(string methodSignature)
        {
            return "method_" + Utilities.FilterIdentifierForInvalidChars(methodSignature);
        }
    }
}
