using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.RLC.RES
{
    [TestClass]
    public class ResistorTest : Framework
    {
        /// <summary>
        /// Create a voltage source shunted by a resistor
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance">Resistance</param>
        /// <returns></returns>
        static Circuit CreateResistorDcCircuit(double dcVoltage, double resistance)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "0", resistance)
            );
            return ckt;
        }

        [TestMethod]
        public void SingleResistorOnDcVoltage_Op()
        {
            /*
             * A circuit contains a DC voltage source 10V and resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through resistor is 0.01 A (Ohms law)
             */
            var ckt = CreateResistorDcCircuit(10, 1000);

            // Create simulation, exports and references
            OP op = new OP("op");
            Func<RealState, double>[] exports = new Func<RealState, double>[1];
            op.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = op.CreateExport("R1", "i");
            };
            double[] references = { 0.01 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
        }

        /* NOTE: needs more work
        [TestMethod]
        public void SingleResistorOnDcVoltage_Ac()
        {
            /*
             * A circuit contains a DC voltage source 10V and resistor 1000 Ohms
             * The test verifies that after AC simulation:
             * 1) a current through resistor is 0.01 A (Ohms law)
             * /
            var ckt = CreateResistorDcCircuit(10, 1000);

            // Create simulation, exports and references
            AC ac = new AC("ac");
            Func<State, double>[] exports = { ac.CreateExport("R1", "ir"), ac.CreateExport("R1", "ii") };

            ckt.Method = new Trapezoidal();
            AC simulation = new AC("Simulation", "lin", 10, 1, 1001);
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var R1 = ckt.Objects["R_1"];
                var current = ((SpiceSharp.Components.Resistor)R1).GetCurrent(ckt);
                Assert.That.AreEqualWithTol(0.0, current, 0, 1e-8);
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
        }
        */

        /// <summary>
        /// Create a voltage divider circuit
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance1">Resistance 1</param>
        /// <param name="resistance2">Resistance 2</param>
        /// <returns></returns>
        static Circuit CreateVoltageDividerResistorDcCircuit(double dcVoltage, double resistance1, double resistance2)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "OUT", resistance1),
                new Resistor("R2", "OUT", "0", resistance2)
            );
            return ckt;
        }

        [TestMethod]
        public void VoltageDividerDcVoltage_Op()
        {
            /*
             * A circuit contains a DC voltage source 100V and two resistors in series (1 and 3 Ohms). 
             * It's a voltage divider.
             * The test verifies that after OP simulation:
             * 1) voltage at "OUT" node is ((R1 + R2) / (R1 * R2)) * V 
             */
            var ckt = CreateVoltageDividerResistorDcCircuit(100, 3, 1);

            // Create simulation, exports and references
            OP op = new OP("op");
            Func<RealState, double>[] exports = new Func<RealState, double>[1];
            op.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = op.CreateExport("R2", "v");
            };
            double[] references = { 100 * 1 / (3 + 1) };

            // Run
            AnalyzeOp(op, ckt, exports, references);
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
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "0", resistance1),
                new Resistor("R2", "IN", "0", resistance2)
            );
            return ckt;
        }

        [TestMethod]
        public void ParallelResistorsDcVoltage_Op()
        {
            /*
             * A circuit contains a DC voltage source 100V and two resistors in parallel (1 and 2 Ohms). 
             * The test verifies that after OP simulation:
             * 1) Current through resistors is 50 and 100A respectively
             */
            double dc = 100;
            double r1 = 2.0;
            double r2 = 1.0;
            var ckt = CreateParallelResistorsDcCircuit(dc, r1, r2);

            // Create simulation, exports and references
            OP op = new OP("op");
            Func<RealState, double>[] exports = new Func<RealState, double>[2];
            double[] references = { dc / r1, dc / r2 };
            op.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = op.CreateExport("R1", "i");
                exports[1] = op.CreateExport("R2", "i");
            };

            // Run
            AnalyzeOp(op, ckt, exports, references);
        }
    }
}
