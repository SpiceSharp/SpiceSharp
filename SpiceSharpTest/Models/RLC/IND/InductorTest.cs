using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpiceSharpTest.Models.IND
{
    [TestClass]
    public class InductorTest : Framework
    {
        [TestMethod]
        public void LCTank_Transient()
        {
            /*
             * Test for LC tank circuit, an inductor parallel with a capacitor will resonate at a frequency of 1/(2*pi*sqrt(LC))
             */
            // Build circuit
            double capacitance = 1e-3;
            double inductance = 1e-6;
            double initialCurrent = 1e-3;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Inductor("L1", "OUT", "0", inductance),
                new Capacitor("C1", "OUT", "0", capacitance)
                );
            ckt.Nodes.IC["OUT"] = 0.0;
            ckt.Objects["L1"].Parameters.Set("ic", initialCurrent);

            /*
             * WARNING: An LC tank is a circuit that oscillates and does not converge. This causes the global truncation error
             * to increase as time goes by!
             * For this reason, the absolute tolerance is made a little bit less strict.
             */
            AbsTol = 1e-9;

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 1e-3);
            tran.MaxStep = 1e-7;

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateExport("C1", "v");
            };

            // Create reference function
            double amplitude = Math.Sqrt(inductance / capacitance) * initialCurrent;
            double omega = 1.0 / Math.Sqrt(inductance * capacitance);
            Func<double, double>[] references = { (double t) => -amplitude * Math.Sin(omega * t) };

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
        }
    }
}
