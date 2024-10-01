using NUnit.Framework;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp;

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
    }
}
