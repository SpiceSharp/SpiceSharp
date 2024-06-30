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
    public class VoltageControlledVoltageSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            double gain = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );

            // Build simulation, exports and references
            var dc = new DC("DC", "V1", -10, 10, 1e-3);
            IExport<double>[] exports = [new RealVoltageExport(dc, "out")];
            Func<double, double>[] references = [sweep => gain * sweep];
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double gain = 12.0;

            // Build circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", gain)
                );

            // Build simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            IExport<Complex>[] exports = [new ComplexVoltageExport(ac, "out")];
            Func<double, Complex>[] references = [sweep => gain * magnitude];
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_OpenCircuitInput_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", 1.0));
            var op = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => op.RunToEnd(ckt));
            Assert.That(ex.Rules.ViolationCount, Is.EqualTo(1));
            Assert.That(ex.Rules.Violations.First(), Is.InstanceOf<FloatingNodeRuleViolation>());
        }

        [Test]
        public void When_VoltageLoop1_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E2", "0", "out", "in", "0", 2.0));
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
                new VoltageControlledVoltageSource("E1", "out", "0", "in", "0", 1.0),
                new VoltageControlledVoltageSource("E2", "out2", "out", "in", "0", 2.0),
                new VoltageControlledVoltageSource("E3", "out2", "0", "in", "0", 3.0));
            var op = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => op.RunToEnd(ckt));
            Assert.That(ex.Rules.ViolationCount, Is.EqualTo(1));
            var violation = ex.Rules.Violations.First();
            Assert.That(violation, Is.InstanceOf<VoltageLoopRuleViolation>());
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
                var ibr = new Variable("branch", Units.Ampere);

                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b", "c", "d").CreateVariable(ibr).Bias()
                    .Parameter(new BaseParameters { Coefficient = 2 });
                yield return new TestCaseData(context.AsProxy(), new double[]
                    {
                        double.NaN, double.NaN, double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, -1.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                        1.0, -1.0, -2.0, 2.0, double.NaN, double.NaN
                    }).SetName("{m}(DC)");
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                IComponentBindingContext context;
                var ibr = new Variable("branch", Units.Ampere);

                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b", "c", "d").CreateVariable(ibr)
                    .Frequency().Parameter(new BaseParameters { Coefficient = 2 });
                context.Variables.Create(Arg.Any<string>(), Units.Ampere).Returns(ibr);
                yield return new TestCaseData(context.AsProxy(), new Complex[]
                    {
                        double.NaN, double.NaN, double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, -1.0, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                        1.0, -1.0, -2.0, 2.0, double.NaN, double.NaN
                    }).SetName("{m}(AC)");
            }
        }
        public static IEnumerable<TestCaseData> Rules
        {
            get
            {
                ComponentRuleParameters parameters;

                yield return new TestCaseData(
                    new Circuit(new VoltageControlledVoltageSource("E1", "out", "0", "inp", "inn", 1.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new FloatingNodeRule(parameters.Variables.Ground)),
                    new[] { typeof(FloatingNodeRuleViolation), typeof(FloatingNodeRuleViolation) });
            }
        }
        */
    }
}
