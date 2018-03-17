using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest
{
    [TestFixture]
    public class BasicExampleTests
    {
        [Test]
        public void When_BasicCircuit_Expect_NoException()
        {
            // <example01_build>
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 2.0e4)
                );
            // </example01_build>

            // <example01_simulate>
            DC dc = new DC("DC 1", "V1", -1.0, 1.0, 0.1);
            dc.OnExportSimulationData += (sender, args) =>
            {
                double input = args.GetVoltage("in");
                double output = args.GetVoltage("out");
                Console.WriteLine($@"{input:G3}V : {output:G3} V");
            };
            dc.Run(ckt);
            // </example01_simulate>
        }
    }
}
