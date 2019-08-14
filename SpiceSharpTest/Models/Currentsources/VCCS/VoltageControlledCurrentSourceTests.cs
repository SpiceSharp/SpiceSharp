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
            var transconductance = 2e-3;
            var resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var dc = new DC("DC", "V1", -10.0, 10.0, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * transconductance * resistance, sweep => sweep * transconductance };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VCCSSmallSignal_Expect_Reference()
        {
            var magnitude = 0.9;
            var transconductance = 2e-3;
            var resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1, 1e4, 3));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { freq => magnitude * transconductance * resistance, freq => magnitude * transconductance };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VCCSDC2_Expect_Reference()
        {
            // Found by Marcin Golebiowski
            var ckt = new Circuit(
                new VoltageSource("V1", "1", "0", 200),
                new Resistor("R1", "1", "0", 10),
                new VoltageControlledCurrentSource("G1", "2", "0", "1", "0", 1.5),
                new Resistor("R2", "2", "0", 100));

            var op = new OP("op1");
            var current = new RealPropertyExport(op, "G1", "i");
            op.ExportSimulationData += (sender, args) => Assert.AreEqual(300.0, current.Value, 1e-12);
            op.Run(ckt);
            current.Destroy();
        }
    }
}
