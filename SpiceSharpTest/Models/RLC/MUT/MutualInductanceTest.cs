using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Circuits;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.MUT
{
    [TestClass]
    public class MutualInductanceTest : Framework
    {
        [TestMethod]
        public void TransformerLoad_Transient()
        {
            /*
             * Step function generator connect to a resistor-inductor in series, coupled to an inductor shunted by another resistor.
             * This linear circuit can be solved analytically. The result may deviate because of truncation errors (going to discrete
             * time points).
             */
            // Create circuit
            double r1 = 100.0;
            double r2 = 500.0;
            double l1 = 10e-3;
            double l2 = 2e-3;
            double k = 0.693;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", 1.0), // new Pulse(0, 1, 10e-6, 1e-12, 1e-12, 1, 2)),
                new Resistor("R1", "IN", "1", r1),
                new Inductor("L1", "1", "0", l1),
                new Inductor("L2", "OUT", "0", l2),
                new Resistor("R2", "OUT", "0", r2),
                new MutualInductance("M1", "L1", "L2", k)
                );
            ckt.Nodes.IC["1"] = 0;
            ckt.Objects["L1"].Parameters.Set("ic", 0);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 1e-4);
            tran.MaxStep = 1e-6;

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateVoltageExport("OUT");
            };

            // Create references
            double mut = k * Math.Sqrt(l1 * l2);
            double a = l1 * l2 - mut * mut;
            double b = r1 * l2 + r2 * l1;
            double c = r1 * r2;
            double D = Math.Sqrt(b * b - 4 * a * c);
            double invtau1 = (-b + D) / (2.0 * a);
            double invtau2 = (-b - D) / (2.0 * a);
            double factor = mut * r2 / a / (invtau1 - invtau2);
            Func<double, double>[] references = {  (double t) => factor * (Math.Exp(t * invtau1) - Math.Exp(t * invtau2)) };

            // Increase the allowed threshold
            // It should also be verfied that the error decreases if the maximum timestep is decreased
            AbsTol = 1.5e-3;

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
        }
    }
}
