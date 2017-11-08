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
            // A circuit contains a DC voltage source 10V and resistor 1000 Ohms
            // The test verifies that after OP simulation:
            // 1) a current through resistor is 0.01 A (Ohms law)
            
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
            // A circuit contains a DC voltage source 10V and resistor 1000 Ohms
            // The test verifies that after AC simulation:
            // 1) a current through resistor is 0.01 A (Ohms law)

            var ckt = CreateResistorDcCircuit(10, 1000);

            ckt.Method = new Trapezoidal();
            AC simulation = new AC("Simulation", "lin", 10, 1, 1001);
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var current = data.Ask(new CircuitIdentifier("R_1"), "i");
                Assert.That.AreEqualWithTol(0.0, current, 0, 1e-8);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }

        [TestMethod]
        public void VoltageDividerDcVoltage_Op()
        {
            // A circuit contains a DC voltage source 100V and two resistors in series (1 and 3 Ohms). 
            // It's a voltage divider.
            // The test verifies that after OP simulation:
            // 1) voltage at "OUT" node is ((R1 + R2) / (R1 * R2)) * V 

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

        [TestMethod]
        public void ParallelResistorsDcVoltage_Op()
        {
            // A circuit contains a DC voltage source 100V and two resistors in parallel (1 and 3 Ohms). 
            // The test verifies that after OP simulation:
            // 1) Current through resistors is 50 and 100A respectively

            var ckt = CreateParallelResistorsDcCircuit(100, 2, 1);

            ckt.Method = new Trapezoidal();
            OP simulation = new OP("Simulation");
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var r1Current = data.Ask(new CircuitIdentifier("R_1"), "i");
                var r2Current = data.Ask(new CircuitIdentifier("R_2"), "i");

                Assert.That.AreEqualWithTol(50, r1Current, 0, 1e-8);
                Assert.That.AreEqualWithTol(100, r2Current, 0, 1e-8);
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

        private static Circuit CreateParallelResistorsDcCircuit(double dcVoltage, double resistance1, double resistance2)
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
                    resistance1),
                new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R_2"),
                    new CircuitIdentifier("IN"),
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
