using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void When_ResistiveDividerOP01_Expect_Reference()
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
        public void When_ResistiveDividerAC01_Expect_Reference()
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
        public void When_ResistiveDividerOP02_Expect_Reference()
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
        public void When_ResistiveDividerAC02_Expect_Reference()
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
        public void When_LowpassRCTRAN01_Expect_Reference()
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
        public void When_ZeroEquivalentResistance_Expect_Exception()
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
            Assert.Throws<NoEquivalentSubcircuitException>(() => op.RunToEnd(ckt));
        }

        [Test]
        public void When_ResistorOP01_Expect_Reference()
        {
            // Relatively straightforward example with only a single resistor

            // Build the circuit
            var inst = Create("X1", [new Resistor("R1", "a", "b", 1e3)], ["a", "b"], ["x", "0"]);
            var ckt = new Circuit(
                inst,
                new CurrentSource("I1", "0", "x", 1.0));

            // Simulation
            var op = new OP("op");
            foreach (int _ in op.Run(ckt))
            {
                Assert.That(op.GetVoltage("x"), Is.EqualTo(1.0e3));
            }
        }

        [Test]
        public void When_CurrentSourceOP01_Expect_Reference()
        {
            // Relatively straightforward example with only a single current source
            // The special thing here is that a current source only contributes to the RHS vector

            var inst = Create("X1", [new CurrentSource("I1", "a", "b", 1.0)], ["a", "b"], ["0", "x"]);
            var ckt = new Circuit(
                inst,
                new Resistor("R1", "0", "x", 1e3));

            // Simulation
            var op = new OP("op");
            foreach (int _ in op.Run(ckt))
            {
                Assert.That(op.GetVoltage("x"), Is.EqualTo(1.0e3));
            }
        }

        [Test]
        public void When_CurrentSourceResistorOP01_Expect_Reference()
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
            foreach (int _ in op.Run(ckt))
            {
                Assert.That(op.GetVoltage("x"), Is.EqualTo(0.5e3));
            }
        }

        [Test]
        public void When_VoltageSourceResistorOP01_Expect_Reference()
        {
            var inst = Create("X1", [new VoltageSource("V1", "c", "a", 1.0), new Resistor("R1", "c", "b", 1e3)], ["a", "b"], ["0", "x"]);
            var ckt = new Circuit(
                inst,
                new Resistor("R1", "0", "x", 1e3));

            // Simulation
            var op = new OP("op");
            foreach (int _ in op.Run(ckt))
            {
                Assert.That(op.GetVoltage("x"), Is.EqualTo(0.5));
            }
        }

        [Test]
        public void When_VoltageSourceResistorOP02_Expect_Reference()
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
            foreach (int _ in op.Run(ckt))
            {
                Assert.That(op.GetVoltage("x"), Is.EqualTo(0.5));
            }
        }

        [Test]
        public void When_DiodeOP01_Expect_NonLocal()
        {
            CompareOP(localSolver =>
            {
                var model = new DiodeModel("DM1");
                model.SetParameter("is", 1e-12);
                var inst = Create("X1", [model, new Diode("D1", "a", "b", "DM1")], ["a", "b"], ["a", "0"]);
                return new Circuit(
                    inst,
                    new VoltageSource("V1", "in", "0", 1.0),
                    new Resistor("R1", "in", "a", 1e3));
            }, simulation => [
                new RealVoltageExport(simulation, "a")
            ]);
        }

        [Test]
        public void When_DiodeVoltageSourcesOP01_Expect_NonLocal()
        {
            CompareOP(localSolver =>
            {
                var model = new DiodeModel("DM1");
                model.SetParameter("is", 1e-12);
                var inst = Create("X1", [model, new VoltageSource("Vs1", "a", "a1", 0), new Diode("D1", "a1", "b1", "DM1"), new VoltageSource("Vs2", "b1", "b", 0)], ["a", "b"], ["a", "0"]);
                return new Circuit(
                    inst,
                    new VoltageSource("V1", "in", "0", 1.0),
                    new Resistor("R1", "in", "a", 1e3));
            }, simulation =>
            [
                new RealVoltageExport(simulation, "a")
            ]);
        }

        [Test]
        public void When_DiodeResistorVoltageSourcesOP01_Expect_NonLocal()
        {
            CompareOP(localSolver =>
            {
                var model = new DiodeModel("DM1");
                model.SetParameter("is", 1e-12);
                var inst = Create("X1", [model, new VoltageSource("Vs1", "a", "a1", 0), new Resistor("R1", "a1", "a2", 1e3), new Diode("D1", "a2", "b1", "DM1"), new VoltageSource("Vs2", "b1", "b", 0)], ["a", "b"], ["a", "0"]);
                return new Circuit(
                    inst,
                    new VoltageSource("V1", "a", "0", 1.0));
            }, simulation =>
            [
                new RealVoltageExport(simulation, new[] { "X1", "a2" })
            ]);
        }

        [Test]
        public void When_HierarchicalResistiveDividerOP01_Expect_Reference()
        {
            var inst = Create("X1", [new Resistor("R1", "a", "b", 1e3), new Resistor("R2", "b", "c", 1e3)], ["a", "c"], ["1", "2"]);
            var inst2 = Create("X1", [inst, new Resistor("R1", "2", "3", 1e3)], ["1", "3"], ["0", "in"]);
            var ckt = new Circuit(
                inst2,
                new VoltageSource("V1", "in", "0", 1.0));

            var op = new OP("op1");
            var export = new RealVoltageExport(op, new[] { "X1", "X1", "b" });
            AnalyzeOp(op, ckt, [export], [1.0 / 3.0]);
        }

        [Test]
        public void When_ExampleCircuitOP01_Expect_NonLocal()
        {
            CompareOP(localSolver =>
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
            }, simulation =>
            {
                return
                [
                    new RealVoltageExport(simulation, "0_0"),
                    new RealVoltageExport(simulation, "0_1"),
                    new RealVoltageExport(simulation, "1_0"),
                    new RealVoltageExport(simulation, "1_1"),
                    new RealVoltageExport(simulation, new[] { "ParentCircuit_0", "Child_0", "1" }),
                    new RealVoltageExport(simulation, new[] { "ParentCircuit_1", "Child_0", "1" }),
                    new RealCurrentExport(simulation, new[] { "ParentCircuit_0", "Vterm1" }),
                    new RealCurrentExport(simulation, new[] { "ParentCircuit_0", "Vterm2" }),
                    new RealCurrentExport(simulation, new[] { "ParentCircuit_1", "Vterm1" }),
                    new RealCurrentExport(simulation, new[] { "ParentCircuit_1", "Vterm2" }),
                ];
            });
        }

        [Test]
        public void When_ExampleCircuitAC01_Expect_NonLocal()
        {
            CompareAC(localSolver =>
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
                    new VoltageSource("vterm", "parallel_terminal_pos", "parallel_terminal_neg", 1).SetParameter("acmag", 1.0),
                    new Resistor("Rgnd", "parallel_terminal_neg", "0", 0));
            }, simulation =>
            {
                return
                [
                    new ComplexVoltageExport(simulation, "0_0"),
                    new ComplexVoltageExport(simulation, "0_1"),
                    new ComplexVoltageExport(simulation, "1_0"),
                    new ComplexVoltageExport(simulation, "1_1"),
                    new ComplexVoltageExport(simulation, new[] { "ParentCircuit_0", "Child_0", "1" }),
                    new ComplexVoltageExport(simulation, new[] { "ParentCircuit_1", "Child_0", "1" }),
                    new ComplexCurrentExport(simulation, new[] { "ParentCircuit_0", "Vterm1" }),
                    new ComplexCurrentExport(simulation, new[] { "ParentCircuit_0", "Vterm2" }),
                    new ComplexCurrentExport(simulation, new[] { "ParentCircuit_1", "Vterm1" }),
                    new ComplexCurrentExport(simulation, new[] { "ParentCircuit_1", "Vterm2" }),
                ];
            });
        }

        [Test]
        public void When_ExampleCircuitNoise01_Expect_NonLocal()
        {
            CompareNoise(localSolver =>
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
                    new VoltageSource("vterm", "parallel_terminal_pos", "parallel_terminal_neg", 1).SetParameter("acmag", 1.0),
                    new Resistor("Rgnd", "parallel_terminal_neg", "0", 0));
            }, "vterm", "parallel_terminal_neg", simulation =>
            {
                return
                [
                    new InputNoiseDensityExport(simulation),
                    new OutputNoiseDensityExport(simulation)
                ];
            });
        }

        [Test]
        public void When_ExampleCircuitOP02_Expect_NonLocal()
        {
            int seed = new Random(DateTime.Now.Second).Next(); // Random seed, but needs to be the same when comparing
            CompareOP(localSolver =>
            {
                // Raised by sudsy - GitHub discussion #225
                // Modified
                int childCircuits = 72;
                int parentCircuits = 28;
                double I01 = 0.00000000001;
                int m1 = 1;
                int ILbase = 8;
                double ILvary = 0.1;
                double Rs = 0.0025;
                int Rsh = 150;

                var r = new Random(seed);
                var parallelComponents = new List<IEntity>();

                for (int parentCircuitIndex = 0; parentCircuitIndex < parentCircuits; parentCircuitIndex++)
                {
                    string parentCircuitID = "ParentCircuit_" + parentCircuitIndex.ToString();
                    string parentN1 = parentCircuitIndex.ToString() + "_" + "0";
                    string parentN2 = parentCircuitIndex.ToString() + "_" + "1";
                    string parentN3 = (parentCircuitIndex + 1).ToString() + "_" + "0";

                    var parentEntities = new List<IEntity>();

                    for (int childIndex = 0; childIndex < childCircuits; childIndex++)
                    {
                        string childCircuitID = "Child_" + childIndex.ToString();
                        string Node1 = "CN_" + childIndex.ToString();
                        string Node2 = "CN_" + (childIndex + 1).ToString();

                        var scDef = new SubcircuitDefinition(new EntityCollection(), new string[] { "posTerm", "negTerm" });

                        var diodeModel = new DiodeModel("J1Diode");
                        diodeModel.Parameters.SaturationCurrent = I01;
                        diodeModel.Parameters.EmissionCoefficient = m1;
                        scDef.Entities.Add(diodeModel);
                        var newDiode = new Diode("D1", "1", "negTerm", "J1Diode");
                        scDef.Entities.Add(newDiode);
                        double IL = ILbase + (ILvary * r.NextDouble()) - (ILvary * r.NextDouble());
                        scDef.Entities.Add(new CurrentSource("iL", "negTerm", "1", IL));
                        scDef.Entities.Add(new Resistor("RS", "1", "posTerm", Rs));
                        scDef.Entities.Add(new Resistor("RSH", "1", "negTerm", Rsh));

                        var scCircuit = new Subcircuit(childCircuitID, scDef, new string[] { Node1, Node2 });
                        parentEntities.Add(scCircuit);
                    }

                    parentEntities.Add(new VoltageSource("Vterm1", "CN_0", "parent_terminal_pos", 0));
                    parentEntities.Add(new VoltageSource("Vterm2", "CN_" + (childCircuits).ToString(), "parent_terminal_neg", 0));

                    var newSubCircuitParentDef = new SubcircuitDefinition(new EntityCollection(), new string[] { "parent_terminal_pos", "parent_terminal_neg" });

                    foreach (var item in parentEntities)
                    {
                        newSubCircuitParentDef.Entities.Add(item);
                    }

                    var newSubCircuit = new Subcircuit(parentCircuitID, newSubCircuitParentDef, new string[] { parentN2, parentN3 });
                    newSubCircuit.Parameters.LocalSolver = localSolver;
                    parallelComponents.Add(newSubCircuit);
                    parallelComponents.Add(new Resistor("R" + parentCircuitIndex.ToString(), parentN1, parentN2, 1));
                }

                string lastNodeName = parentCircuits.ToString() + "_" + "0";
                parallelComponents.Add(new VoltageSource("Vterm1", "0_0", "parallel_terminal_pos", 0));
                parallelComponents.Add(new VoltageSource("Vterm2", lastNodeName, "parallel_terminal_neg", 0));

                var solveCirc = new Circuit();
                var newParallel = new Parallel("PL1", parallelComponents);
                var workDistributor = new SpiceSharp.Components.ParallelComponents.TPLWorkDistributor();
                newParallel.SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingBehavior), workDistributor));
                solveCirc.Add(newParallel);

                // Ground the negative lead (will not work without something grounded)
                solveCirc.Add(new Resistor("Rgnd", "parallel_terminal_neg", "0", 0));
                // Define the vterm for solving
                solveCirc.Add(new VoltageSource("vterm", "parallel_terminal_pos", "parallel_terminal_neg", 0));
                return solveCirc;
            }, simulation =>
            [
                new RealVoltageExport(simulation, "0_0"),
                new RealVoltageExport(simulation, "0_1"),
                new RealVoltageExport(simulation, "1_0"),
                new RealVoltageExport(simulation, "1_1"),
                new RealVoltageExport(simulation, new[] { "PL1", "ParentCircuit_0", "Child_0", "1" }),
                new RealCurrentExport(simulation, new[] { "PL1", "ParentCircuit_0", "Vterm1" }),
                new RealCurrentExport(simulation, new[] { "PL1", "ParentCircuit_0", "Vterm2" }),
                new RealCurrentExport(simulation, "vterm"),
            ]);
        }

        /// <summary>
        /// Compares the results of an operating point simulation of two circuits, one with and one without local solver.
        /// </summary>
        /// <param name="buildCircuit">The function that will create the circuits. The first argument indicates whether to use a local solver.</param>
        /// <param name="buildExports">The function that will generate the exports that will be compared.</param>
        private void CompareOP(Func<bool, Circuit> buildCircuit, Func<IBiasingSimulation, IExport<double>[]> buildExports)
        {
            var cktExpected = buildCircuit(false);
            var cktActual = buildCircuit(true);

            var opExpected = new OP("expected");
            var opActual = new OP("actual");

            var exportsExpected = buildExports(opExpected);
            var exportsActual = buildExports(opActual);
            Assert.That(exportsActual.Length, Is.EqualTo(exportsExpected.Length));
            Assert.That(exportsExpected.Length, Is.GreaterThan(0));

            // Simulate the expected values
            double[] expected = new double[exportsExpected.Length];
            foreach (int _ in opExpected.Run(cktExpected))
            {
                for (int i = 0; i < expected.Length; i++)
                    expected[i] = exportsExpected[i].Value;
            }

            // Compare to the actual values
            bool didCheck = false;
            var parameters = opExpected.BiasingParameters;
            foreach (int _ in opActual.Run(cktActual))
            {
                for (int i = 0; i < expected.Length; i++)
                {
                    double actual = exportsActual[i].Value;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected[i])) * parameters.RelativeTolerance + parameters.AbsoluteTolerance;
                    Assert.That(actual, Is.EqualTo(expected[i]).Within(tol));
                }
                didCheck = true;
            }
            Assert.That(didCheck);

            DestroyExports(exportsExpected);
            DestroyExports(exportsActual);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildCircuit">The function that will create the circuits. The first argument indicates whether to use a local solver.</param>
        /// <param name="buildExports">The function that will generate the exports that will be compared.</param>
        /// <param name="frequencies">The frequency points that should tested. If <c>null</c> (default) then the frequency range is 1024 points from 0Hz to 1GHz.</param>
        private void CompareAC(Func<bool, Circuit> buildCircuit, Func<IFrequencySimulation, IExport<Complex>[]> buildExports, IEnumerable<double> frequencies = null)
        {
            var cktExpected = buildCircuit(false);
            var cktActual = buildCircuit(true);

            frequencies ??= new LinearSweep(0.0, 1e9, 1024);
            var opExpected = new AC("expected", frequencies);
            var opActual = new AC("actual", frequencies);
            int points = frequencies.Count();

            var exportsExpected = buildExports(opExpected);
            var exportsActual = buildExports(opActual);
            Assert.That(exportsActual.Length, Is.EqualTo(exportsExpected.Length));
            Assert.That(exportsExpected.Length, Is.GreaterThan(0));

            // Simulate the expected values
            var expected = new Complex[points][];
            for (int i = 0; i < expected.Length; i++)
                expected[i] = new Complex[exportsExpected.Length];
            int index = 0;
            foreach (int _ in opExpected.Run(cktExpected, AC.ExportSmallSignal))
            {
                for (int i = 0; i < exportsExpected.Length; i++)
                    expected[index][i] = exportsExpected[i].Value;
                index++;
            }

            // Compare to the actual values
            index = 0;
            foreach (int _ in opActual.Run(cktActual, AC.ExportSmallSignal))
            {
                for (int i = 0; i < exportsActual.Length; i++)
                {
                    var actual = exportsActual[i].Value;
                    double tol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected[index][i].Real)) * 1e-6 + 1e-12;
                    Assert.That(actual.Real, Is.EqualTo(expected[index][i].Real).Within(tol));
                    tol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected[index][i].Imaginary)) * 1e-6 + 1e-12;
                    Assert.That(actual.Imaginary, Is.EqualTo(expected[index][i].Imaginary).Within(tol));
                }
                index++;
            }
            Assert.That(index, Is.EqualTo(points));

            DestroyExports(exportsExpected);
            DestroyExports(exportsActual);
        }

        /// <summary>
        /// Compares the results of a noise simulation of two circuits, one with and one without local solver.
        /// </summary>
        /// <param name="buildCircuit">The function that will create the circuits. The first argument indicates whether to use a local solver.</param>
        /// <param name="input">The name of the voltage source that indicates the input.</param>
        /// <param name="output">The name of the output node.</param>
        /// <param name="buildExports">The function that will generate the exports that will be compared.</param>
        /// <param name="frequencies">The frequency points that should tested. If <c>null</c> (default) then the frequency range is 1024 points from 0Hz to 1GHz.</param>
        private void CompareNoise(Func<bool, Circuit> buildCircuit, string input, string output, Func<INoiseSimulation, IExport<double>[]> buildExports, IEnumerable<double> frequencies = null)
        {
            var cktExpected = buildCircuit(false);
            var cktActual = buildCircuit(true);

            frequencies ??= new LinearSweep(0, 1e9, 1024);
            var opExpected = new Noise("expected", input, output, frequencies);
            var opActual = new Noise("actual", input, output, frequencies);
            int points = frequencies.Count();

            var exportsExpected = buildExports(opExpected);
            var exportsActual = buildExports(opActual);
            Assert.That(exportsActual.Length, Is.EqualTo(exportsExpected.Length));
            Assert.That(exportsExpected.Length, Is.GreaterThan(0));

            // Simulate the expected values
            double[][] expected = new double[points][];
            for (int i = 0; i < points; i++)
                expected[i] = new double[exportsExpected.Length];
            int index = 0;
            foreach (int _ in opExpected.Run(cktExpected, Noise.ExportNoise))
            {
                for (int i = 0; i < exportsExpected.Length; i++)
                    expected[index][i] = exportsExpected[i].Value;
                index++;
            }

            // Compare to the actual values
            index = 0;
            foreach (int _ in opActual.Run(cktActual, Noise.ExportNoise))
            {
                for (int i = 0; i < exportsActual.Length; i++)
                {
                    double actual = exportsActual[i].Value;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected[index][i])) * 1e-6;
                    Assert.That(actual, Is.EqualTo(expected[index][i]).Within(tol));
                }
                index++;
            }
            Assert.That(index, Is.EqualTo(points));

            DestroyExports(exportsExpected);
            DestroyExports(exportsActual);
        }
    }
}
