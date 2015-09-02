using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using Orleans.Runtime;
using UnitTestGrains;

namespace UnitTests.General
{
    [TestClass]
    public class GetGrainTests : UnitTestBase
    {
        public GetGrainTests()
            : base(true)
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestMethod, TestCategory("BVT"), TestCategory("Functional"), TestCategory("GetGrain")]
        [ExpectedException(typeof(OrleansException))]
        public void GetGrain_Ambiguous()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase>(GetRandomGrainId());
        }

        [TestMethod, TestCategory("BVT"), TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_Ambiguous_WithDefault()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase4>(GetRandomGrainId());
            Assert.IsFalse(g.Foo().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_WithFullName()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase>(GetRandomGrainId(), "UnitTestGrains.BaseGrain");
            Assert.IsTrue(g.Foo().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_WithPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase>(GetRandomGrainId(), "UnitTestGrains.Base");
            Assert.IsTrue(g.Foo().Result);
        }

        [TestMethod, TestCategory("BVT"), TestCategory("Functional"), TestCategory("GetGrain")]
        [ExpectedException(typeof(OrleansException))]
        public void GetGrain_AmbiguousPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase>(GetRandomGrainId(), "UnitTestGrains");
        }

        [TestMethod, TestCategory("BVT"), TestCategory("Functional"), TestCategory("GetGrain")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetGrain_WrongPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase>(GetRandomGrainId(), "Foo");
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_Derived_NoPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IDerivedFromBase>(GetRandomGrainId());
            Assert.IsFalse(g.Foo().Result);
            Assert.IsTrue(g.Bar().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_Derived_WithFullName()
        {
            var g = GrainClient.GrainFactory.GetGrain<IDerivedFromBase>(GetRandomGrainId(), "UnitTestGrains.DerivedFromBase");
            Assert.IsFalse(g.Foo().Result);
            Assert.IsTrue(g.Bar().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_Derived_WithFullName_FromBase()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase>(GetRandomGrainId(), "UnitTestGrains.DerivedFromBase");
            Assert.IsFalse(g.Foo().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_Derived_WithPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IDerivedFromBase>(GetRandomGrainId(), "UnitTestGrains");
            Assert.IsFalse(g.Foo().Result);
            Assert.IsTrue(g.Bar().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetGrain_Derived_WithWrongPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IDerivedFromBase>(GetRandomGrainId(), "Foo");
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_OneImplementation_NoPrefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase1>(GetRandomGrainId());
            Assert.IsFalse(g.Foo().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_OneImplementation_Prefix()
        {
            var g = GrainClient.GrainFactory.GetGrain<IBase1>(GetRandomGrainId(), "UnitTestGrains.BaseGrain1");
            Assert.IsFalse(g.Foo().Result);
        }

        [TestMethod, TestCategory("Functional"), TestCategory("GetGrain")]
        public void GetGrain_MultipleUnrelatedInterfaces()
        {
            var g1 = GrainClient.GrainFactory.GetGrain<IBase3>(GetRandomGrainId());
            Assert.IsFalse(g1.Foo().Result);
            var g2 = GrainClient.GrainFactory.GetGrain<IBase2>(GetRandomGrainId());
            Assert.IsTrue(g2.Bar().Result);
        }
    }
}
