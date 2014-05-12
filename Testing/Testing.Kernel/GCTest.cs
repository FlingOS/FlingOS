using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Kernel;
using Kernel.FOS_System;

namespace Testing._Kernel
{
    /// <summary>
    /// Test class for the GC class.
    /// </summary>
    [TestClass]
    public unsafe class GCTest
    {
        /// <summary>
        /// Initialises the test class.
        /// </summary>
        /// <param name="context">The tyest context.</param>
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Kernel.FOS_System.GC.Init();
        }
        /// <summary>
        /// Cleans up the test class.
        /// </summary>
        [ClassCleanup]
        public static void Cleanup()
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)Heap.FBlock);
        }

        /// <summary>
        /// Tests the SetSignature() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void SetSignature_Test()
        {
            GCHeader* newObjHeaderPtr = (GCHeader*)Heap.Alloc((uint)sizeof(GCHeader));
            Kernel.FOS_System.GC.SetSignature(newObjHeaderPtr);
            Assert.AreEqual(newObjHeaderPtr->Sig1, 0x5C0EADE2U, "Sig1 not set properly!");
            Assert.AreEqual(newObjHeaderPtr->Sig2, 0x5C0EADE2U, "Sig2 not set properly!");
            Assert.AreEqual(newObjHeaderPtr->Checksum, 0xB81D5BC4U, "Checksum not set properly!");
        }
        /// <summary>
        /// Tests the CheckSignature() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void CheckSignature_Test()
        {
            GCHeader* newObjHeaderPtr = (GCHeader*)Heap.Alloc((uint)sizeof(GCHeader));
            Kernel.FOS_System.GC.SetSignature(newObjHeaderPtr);
            Assert.IsTrue(Kernel.FOS_System.GC.CheckSignature(newObjHeaderPtr), "Correct GC header signature not recognised!");
            newObjHeaderPtr->Sig1 = 0;
            Assert.IsTrue(!Kernel.FOS_System.GC.CheckSignature(newObjHeaderPtr), "Incorrect GC header signature recognised mistakenly!");
            newObjHeaderPtr->Sig1 = newObjHeaderPtr->Sig2;
            newObjHeaderPtr->Sig2 = 0;
            Assert.IsTrue(!Kernel.FOS_System.GC.CheckSignature(newObjHeaderPtr), "Incorrect GC header signature recognised mistakenly!");
            newObjHeaderPtr->Sig2 = newObjHeaderPtr->Sig1;
            newObjHeaderPtr->Checksum = 0;
            Assert.IsTrue(!Kernel.FOS_System.GC.CheckSignature(newObjHeaderPtr), "Incorrect GC header signature recognised mistakenly!");
        }

        /// <summary>
        /// Tests the NewObj() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void NewObj_Test()
        {
            int startNumObjs = Kernel.FOS_System.GC.NumObjs;

            Kernel.FOS_System.Type newObjType = new TestType();
            byte* newObjPtr = (byte*)Kernel.FOS_System.GC.NewObj(newObjType);
            GCHeader* newObjHeaderPtr = (GCHeader*)(newObjPtr - sizeof(GCHeader));
            Assert.IsTrue(Kernel.FOS_System.GC.CheckSignature(newObjHeaderPtr), "Signature not set properly!");
            Assert.AreEqual(1u, newObjHeaderPtr->RefCount, "Ref count not set to 1!");
            Assert.AreEqual(startNumObjs + 1, Kernel.FOS_System.GC.NumObjs, "NumObjs not set correctly!");

            for (int i = 0; i < newObjType.Size; i++)
            {
                Assert.AreEqual(0u, newObjPtr[i], "Memory not initialised to 0 properly!");
            }
        }

        /// <summary>
        /// Tests the IncrementRefCount() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void IncrementRefCount_Test()
        {
            Kernel.FOS_System.Type newObjType = new TestType();
            byte* newObjPtr = (byte*)Kernel.FOS_System.GC.NewObj(newObjType);
            GCHeader* newObjHeaderPtr = (GCHeader*)(newObjPtr - sizeof(GCHeader));
            Assert.AreEqual(1u, newObjHeaderPtr->RefCount, "Ref count not set to 1!");
            Kernel.FOS_System.GC._IncrementRefCount(newObjPtr);
            Assert.AreEqual(2u, newObjHeaderPtr->RefCount, "Ref count not incremented!");
        }
        /// <summary>
        /// Tests the DecrementRefCount() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void DecrementRefCount_Test()
        {
            uint minUsedBlocks = Heap.FBlock->used;
            int startNumObjs = Kernel.FOS_System.GC.NumObjs;

            Kernel.FOS_System.Type newObjType = new TestType();
            byte* newObjPtr = (byte*)Kernel.FOS_System.GC.NewObj(newObjType);
            GCHeader* newObjHeaderPtr = (GCHeader*)(newObjPtr - sizeof(GCHeader));
            Assert.AreEqual(1u, newObjHeaderPtr->RefCount, "Ref count not set to 1!");
            Kernel.FOS_System.GC._DecrementRefCount(newObjPtr);
            Assert.AreEqual(0u, newObjHeaderPtr->RefCount, "Ref count not decremented!");
            Assert.AreEqual(startNumObjs, Kernel.FOS_System.GC.NumObjs, "Num objs not decremented!");

            Assert.AreEqual(minUsedBlocks, Heap.FBlock->used, "Memory not free'd after ref count hit 0!");
        }
    }

    class TestType : Kernel.FOS_System.Type
    {
        public override System.Reflection.Assembly Assembly
        {
            get { throw new NotImplementedException(); }
        }
        public override string AssemblyQualifiedName
        {
            get { throw new NotImplementedException(); }
        }
        public override System.Type BaseType
        {
            get { throw new NotImplementedException(); }
        }
        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }
        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
        public override System.Type UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }
        protected override bool HasElementTypeImpl()
        {
            throw new NotImplementedException();
        }
        public override System.Type GetElementType()
        {
            throw new NotImplementedException();
        }
        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }
        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }
        protected override bool IsPointerImpl()
        {
            throw new NotImplementedException();
        }
        protected override bool IsByRefImpl()
        {
            throw new NotImplementedException();
        }
        protected override bool IsArrayImpl()
        {
            throw new NotImplementedException();
        }
        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.MemberInfo[] GetMember(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return base.GetMember(name, bindingAttr);
        }
        public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Type returnType, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }
        public override System.Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }
        public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }
        public override string Namespace
        {
            get { throw new NotImplementedException(); }
        }
        public override string FullName
        {
            get { throw new NotImplementedException(); }
        }
        public override System.Reflection.Module Module
        {
            get { throw new NotImplementedException(); }
        }
        public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }
        public override Guid GUID
        {
            get { throw new NotImplementedException(); }
        }

        public TestType()
        {
            Size = 1024;
            Id = 12;
        }
    }
}
