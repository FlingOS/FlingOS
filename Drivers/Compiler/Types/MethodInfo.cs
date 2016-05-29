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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Drivers.Compiler.Attributes;

namespace Drivers.Compiler.Types
{
    /// <summary>
    ///     Container for information about a method loaded from an IL library being compiled.
    /// </summary>
    public class MethodInfo
    {
        /// <summary>
        ///     List of information about the arguments of the method.
        /// </summary>
        public List<VariableInfo> ArgumentInfos = new List<VariableInfo>();

        /// <summary>
        ///     The ID of the method (can also be used as a label).
        /// </summary>
        private string id;

        /// <summary>
        ///     The unique identifier of the method (within a type).
        /// </summary>
        public int IDValue = int.MinValue;

        /// <summary>
        ///     List of information about the local variables of the method.
        /// </summary>
        public List<VariableInfo> LocalInfos = new List<VariableInfo>();

        /// <summary>
        ///     The method body (i.e. IL bytes) extracted from the underlying method base.
        /// </summary>
        private MethodBody methodBody;

        /// <summary>
        ///     The plug attribute obtained applied to the method (null if it was not applied).
        /// </summary>
        public PluggedMethodAttribute PlugAttribute = null;

        /// <summary>
        ///     Whether the method has been preprocessed or not.
        /// </summary>
        /// <remarks>
        ///     Used to prevent the IL preprocessor executing more than once for the same method.
        /// </remarks>
        public bool Preprocessed = false;

        /// <summary>
        ///     The priority value of the method (if any).
        /// </summary>
        private long? priority;

        /// <summary>
        ///     Information about the return value of the method.
        /// </summary>
        public VariableInfo ReturnInfo = null;

        /// <summary>
        ///     The signature of the method (used to construct to ID).
        /// </summary>
        public string signature;

        /// <summary>
        ///     The underlying System.Reflection.MethodInfo obtained from the library's Assembly.
        /// </summary>
        public MethodBase UnderlyingInfo;

        /// <summary>
        ///     Whether to apply the garbage collector modifications to the method or not.
        /// </summary>
        /// <value>Gets whether the method had the NoGC attribute applied or not.</value>
        public bool ApplyGC
        {
            get { return UnderlyingInfo.GetCustomAttributes(typeof(NoGCAttribute), false).Length == 0; }
        }

        /// <summary>
        ///     Whether to apply the debug modifications to the method or not.
        /// </summary>
        /// <value>Gets whether the method had the NoDebug attribute applied or not.</value>
        public bool ApplyDebug
        {
            get { return UnderlyingInfo.GetCustomAttributes(typeof(NoDebugAttribute), false).Length == 0; }
        }

        /// <summary>
        ///     Whether the method is plugged or not.
        /// </summary>
        /// <value>Gets whether the method had the PluggedMethod attribute applied or not.</value>
        public bool IsPlugged
        {
            get { return PlugAttribute != null; }
        }

        /// <summary>
        ///     Whether the method is a constructor or not.
        /// </summary>
        /// <value>Gets whether the underlying info is System.Reflection.ConstructorInfo or not.</value>
        public bool IsConstructor
        {
            get { return UnderlyingInfo is ConstructorInfo; }
        }

        /// <summary>
        ///     Whether the method is static or not.
        /// </summary>
        /// <value>Gets the value of the underlying info's IsStatic property.</value>
        public bool IsStatic
        {
            get { return UnderlyingInfo.IsStatic; }
        }

        /// <summary>
        ///     The method body (i.e. IL bytes) extracted from the underlying method base.
        /// </summary>
        /// <value>Gets the value of the methodBody field or generates the value if it is null.</value>
        public MethodBody MethodBody
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

        /// <summary>
        ///     The ID of the method (can also be used as a label).
        /// </summary>
        /// <value>Gets the value of the id field or generates the value if it is null.</value>
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

        /// <summary>
        ///     The signature of the method (used to construct to ID).
        /// </summary>
        /// <value>Gets the value of the signature field or generates the value if it is null.</value>
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

        /// <summary>
        ///     The priority value of the method (if any).
        /// </summary>
        /// <value>Gets the value of the priority field or generates the value if it is null.</value>
        public long Priority
        {
            get
            {
                if (!priority.HasValue)
                {
                    object[] priorAttrs = UnderlyingInfo.GetCustomAttributes(typeof(SequencePriorityAttribute), false);
                    if (priorAttrs.Length > 0)
                    {
                        SequencePriorityAttribute priorAttr = (SequencePriorityAttribute)priorAttrs[0];
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

        /// <summary>
        ///     Gets a human-readable representation of the method.
        /// </summary>
        /// <remarks>
        ///     Generates a nice, human-readable name of the form NameSpace.DeclaringType.MethodName(ArgumentType, ArgumentType).
        /// </remarks>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            string result = UnderlyingInfo.DeclaringType.FullName + "." + UnderlyingInfo.Name + "(";

            if (ArgumentInfos.Count - 1 > 0)
            {
                result += string.Join(", ", ArgumentInfos.ConvertAll(x => x.ToString()).ToArray(), 0,
                    ArgumentInfos.Count - 1);
            }

            result += ")";

            return result;
        }

        /// <summary>
        ///     Generates the signature string for the specified method.
        /// </summary>
        /// <param name="aMethod">The method to generate the signature of.</param>
        /// <returns>The signature string.</returns>
        public static string GetMethodSignature(MethodBase aMethod)
        {
            string[] paramTypes = aMethod.GetParameters().Select(x => x.ParameterType).Select(x => x.FullName).ToArray();
            string returnType = "";
            string declaringType = "";
            string methodName = "";
            if (aMethod.IsConstructor || aMethod is ConstructorInfo)
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

        /// <summary>
        ///     Generates a signature string using the specified parts.
        /// </summary>
        /// <param name="declaringType">The type which declares the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="paramTypes">The list of the types of the method's arguments.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature string.</returns>
        public static string GetMethodSignature(string returnType, string declaringType, string methodName,
            string[] paramTypes)
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

        /// <summary>
        ///     Generates an ID (which can be used as a label) from the specified signature.
        /// </summary>
        /// <param name="methodSignature">The signature to use to generate the label.</param>
        /// <returns>The ID.</returns>
        public static string CreateMethodID(string methodSignature)
        {
            return "method_" + Utilities.FilterIdentifierForInvalidChars(methodSignature);
        }
    }
}