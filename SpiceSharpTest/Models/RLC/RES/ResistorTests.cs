using NUnit.Framework;
using System;
using System.Numerics;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class ResistorTests : Framework
    {
        /// <summary>
        /// Create a voltage source shunted by a resistor
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance">Resistance</param>
        /// <returns></returns>
        static Circuit CreateResistorDcCircuit(double dcVoltage, double resistance)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "0", resistance)
            );
            return ckt;
        }

        [Test]
        public void When_ResistorOperatingPoint_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 10V and resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through resistor is 0.01 A (Ohms law)
             */
            var ckt = CreateResistorDcCircuit(10, 1000);

            // Create simulation, exports and references
            var op = new OP("op");
            var exports = new Export<double>[1];
            exports[0] = new RealCurrentExport(op, "V1");
            double[] references = { -0.01 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ResistorDividerSmallSignal_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 10V and resistor 1000 Ohms
             * The test verifies that after AC simulation:
             * 1) a current through resistor is 0.01 A (Ohms law)
             */
            var ckt = CreateResistorDcCircuit(10, 1000);
            ckt["V1"].SetParameter("acmag", 1.0);

            // Create simulation, exports and references
            var ac = new AC("ac", new LinearSweep(1.0, 10001, 10));
            Export<Complex>[] exports = { new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { f => 1e-3 };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a voltage divider circuit
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance1">Resistance 1</param>
        /// <param name="resistance2">Resistance 2</param>
        /// <returns></returns>
        static Circuit CreateVoltageDividerResistorDcCircuit(double dcVoltage, double resistance1, double resistance2)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "OUT", resistance1),
                new Resistor("R2", "OUT", "0", resistance2)
            );
            return ckt;
        }

        [Test]
        public void When_ResistorDividerOperatingPoint_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 100V and two resistors in series (1 and 3 Ohms). 
             * It's a voltage divider.
             * The test verifies that after OP simulation:
             * 1) voltage at "OUT" node is ((R1 + R2) / (R1 * R2)) * V 
             */
            var ckt = CreateVoltageDividerResistorDcCircuit(100, 3, 1);

            // Create simulation, exports and references
            var op = new OP("op");
            Export<double>[] exports = { new RealPropertyExport(op, "R2", "v") };
            double[] references = { 100.0 * 1.0 / (3.0 + 1.0) };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a voltage source shunted by two resistors
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance1">Resistance 1</param>
        /// <param name="resistance2">Resistance 2</param>
        /// <returns></returns>
        static Circuit CreateParallelResistorsDcCircuit(double dcVoltage, double resistance1, double resistance2)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "0", resistance1),
                new Resistor("R2", "IN", "0", resistance2)
            );
            return ckt;
        }

        [Test]
        public void When_ResistorParallelOperatingPoint_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 100V and two resistors in parallel (1 and 2 Ohms). 
             * The test verifies that after OP simulation:
             * 1) Current through resistors is 50 and 100A respectively
             */
            double dc = 100;
            var r1 = 2.0;
            var r2 = 1.0;
            var ckt = CreateParallelResistorsDcCircuit(dc, r1, r2);

            // Create simulation, exports and references
            var op = new OP("op");
            Export<double>[] exports = { new RealPropertyExport(op, "R1", "i"), new RealPropertyExport(op, "R2", "i") };
            double[] references = { dc / r1, dc / r2 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ResistorParameterAsked_Expect_Reference()
        {
            var ckt = new Circuit(
                new Resistor("R1", "out", "0", 1.0e3),
                new Resistor("R2", "in", "0", 10e3),
                new VoltageSource("V1", "in", "0", 1.0));

            // Create simulation exports and references
            var op = new OP("op");
            Export<double>[] exports =
            {
                new RealPropertyExport(op, "R1", "resistance"),
                new RealPropertyExport(op, "V1", "dc"),
            };
            double[] references = { 1.0e3, 1.0 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
