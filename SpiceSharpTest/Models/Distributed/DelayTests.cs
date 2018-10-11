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
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-7, 1e-7, 1e-7, 10e-6, 20e-6)),
                new Delay("Delay1", "in", "out", 5e-6)
            );

            // Build the simulation and exports
            var tran = new Transient("tran", 1e-6, 50e-6);
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
