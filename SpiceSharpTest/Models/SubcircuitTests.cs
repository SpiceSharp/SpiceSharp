using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
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
                new Subcircuit("X1", new Circuit(
                    new Resistor("R1", "a", "b", 1e3),
                    new Resistor("R2", "b", "0", 1e3)
                    ), "a", "b").Connect("in", "out")
                );

            var op = new OP("op");
            var exports = new[] { new RealVoltageExport(op, "out") };
            AnalyzeOp(op, ckt, exports, new[] { 0.5 });
            DestroyExports(exports);
        }

        [Test]
        public void When_SubcircuitSameName_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1e3),
                new Subcircuit("X1", new Circuit(
                    new Resistor("R1", "a", "b", 1e3)
                    ), "a", "b").Connect("out", "0")
                );

            var op = new OP("op");
            var exports = new[] { new RealVoltageExport(op, "out") };
            AnalyzeOp(op, ckt, exports, new[] { 0.5 });
            DestroyExports(exports);
        }

        [Test]
        public void When_SubcircuitWithInternalNodes_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", new Circuit(
                    new Resistor("R1", "a", "b", 1e3),
                    new Resistor("R2", "b", "c", 1e3),
                    new Resistor("R3", "c", "0", 1e3)
                    ), "a", "b").Connect("in", "out")
                );

            var op = new OP("op");
            var exports = new[] { new RealVoltageExport(op, "out"), new RealVoltageExport(op, "X1/c") };
            AnalyzeOp(op, ckt, exports, new[] { 2.0 / 3.0, 1.0 / 3.0 });
            DestroyExports(exports);
        }

        [Test]
        public void When_SubcircuitWithGlobalModel_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new DiodeModel("DM1").SetParameter("is", 2.5e-9),
                new Subcircuit("X1", new Circuit(
                    new Resistor("R1", "rect_in", "rect_out", 1e3),
                    new Diode("D1", "rect_out", "0", "DM1")
                    ), "rect_in", "rect_out").Connect("in", "out"),
                new Capacitor("C1", "out", "0", 1e-9)
                );

            var ac = new AC("ac 1");
            ac.Run(ckt);
        }

        [Test]
        public void When_SubcircuitShared_Expect_Reference()
        {
            var subckt = new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)
                );

            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", subckt, "a", "b").Connect("in", "int" ),
                new Subcircuit("X2", subckt, "a", "b").Connect("int", "out")
                );

            var op = new OP("op");
            op.AfterSetup += (sender, args) =>
            {
                var eb = op.EntityBehaviors["X1"].GetParameter<BehaviorContainerCollection>("b");
                eb["R1"].SetPrincipalParameter(6e3);
                eb["R2"].SetPrincipalParameter(6e3);
            };
            var exports = new[] { new RealVoltageExport(op, "out") };
            AnalyzeOp(op, ckt, exports, new[] { 0.2 });
            DestroyExports(exports);
        }

        [Test]
        public void When_SubcircuitNotShared_Expect_Reference()
        {
            var subckt = new Circuit(
                new Resistor("R1", "a", "b", 1e3) { LinkParameters = false },
                new Resistor("R2", "b", "0", 1e3) { LinkParameters = false }
                );

            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", subckt, "a", "b").Connect("in", "int"),
                new Subcircuit("X2", subckt, "a", "b").Connect("int", "out")
                );

            var op = new OP("op");
            op.AfterSetup += (sender, args) =>
            {
                var eb = op.EntityBehaviors["X1"].GetParameter<BehaviorContainerCollection>("b");
                eb["R1"].SetPrincipalParameter(6e3);
                eb["R2"].SetPrincipalParameter(6e3);
            };
            var exports = new[] { new RealVoltageExport(op, "out") };
            AnalyzeOp(op, ckt, exports, new[] { 0.1 });
            DestroyExports(exports);
        }
    }
}
