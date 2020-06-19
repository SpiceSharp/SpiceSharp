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
            IExport<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
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
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
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

        [Test]
        public void When_FloatingOutput_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new Resistor("R1", "0", "1", 1e3),
                new VoltageControlledCurrentSource("F1", "out", "0", "in", "0", 12.0)
                );

            // Make the simulation and run it
            var dc = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => dc.Run(ckt));
            Assert.AreEqual(2, ex.Rules.ViolationCount);
            var violations = ex.Rules.Violations.ToArray();
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violations[0]);
            Assert.AreEqual("out", ((FloatingNodeRuleViolation)violations[0]).FloatingVariable.Name);
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violations[1]);
            Assert.AreEqual("in", ((FloatingNodeRuleViolation)violations[1]).FloatingVariable.Name);
        }

        /*
        [TestCaseSource(nameof(Biasing))]
        public void When_BiasingBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new BiasingBehavior("E1", context.Value);
            ((IBiasingBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IBiasingSimulationState>().Solver, expected);
        }

        [TestCaseSource(nameof(Frequency))]
        public void When_FrequencyBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, Complex[] expected)
        {
            var behavior = new FrequencyBehavior("E1", context.Value);
            ((IFrequencyBehavior)behavior).InitializeParameters();
            ((IFrequencyBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IComplexSimulationState>().Solver, expected);
        }

        [TestCaseSource(nameof(Rules))]
        public void When_Rules_Expect_Reference(Circuit ckt, ComponentRules rules, Type[] violations)
        {
            ckt.Validate(rules);
            Assert.AreEqual(violations.Length, rules.ViolationCount);
            int index = 0;
            foreach (var violation in rules.Violations)
                Assert.AreEqual(violations[index++], violation.GetType());
        }

        public static IEnumerable<TestCaseData> Biasing
        {
            get
            {
                IComponentBindingContext context;

                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b", "c", "d").Bias().Parameter(new BaseParameters { Coefficient = 2 });
                yield return new TestCaseData(context.AsProxy(), new double[]
                    {
                        double.NaN, double.NaN, 2.0, -2.0, double.NaN,
                        double.NaN, double.NaN, -2.0, 2.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN
                    }).SetName("{m}(DC)");
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                IComponentBindingContext context;

                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b", "c", "d").Frequency().Parameter(new BaseParameters { Coefficient = 2 });
                yield return new TestCaseData(context.AsProxy(), new Complex[]
                    {
                        double.NaN, double.NaN, 2.0, -2.0, double.NaN,
                        double.NaN, double.NaN, -2.0, 2.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN
                    }).SetName("{m}(AC)");
            }
        }
        public static IEnumerable<TestCaseData> Rules
        {
            get
            {
                ComponentRuleParameters parameters;

                yield return new TestCaseData(
                    new Circuit(new VoltageControlledCurrentSource("H1", "out", "0", "inp", "inn", 1.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new FloatingNodeRule(parameters.Variables.Ground)),
                    new[] { typeof(FloatingNodeRuleViolation), typeof(FloatingNodeRuleViolation), typeof(FloatingNodeRuleViolation) });
            }
        }
        */
    }
}
