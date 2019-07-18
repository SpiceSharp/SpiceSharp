using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageControlledVoltageSourceTests : Framework
    {
        [Test]
        public void When_VCVSDC_Expect_Reference()
        {
            var gain = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );

            // Build simulation, exports and references
            var dc = new DC("DC", "V1", -10, 10, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out") };
            Func<double, double>[] references = { sweep => gain * sweep };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VCVSSmallSignal_Expect_Reference()
        {
            var magnitude = 0.9;
            var gain = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );
            ckt["V1"].SetParameter("acmag", magnitude);

            // Build simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };
            Func<double, Complex>[] references = { sweep => gain * magnitude };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
