using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpTest.Models;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class DCTests : Framework
    {
        private Diode CreateDiode(string name, string anode, string cathode, string model)
        {
            var d = new Diode(name) {Model = model};
            d.Connect(anode, cathode);
            return d;
        }

        private DiodeModel CreateDiodeModel(string name, string parameters)
        {
            var model = new DiodeModel(name);
            ApplyParameters(model, parameters);
            return model;
        }

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
            var config = dc.Configurations.Get<DCConfiguration>();
            config.Sweeps.Add(new SweepConfiguration("R2", 0.0, 1e4, 1e3)); // Sweep R2 from 0 to 10k per 1k
            config.Sweeps.Add(new SweepConfiguration("V1", 0, 5, 0.1)); // Sweep V1 from 0V to 5V per 100mV
            dc.OnParameterSearch += (sender, args) =>
            {
                if (args.Name.Equals("R2"))
                {
                    args.Result = dc.EntityParameters["R2"].GetParameter<Parameter<double>>("resistance");
                    args.TemperatureNeeded = true;
                }
            };

            // Run simulation
            dc.ExportSimulationData += (sender, args) =>
            {
                var resistance = dc.Sweeps[0].CurrentValue;
                var voltage = dc.Sweeps[1].CurrentValue;
                var expected = voltage * resistance / (resistance + 1.0e4);
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            dc.Run(ckt);
        }

        [Test]
        public void When_DiodeDC_Expect_NoException()
        {
            /*
             * Bug found by Marcin Golebiowski
             * Running simulations twice will give rise to errors. We are using a diode model here
             * in order to make sure we're use states, extra equations, etc.
             */

            var ckt = new Circuit();
            ckt.Add(
                CreateDiode("D1", "OUT", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 0.0)
            );

            // Create simulations
            var dc = new DC("DC 1", "V1", -1, 1, 10e-3);
            var op = new OP("OP 1");

            // Create exports
            var dcExportV1 = new RealPropertyExport(dc, "V1", "i");
            var dcExportV12 = new RealPropertyExport(dc, "V1", "i");
            dc.ExportSimulationData += (sender, args) =>
            {
                var v1 = dcExportV1.Value;
                var v12 = dcExportV12.Value;
            };
            var opExportV1 = new RealPropertyExport(op, "V1", "i");
            op.ExportSimulationData += (sender, args) =>
            {
                var v1 = opExportV1.Value;
            };

            // Run DC and op
            dc.Run(ckt);
            dc.Run(ckt);
        }
    }
}
