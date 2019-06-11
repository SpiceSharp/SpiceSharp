using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;

namespace SpiceSharpTest.Circuits
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void When_GroundNameInvalid_Expect_Exception()
        {
            // Verifies that CircuitException is thrown during Check when circuit has a ground node called "GND"
            var ckt = CreateCircuit("gnd");
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }

        [Test]
        public void When_GroundNameValid_Expect_NoException()
        {
            // Verifies that CircuitException is not thrown during Check when circuit has a ground node called "gnd"
            var ckt = CreateCircuit("GND");
            ckt.Validate();
        }

        [Test]
        public void When_GroundNameZero_Expect_NoException()
        {
            // Verifies that CircuitException is not thrown during Check when circuit has a ground node called "0"
            var ckt = CreateCircuit("0");
            ckt.Validate();
        }

        /// <summary>
        /// Creates a circuit with a resistor and a voltage source which is connected to IN
        /// node and a ground node with name <paramref name="groundNodeName"/>
        /// </summary>
        /// <param name="groundNodeName"></param>
        /// <returns></returns>
        static Circuit CreateCircuit(string groundNodeName)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", groundNodeName, 1.0),
                new Resistor("R1", "IN", groundNodeName, 1.0e3));
            return ckt;
        }

        [Test]
        public void When_VoltageLoop_Expect_CircuitException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "A", "0", 1.0),
                new VoltageSource("V2", "B", "A", 1.0),
                new VoltageSource("V3", "B", "A", 1.0)
                );
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }

        [Test]
        public void When_VoltageLoop2_Expect_CircuitException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "A", "0", 1.0),
                new VoltageSource("V2", "A", "B", 1.0),
                new VoltageSource("V3", "B", "C", 1.0),
                new VoltageSource("V4", "C", "D", 1.0),
                new VoltageSource("V5", "D", "E", 1.0),
                new VoltageSource("V6", "E", "F", 1.0),
                new VoltageSource("V7", "F", "G", 1.0),
                new VoltageSource("V8", "G", "H", 1.0),
                new VoltageSource("V9", "H", "I", 1.0),
                new VoltageSource("V10", "I", "0", 1.0)
                );
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }

        [Test]
        public void When_FloatingNode_Expect_CircuitException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "gnd", 1.0),
                new Capacitor("C1", "in", "out", 1e-12),
                new Capacitor("C2", "out", "gnd", 1e-12)
                );
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }

        [Test]
        public void When_FloatingNode2_Expect_CircuitException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "input", "gnd", 1.0),
                new VoltageControlledVoltageSource("E1", "out", "gnd", "in", "gnd", 2.0),
                new VoltageControlledVoltageSource("E2", "out2", "gnd", "out", "gnd", 1.0)
                );
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }

        [Test]
        public void When_CCCSFloatingNodeValidator_Expect_CircuitException()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0),
                new VoltageSource("V1", "in", "0", 0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 12.0)
            );
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }

        [Test]
        public void When_CurrentSourceSeriesValidator_Expect_CircuitException()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 1.0),
                new CurrentSource("I2", "0", "in", 2.0));
            Assert.Throws<CircuitException>(() => ckt.Validate());
        }
    }
}
