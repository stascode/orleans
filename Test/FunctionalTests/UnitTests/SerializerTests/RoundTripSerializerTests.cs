﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans;
using UnitTestGrainInterfaces;

namespace UnitTests.SerializerTests
{
    [TestClass]
    public class RoundTripSerializerTests : UnitTestBase
    {
        public RoundTripSerializerTests() : base(false)
        {}

        [TestMethod, TestCategory("Functional"), TestCategory("Serialization")]
        public void Serialize_TestMethodResultEnum()
        {
            var grain = GrainClient.GrainFactory.GetGrain<IEnumResultGrain>(GetRandomGrainId());
            try
            {
                CampaignEnemyTestType result = grain.GetEnemyType().Result;
                Assert.AreEqual(CampaignEnemyTestType.Enemy2, result, "Enum return value wasn't transmitted properly");
            }
            catch (Exception exception)
            {
                Assert.Fail("Call to grain method with enum return threw exception: " + exception);
            }
        }
    }
}
