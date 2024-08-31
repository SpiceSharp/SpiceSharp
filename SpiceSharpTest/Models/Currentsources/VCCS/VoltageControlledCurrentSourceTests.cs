using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Linq;
using System.Numerics;

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
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var dc = new DC("DC", "V1", -10.0, 10.0, 1e-3);
            IExport<double>[] exports = [new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i")];
            Func<double, double>[] references = [sweep => sweep * transconductance * resistance, sweep => sweep * transconductance];
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VCCSSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double transconductance = 2e-3;
            double resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1, 1e4, 3));
            IExport<Complex>[] exports = [new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i")];
            Func<double, Complex>[] references = [freq => magnitude * transconductance * resistance, freq => magnitude * transconductance];
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

            foreach (int _ in op.Run(ckt))
                Assert.That(current.Value, Is.EqualTo(300.0).Within(1e-12));
        }

        [Test]
        public void When_FloatingOutput_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new Resistor("R1", "0", "1", 1e3),
                new VoltageControlledCurrentSource("F1", "out", "0", "in", "0", 12.0)
                );

            // Make the simulation and run it
            var dc = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => dc.RunToEnd(ckt));
            Assert.That(ex.Rules.ViolationCount, Is.EqualTo(2));
            var violations = ex.Rules.Violations.ToArray();
            Assert.That(violations[0], Is.InstanceOf<FloatingNodeRuleViolation>());
            Assert.That(((FloatingNodeRuleViolation)violations[0]).FloatingVariable.Name, Is.EqualTo("out"));
            Assert.That(violations[1], Is.InstanceOf<FloatingNodeRuleViolation>());
            Assert.That(((FloatingNodeRuleViolation)violations[1]).FloatingVariable.Name, Is.EqualTo("in"));
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            var ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F2", "ref", "0", "in", "0", 1.0),
                new Resistor("R1", "ref", "0", 1.0));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R1", "ref", "0", 1.0));

            var op = new OP("op");
            var exports = new[] { new RealVoltageExport(op, "ref") };
            Compare(op, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplierAC_Expect_Reference()
        {
            var ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F2", "ref", "0", "in", "0", 1.0),
                new Resistor("R1", "ref", "0", 1.0));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R1", "ref", "0", 1.0));

            var ac = new AC("ac");
            var exports = new[] { new ComplexVoltageExport(ac, "ref") };
            Compare(ac, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
