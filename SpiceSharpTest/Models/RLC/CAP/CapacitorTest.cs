using System;
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
        public void Circuit_RC_Transient()
        {
            /*
             * A test for a RC circuit (DC voltage, resistor, capacitor)
             * An init voltage on capacitor is 0V
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
            AnalyseTransient(tran, ckt, exports, references);
        }
    }
}
