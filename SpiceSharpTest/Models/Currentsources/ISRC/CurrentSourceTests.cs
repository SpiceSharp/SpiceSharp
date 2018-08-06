using System.Collections.Generic;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentSourceTests : Framework
    {
        /// <summary>
        /// Creates a circuit with a resistor and a voltage source which is connected to IN
        /// node and a ground node
        /// </summary>
        static Circuit CreateResistorCircuit(double current, double resistance)
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "IN", current),
                new Resistor("R1", "IN", "0", resistance));
            return ckt;
        }

        [Test]
        public void When_CurrentSource_ResistorOperatingPoint_Expect_Reference()
        {
            /*
             * A circuit contains a current source 10A and resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through resistor is 10A
             * 2) the voltage across the current source is 1000V
             */
            var ckt = CreateResistorCircuit(10, 1.0e3);

            // Create simulation, exports and references
            var op = new OP("op");
            var exports = new Export<double>[2];
            exports[0] = new RealPropertyExport(op, "I1", "v");
            exports[1] = new RealPropertyExport(op, "R1", "i");
            double[] references =
            {
                -10.0e3,
                10
            };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
        }

        /// <summary>
        /// Create a circuit with a current source and N resistors in series to ground
        /// </summary>
        /// <param name="count">Number of resistors to ground</param>
        /// <param name="current">Current (A)</param>
        /// <param name="resistance">Resistance (Ohm)</param>
        /// <returns></returns>
        static Circuit CreateResistorsInSeriesCircuit(int count, double current, double resistance)
        {
            Assert.IsTrue(count > 1);
            var ckt = new Circuit(
                new CurrentSource("I1", "IN", "0", current),
                new Resistor("R1", "IN", "B1", resistance),
                new Resistor($"R{count}", $"B{count - 1}", "0", resistance)
                );
            for (var i = 2; i <= count - 1; i++)
            {
                ckt.Objects.Add(new Resistor($"R{i}", $"B{i - 1}", $"B{i}", resistance));
            }
            return ckt;
        }

        [Test]
        public void When_CurrentSource_ResistorsInSeriesOperatingPoint_Expect_Reference()
        {
            /*
             * A circuit contains a current source 100A and 500 resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through each resistor is 100A
             * 2) a voltage across the current source is 500000V (currentInAmp * resistanceInOhms * resistorCount)
             */
            var currentInAmp = 100;
            var resistanceInOhms = 10;
            var resistorCount = 500;
            var ckt = CreateResistorsInSeriesCircuit(resistorCount, currentInAmp, resistanceInOhms);
            var op = new OP("op");

            // Create exports
            var exports = new List<Export<double>>();
            for (var i = 1; i <= resistorCount; i++)
                exports.Add(new RealPropertyExport(op, $"R{i}", "i"));
            exports.Add(new RealPropertyExport(op, "I1", "v"));
            
            // Add references
            var references = new List<double>();
            for (var i = 1; i <= resistorCount; i++)
                references.Add(-100);
            references.Add(-currentInAmp * resistanceInOhms * resistorCount);
            
            // Run test
            AnalyzeOp(op, ckt, exports, references);
        }
    }
}
