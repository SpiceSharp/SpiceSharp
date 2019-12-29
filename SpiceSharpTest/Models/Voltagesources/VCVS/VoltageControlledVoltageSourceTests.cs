using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;
using System.Linq;
using SpiceSharp.Validation;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageControlledVoltageSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            var gain = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );

            // Build simulation, exports and references
            var dc = new DC("DC", "V1", -10, 10, 1e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out") };
            Func<double, double>[] references = { sweep => gain * sweep };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            var magnitude = 0.9;
            var gain = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );

            // Build simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };
            Func<double, Complex>[] references = { sweep => gain * magnitude };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_OpenCircuitInput_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", 1.0));
            var op = new OP("op");
            var ex = Assert.Throws<SimulationValidationFailed>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(ex.Rules.Violations.First());
        }

        [Test]
        public void When_VoltageLoop1_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E2", "0", "out", "in", "0", 2.0));
            var op = new OP("op");
            var ex = Assert.Throws<SimulationValidationFailed>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            var violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<VoltageLoopRuleViolation>(violation);
        }

        [Test]
        public void When_VoltageLoop2_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E2", "out2", "out", "in", "0", 2.0),
                new VoltageControlledVoltageSource("E3", "out2", "0", "in", "0", 3.0));
            var op = new OP("op");
            var ex = Assert.Throws<SimulationValidationFailed>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            var violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<VoltageLoopRuleViolation>(violation);
        }
    }
}
