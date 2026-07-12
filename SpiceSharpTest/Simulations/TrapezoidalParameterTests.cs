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
    }
}