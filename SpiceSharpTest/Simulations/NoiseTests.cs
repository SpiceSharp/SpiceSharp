using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class NoiseTests
    {
        [Test]
        public void When_NoiseRerun_Expect_Same()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var noise = new Noise("noise 1", "V1", "out", new DecadeSweep(1, 1e9, 10));
            var export = new OutputNoiseDensityExport(noise);

            // Run the simulation a first time for building the reference values
            var r = new List<double>();
            foreach (int _ in noise.Run(ckt, Noise.ExportNoise))
                r.Add(export.Value);

            // Rerun the simulation for building the reference values
            int index = 0;
            foreach (int _ in noise.Rerun(Noise.ExportNoise))
                Assert.That(export.Value, Is.EqualTo(r[index++]).Within(1e-20));
        }

        [Test]
        public void When_NoiseRun_Expect_YieldFlags()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var noise = new Noise("noise 1", "V1", "out", new DecadeSweep(1, 1e9, 10));

            int flags = 0;
            foreach (int flag in noise.Run(ckt, mask: -1))
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
                Noise.ExportOperatingPoint |
                Noise.ExportSmallSignal |
                Noise.ExportNoise));
        }
    }
}
