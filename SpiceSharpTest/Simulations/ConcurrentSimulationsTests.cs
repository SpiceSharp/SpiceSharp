using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpTest.Models;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class ConcurrentSimulationsTests : Framework
    {        
        [Test]
        public void When_DCSweepResistorParameter_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 1.0e4)
                );

            // Do a DC sweep where one of the sweeps is a parameter
            var cconfig = new CollectionConfiguration() {CloneParameters = true};
            var dcSimulations = new List<DC>();
            var n = 4;
            for (var i = 0; i < n; i++)
            {
                var dc = new DC("DC " + i);
                dc.Configurations.Add(cconfig);
                var config = dc.Configurations.Get<DCConfiguration>();
                config.Sweeps.Add(new SweepConfiguration("R2", 0.0, 1e4, 1e3)); // Sweep R2 from 0 to 10k per 1k
                config.Sweeps.Add(new SweepConfiguration("V1", 1, 5, 0.1)); // Sweep V1 from 1V to 5V per 100mV
                dc.OnParameterSearch += (sender, args) =>
                {
                    if (args.Name.Equals("R2"))
                    {
                        args.Result = dc.EntityParameters["R2"].GetParameter<Parameter<double>>("resistance");
                        args.TemperatureNeeded = true;
                    }
                };
                dc.ExportSimulationData += (sender, args) =>
                {
                    var resistance = dc.Sweeps[0].CurrentValue;
                    var voltage = dc.Sweeps[1].CurrentValue;
                    var expected = voltage * resistance / (resistance + 1.0e4);
                    Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
                };

                dcSimulations.Add(dc);
            }
            var maxConcurrentSimulations = 2;

            Parallel.ForEach(
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
            var n = 10000;

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
            Parallel.ForEach(
                transientSimulations,
                new ParallelOptions() { MaxDegreeOfParallelism = maxConcurrentSimulations },
                (simulation) => simulation.Run(ckt));
        }
    }
}
