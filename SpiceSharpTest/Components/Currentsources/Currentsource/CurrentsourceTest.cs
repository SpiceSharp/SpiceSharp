using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharpTest.Utils;

namespace SpiceSharpTest.Components.Currentsources.Currentsource
{
    [TestClass]
    public class CurrentsourceTest
    {
        [TestMethod]
        public void SingleResistorOnCurrentSource_Op()
        {
            var ckt = CreateResistorCircuit(10, 1000);

            ckt.Method = new Trapezoidal();
            OP simulation = new OP("Simulation");
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var currentOnResistor = data.Ask(new CircuitIdentifier("R_1"), "i");
                var volategeOnCurrentSource = data.Ask(new CircuitIdentifier("I_1"), "v");
                Assert.That.AreEqualWithTol(10, currentOnResistor, 0, 1e-8);
                Assert.That.AreEqualWithTol(10000, volategeOnCurrentSource, 0, 1e-8);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }

        [TestMethod]
        public void ResistorsInSeriesOnCurrentSource_Op()
        {
            int currentInAmp = 100;
            int resistanceInOhms = 10;
            int resistorCount = 500;

            var ckt = CreateResistorsInSeriesCircuit(resistorCount, currentInAmp, resistanceInOhms);

            ckt.Method = new Trapezoidal();
            OP simulation = new OP("Simulation");
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var currentOnResistor = data.Ask(new CircuitIdentifier("R1"), "i");
                var volategeOnCurrentSource = data.Ask(new CircuitIdentifier("I_1"), "v");
                Assert.That.AreEqualWithTol(currentInAmp, currentOnResistor, 0, 1e-8);
                Assert.That.AreEqualWithTol(currentInAmp * resistanceInOhms * resistorCount, volategeOnCurrentSource, 0, 1e-6);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }

        private static Circuit CreateResistorsInSeriesCircuit(int count, double current, double resistance)
        {
            Assert.IsTrue(count > 1);

            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new SpiceSharp.Components.Currentsource(
                    new CircuitIdentifier("I_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    current));


            ckt.Objects.Add(new SpiceSharp.Components.Resistor(
                new CircuitIdentifier("R1"),
                new CircuitIdentifier("IN"),
                new CircuitIdentifier("B1"),
                resistance));

            for (var i = 2; i <= count - 1; i++)
            {
                ckt.Objects.Add(new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R" + i),
                    new CircuitIdentifier("B" + (i - 1)),
                    new CircuitIdentifier("B" + i),
                    resistance));
            }

            ckt.Objects.Add(new SpiceSharp.Components.Resistor(
                new CircuitIdentifier("R" + count),
                new CircuitIdentifier("B" + (count - 1)),
                new CircuitIdentifier("gnd"),
                resistance));

            return ckt;
        }

        private static Circuit CreateResistorCircuit(double current, double resistance)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new SpiceSharp.Components.Currentsource(
                    new CircuitIdentifier("I_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    current),
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
