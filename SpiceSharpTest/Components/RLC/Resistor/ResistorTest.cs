using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharpTest.Utils;

namespace SpiceSharpTest.Components.RLC.Resistor
{
    [TestClass]
    public class ResistorTest
    {
        [TestMethod]
        public void SingleResistorOnDcVoltage_Op()
        {
            var ckt = CreateResistorDcCircuit(10, 1000);

            ckt.Method = new Trapezoidal();
            OP simulation = new OP("Simulation");
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var current = data.Ask(new CircuitIdentifier("R_1"), "i");
                Assert.That.AreEqualWithTol(0.01, current, 0, 1e-8);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }

        [TestMethod]
        public void SingleResistorOnDcVoltage_Ac()
        {
            var ckt = CreateResistorDcCircuit(10, 1000);

            ckt.Method = new Trapezoidal();
            AC simulation = new AC("Simulation", "lin", 10, 1, 1001);
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var current = data.Ask(new CircuitIdentifier("R_1"), "i");
                Assert.That.AreEqualWithTol(0.01, current, 0, 1e-8);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }

        [TestMethod]
        public void VoltageDividerDcVoltage_Op()
        {
            var ckt = CreateVoltageDividerResistorDcCircuit(100, 3, 1);

            ckt.Method = new Trapezoidal();
            OP simulation = new OP("Simulation");
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var outVoltage = data.GetVoltage(new CircuitIdentifier("OUT"));
                Assert.That.AreEqualWithTol(25, outVoltage, 0, 1e-8);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }

        private static Circuit CreateVoltageDividerResistorDcCircuit(double dcVoltage, double resistance1, double resistance2)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource(
                    new CircuitIdentifier("V_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    dcVoltage),
                new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("OUT"),
                    resistance1),
                new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R_2"),
                    new CircuitIdentifier("OUT"),
                    new CircuitIdentifier("gnd"),
                    resistance2)
            );
            return ckt;
        }

        private static Circuit CreateResistorDcCircuit(double dcVoltage, double resistance)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource(
                    new CircuitIdentifier("V_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    dcVoltage),
                new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    resistance)
            );
            return ckt;
        }
    }
}
