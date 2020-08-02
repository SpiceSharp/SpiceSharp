using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.ParallelComponents;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SimpleParallelTests : Framework
    {
        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelBiasingLoadOp_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Resistor("R2", "out", "0", 1e3))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingSimulation), workDistributor))
                );

            var op = new OP("op");
            var exports = new IExport<double>[] { new RealVoltageExport(op, "out") };
            var references = new double[] { 0.5 };
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(BoolWorkDistributor))]
        public void When_ParallelConvergenceOp_Expect_Reference(IWorkDistributor<bool> workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Resistor("R2", "out", "0", 1e3))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IConvergenceBehavior), workDistributor)));

            var op = new OP("op");
            var exports = new IExport<double>[] { new RealVoltageExport(op, "out") };
            var references = new double[] { 0.5 };
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelAcLoadAc_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Capacitor("C1", "out", "0", 1e-6))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IFrequencyBehavior), workDistributor))
                );

            var ac = new AC("ac", new DecadeSweep(1, 1e6, 2));
            var exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "out") };
            var references = new Func<double, Complex>[] { f => 1.0 / (1.0 + new Complex(0.0, f * 2e-3 * Math.PI)) };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelBiasingLoadTransient_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Capacitor("C1", "out", "0", 1e-6))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingBehavior), workDistributor)));

            var tran = new Transient("tran", 1e-7, 10e-6);
            var exports = new IExport<double>[] { new RealVoltageExport(tran, "out") };
            var references = new Func<double, double>[] { time => 1.0 };
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelTransientInitTransient_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Capacitor("C1", "out", "0", 1e-6),
                    new Capacitor("C2", "out", "0", 1e-6))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(ITimeBehavior), workDistributor)));

            var tran = new Transient("tran", 1e-7, 10e-6);
            var exports = new IExport<double>[] { new RealVoltageExport(tran, "out") };
            var references = new Func<double, double>[] { time => 1.0 };
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        public static IEnumerable<TestCaseData> WorkDistributor
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(new TPLWorkDistributor());
            }
        }

        public static IEnumerable<TestCaseData> BoolWorkDistributor
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(new TPLBooleanAndWorkDistributor());
            }
        }
    }
}
