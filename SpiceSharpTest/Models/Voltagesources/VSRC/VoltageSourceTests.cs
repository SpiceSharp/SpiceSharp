using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageSourceTests : Framework
    {
        [Test]
        public void When_VoltageSourceSeries_Expect_Reference()
        {
            double[] voltages = { 1.0, 1.5, 2.8, 3.9, 0.5, -0.1, -0.5 };

            // Build the circuit
            var ckt = new Circuit();
            var sum = 0.0;
            for (var i = 0; i < voltages.Length; i++)
            {
                ckt.Add(new VoltageSource($"V{i + 1}", $"{i + 1}", $"{i}", voltages[i]));
                sum += voltages[i];
            }

            // Build the simulation, exports and references
            var op = new OP("OP");
            IExport<double>[] exports = { new RealVoltageExport(op, $"{voltages.Length}") };
            double[] references = { sum };
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VoltageSourceSmallSignal_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0));
            var ac = new AC("ac", new DecadeSweep(1.0, 100, 5));
            var exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "in") };
            var references = new Func<double, Complex>[] { f => new Complex(1.0, 0.0) };
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
