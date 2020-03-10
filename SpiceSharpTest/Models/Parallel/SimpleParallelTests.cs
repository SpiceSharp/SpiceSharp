using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Components.ParallelBehaviors;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SimpleParallelTests : Framework
    {
        [Test]
        public void When_SimpleNoParallelOp_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new ParallelComponents("PC1", new Resistor("R1", "in", "out", 1e3), new Resistor("R2", "out", "0", 1e3)));

            var op = new OP("op");
            var exports = new IExport<double>[] { new RealVoltageExport(op, "out") };
            var references = new double[] { 0.5 };
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleParallelOp_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new ParallelComponents("PC1", new Resistor("R1", "in", "out", 1e3), new Resistor("R2", "out", "0", 1e3)));
            ckt["PC1"].SetParameter("biasing.load", new TPLWorkDistributor());

            var op = new OP("op");
            var exports = new IExport<double>[] { new RealVoltageExport(op, "out") };
            var references = new double[] { 0.5 };
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleParallelAc_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new ParallelComponents("PC1", new Resistor("R1", "in", "out", 1e3), new Capacitor("C1", "out", "0", 1e-6)));
            ckt["PC1"].SetParameter("biasing.load", new TPLWorkDistributor());

            var ac = new AC("ac", new DecadeSweep(1, 1e6, 2));
            var exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "out") };
            var references = new Func<double, Complex>[] { f => 1.0 / (1.0 + new Complex(0.0, f * 2e-3 * Math.PI)) };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
