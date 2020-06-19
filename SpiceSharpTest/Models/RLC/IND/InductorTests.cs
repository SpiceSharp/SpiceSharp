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
    public class InductorTests : Framework
    {
        [Test]
        public void When_LowpassRLOP_Expect_Reference()
        {
            /*
             * Lowpass RL circuit
             * The inductor should act like an short circuit
             */
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 1.0),
                new Inductor("L1", "IN", "OUT", 1e-3),
                new Resistor("R1", "OUT", "0", 1.0e3));

            // Create simulation
            var op = new OP("op");

            // Create exports
            var exports = new IExport<double>[1];
            exports[0] = new RealVoltageExport(op, "OUT");

            // Create references
            double[] references = { 1.0 };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_LowpassRLSmallSignal_Expect_Reference()
        {
            /*
             * Lowpass RL filter in the frequency domain should have a single pole at s=-2pi*R/L
             */
            // Create circuit
            double resistance = 1;
            var inductance = 1e-3;
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Inductor("L1", "IN", "OUT", inductance),
                new Resistor("R1", "OUT", "0", resistance));

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(0.1, 1e6, 10));

            // Create exports
            var exports = new IExport<Complex>[1];
            exports[0] = new ComplexVoltageExport(ac, "OUT");

            // Create references
            Func<double, Complex>[] references = { f => 1.0 / new Complex(1.0, inductance / resistance * 2 * Math.PI * f) };

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_LCTankTransient_Expect_Reference()
        {
            /*
             * Test for LC tank circuit, an inductor parallel with a capacitor will resonate at a frequency of 1/(2*pi*sqrt(LC))
             */
            // Build circuit
            var capacitance = 1e-3;
            var inductance = 1e-6;
            var initialCurrent = 1e-3;
            var ckt = new Circuit(
                new Inductor("L1", "OUT", "0", inductance)
                    .SetParameter("ic", initialCurrent),
                new Capacitor("C1", "OUT", "0", capacitance)
                );

            /*
             * WARNING: An LC tank is a circuit that oscillates and does not converge. This causes the global truncation error
             * to increase as time goes by!
             * For this reason, the absolute tolerance is made a little bit less strict.
             */
            AbsTol = 1e-9;

            // Create simulation
            var tran = new Transient("tran", 1e-9, 1e-3, 1e-7);
            tran.TimeParameters.InitialConditions["OUT"] = 0.0;

            // Create exports
            var exports = new IExport<double>[1];
            exports[0] = new RealPropertyExport(tran, "C1", "v");

            // Create reference function
            var amplitude = Math.Sqrt(inductance / capacitance) * initialCurrent;
            var omega = 1.0 / Math.Sqrt(inductance * capacitance);
            Func<double, double>[] references = { t => -amplitude * Math.Sin(omega * t) };

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_InductorLoop_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 1e-3),
                new Inductor("L1", "in", "0", 1e-6),
                new Inductor("L2", "in", "0", 1e-6));

            var op = new OP("op");
            var ex = Assert.Throws<ValidationFailedException>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            var violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<VoltageLoopRuleViolation>(violation);
        }

        /*
        [TestCaseSource(nameof(Temperature))]
        public void When_TemperatureBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double expected)
        {
            var behavior = new TemperatureBehavior("L1", context.Value);
            ((ITemperatureBehavior)behavior).Temperature();
            Check.Double(behavior.Inductance, expected);
        }
        [TestCaseSource(nameof(Biasing))]
        public void When_BiasingBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new BiasingBehavior("L1", context.Value);
            ((ITemperatureBehavior)behavior).Temperature();
            ((IBiasingBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IBiasingSimulationState>().Solver, expected);
        }
        [TestCaseSource(nameof(Frequency))]
        public void When_FrequencyBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, Complex[] expected)
        {
            var behavior = new FrequencyBehavior("L1", context.Value);
            ((ITemperatureBehavior)behavior).Temperature();
            ((IBiasingBehavior)behavior).Load();
            ((IFrequencyBehavior)behavior).InitializeParameters();
            ((IFrequencyBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IComplexSimulationState>().Solver, expected);
        }
        [TestCaseSource(nameof(Transient))]
        public void When_TimeBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new TimeBehavior("L1", context.Value);
            ((ITemperatureBehavior)behavior).Temperature();
            ((ITimeBehavior)behavior).InitializeStates();
            ((ITimeBehavior)behavior).Load();
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

        public static IEnumerable<TestCaseData> Temperature
        {
            get
            {
                IComponentBindingContext context;
                context = Substitute.For<IComponentBindingContext>()
                    .Parameter(new BaseParameters(1e-9));
                yield return new TestCaseData(context.AsProxy(), 1e-9).SetName("{m}(L=1n)");
            }
        }
        public static IEnumerable<TestCaseData> Biasing
        {
            get
            {
                IComponentBindingContext context;
                
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Bias().Parameter(new BaseParameters(1e-9));
                yield return new TestCaseData(context.AsProxy(), new double[]
                {
                    double.NaN, double.NaN, 1.0, double.NaN,
                    double.NaN, double.NaN, -1.0, double.NaN,
                    1.0, -1.0, double.NaN, double.NaN
                });
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                IComponentBindingContext context;

                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Frequency(1.0).Parameter(new BaseParameters(1e-9));
                yield return new TestCaseData(context.AsProxy(), new Complex[]
                {
                    double.NaN, double.NaN, 1.0, double.NaN,
                    double.NaN, double.NaN, -1.0, double.NaN,
                    1.0, -1.0, new Complex(0, -2e-9 * Math.PI), double.NaN
                }).SetName("{m}(L=1e-9, f=1Hz)");
            }
        }
        public static IEnumerable<TestCaseData> Transient
        {
            get
            {
                IComponentBindingContext context;
                var ibr = new Variable("branch", Units.Ampere);

                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").CreateVariable(new Variable("branch", Units.Ampere))
                    .Transient(0.5, 1e-3).Parameter(new BaseParameters(1e-6));
                double g = 1e-6 / 1e-3;
                double c = 1.0;
                yield return new TestCaseData(context.AsProxy(), new double[]
                {
                    double.NaN, double.NaN, 1, double.NaN,
                    double.NaN, double.NaN, -1, double.NaN,
                    1, -1, -g, c
                }).SetName("{m}(L=1u dt=1n dvdt=1)");
            }
        }
        public static IEnumerable<TestCaseData> Rules
        {
            get
            {
                ComponentRuleParameters parameters;
                yield return new TestCaseData(
                    new Circuit(new Inductor("L1", "a", "b", 1.0), new Inductor("L2", "b", "a", 2.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new VoltageLoopRule()),
                    new[] { typeof(VoltageLoopRuleViolation) });

                yield return new TestCaseData(
                    new Circuit(new Inductor("L1", "a", "b", 1.0), new Inductor("L2", "b", "c", 2.0), new Inductor("L3", "c", "a", 3.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new VoltageLoopRule()),
                    new[] { typeof(VoltageLoopRuleViolation) });
            }
        }
        */
    }
}
