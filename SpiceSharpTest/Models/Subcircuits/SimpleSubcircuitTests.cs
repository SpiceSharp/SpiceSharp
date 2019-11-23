using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SimpleSubcircuitTests : Framework
    {
        [Test]
        public void When_SimpleSubcircuit_Expect_Reference()
        {
            // Define the subcircuit
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)),
                "a", "b");

            // Define the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 5.0),
                new Subcircuit("X1", subckt).Connect("in", "out"));

            // Simulate the circuit
            var op = new OP("op");
            IExport<double>[] exports = new[] { new RealVoltageExport(op, "out") };
            IEnumerable<double> references = new double[] { 2.5 };
            AnalyzeOp(op, ckt, exports, references);
        }

        [Test]
        public void When_RecursiveSubcircuit_Expect_Reference()
        {
            // Define the subcircuit
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "c", 1e3)),
                "a", "c");

            // Define the parent subcircuit
            var subckt2 = new SubcircuitDefinition(new Circuit(
                new Subcircuit("X1", subckt).Connect("x", "y"),
                new Subcircuit("X2", subckt).Connect("y", "z")),
                "x", "y", "z");

            // Define the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0),
                new Subcircuit("X1", subckt2).Connect("in", "out", "0"));

            // Simulate the circuit
            var op = new OP("op");
            IExport<double>[] exports = new[] { new RealVoltageExport(op, "out") };
            IEnumerable<double> references = new double[] { 1.0 };
            AnalyzeOp(op, ckt, exports, references);
        }
    }
}
