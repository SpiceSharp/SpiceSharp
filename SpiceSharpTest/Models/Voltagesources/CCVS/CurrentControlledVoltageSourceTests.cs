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
    public class CurrentControlledVoltageSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            double transimpedance = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", transimpedance)
                );

            // Build simulation, exports and references
            var dc = new DC("DC", "I1", -10, 10, 1e-3);
            IExport<double>[] exports = [new RealVoltageExport(dc, "out")];
            Func<double, double>[] references = [sweep => transimpedance * sweep];
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double transimpedance = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", transimpedance)
                );

            // Build simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            IExport<Complex>[] exports = [new ComplexVoltageExport(ac, "out")];
            Func<double, Complex>[] references = [sweep => transimpedance * magnitude];
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VoltageLoop1_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", 1.0),
                new CurrentControlledVoltageSource("F2", "0", "out", "V1", 2.0));
            var op = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => op.RunToEnd(ckt));
            Assert.That(ex.Rules.ViolationCount, Is.EqualTo(1));
            var violation = ex.Rules.Violations.First();
            Assert.That(violation, Is.InstanceOf<VoltageLoopRuleViolation>());
        }

        [Test]
        public void When_VoltageLoop2_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", 1.0),
                new CurrentControlledVoltageSource("F2", "out2", "out", "V1", 2.0),
                new CurrentControlledVoltageSource("F3", "out2", "0", "V1", 3.0));
            var op = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => op.RunToEnd(ckt));
            Assert.That(ex.Rules.ViolationCount, Is.EqualTo(1));
            var violation = ex.Rules.Violations.First();
            Assert.That(violation, Is.InstanceOf<VoltageLoopRuleViolation>());
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
                var ibr = new Variable("V1".Combine("branch"), Units.Ampere);
                var ibrc = new Variable("VC".Combine("branch"), Units.Ampere);

                // Simple DC
                context = Substitute.For<ICurrentControlledBindingContext>()
                    .Nodes("a", "b").CreateVariable(ibr).BranchControlled(ibrc)
                    .Bias().Parameter(new BaseParameters { Coefficient = 2 });
                yield return new TestCaseData(context.AsProxy(), new[]
                    {
                        double.NaN, double.NaN, 1.0, double.NaN, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN, double.NaN,
                        1.0, -1.0, double.NaN, -2.0, double.NaN, // Branch equation
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN // Controlling branch equation
                    })
                    .SetName("{m}(DC)");
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                ICurrentControlledBindingContext context;
                var ibr = new Variable("V1".Combine("branch"), Units.Ampere);
                var ibrc = new Variable("VC".Combine("branch"), Units.Ampere);

                // Simple DC
                context = Substitute.For<ICurrentControlledBindingContext>()
                    .Nodes("a", "b").CreateVariable(ibr).BranchControlled(ibrc)
                    .Frequency().Parameter(new BaseParameters { Coefficient = 2 });
                yield return new TestCaseData(context.AsProxy(), new Complex[]
                    {
                        double.NaN, double.NaN, 1.0, double.NaN, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN, double.NaN,
                        1.0, -1.0, double.NaN, -2.0, double.NaN, // Branch equation
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN // Controlling branch equation
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
                    new Circuit(new CurrentControlledVoltageSource("F1", "out", "0", "V1", 1.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new FloatingNodeRule(parameters.Variables.Ground)),
                    new Type[] { });
            }
        }
        */
    }
}
