using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

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
            IEnumerable<double> references = [2.5];
            AnalyzeOp(op, ckt, exports, references);
        }

        [Test]
        public void When_SimpleSubcircuit_Measure_Subcircuit_Current_Expect_Reference()
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
            IExport<double>[] exports = new[] { new RealPropertyExport(op, new[] { "X1", "R1" }, "i") };
            IEnumerable<double> references = [5.0 / 2e3];
            AnalyzeOp(op, ckt, exports, references);
        }

        [Test]
        public void When_SimpleSubcircuit_Measure_Multi_Subcircuit_Current_Expect_Reference()
        {
            // Define the subcircuit
            var subckt1 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "c", 1e3),
                new VoltageSource("V1", "c", "0", 0.0)),
                "a");

            // just another wrapper
            var subckt2 = new SubcircuitDefinition(new Circuit(
                new Subcircuit("Vdiv", subckt1).Connect("in")),
                "in");

            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 5.0),
                new Subcircuit("X1", subckt2).Connect("in"));

            // Simulate the circuit
            var op = new OP("op");
            var exports = new IExport<double>[] {
                new RealPropertyExport(op, new[] { "X1", "Vdiv", "R1" }, "i"),
                new RealCurrentExport(op, new[] { "X1", "Vdiv", "V1" })
            };
            IEnumerable<double> references = [5.0 / 2e3, 5.0 / 2e3];
            AnalyzeOp(op, ckt, exports, references);
        }

        [Test]
        public void When_SimpleSubcircuit_Measure_Multi_Subcircuit_Current2_Expect_Reference()
        {
            // Define the subcircuit
            var subckt1 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)),
                "a");

            // board level
            var subckt2 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "in", "0", 1),//same name to make sure it finds correct R1
                new Subcircuit("Vdiv", subckt1).Connect("in")),
                "in");

            //connect voltage source to board
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 5.0),
                new Subcircuit("X1", subckt2).Connect("in"));

            // Simulate the circuit
            var op = new OP("op");
            var exports = new IExport<double>[] {
                new RealPropertyExport(op, new[] { "X1", "Vdiv", "R1" }, "i"),
                new RealPropertyExport(op, new[] { "X1", "R1" }, "i"),
                new RealPropertyExport(op, new[] { "X1", "Vdiv", "R2" }, "v"),
                new RealVoltageExport(op, new[] { "X1", "Vdiv", "b" }),
            };
            IEnumerable<double> references = [5.0 / 2e3, 5.0, 2.5, 2.5];
            AnalyzeOp(op, ckt, exports, references);
        }

        [Test]
        public void When_SimpleSubcircuit_DC_Measure_Multi_Subcircuit_Expect_Reference()
        {
            // Define the subcircuit
            var subckt1 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)),
                "a");

            // board level
            var subckt2 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "in", "0", 1),//same name to make sure it finds correct R1
                new Subcircuit("Vdiv", subckt1).Connect("in")),
                "in");

            //connect voltage source to board
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 5.0),
                new Subcircuit("X1", subckt2).Connect("in"));

            // <example01_simulate2>
            // Create a DC simulation that sweeps V1 from 1V to 2V in steps of 1V
            var dc = new DC("DC 1", "V1", 1.0,2.0,1.0);

            // Create exports
            string[] subcircuitOutput = ["X1", "Vdiv", "R2"];

            // Create exports
            IExport<double>[] exports = [
                new RealPropertyExport(dc, "V1", "v"),
                new RealPropertyExport(dc, subcircuitOutput, "v"),
                new RealPropertyExport(dc, subcircuitOutput, "i"),
                new RealPropertyExport(dc, subcircuitOutput, "resistance")
            ];

            double[][] references =
            [
                [1.0, 2.0],
                [0.5, 1.0],
                [1/2e3, 2/2e3],
                [1e3, 1e3],
            ];

            // Run test
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSubcircuit_Tran_Measure_Multi_Subcircuit_Expect_Reference()
        {
            // Define the subcircuit
            var subckt1 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)),
                "a");

            // board level
            var subckt2 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "in", "0", 1),//same name to make sure it finds correct R1
                new Subcircuit("Vdiv", subckt1).Connect("in")),
                "in");

            //connect voltage source to board
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 5.0),
                new Subcircuit("X1", subckt2).Connect("in"));

            // <example01_simulate2>
            var tran = new Transient("transient", 1e-9, 2e-11);

            // Create exports
            string[] subcircuitOutput = ["X1", "Vdiv", "R2"];

            // Create exports
            IExport<double>[] exports = [ new GenericExport<double>(tran, () => tran.GetState<IIntegrationMethod>().Time),
                                        new RealPropertyExport(tran, "V1", "v"),
                                        new RealPropertyExport(tran, subcircuitOutput, "v"),
                                        new RealPropertyExport(tran, subcircuitOutput, "i"),
                                        new RealPropertyExport(tran, subcircuitOutput, "resistance")
                                        ];

            // Create references
            IEnumerable<Func<double, double>> references = [
                t => t,
                t => 5.0,
                t => 2.5,
                t => 5/2e3,
                t => 1e3,
            ];

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);

        }

        [Test]
        public void When_SimpleSubcircuit_AC_Measure_Multi_Subcircuit_Expect_Reference()
        {
            // Define the subcircuit
            var subckt1 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "c", 1e3),
                new VoltageSource("V1", "c", "0", 0.0)),
                "a");

            // board level
            var subckt2 = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "in", "0", 1),//same name to make sure it finds correct R1
                new Subcircuit("Vdiv", subckt1).Connect("in")),
                "in");

            //connect voltage source to board
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 5.0).SetParameter("acmag", 1.0),
                new Subcircuit("X1", subckt2).Connect("in"));

            // Create a DC simulation that sweeps V1 from 1V to 2V in steps of 1V
            var ac = new AC("AC 1", new DecadeSweep(0.1, 1e6, 2));

            // Create exports
            IExport<Complex>[] exports = [
                new ComplexPropertyExport(ac, new[] { "X1", "Vdiv", "R2" }, "i"),
                new ComplexCurrentExport(ac, new[] { "X1", "Vdiv", "V1" }),
                new ComplexVoltageExport(ac, new[] { "X1", "Vdiv", "b" }),
                ];

            Complex[][] references =
            [
                [0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3],
                [0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3, 0.5e-3],
                [0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5],
            ];

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
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
            IEnumerable<double> references = [1.0];
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SubcircuitAccess_Expect_Reference()
        {
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "c", 1e3)), "a", "c");
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Subcircuit("X1", subckt, "in", "out"),
                new Subcircuit("X2", subckt, "out", "0"));

            var op = new OP("op");
            op.RunToEnd(ckt);
            var behaviors = op.EntityBehaviors["X2"].GetValue<SpiceSharp.Components.Subcircuits.EntitiesBehavior>();
            Assert.That(behaviors.LocalBehaviors["R2"].GetProperty<double>("v"), Is.EqualTo(10.0 / 4.0).Within(1e-12));

            var state = behaviors.GetState<IBiasingSimulationState>();
            Assert.That(state.Solution[state.Map[state.GetSharedVariable("b")]], Is.EqualTo(10.0 / 4.0).Within(1e-12));
        }
    }
}
