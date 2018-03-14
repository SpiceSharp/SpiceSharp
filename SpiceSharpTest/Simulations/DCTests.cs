using System;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class DCTests
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
            var dc = new DC("DC 1");
            var config = dc.ParameterSets.Get<DcConfiguration>();
            config.Sweeps.Add(new SweepConfiguration("R2", 0.0, 1e4, 1e3)); // Sweep R2 from 0 to 10k per 1k
            config.Sweeps.Add(new SweepConfiguration("V1", 0, 5, 0.1)); // Sweep V1 from 0V to 5V per 100mV
            dc.OnParameterSearch += (sender, args) =>
            {
                if (args.Name.Equals(new Identifier("R2")))
                {
                    args.Result = ckt.Objects["R2"].ParameterSets.GetParameter("resistance");
                    args.TemperatureNeeded = true;
                }
            };

            // Run simulation
            dc.OnExportSimulationData += (sender, args) =>
            {
                var resistance = dc.Sweeps[0].CurrentValue;
                var voltage = dc.Sweeps[1].CurrentValue;
                var expected = voltage * resistance / (resistance + 1.0e4);
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            dc.Run(ckt);
        }
    }
}
