using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Examples
{
    [TestFixture]
    public class NonlinearResistorTests
    {
        [Test]
        public void When_RunNonlinearResistor_Expect_NoException()
        {
            // <example_customcomponent_nonlinearresistor_test>
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "out", "0", 0.0),
                new NonlinearResistor("RNL1", "out", "0")
            );
            ckt.Entities["RNL1"].SetParameter("a", 2.0e3);
            ckt.Entities["RNL1"].SetParameter("b", 0.5);

            // Setup the simulation and export our current
            var dc = new DC("DC", "V1", -2.0, 2.0, 1e-2);
            var current = new RealPropertyExport(dc, "V1", "i");
            dc.ExportSimulationData += (sender, args) => Console.WriteLine(-current.Value);
            dc.Run(ckt);
            // </example_customcomponent_nonlinearresistor_test>
        }
    }
}
