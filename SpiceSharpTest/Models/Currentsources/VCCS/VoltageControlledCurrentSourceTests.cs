using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageControlledCurrentSourceTests : Framework
    {
        [Test]
        public void When_VCCSDC_Expect_Reference()
        {
            double transconductance = 2e-3;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            DC dc = new DC("DC", "V1", -10.0, 10.0, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * transconductance * resistance, sweep => sweep * transconductance };
            AnalyzeDC(dc, ckt, exports, references);
        }

        [Test]
        public void When_VCCSSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double transconductance = 2e-3;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );
            ckt.Objects["V1"].ParameterSets.SetParameter("acmag", magnitude);

            // Make the simulation, exports and references
            AC ac = new AC("AC", new SpiceSharp.Simulations.Sweeps.DecadeSweep(1, 1e4, 3));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { freq => magnitude * transconductance * resistance, freq => magnitude * transconductance };
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
