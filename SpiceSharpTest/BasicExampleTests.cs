using System;
using System.Globalization;
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
            // Create a DC simulation that sweeps V1 from -1V to 1V in steps of 100mV
            DC dc = new DC("DC 1", "V1", -1.0, 1.0, 0.2);

            // Catch exported data
            dc.OnExportSimulationData += (sender, args) =>
            {
                double input = args.GetVoltage("in");
                double output = args.GetVoltage("out");
                Console.WriteLine($@"{input:G3} V : {output:G3} V");
            };
            dc.Run(ckt);
            // </example01_simulate>
        }

        [Test]
        public void When_BasicCircuitExports_Expect_NoException()
        {
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 2.0e4)
            );

            // <example01_simulate2>
            // Create a DC simulation that sweeps V1 from -1V to 1V in steps of 100mV
            DC dc = new DC("DC 1", "V1", -1.0, 1.0, 0.2);

            // Create exports
            Export<double> inputExport = new RealVoltageExport(dc, "in");
            Export<double> outputExport = new RealVoltageExport(dc, "out");
            Export<double> currentExport = new RealPropertyExport(dc, "V1", "i");

            // Catch exported data
            dc.OnExportSimulationData += (sender, args) =>
            {
                Console.WriteLine($@"{inputExport.Value:G3} V : {outputExport.Value:G3} V, {currentExport.Value:G3} A");
            };
            dc.Run(ckt);
            // </example01_simulate2>
        }

        [Test]
        public void When_BJTIVCharacteristic_Expect_NoException()
        {
            // Make the bipolar junction transistor
            var bjt = new BipolarJunctionTransistor("Q1");
            bjt.Connect("c", "b", "0", "0");
            var bjtmodel = new BipolarJunctionTransistorModel("example");
            bjtmodel.SetParameter("bf", 150.0);
            bjtmodel.SetParameter("is", 1.5e-14);
            bjtmodel.SetParameter("vaf", 30);
            bjt.SetModel(bjtmodel);

            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "b", 1e-3),
                new VoltageSource("V1", "c", "0", 0.0),
                bjt
                );

            // Sweep the base current and vce voltage
            DC dc = new DC("DC 1", new[]
            {
                new SweepConfiguration("I1", 0, 1e-3, 1e-4),
                new SweepConfiguration("V1", 0, 2, 0.1),                
            });
            
            // Export the collector current
            Export<double> currentExport = new RealPropertyExport(dc, "Q1", "cc");

            // Run the simulation
            dc.OnExportSimulationData += (sender, args) =>
            {
                Console.Write(currentExport.Value.ToString(CultureInfo.InvariantCulture));
                Console.Write(@", ");
            };
            dc.Run(ckt);
        }
    }
}
