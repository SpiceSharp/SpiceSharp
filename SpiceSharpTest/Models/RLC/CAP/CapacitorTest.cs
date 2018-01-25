using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.RLC.CAP
{
    [TestClass]
    public class CapacitorTest : Framework
    {
        [TestMethod]
        public void LowpassRC_DC()
        {
            /*
             * Lowpass RC circuit
             * The capacitor should act like an open circuit
             */
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", 1.0),
                new Resistor("R1", "IN", "OUT", 10e3),
                new Capacitor("C1", "OUT", "0", 1e-6));

            // Create simulation
            OP op = new OP("op");

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            op.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = op.CreateVoltageExport("OUT");
            };

            // Create references
            double[] references = { 1.0 };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
        }

        [TestMethod]
        public void LowpassRC_Transient()
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
                new Voltagesource("V1", "IN", "0", dcVoltage)
                );
            ckt.Nodes.IC["OUT"] = 0.0;

            // Create simulation, exports and references
            Transient tran = new Transient("tran", 1e-8, 10e-6);
            Func<State, double>[] exports = new Func<State, double>[1];
            Func<double, double>[] references = { (double t) => dcVoltage * (1.0 - Math.Exp(-t / tau)) };
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateExport("C1", "v");
            };

            // Run
            AnalyzeTransient(tran, ckt, exports, references);
        }

        [TestMethod]
        public void LowpassRC_AC()
        {
            /*
             * Lowpass RC filter in the frequency domain should have a single pole at s=-2pi*R*C
             */
            // Create circuit
            double resistance = 1e3;
            double capacitance = 1e-6;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", 0.0),
                new Resistor("R1", "IN", "OUT", resistance),
                new Capacitor("C1", "OUT", "0", capacitance)
                );
            ckt.Objects["V1"].Parameters.SetProperty("acmag", 1.0);

            // Create simulation
            AC ac = new AC("ac", "dec", 10, 0.1, 1.0e6);

            // Create exports
            Func<State, Complex>[] exports = new Func<State, Complex>[1];
            ac.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = ac.CreateAcExport("C1", "v");
            };

            // Create references
            Func<double, Complex>[] references = { (double f) => 1.0 / new Complex(1.0, resistance * capacitance * 2 * Math.PI * f) };

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
