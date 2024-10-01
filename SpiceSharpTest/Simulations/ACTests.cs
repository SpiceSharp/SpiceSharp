using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class ACTests
    {
        [Test]
        public void When_ACRerun_Expect_Same()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var ac = new AC("ac 1", new DecadeSweep(1, 1e9, 10));
            var export = new ComplexVoltageExport(ac, "out");

            // Run the simulation a first time for building the reference values
            var r = new List<Complex>();
            foreach (int _ in ac.Run(ckt, AC.ExportSmallSignal))
                r.Add(export.Value);

            // Rerun the simulation for building the reference values
            int index = 0;
            foreach (int _ in ac.Rerun(AC.ExportSmallSignal))
            {
                Assert.That(export.Value.Real, Is.EqualTo(r[index].Real).Within(1e-20));
                Assert.That(export.Value.Imaginary, Is.EqualTo(r[index++].Imaginary).Within(1e-20));
            }
        }

        [Test]
        public void When_ACRun_Expect_YieldFlags()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var ac = new AC("ac 1", new DecadeSweep(1, 1e9, 10));

            int flags = 0;
            foreach (int flag in ac.Run(ckt, mask: -1))
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
                AC.ExportOperatingPoint |
                AC.ExportSmallSignal));
        }
    }
}
