using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageSourceTests : Framework
    {
        [Test]
        public void When_SimpleSeriesOP_Expect_Reference()
        {
            double[] voltages = [1.0, 1.5, 2.8, 3.9, 0.5, -0.1, -0.5];

            // Build the circuit
            var ckt = new Circuit();
            double sum = 0.0;
            for (int i = 0; i < voltages.Length; i++)
            {
                ckt.Add(new VoltageSource($"V{i + 1}", $"{i + 1}", $"{i}", voltages[i]));
                sum += voltages[i];
            }

            // Build the simulation, exports and references
            var op = new OP("OP");
            IExport<double>[] exports = [new RealVoltageExport(op, $"{voltages.Length}")];
            double[] references = [sum];
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0));
            var ac = new AC("ac", new DecadeSweep(1.0, 100, 5));
            var exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "in") };
            var references = new Func<double, Complex>[] { f => new Complex(1.0, 0.0) };
            AnalyzeAC(ac, ckt, exports, references);
        }

        /*
        [TestCaseSource(nameof(Biasing))]
        public void When_BiasingBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new BiasingBehavior("V1", context.Value);
            ((IBiasingBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IBiasingSimulationState>().Solver, expected);
        }

        [TestCaseSource(nameof(Frequency))]
        public void When_FrequencyBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, Complex[] expected)
        {
            var behavior = new FrequencyBehavior("V1", context.Value);
            ((IFrequencyBehavior)behavior).InitializeParameters();
            ((IFrequencyBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IComplexSimulationState>().Solver, expected);
        }

        [TestCaseSource(nameof(Accept))]
        public void When_AcceptBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new AcceptBehavior("V1", context.Value);
            ((IAcceptBehavior)behavior).Probe();
            ((IBiasingBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IBiasingSimulationState>().Solver, expected);
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

                // Simple DC
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Bias().Parameter(new IndependentSourceParameters(2.0));
                yield return new TestCaseData(context.AsProxy(), new double[]
                    {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        1.0, -1.0, double.NaN, 2.0
                    }).SetName("{m}(DC=2)");

                // Using SourceFactor
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Bias(null, i => i.SourceFactor.Returns(0.5)).Parameter(new IndependentSourceParameters(-3.0));
                yield return new TestCaseData(context.AsProxy(),
                    new[] {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        1.0, -1.0, double.NaN, -1.5
                    }).SetName("{m}(SourceFactor=0.5)");
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                IComponentBindingContext context;
                var ibr = new Variable("branch", Units.Ampere);

                // Simple AC magnitude 1
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Frequency().Parameter(new IndependentSourceParameters(1))
                    .Parameter(new IndependentSourceFrequencyParameters(1, 0));
                yield return new TestCaseData(context.AsProxy(),
                    new Complex[] {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        1.0, -1.0, double.NaN, 1.0
                    }).SetName("{m}(AC 1 0)");

                // Simple AC magnitude 1
                var v = new Complex(Math.Cos(0.5 * Math.PI), Math.Sin(0.5 * Math.PI)) * 0.5;
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(ibr)
                    .Frequency().Parameter(new IndependentSourceParameters(1))
                    .Parameter(new IndependentSourceFrequencyParameters { AcMagnitude = 0.5, AcPhase = 90 });
                yield return new TestCaseData(context.AsProxy(),
                    new Complex[] {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        1.0, -1.0, double.NaN, v
                    }).SetName("{m}(AC 0.5 90)");
            }
        }
        public static IEnumerable<TestCaseData> Accept
        {
            get
            {
                IComponentBindingContext context;
                var ibr = new Variable("branch", Units.Ampere);

                // Transient analysis with waveform
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Transient(0.3, 0.01).Parameter(new IndependentSourceParameters { Waveform = new Sine(0, 1, 1) });
                var v = Math.Sin(0.3 * 2 * Math.PI);
                yield return new TestCaseData(context.AsProxy(),
                    new[] {
                        double.NaN, double.NaN, 1.0, double.NaN,
                        double.NaN, double.NaN, -1.0, double.NaN,
                        1.0, -1.0, double.NaN, v
                    }).SetName("{m}(Sine)");
            }
        }
        public static IEnumerable<TestCaseData> Rules
        {
            get
            {
                yield return new TestCaseData(
                    new Circuit(
                        new VoltageSource("V1", "a", "b", 1.0),
                        new VoltageSource("V2", "b", "a", 2.0)),
                    new ComponentRules(new ComponentRuleParameters(), new VoltageLoopRule()),
                    new[] { typeof(VoltageLoopRuleViolation) });

                yield return new TestCaseData(
                    new Circuit(
                        new VoltageSource("V1", "a", "b", 1.0),
                        new VoltageSource("V2", "b", "c", 2.0),
                        new VoltageSource("V3", "c", "a", 3.0)),
                    new ComponentRules(new ComponentRuleParameters(), new VoltageLoopRule()),
                    new[] { typeof(VoltageLoopRuleViolation) });
            }
        }
        */
    }
}
