using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentSourceTests : Framework
    {
        /// <summary>
        /// Creates a circuit with a resistor and a voltage source which is connected to IN
        /// node and a ground node
        /// </summary>
        static Circuit CreateResistorCircuit(double current, double resistance)
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "IN", current),
                new Resistor("R1", "IN", "0", resistance));
            return ckt;
        }

        [Test]
        public void When_ResistorOP_Expect_Reference()
        {
            /*
             * A circuit contains a current source 10A and resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through resistor is 10A
             * 2) the voltage across the current source is 1000V
             */
            var ckt = CreateResistorCircuit(10, 1.0e3);

            // Create simulation, exports and references
            var op = new OP("op");
            var exports = new IExport<double>[2];
            exports[0] = new RealPropertyExport(op, "I1", "v");
            exports[1] = new RealPropertyExport(op, "R1", "i");
            double[] references =
            {
                -10.0e3,
                10
            };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a circuit with a current source and N resistors in series to ground
        /// </summary>
        /// <param name="count">Number of resistors to ground</param>
        /// <param name="current">Current (A)</param>
        /// <param name="resistance">Resistance (Ohm)</param>
        /// <returns></returns>
        static Circuit CreateResistorsInSeriesCircuit(int count, double current, double resistance)
        {
            Assert.IsTrue(count > 1);
            var ckt = new Circuit(
                new CurrentSource("I1", "IN", "0", current),
                new Resistor("R1", "IN", "B1", resistance),
                new Resistor($"R{count}", $"B{count - 1}", "0", resistance)
                );
            for (var i = 2; i <= count - 1; i++)
            {
                ckt.Add(new Resistor($"R{i}", $"B{i - 1}", $"B{i}", resistance));
            }
            return ckt;
        }

        [Test]
        public void When_SeriesResistorOP_Expect_Reference()
        {
            /*
             * A circuit contains a current source 100A and 500 resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through each resistor is 100A
             * 2) a voltage across the current source is 500000V (currentInAmp * resistanceInOhms * resistorCount)
             */
            var currentInAmp = 100;
            var resistanceInOhms = 10;
            var resistorCount = 500;
            var ckt = CreateResistorsInSeriesCircuit(resistorCount, currentInAmp, resistanceInOhms);
            var op = new OP("op");

            // Create exports
            var exports = new List<IExport<double>>();
            for (var i = 1; i <= resistorCount; i++)
                exports.Add(new RealPropertyExport(op, $"R{i}", "i"));
            exports.Add(new RealPropertyExport(op, "I1", "v"));

            // Add references
            var references = new List<double>();
            for (var i = 1; i <= resistorCount; i++)
                references.Add(-100);
            references.Add(-currentInAmp * resistanceInOhms * resistorCount);

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ResistorTransient_Expect_NoException()
        {
            // Found by Marcin Golebiowski
            var ckt = new Circuit(
                new CurrentSource("I1", "1", "0", new Pulse(0, 6, 3.69e-6, 41e-9, 41e-9, 3.256e-6, 6.52e-6)),
                new Resistor("R1", "1", "0", 10.0)
            );

            var tran = new Transient("tran", 1e-8, 1e-5);
            tran.Run(ckt);
        }

        [Test]
        public void When_Cloned_Expect_Reference()
        {
            // Let's check cloning of entities here.
            var isrc = (CurrentSource)new CurrentSource("I1", "A", "B", 1.0)
                .SetParameter("ac", new double[] { 1.0, 2.0 })
                .SetParameter("waveform", new Pulse(0.0, 1.0, 1e-9, 1e-8, 1e-7, 1e-6, 1e-5));

            // Clone the entity
            var clone = (CurrentSource)((SpiceSharp.ICloneable)isrc).Clone();

            // Change some stuff (should not be reflected in the clone)
            isrc.GetProperty<IWaveformDescription>("waveform").SetParameter("v2", 2.0);

            // Check
            Assert.AreEqual(isrc.Name, clone.Name);
            var origNodes = isrc.Nodes;
            var cloneNodes = clone.Nodes;
            Assert.AreEqual(origNodes[0], cloneNodes[0]);
            Assert.AreEqual(origNodes[1], cloneNodes[1]);
            var waveform = (Pulse)clone.GetProperty<IWaveformDescription>("waveform");
            Assert.AreEqual(0.0, waveform.InitialValue, 1e-12);
            Assert.AreEqual(1.0, waveform.PulsedValue, 1e-12);
            Assert.AreEqual(2.0, isrc.GetProperty<IWaveformDescription>("waveform").GetProperty<double>("v2"));
            Assert.AreEqual(1.0, waveform.GetProperty<double>("v2"));
            Assert.AreEqual(1e-5, waveform.GetProperty<double>("per"), 1e-12);
        }

        /*
        [TestCaseSource(nameof(Biasing))]
        public void When_BiasingBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new BiasingBehavior("I1", context.Value);
            ((IBiasingBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IBiasingSimulationState>().Solver, expected);
        }
        [TestCaseSource(nameof(Frequency))]
        public void When_FrequencyBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, Complex[] expected)
        {
            var behavior = new FrequencyBehavior("I1", context.Value);
            ((IFrequencyBehavior)behavior).InitializeParameters();
            ((IFrequencyBehavior)behavior).Load();
            Check.Solver(context.Value.GetState<IComplexSimulationState>().Solver, expected);
            Check.Complex(behavior.ComplexCurrent, expected[expected.Length - 1]);
        }
        [TestCaseSource(nameof(Accept))]
        public void When_AcceptBehavior_Expect_Reference(Proxy<IComponentBindingContext> context, double[] expected)
        {
            var behavior = new AcceptBehavior("I1", context.Value);
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
                    .Nodes("a", "b").Bias()
                    .Parameter(new IndependentSourceParameters(1.0));
                yield return new TestCaseData(context.AsProxy(), new[] 
                    { 
                        double.NaN, double.NaN, -1.0,
                        double.NaN, double.NaN, 1.0 })
                    .SetName("{m}(DC=1)");

                // Using SourceFactor
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").Bias(null, i => i.SourceFactor.Returns(0.5))
                    .Parameter(new IndependentSourceParameters(-3.0));
                yield return new TestCaseData(context.AsProxy(), 
                    new[] { 
                        double.NaN, double.NaN, 1.5,
                        double.NaN, double.NaN, -1.5 })
                    .SetName("{m}(SourceFactor=0.5)");
            }
        }
        public static IEnumerable<TestCaseData> Frequency
        {
            get
            {
                IComponentBindingContext context;

                // Simple AC magnitude 1
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").Bias().Frequency(1)
                    .Parameter(new IndependentSourceParameters(1))
                    .Parameter(new IndependentSourceFrequencyParameters(1, 0));
                yield return new TestCaseData(context.AsProxy(), 
                    new Complex[] { 
                        double.NaN, double.NaN, -1.0,
                        double.NaN, double.NaN, 1.0 })
                    .SetName("{m}(AC 1 0)");

                // Simple AC magnitude 1
                var v = new Complex(Math.Cos(0.5 * Math.PI), Math.Sin(0.5 * Math.PI)) * 0.5;
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").Bias().Frequency(1)
                    .Parameter(new IndependentSourceParameters(1))
                    .Parameter(new IndependentSourceFrequencyParameters { AcMagnitude = 0.5, AcPhase = 90 });
                yield return new TestCaseData(context.AsProxy(), 
                    new Complex[] {
                        double.NaN, double.NaN, -v,
                        double.NaN, double.NaN, v })
                    .SetName("{m}(AC 0.5 90)");
            }
        }
        public static IEnumerable<TestCaseData> Accept
        {
            get
            {
                IComponentBindingContext context;

                // Transient analysis with waveform
                context = Substitute.For<IComponentBindingContext>()
                    .Nodes("a", "b").Bias().Transient(0.3, 0.01).Parameter(new IndependentSourceParameters { Waveform = new Sine(0, 1, 1) });
                var v = Math.Sin(0.3 * 2 * Math.PI);
                yield return new TestCaseData(context.AsProxy(), 
                    new[] { 
                        double.NaN, double.NaN, -v,
                        double.NaN, double.NaN, v }).SetName("{m}(Sine)");
            }
        }
        public static IEnumerable<TestCaseData> Rules
        {
            get
            {
                ComponentRuleParameters parameters;

                yield return new TestCaseData(
                    new Circuit(new CurrentSource("I1", "in", "0", 1.0)),
                    new ComponentRules(parameters = new ComponentRuleParameters(), new FloatingNodeRule(parameters.Variables.Ground)),
                    new[] { typeof(FloatingNodeRuleViolation) });
            }
        }
        */
    }
}
