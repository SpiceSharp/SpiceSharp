using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SubcircuitTests : Framework
    {
        [Test]
        public void When_SubcircuitSimple_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", new[] { "a", "b" }, new Circuit(
                    new Resistor("R1", "a", "b", 1e3),
                    new Resistor("R2", "b", "0", 1e3)
                    ), new[] { "in", "out" })
                );

            var op = new OP("op");
            AnalyzeOp(op, ckt, new[] { new RealVoltageExport(op, "out") }, new[] { 0.5 });
        }

        [Test]
        public void When_SubcircuitWithInternalNodes_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", new[] { "a", "b" }, new Circuit(
                    new Resistor("R1", "a", "b", 1e3),
                    new Resistor("R2", "b", "c", 1e3),
                    new Resistor("R3", "c", "0", 1e3)
                    ), new[] { "in", "out" })
                );

            var op = new OP("op");
            AnalyzeOp(op, ckt, new[] { new RealVoltageExport(op, "out"), new RealVoltageExport(op, "X1/c") }, new[] { 2.0 / 3.0, 1.0 / 3.0 });
        }
    }
}
