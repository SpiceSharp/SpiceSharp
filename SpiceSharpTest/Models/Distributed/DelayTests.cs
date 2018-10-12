using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Distributed
{
    [TestFixture]
    public class DelayTests : Framework
    {
        [Test]
        public void When_DelayTransient_Expect_Reference()
        {
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-7, 1e-7, 1e-7, 1e-5, 2e-5)),
                new VoltageDelay("Delay1", "out", "0", "in", "0", 0.5e-5)
            );

            // Build the simulation
            var tran = new Transient("tran", 1e-7, 10e-5);
            var input = new RealVoltageExport(tran, "in");
            var output = new RealVoltageExport(tran, "out");
            tran.ExportSimulationData += (sender, args) =>
            {
                Console.Write(args.Time.ToString(CultureInfo.InvariantCulture) + ", " +
                              input.Value.ToString(CultureInfo.InvariantCulture) + ", " +
                              output.Value.ToString(CultureInfo.InvariantCulture) + "; ");
            };

            tran.Run(ckt);
        }
    }
}
