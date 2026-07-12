using NUnit.Framework;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp;
using SpiceSharp.Validation;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class OPTests
    {
        [Test]
        public void When_OPRun_Expect_YieldFlags()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var tran = new OP("op 1");

            int flags = 0;
            foreach (int flag in tran.Run(ckt, mask: -1))
                flags |= flag;

            Assert.That(flags, Is.EqualTo(
                Simulation.BeforeSetup |
                Simulation.AfterSetup |
                Simulation.BeforeValidation |
                Simulation.AfterValidation |
                Simulation.BeforeExecute |
                Simulation.AfterExecute |
                Simulation.BeforeUnsetup |
                Simulation.AfterUnsetup |
                BiasingSimulation.BeforeTemperature |
                BiasingSimulation.AfterTemperature |
                OP.ExportOperatingPoint));
        }

        [Test]
        public void When_RunIsDisposedAfterSetup_Expect_SimulationCanRunAgain()
        {
            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "0", 1.0));
            var simulation = new OP("op");

            using (var enumerator = simulation.Run(circuit, Simulation.AfterSetup).GetEnumerator())
            {
                Assert.That(enumerator.MoveNext(), Is.True);
                Assert.That(enumerator.Current, Is.EqualTo(Simulation.AfterSetup));
            }

            Assert.That(simulation.CurrentRun, Is.EqualTo(-1));
            Assert.That(simulation.Status, Is.EqualTo(SimulationStatus.None));
            Assert.DoesNotThrow(() => simulation.RunToEnd(circuit));
        }

        [Test]
        public void When_RunValidationFails_Expect_SimulationCanRunAgain()
        {
            var invalidCircuit = new Circuit(new Resistor("R1", "a", "b", 1.0));
            var validCircuit = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "0", 1.0));
            var simulation = new OP("op");

            Assert.Throws<ValidationFailedException>(() => simulation.RunToEnd(invalidCircuit));
            Assert.That(simulation.CurrentRun, Is.EqualTo(-1));
            Assert.That(simulation.Status, Is.EqualTo(SimulationStatus.None));
            Assert.DoesNotThrow(() => simulation.RunToEnd(validCircuit));
        }
    }
}
