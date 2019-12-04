using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics.Validation;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentControlledVoltageSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            var transimpedance = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", transimpedance)
                );

            // Build simulation, exports and references
            var dc = new DC("DC", "I1", -10, 10, 1e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out") };
            Func<double, double>[] references = { sweep => transimpedance * sweep };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            var magnitude = 0.9;
            var transimpedance = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", transimpedance)
                );

            // Build simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };
            Func<double, Complex>[] references = { sweep => transimpedance * magnitude };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ShortedValidation_Expect_ShortCircuitComponentException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new CurrentControlledVoltageSource("E1", "in", "in", "V1", 1));
            Assert.Throws<ShortCircuitComponentException>(() => ckt.Validate());
        }

        [Test]
        public void When_VoltageLoopValidation_Expect_VoltageLoopException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new CurrentControlledVoltageSource("E1", "out", "0", "V1", 1),
                new VoltageSource("V2", "out", "0", 0));
            Assert.Throws<VoltageLoopException>(() => ckt.Validate());
        }
    }
}
