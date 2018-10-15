using System;
using System.Globalization;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class TransmissionLineTests : Framework
    {
        [Test]
        public void When_LosslessTransmissionLineTransient_Expect_NoException()
        {
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(1, 5, 2e-6, 1e-9, 1e-9, 5e-6, 10e-6)),
                new Resistor("Rsource", "in", "a", 100),
                new LosslessTransmissionLine("T1", "a", "0", "b", "0", 50.0, 1e-6),
                new Resistor("Rload", "b", "0", 25)
            );
            ckt.Entities["T1"].SetParameter("reltol", 0.5);

            // Build the simulation
            var tran = new Transient("tran", 1e-6, 20e-6);
            var inputExport = new RealVoltageExport(tran, "a");
            var outputExport = new RealVoltageExport(tran, "b");
            tran.ExportSimulationData += (sender, args) =>
            {
                Console.Write(args.Time.ToString(CultureInfo.InvariantCulture) + ", ");
                Console.Write(inputExport.Value.ToString(CultureInfo.InvariantCulture) + ", ");
                Console.Write(outputExport.Value.ToString(CultureInfo.InvariantCulture) + "; ");
            };
            tran.Run(ckt);
        }
    }
}
