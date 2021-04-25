using NUnit.Framework;
using SpiceSharp.Attributes;
using System;

namespace SpiceSharpTest.General
{
    [GeneratedParameters]
    public partial class TestParameters
    {
        [GreaterThan(1.0)]
        private double _greaterThan1;

        [LessThan(1.0)]
        private double _lessThan1;

        [GreaterThanOrEquals(1.0)]
        private double _greaterThanOrEquals1;

        [LessThanOrEquals(1.0)]
        private double _lessThanOrEquals1;

        [LowerLimit(1.0)]
        private double _lowerLimit1;

        [UpperLimit(1.0)]
        private double _upperLimit1;
    }

    [TestFixture]
    public class GeneratorTests
    {
        [Test]
        public void When_GreaterThan_Expect_Reference()
        {
            var p = new TestParameters();
            p.GreaterThan1 = 1.5;
            Assert.Throws<ArgumentOutOfRangeException>(() => p.GreaterThan1 = 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => p.GreaterThan1 = 0.5);
        }

        [Test]
        public void When_LessThan_Expect_Reference()
        {
            var p = new TestParameters();
            p.LessThan1 = 0.5;
            Assert.Throws<ArgumentOutOfRangeException>(() => p.LessThan1 = 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => p.LessThan1 = 1.5);
        }

        [Test]
        public void When_GreaterThanOrEquals_Expect_Reference()
        {
            var p = new TestParameters();
            p.GreaterThanOrEquals1 = 1.5;
            p.GreaterThanOrEquals1 = 1.0;
            Assert.Throws<ArgumentOutOfRangeException>(() => p.GreaterThanOrEquals1 = 0.5);
        }

        [Test]
        public void When_LessThanOrEquals_Expect_Reference()
        {
            var p = new TestParameters();
            p.LessThanOrEquals1 = 0.5;
            p.LessThanOrEquals1 = 1.0;
            Assert.Throws<ArgumentOutOfRangeException>(() => p.LessThanOrEquals1 = 1.5);
        }

        [Test]
        public void When_LowerLimit_Expect_Reference()
        {
            var p = new TestParameters();
            p.LowerLimit1 = 1.5;
            Assert.AreEqual(1.5, p.LowerLimit1, 1e-12);
            p.LowerLimit1 = 0.5;
            Assert.AreEqual(1.0, p.LowerLimit1, 1e-12);
        }

        [Test]
        public void When_UpperLimit_Expect_Reference()
        {
            var p = new TestParameters();
            p.UpperLimit1 = 1.5;
            Assert.AreEqual(1.0, p.UpperLimit1, 1e-12);
            p.UpperLimit1 = 0.5;
            Assert.AreEqual(0.5, p.UpperLimit1, 1e-12);
        }
    }
}
