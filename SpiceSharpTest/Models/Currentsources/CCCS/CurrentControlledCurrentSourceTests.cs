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
            var gain = 0.85;
            var resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var dc = new DC("DC", "I1", -10.0, 10.0, 1e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * gain * resistance, sweep => sweep * gain };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            var magnitude = 0.6;
            var gain = 0.85;
            var resistance = 1e4;

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
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { freq => magnitude * gain * resistance, freq => magnitude * gain };
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
            var ex = Assert.Throws<ValidationFailedException>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            var violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violation);
            Assert.AreEqual("out", ((FloatingNodeRuleViolation)violation).FloatingVariable.Name);
        }

        /*
        [TestCaseSource(nameof(Biasing))]
        public void When_BiasingBehavior_Expect_Reference(Proxy<ICurrentControlledBindingContext> context, double[] expected)
        {
            var behavior = new BiasingBehavior("F1", context.Value);
            ((IBiasingBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IBiasingSimulationState>().Solver, expected);
        }

        [TestCaseSource(nameof(Frequency))]
        public void When_FrequencyBehavior_Expect_Reference(Proxy<ICurrentControlledBindingContext> context, Complex[] expected)
        {
            var behavior = new FrequencyBehavior("F1", context.Value);
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
                ICurrentControlledBindingContext context;

                // Simple DC
                context = Substitute.For<ICurrentControlledBindingContext>()
                    .Nodes("a", "b").BranchControlled(new Variable("controlbranch", Units.Ampere))
                    .Bias().Parameter(new BaseParameters { Coefficient = 1 });
                yield return new TestCaseData(context.AsProxy(), new[]
                    {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN // No voltage source
                    })
                    .SetName("{m}(DC)");
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                ICurrentControlledBindingContext context;
                var ibr = new Variable("branch", Units.Ampere);

                // Simple DC
                context = Substitute.For<ICurrentControlledBindingContext>()
                    .Nodes("a", "b").BranchControlled(new Variable("controlbranch", Units.Ampere)).Frequency()
                    .Parameter(new BaseParameters { Coefficient = 1 });
                yield return new TestCaseData(context.AsProxy(), new Complex[]
                    {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN // No voltage source
                    })
                    .SetName("{m}(AC)");
            }
        }
        public static IEnumerable<TestCaseData> Rules
        {
            get
            {
                ComponentRuleParameters parameters;

                yield return new TestCaseData(
                    new Circuit(new CurrentControlledCurrentSource("F1", "out", "0", "V1", 1.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new FloatingNodeRule(parameters.Variables.Ground)),
                    new[] { typeof(FloatingNodeRuleViolation) });
            }
        }
        */
    }
}
