using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Circuits
{
    /// <summary>
    /// Tests for <seealso cref="Circuit" />
    /// </summary>
    [TestFixture]
    public class CircuitTests
    {
        [Test]
        public void When_CircuitInstantiate_Expect_Entities()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0));
            var subckt = new ComponentInstanceData(new Circuit(
                new Resistor("Ra", "in", "out", 100),
                new Resistor("Rb", "out", "0", 100)));

            // Instantiate a subcircuit
            subckt.Name = "A";
            subckt.NodeMap.Add("0", "0");
            subckt.NodeMap["in"] = "in";
            subckt.NodeMap["out"] = "out";
            ckt.Instantiate(subckt);

            // Instantiate the subcircuit a second time
            subckt.Name = "B";
            subckt.NodeMap.Remove("out");
            subckt.NodeMap["in"] = "out";
            ckt.Instantiate(subckt);

            // Test
            Assert.AreEqual(ckt.Contains("A/Ra"), true);
            Assert.AreEqual(ckt.Contains("A/Rb"), true);
            Assert.AreEqual(ckt.Contains("B/Ra"), true);
            Assert.AreEqual(ckt.Contains("B/Rb"), true);
        }

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

        [Test]
        public void When_CircuitInstantiateOp_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0));
            var subckt = new ComponentInstanceData(new Circuit(
                new Resistor("Ra", "in", "out", 100),
                new Resistor("Rb", "out", "0", 100)));

            // Instantiate a subcircuit
            subckt.Name = "A";
            subckt.NodeMap.Add("0", "0");
            subckt.NodeMap["in"] = "in";
            subckt.NodeMap["out"] = "out";
            ckt.Instantiate(subckt);

            // Instantiate the subcircuit a second time
            subckt.Name = "B";
            subckt.NodeMap.Remove("out");
            subckt.NodeMap["in"] = "out";
            ckt.Instantiate(subckt);

            // Do an operating point simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, e) =>
            {
                // vout = Ra / (Ra + Rb//(Ra+Rb))
                Assert.AreEqual(0.4, e.GetVoltage("out"), 1e-12);
            };
            op.Run(ckt);
        }
    }
}
