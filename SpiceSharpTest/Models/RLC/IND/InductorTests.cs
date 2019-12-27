using System;
using System.Numerics;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using NUnit.Framework;
using SpiceSharp.Diagnostics.Validation;

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

        /*
         * Parallel inductors cause a singular exception, because they are short-circuited at DC. If multiple
         * short-circuit components are in parallel, the simulator can't find the current through each of the
         * inductors, and this shows as a singular matrix.
        [Test]
        public void When_InductorMultiplierSmallSignal_Expect_Reference()
        {
            // Create circuit
            var cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "out", "0", 1e3));
            ParallelSeries(cktReference, name => new Inductor(name, "", "", 1e-6), "in", "out", 2, 1);
            var cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new Inductor("L1", "in", "out", 1e-6).SetParameter("m", 2.0).SetParameter("n", 1.0),
                new Resistor("R1", "out", "0", 1e3));

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(0.1, 1e6, 10));
            var exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "out") };

            // Run test
            Compare(ac, cktReference, cktActual, exports);
            DestroyExports(exports);
        }
        */

        [Test]
        public void When_ShortedValidation_Expect_ShortCircuitComponentException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Inductor("L1", "in", "in", 1e-9));
            Assert.Throws<ShortCircuitComponentException>(() => ckt.Validate());
        }

        [Test]
        public void When_VoltageLoopValidation_Expect_VoltageLoopException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Inductor("L1", "in", "0", 1e-9));
            Assert.Throws<VoltageLoopException>(() => ckt.Validate());
        }
    }
}
