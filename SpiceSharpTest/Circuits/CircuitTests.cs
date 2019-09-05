using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;

namespace SpiceSharpTest.Circuits
{
    /// <summary>
    /// Tests for <seealso cref="Circuit" />
    /// </summary>
    [TestFixture]
    public class CircuitTests
    {
        [Test]
        public void When_CircuitMerge_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0));
            var subckt = new Circuit(
                new Resistor("Ra", "in", "out", 100),
                new Resistor("Rb", "out", "0", 100));

            // Merge the subcircuit into the circuit
            ckt.Merge(subckt);

            // Test
            Assert.AreEqual(ckt["Ra"], subckt["Ra"]);
            Assert.AreEqual(ckt["Rb"], subckt["Rb"]);
        }
    }
}
