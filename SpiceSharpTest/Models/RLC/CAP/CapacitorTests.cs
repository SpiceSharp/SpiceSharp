using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CapacitorTests : Framework
    {
        [Test]
        public void When_Capacitor_LowpassRCOperatingPoint_Expect_Reference()
        {
            /*
             * Lowpass RC circuit
             * The capacitor should act like an open circuit
             */
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", "0", 1.0),
                new Resistor("R1", "IN", "OUT", 10e3),
                new Capacitor("C1", "OUT", "0", 1e-6));

            // Create simulation
            Op op = new Op("op");

            // Create exports
            Export<double>[] exports = new Export<double>[1];
            exports[0] = new RealVoltageExport(op, "OUT");

            // Create references
            double[] references = { 1.0 };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
        }

        [Test]
        public void When_Capacitor_LowpassRCTransient_Expect_Reference()
        {
            /*
             * A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
             * The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
             */
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Capacitor("C1", "OUT", "0", capacitance),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage)
                );
            ckt.Nodes.InitialConditions["OUT"] = 0.0;

            // Create simulation, exports and references
            Transient tran = new Transient("tran", 1e-8, 10e-6);
            Export<double>[] exports = { new RealPropertyExport(tran, "C1", "v") };
            Func<double, double>[] references = { (double t) => dcVoltage * (1.0 - Math.Exp(-t / tau)) };

            // Run
            AnalyzeTransient(tran, ckt, exports, references);
        }

        [Test]
        public void When_Capacitor_LowpassRCSmallSignal_Expect_Reference()
        {
            /*
             * Lowpass RC filter in the frequency domain should have a single pole at s=-2pi*R*C
             */
            // Create circuit
            double resistance = 1e3;
            double capacitance = 1e-6;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", "0", 0.0),
                new Resistor("R1", "IN", "OUT", resistance),
                new Capacitor("C1", "OUT", "0", capacitance)
                );
            ckt.Objects["V1"].ParameterSets.SetProperty("acmag", 1.0);

            // Create simulation
            Ac ac = new Ac("ac", new SpiceSharp.Simulations.Sweeps.DecadeSweep(0.1, 1.0e6, 10));

            // Create exports
            Export<Complex>[] exports = { new ComplexPropertyExport(ac, "C1", "v") };

            // Create references
            Func<double, Complex>[] references = { (double f) => 1.0 / new Complex(1.0, resistance * capacitance * 2 * Math.PI * f) };

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
