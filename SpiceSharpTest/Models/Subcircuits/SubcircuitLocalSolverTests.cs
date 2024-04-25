using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SubcircuitLocalSolverTests : Framework
    {
        private Subcircuit Create(string name, IEntity[] entities, string[] pins, string[] nodes)
        {
            var subckt = new Circuit(entities);
            var def = new SubcircuitDefinition(subckt, pins);
            var inst = new Subcircuit(name, def, nodes);
            inst.Parameters.LocalSolver = true;
            return inst;
        }

        [Test]
        public void When_Op_Expect_Reference()
        {
            // No internal nodes
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)),
                "a", "b");
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", subckt, "in", "out")
                    .SetParameter("localsolver", true));

            var op = new OP("op");
            IExport<double>[] exports = new[] {
                new RealVoltageExport(op, "out"),
                new RealVoltageExport(op, new[] { "X1", "b" }),
            };
            IEnumerable<double> references = [0.5, 0.5];
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_Ac_Expect_Reference()
        {
            // No internal nodes
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "0", 1e3)),
                "a", "b");
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new Subcircuit("X1", subckt, "in", "out")
                    .SetParameter("localsolver", true));

            var ac = new AC("ac", new DecadeSweep(1, 100, 3));
            IExport<Complex>[] exports = new[] {
                new ComplexVoltageExport(ac, "out"),
                new ComplexVoltageExport(ac, new[] { "X1", "b" })
            };
            IEnumerable<Func<double, Complex>> references = [f => 0.5, f => 0.5];
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_Op2_Expect_Reference()
        {
            // One internal node
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "c", 1e3),
                new Resistor("R3", "b", "0", 1e3)),
                "a", "c");
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", subckt, "in", "out")
                    .SetParameter("localsolver", true));

            var op = new OP("op");
            IExport<double>[] exports = new[]
            {
                new RealVoltageExport(op, "out"),
                new RealVoltageExport(op, "X1".Combine("b")),
                new RealVoltageExport(op, "X1".Combine("c"))
            };
            IEnumerable<double> references = [0.5, 0.5, 0.5];
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_AC2_Expect_Reference()
        {
            // One internal node
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Resistor("R2", "b", "c", 1e3),
                new Resistor("R3", "b", "0", 1e3)),
                "a", "c");
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new Subcircuit("X1", subckt, "in", "out")
                    .SetParameter("localsolver", true));

            var ac = new AC("ac", new DecadeSweep(1, 100, 3));
            IExport<Complex>[] exports = new[] { new ComplexVoltageExport(ac, "out") };
            IEnumerable<Func<double, Complex>> references = [f => 0.5];
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_Transient_Expect_Reference()
        {
            // With internal states
            var subckt = new SubcircuitDefinition(new Circuit(
                new Resistor("R1", "a", "b", 1e3),
                new Capacitor("C1", "b", "0", 1e-6)),
                "a", "b");
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Subcircuit("X1", subckt, "in", "out")
                    .SetParameter("localsolver", true));

            var tran = new Transient("transient", 1e-6, 1e-3);
            tran.TimeParameters.InitialConditions.Add("out", 0.0);
            IExport<double>[] exports = new[] { new RealVoltageExport(tran, "out") };
            IEnumerable<Func<double, double>> references = [t => 1.0 - Math.Exp(-t * 1e3)];
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_Op3_Expect_Reference()
        {
            // Variable that makes an equivalent circuit impossible
            var subckt = new SubcircuitDefinition(new Circuit(
                new VoltageSource("V1", "a", "0", 1.0)), "a");
            var ckt = new Circuit(
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 1e3),
                new Subcircuit("X1", subckt, "in")
                    .SetParameter("localsolver", true));

            var op = new OP("op");
            IExport<double>[] exports = new[] { new RealVoltageExport(op, "out") };
            Assert.Throws<NoEquivalentSubcircuitException>(() => op.Run(ckt));
        }

        [Test]
        public void When_Resistors_Expect_Reference()
        {
            // Relatively straightforward example with only a single resistor

            // Build the circuit
            var inst = Create("X1", [new Resistor("R1", "a", "b", 1e3)], ["a", "b"], ["x", "0"]);
            var ckt = new Circuit(
                inst,
                new CurrentSource("I1", "0", "x", 1.0));

            // Simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(1.0e3, args.GetVoltage("x"));
            };
            op.Run(ckt);
        }

        [Test]
        public void When_CurrentSource_Expect_Reference()
        {
            // Relatively straightforward example with only a single current source
            // The special thing here is that a current source only contributes to the RHS vector

            var inst = Create("X1", [new CurrentSource("I1", "a", "b", 1.0)], ["a", "b"], ["0", "x"]);
            var ckt = new Circuit(
                inst,
                new Resistor("R1", "0", "x", 1e3));

            // Simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(1.0e3, args.GetVoltage("x"));
            };
            op.Run(ckt);
        }

        [Test]
        public void When_CurrentSourceResistor_Expect_Reference()
        {
            // Relatively straightforward example with only a single current source and a single resistor.
            // The special thing here is that the Y-matrix and RHS elements are nicely defined

            var inst = Create("X1",
                [
                    new CurrentSource("I1", "a", "b", 1.0), 
                    new Resistor("R1", "a", "b", 1e3)
                ], ["a", "b"], ["0", "x"]);
            var ckt = new Circuit(
                inst,
                new Resistor("R1", "0", "x", 1e3));

            // Simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(0.5e3, args.GetVoltage("x"));
            };
            op.Run(ckt);
        }

        [Test]
        public void When_VoltageSourceResistor_Expect_Reference()
        {
            var inst = Create("X1", [new VoltageSource("V1", "c", "a", 1.0), new Resistor("R1", "c", "b", 1e3)], ["a", "b"], ["0", "x"]);
            var ckt = new Circuit(
                inst,
                new Resistor("R1", "0", "x", 1e3));

            // Simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(0.5, args.GetVoltage("x"));
            };
            op.Run(ckt);
        }

        [Test]
        public void When_VoltageSourceResistor2_Expect_Reference()
        {
            var inst = Create("X1", [
                new Resistor("R1a", "a", "c", 500),
                new VoltageSource("V1", "d", "c", 1.0),
                new Resistor("R1", "d", "b", 500)],
                ["a", "b"], ["0", "x"]);
            var ckt = new Circuit(
                inst,
                new Resistor("R1", "0", "x", 1e3));

            // Simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(0.5, args.GetVoltage("x"));
            };
            op.Run(ckt);
        }

        [Test]
        public void When_Diode_Expect_Reference()
        {
            var model = new DiodeModel("DM1");
            model.SetParameter("is", 1e-12);
            var inst = Create("X1", [model, new Diode("D1", "a", "b", "DM1")], ["a", "b"], ["a", "0"]);
            var ckt = new Circuit(
                inst,
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "a", 1e3));

            var op = new OP("op1");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.Greater(args.GetVoltage("a"), 0.5);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_DiodeVoltageSources_Expect_Reference()
        {
            var model = new DiodeModel("DM1");
            model.SetParameter("is", 1e-12);
            var inst = Create("X1", [model, new VoltageSource("Vs1", "a", "a1", 0), new Diode("D1", "a1", "b1", "DM1"), new VoltageSource("Vs2", "b1", "b", 0)], ["a", "b"], ["a", "0"]);
            var ckt = new Circuit(
                inst,
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "a", 1e3));

            var op = new OP("op1");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.Greater(args.GetVoltage("a"), 0.5);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_DiodeResistorVoltageSources_Expect_Reference()
        {
            var model = new DiodeModel("DM1");
            model.SetParameter("is", 1e-12);
            var inst = Create("X1", [model, new VoltageSource("Vs1", "a", "a1", 0), new Resistor("R1", "a1", "a2", 1e3), new Diode("D1", "a2", "b1", "DM1"), new VoltageSource("Vs2", "b1", "b", 0)], ["a", "b"], ["a", "0"]);
            var ckt = new Circuit(
                inst,
                new VoltageSource("V1", "a", "0", 1.0));

            var op = new OP("op1");
            var export = new RealVoltageExport(op, ["X1", "a2"]);
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.Greater(export.Value, 0.5);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_Hierarchical_Expect_Reference()
        {
            var inst = Create("X1", [new Resistor("R1", "a", "b", 1e3), new Resistor("R2", "b", "c", 1e3)], ["a", "c"], ["1", "2"]);
            var inst2 = Create("X1", [inst, new Resistor("R1", "2", "3", 1e3)], ["1", "3"], ["0", "in"]);
            var ckt = new Circuit(
                inst2,
                new VoltageSource("V1", "in", "0", 1.0));

            var op = new OP("op1");
            var export = new RealVoltageExport(op, ["X1", "X1", "b"]);
            AnalyzeOp(op, ckt, [export], [1.0 / 3.0]);
        }

        [Test]
        public void When_KnownMatrix1_Expect_Reference()
        {
            Circuit BuildCircuit(bool localSolver)
            {
                var def1 = new SubcircuitDefinition(new Circuit(
                    new Resistor("R1", "posTerm", "1", 0.0025),
                    new Resistor("Rdiode", "1", "negTerm", 1.0 / 0.707107),
                    new CurrentSource("Idiode", "1", "negTerm", -0.371755)),
                    ["posTerm", "negTerm"]);

                var def2 = new SubcircuitDefinition(new Circuit(
                    new Subcircuit("Child_0", def1, "CN_0", "CN_1"),
                    new VoltageSource("Vterm1", "CN_0", "parent_terminal_pos", 0),
                    new VoltageSource("Vterm2", "CN_1", "parent_terminal_neg", 0)),
                    ["parent_terminal_pos", "parent_terminal_neg"]);

                return new Circuit(
                    new Subcircuit("ParentCircuit_0", def2, "0_1", "1_0").SetParameter("localsolver", localSolver),
                    new Resistor("R1", "0_1", "0_0", 1),
                    new Subcircuit("ParentCircuit_1", def2, "1_1", "2_0").SetParameter("localsolver", localSolver),
                    new Resistor("R2", "1_0", "1_1", 1),
                    new VoltageSource("Vterm1", "0_0", "parallel_terminal_pos", 0),
                    new VoltageSource("Vterm2", "2_0", "parallel_terminal_neg", 0),
                    new VoltageSource("vterm", "parallel_terminal_pos", "parallel_terminal_neg", 1),
                    new Resistor("Rgnd", "parallel_terminal_neg", "0", 0));
            }

            var ckt = BuildCircuit(true);
            var op = new OP("op");
            op.Run(ckt);
        }
    }
}
