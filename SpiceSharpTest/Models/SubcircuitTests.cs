using NUnit.Framework;
using SpiceSharp;
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
        public void When_SubcircuitSameName_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1e3),
                new Subcircuit("X1", new[] { "a", "b" }, new Circuit(
                    new Resistor("R1", "a", "b", 1e3)
                    ), new[] { "out", "0" })
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

        [Test]
        public void When_SubcircuitWithGlobalModel_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new DiodeModel("DM1").SetParameter("is", 2.5e-9),
                new Subcircuit("X1", new[] { "rect_in", "rect_out" }, new Circuit(
                    new Resistor("R1", "rect_in", "rect_out", 1e3),
                    new Diode("D1", "rect_out", "0", "DM1")
                    ), new[] { "in", "out" }),
                new Capacitor("C1", "out", "0", 1e-9)
                );

            var ac = new AC("ac 1");
            ac.Run(ckt);
        }
    }
}
