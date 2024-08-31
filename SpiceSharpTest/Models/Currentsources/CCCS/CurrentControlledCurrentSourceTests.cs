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
    public class CurrentControlledCurrentSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            double gain = 0.85;
            double resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var dc = new DC("DC", "I1", -10.0, 10.0, 1e-3);
            IExport<double>[] exports = [new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i")];
            Func<double, double>[] references = [sweep => sweep * gain * resistance, sweep => sweep * gain];
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            double magnitude = 0.6;
            double gain = 0.85;
            double resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1, 1e4, 3));
            IExport<Complex>[] exports = [new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i")];
            Func<double, Complex>[] references = [freq => magnitude * gain * resistance, freq => magnitude * gain];
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_FloatingOutput_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0),
                new VoltageSource("V1", "in", "0", 0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 12.0)
                );

            // Make the simulation and run it
            var op = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => op.RunToEnd(ckt));
            Assert.That(ex.Rules.ViolationCount, Is.EqualTo(1));
            var violation = ex.Rules.Violations.First();
            Assert.That(violation, Is.InstanceOf<FloatingNodeRuleViolation>());
            Assert.That(((FloatingNodeRuleViolation)violation).FloatingVariable.Name, Is.EqualTo("out"));
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            var ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "ref", "0", "V1", 1.0),
                new CurrentControlledCurrentSource("F2", "ref", "0", "V1", 1.0),
                new Resistor("R1", "ref", "0", 1.0));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "ref", "0", "V1", 1.0)
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
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 1.0),
                new CurrentControlledCurrentSource("F2", "out", "0", "V1", 1.0),
                new Resistor("R2", "out", "0", 1.0));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R2", "out", "0", 1.0));

            var ac = new AC("ac");
            var exports = new[] { new ComplexVoltageExport(ac, "out") };
            Compare(ac, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
