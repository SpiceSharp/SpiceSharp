using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;

namespace SpiceSharpTest
{
    public static class Test
    {
        /// <summary>
        /// The tolerance for DC analysis
        /// </summary>
        public static double DCTolerance = 1e-6;

        /// <summary>
        /// Run a DC simulation and validate the model
        /// </summary>
        /// <param name="dc">The DC simulation</param>
        /// <param name="vsrc">The voltage source of which the current is measured</param>
        /// <param name="expected">The expected currents</param>
        public static void TestDCCurrent(Circuit ckt, DC dc, string vsrc, double[] expected, double reltol, double abstol)
        {
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var v = (Voltagesource)data.GetObject(vsrc);
                double cvoltage = data.GetVoltage("OUT");
                double current = v.GetCurrent(ckt);
                double expect = expected[index];
                double tol = Math.Abs(current) * reltol + abstol;

                Assert.AreEqual(expect, current, tol);
            };
            ckt.Simulate(dc);
        }
    }
}
