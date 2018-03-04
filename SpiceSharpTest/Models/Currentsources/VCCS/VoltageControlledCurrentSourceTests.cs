using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestClass]
    public class VoltageControlledCurrentSourceTests : Framework
    {
        [TestMethod]
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
            Dc dc = new Dc("DC", "V1", -10.0, 10.0, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { (double sweep) => sweep * transconductance * resistance, (double sweep) => sweep * transconductance };
            AnalyzeDC(dc, ckt, exports, references);
        }

        [TestMethod]
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
            ckt.Objects["V1"].ParameterSets.SetProperty("acmag", magnitude);

            // Make the simulation, exports and references
            Ac ac = new Ac("AC", new SpiceSharp.Simulations.Sweeps.DecadeSweep(1, 1e4, 3));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { (double freq) => magnitude * transconductance * resistance, (double freq) => magnitude * transconductance };
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
