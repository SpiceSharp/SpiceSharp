using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.ParallelComponents;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SimpleParallelTests : Framework
    {
        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelBiasingLoadOp_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Resistor("R2", "out", "0", 1e3))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingBehavior), workDistributor))
                );

            var op = new OP("op");
            var exports = new IExport<double>[] { new RealVoltageExport(op, "out") };
            double[] references = [0.5];
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(BoolWorkDistributor))]
        public void When_ParallelConvergenceOp_Expect_Reference(IWorkDistributor<bool> workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Resistor("R2", "out", "0", 1e3))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IConvergenceBehavior), workDistributor)));

            var op = new OP("op");
            var exports = new IExport<double>[] { new RealVoltageExport(op, "out") };
            double[] references = [0.5];
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelAcLoadAc_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Capacitor("C1", "out", "0", 1e-6))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IFrequencyBehavior), workDistributor))
                );

            var ac = new AC("ac", new DecadeSweep(1, 1e6, 2));
            var exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "out") };
            var references = new Func<double, Complex>[] { f => 1.0 / (1.0 + new Complex(0.0, f * 2e-3 * Math.PI)) };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelBiasingLoadTransient_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Capacitor("C1", "out", "0", 1e-6))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingBehavior), workDistributor)));

            var tran = new Transient("tran", 1e-7, 10e-6);
            var exports = new IExport<double>[] { new RealVoltageExport(tran, "out") };
            var references = new Func<double, double>[] { time => 1.0 };
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [TestCaseSource(typeof(SimpleParallelTests), nameof(WorkDistributor))]
        public void When_ParallelTransientInitTransient_Expect_Reference(IWorkDistributor workDistributor)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Parallel("PC1",
                    new Resistor("R1", "in", "out", 1e3),
                    new Capacitor("C1", "out", "0", 1e-6),
                    new Capacitor("C2", "out", "0", 1e-6))
                    .SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(ITimeBehavior), workDistributor)));

            var tran = new Transient("tran", 1e-7, 10e-6);
            var exports = new IExport<double>[] { new RealVoltageExport(tran, "out") };
            var references = new Func<double, double>[] { time => 1.0 };
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void TestParallelSolve()
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

            var r = new Random(); // Should be declared at the topmost level
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
                newSubCircuit.Parameters.LocalSolver = true;
                parallelComponents.Add(newSubCircuit);
                parallelComponents.Add(new Resistor("R" + parentCircuitIndex.ToString(), parentN1, parentN2, 1));
            }

            string lastNodeName = parentCircuits.ToString() + "_" + "0";
            parallelComponents.Add(new VoltageSource("Vterm1", "0_0", "parallel_terminal_pos", 0));
            parallelComponents.Add(new VoltageSource("Vterm2", lastNodeName, "parallel_terminal_neg", 0));

            var solveCirc = new Circuit();
            var newParallel = new Parallel("PL1", parallelComponents);
            var workDistributor = new TPLWorkDistributor();
            newParallel.SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingBehavior), workDistributor));
            solveCirc.Add(newParallel);

            // Ground the negative lead (will not work without something grounded)
            solveCirc.Add(new Resistor("Rgnd", "parallel_terminal_neg", "0", 0));
            // Define the vterm for solving
            solveCirc.Add(new VoltageSource("vterm", "parallel_terminal_pos", "parallel_terminal_neg", 0));

            var dc = new DC("DC1", "vterm", 0, parentCircuits * 55, parentCircuits * 55 / 100);

            var currentExport = new RealCurrentExport(dc, "vterm");

            double maxPower = 0.0;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (int _ in dc.Run(solveCirc))
            {
                double input = dc.GetVoltage("parallel_terminal_pos");
                double output = currentExport.Value;

                double power = input * output;
                if (power > maxPower)
                {
                    maxPower = power;
                }
            }
            watch.Stop();
            Assert.That(maxPower, Is.Not.EqualTo(0.0));
        }

        public static IEnumerable<TestCaseData> WorkDistributor
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(new TPLWorkDistributor());
            }
        }

        public static IEnumerable<TestCaseData> BoolWorkDistributor
        {
            get
            {
                yield return new TestCaseData(null);
                yield return new TestCaseData(new TPLBooleanAndWorkDistributor());
            }
        }
    }
}
