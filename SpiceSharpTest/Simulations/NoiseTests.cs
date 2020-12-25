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
            void BuildReference(object sender, ExportDataEventArgs args) => r.Add(export.Value);
            noise.ExportSimulationData += BuildReference;
            noise.Run(ckt);
            noise.ExportSimulationData -= BuildReference;

            // Rerun the simulation for building the reference values
            var index = 0;
            void CheckReference(object sender, ExportDataEventArgs args)
            {
                Assert.AreEqual(r[index++], export.Value, 1e-20);
            }
            noise.ExportSimulationData += CheckReference;
            noise.Rerun();
            noise.ExportSimulationData -= CheckReference;
        }
    }
}
