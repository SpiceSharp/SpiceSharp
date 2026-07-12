using NUnit.Framework;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class TrapezoidalParameterTests
    {
        [TestCase(-0.1)]
        [TestCase(1.0)]
        [TestCase(1.1)]
        [TestCase(double.NaN)]
        public void When_SpiceTrapezoidalXmuIsInvalid_Expect_Exception(double value)
        {
            var method = new Trapezoidal();

            Assert.Throws<ArgumentOutOfRangeException>(() => method.Xmu = value);
        }

        [TestCase(-0.1)]
        [TestCase(1.0)]
        [TestCase(1.1)]
        [TestCase(double.NaN)]
        public void When_FixedTrapezoidalXmuIsInvalid_Expect_Exception(double value)
        {
            var method = new FixedTrapezoidal();

            Assert.Throws<ArgumentOutOfRangeException>(() => method.Xmu = value);
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void When_FixedStepIsNotFinite_Expect_Exception(double value)
        {
            Assert.That(() => new FixedEuler { Step = value }, Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new FixedTrapezoidal { Step = value }, Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void When_FixedStepIsUnset_Expect_CreateException()
        {
            Assert.That(() => new FixedEuler().Create(null), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new FixedTrapezoidal().Create(null), Throws.InstanceOf<ArgumentException>());
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void When_VariableStepLimitsAreNotFinite_Expect_Exception(double value)
        {
            var method = new Trapezoidal();

            Assert.That(() => method.MaxStep = value, Throws.InstanceOf<ArgumentException>());
            Assert.That(() => method.MinStep = value, Throws.InstanceOf<ArgumentException>());
            Assert.That(() => method.MaximumExpansion = value, Throws.InstanceOf<ArgumentException>());
        }
    }
}