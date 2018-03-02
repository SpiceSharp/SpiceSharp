using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Sweeps;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models
{
    [TestClass]
    public class VoltageControlledVoltageSourceTests : Framework
    {
        [TestMethod]
        public void When_VCVSDC_Expect_Reference()
        {
            double gain = 12.0;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );

            // Build simulation, exports and references
            DC dc = new DC("DC", "V1", -10, 10, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out") };
            Func<double, double>[] references = { (double sweep) => gain * sweep };
            AnalyzeDC(dc, ckt, exports, references);
        }

        [TestMethod]
        public void When_VCVSSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double gain = 12.0;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );
            ckt.Objects["V1"].ParameterSets.SetProperty("acmag", magnitude);

            // Build simulation, exports and references
            AC ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };
            Func<double, Complex>[] references = { (double sweep) => gain * magnitude };
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
