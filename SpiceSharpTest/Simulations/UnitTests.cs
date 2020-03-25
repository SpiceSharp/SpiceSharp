using NUnit.Framework;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void When_UnitRelation_Expect_Reference()
        {
            Assert.AreEqual((Units)Units.Watt, (Units)Units.Ampere * Units.Volt); // Power
            Assert.AreEqual((Units)Units.Ohm, (Units)Units.Volt / Units.Ampere); // Law of Ohm
            Assert.AreEqual((Units)Units.Mho, (Units)Units.Ampere / Units.Volt); // Inverted law of Ohm
            Assert.AreEqual((Units)Units.Hertz, new Units() / Units.Second); // Frequency
            Assert.AreEqual((Units)Units.Farad, (Units)Units.Ampere * Units.Second / Units.Volt); // i = C dv/dt => C = i * dt / dv
            Assert.AreEqual((Units)Units.Henry, (Units)Units.Volt * Units.Second / Units.Ampere); // v = L di/dt => L = v * dt / di
            Assert.AreEqual((Units)Units.VoltPerMeter, (Units)Units.Volt / Units.Meter); // E = V/m
        }
    }
}
