using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpTest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class ConcurrentSimulationsTests : Framework
    {
        [Test]
        public void When_DCSweepResistorParameter_Expect_Reference()
        {
            // Note: We specify LinkParameters = false for entities that should not share data across different threads.
            // The voltage source and resistor R2 are swept for different instances so they should be completely independent.
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0) { LinkParameters = false },
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 1.0e4) { LinkParameters = false }
                );

            // Do a DC sweep where one of the sweeps is a parameter
            var dcSimulations = new List<DC>();
            var n = 4;
            for (var i = 0; i < n; i++)
            {
                var dc = new DC("DC " + i);
                dc.DCParameters.Sweeps.Add(new ParameterSweep("R2", "resistance", new LinearSweep(0.0, 1e4, 1e3), container =>
                {
                    container.GetValue<ITemperatureBehavior>().Temperature();
                })); // Sweep R2 from 0 to 10k per 1k
                dc.DCParameters.Sweeps.Add(new ParameterSweep("V1", new LinearSweep(1, 5, 0.1))); // Sweep V1 from 1V to 5V per 100mV
                dc.ExportSimulationData += (sender, args) =>
                {
                    var resistance = Math.Max(dc.GetCurrentSweepValue()[0], SpiceSharp.Components.Resistors.Parameters.MinimumResistance);
                    var voltage = dc.GetCurrentSweepValue()[1];
                    var expected = voltage * resistance / (resistance + 1.0e4);
                    Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
                };

                dcSimulations.Add(dc);
            }
            var maxConcurrentSimulations = 2;

            System.Threading.Tasks.Parallel.ForEach(
                dcSimulations,
                new ParallelOptions() { MaxDegreeOfParallelism = maxConcurrentSimulations },
                (simulation) => simulation.Run(ckt));
        }

        [Test]
        public void When_FloatingRTransient_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10.0)
            );

            var transientSimulations = new List<Transient>();
            var n = 1000;

            for (var i = 0; i < n; i++)
            {
                // Create the transient analysis
                var tran = new Transient("Tran 1", 1e-6, 10.0);
                tran.ExportSimulationData += (sender, args) =>
                {
                    Assert.AreEqual(args.GetVoltage("out"), 10.0, 1e-12);
                };

                transientSimulations.Add(tran);
            }

            var maxConcurrentSimulations = 8;
            System.Threading.Tasks.Parallel.ForEach(
                transientSimulations,
                new ParallelOptions() { MaxDegreeOfParallelism = maxConcurrentSimulations },
                (simulation) => simulation.Run(ckt));
        }
    }
}
