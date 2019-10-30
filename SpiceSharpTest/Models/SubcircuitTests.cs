using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Numerics;

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
                var eb = op.EntityBehaviors["X1"].GetParameter<BehaviorContainerCollection[]>()[0];
                eb["R1"].SetParameter(6e3);
                eb["R2"].SetParameter(6e3);
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
                var eb = op.EntityBehaviors["X1"].GetParameter<BehaviorContainerCollection[]>()[0];
                eb["R1"].SetParameter(6e3);
                eb["R2"].SetParameter(6e3);
            };
            var exports = new[] { new RealVoltageExport(op, "out") };
            AnalyzeOp(op, ckt, exports, new[] { 0.1 });
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelRC_Expect_Reference()
        {
            /*
             * It doesn't really make sense to load resistors and capacitors in parallel,
             * their loading methods are incredibly short and cheap. Multithreading would only become more
             * interesting for longer loading methods, such as the BSIM models.
             * 
             * This example actually works with reduced performance, because it creates new objects to
             * avoid concurrent writes and it takes some overhead to create the multiple tasks every
             * time the matrix is loaded (which is very often). The performance benefits will only 
             * become apparent if the CPU work is the limiting factor.
             * 
             * This is just a test to check that the multithreading works correctly.
             */
            int count = 1000;
            var sub = new Subcircuit("X1", new[] { new Circuit(), new Circuit() }, "in", "out");
            sub.Connect("in", "out");
            string input = "in";
            for (var i = 1; i <= count; i++)
            {
                string output = "out";
                if (i < count)
                    output += i;
                int task = i <= count / 2 ? 0 : 1;
                sub.Entities[task].Add(new Resistor("R" + i, input, output, 0.5));
                sub.Entities[task].Add(new Capacitor("C" + i, output, "0", 1e-6));
                input = output;
            }
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                sub
                );
            ckt["X1"].Parameters.Add(
                new SpiceSharp.Components.SubcircuitBehaviors.BiasingParameters()
                    .SetParameter("biasing.load", true)
                    .SetParameter("biasing.solve", true)
                    );
            ckt["X1"].Parameters.Add(
                new SpiceSharp.Components.SubcircuitBehaviors.FrequencyParameters().SetParameter("frequency.load", true));

            // Build the simulation
            var ac = new AC("ac", new LinearSweep(0, 1, 3));
            double[] reference = new[] { 1.00000000000083, 0, 0.80621270646045, -0.441463380433918, 0.469677271946689, -0.595014538138315 };

            // Track the current values
            Complex[] current = new Complex[3];
            int index = 0;
            void TrackCurrent(object sender, ExportDataEventArgs args)
            {
                var actual = args.GetComplexVoltage("out");
                Assert.AreEqual(reference[index++], actual.Real, 1e-12);
                Assert.AreEqual(reference[index++], actual.Imaginary, 1e-12);
            }
            ac.ExportSimulationData += TrackCurrent;
            ac.Run(ckt);
            ac.ExportSimulationData -= TrackCurrent;
        }
    }
}
