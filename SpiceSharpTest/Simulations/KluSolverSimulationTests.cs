using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// End to end checks that a simulation configured to use the KLU solver produces the same
/// answers as the same simulation using the default sparse solver.
/// </summary>
[TestFixture]
public class KluSolverSimulationTests
{
    private static Circuit CreateResistiveDivider()
        => new(
            new VoltageSource("V1", "in", "0", 10.0),
            new Resistor("R1", "in", "out", 1e3),
            new Resistor("R2", "out", "0", 3e3));

    private static Circuit CreateDiodeCircuit()
        => new(
            new VoltageSource("V1", "in", "0", 0.0),
            new Resistor("R1", "in", "a", 1e3),
            new Diode("D1", "a", "0", "1N914"),
            new DiodeModel("1N914") { Parameters = { SaturationCurrent = 2.52e-9, EmissionCoefficient = 1.752 } });

    private static Circuit CreateRlcCircuit()
        => new(
            new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-9, 1e-9, 1e-4, 2e-4)),
            new Resistor("R1", "in", "out", 100.0),
            new Inductor("L1", "out", "mid", 1e-3),
            new Capacitor("C1", "mid", "0", 1e-9),
            new Resistor("R2", "mid", "0", 1e4));

    private static Circuit CreateAmplifier()
        => new(
            new VoltageSource("Vsupply", "vdd", "0", 5.0),
            new VoltageSource("Vin", "in", "0", 0.0),
            new Resistor("Rb", "in", "b", 10e3),
            new Resistor("Rc", "vdd", "c", 2e3),
            new Resistor("Re", "e", "0", 500.0),
            new Capacitor("Ce", "e", "0", 1e-6),
            new BipolarJunctionTransistor("Q1", "c", "b", "e", "0", "mod"),
            new BipolarJunctionTransistorModel("mod")
            {
                Parameters = { SatCur = 1e-15, BetaF = 100.0, BetaR = 1.0 }
            });

    [Test]
    public void When_OperatingPointWithKlu_Expect_ReferenceVoltage()
    {
        var op = new OP("op");
        op.BiasingParameters.Solver = SolverTypes.Klu;
        var export = new RealVoltageExport(op, "out");

        foreach (int _ in op.Run(CreateResistiveDivider()))
            Assert.That(export.Value, Is.EqualTo(7.5).Within(1e-12));
    }

    [Test]
    public void When_DcSweepWithKlu_Expect_SameAsSparseSolver()
    {
        double[] reference = RunDcSweep(SolverTypes.Sparse);
        double[] actual = RunDcSweep(SolverTypes.Klu);

        Assert.That(actual.Length, Is.EqualTo(reference.Length));
        for (int i = 0; i < reference.Length; i++)
            Assert.That(actual[i], Is.EqualTo(reference[i]).Within(1e-9), $"point {i}");
    }

    private static double[] RunDcSweep(SolverTypes solver)
    {
        var dc = new DC("dc", "V1", 0.0, 2.0, 0.05);
        dc.BiasingParameters.Solver = solver;
        var export = new RealVoltageExport(dc, "a");
        var values = new List<double>();
        foreach (int _ in dc.Run(CreateDiodeCircuit(), Simulation.Exports))
            values.Add(export.Value);
        return [.. values];
    }

    [Test]
    public void When_TransientWithKlu_Expect_SameAsSparseSolver()
    {
        var reference = RunTransient(SolverTypes.Sparse);
        var actual = RunTransient(SolverTypes.Klu);

        Assert.That(actual.Count, Is.EqualTo(reference.Count));
        for (int i = 0; i < reference.Count; i++)
        {
            Assert.That(actual[i].Time, Is.EqualTo(reference[i].Time).Within(1e-18));
            Assert.That(actual[i].Value, Is.EqualTo(reference[i].Value).Within(1e-9));
        }
    }

    private static List<(double Time, double Value)> RunTransient(SolverTypes solver)
    {
        var transient = new Transient("tran", 1e-7, 5e-5);
        transient.BiasingParameters.Solver = solver;
        var export = new RealVoltageExport(transient, "mid");
        var values = new List<(double, double)>();
        foreach (int _ in transient.Run(CreateRlcCircuit(), Simulation.Exports))
            values.Add((transient.GetState<IIntegrationMethod>().Time, export.Value));
        return values;
    }

    [Test]
    public void When_AcWithKlu_Expect_SameAsSparseSolver()
    {
        var reference = RunAc(SolverTypes.Sparse);
        var actual = RunAc(SolverTypes.Klu);

        Assert.That(actual.Count, Is.EqualTo(reference.Count));
        for (int i = 0; i < reference.Count; i++)
        {
            double tolerance = Math.Max(reference[i].Magnitude * 1e-9, 1e-18);
            Assert.That((actual[i] - reference[i]).Magnitude, Is.LessThan(tolerance), $"point {i}");
        }
    }

    private static List<Complex> RunAc(SolverTypes solver)
    {
        var ac = new AC("ac", new DecadeSweep(1.0, 1e9, 10));
        ac.BiasingParameters.Solver = solver;
        ac.FrequencyParameters.Solver = solver;
        var circuit = CreateRlcCircuit();
        circuit["V1"].SetParameter("acmag", 1.0);
        var export = new ComplexVoltageExport(ac, "mid");
        var values = new List<Complex>();
        foreach (int _ in ac.Run(circuit, Simulation.Exports))
            values.Add(export.Value);
        return values;
    }

    [Test]
    public void When_NoiseWithKlu_Expect_SameAsSparseSolver()
    {
        // Noise analysis solves the adjoint system, so this is what exercises the transposed
        // substitution end to end.
        var reference = RunNoise(SolverTypes.Sparse);
        var actual = RunNoise(SolverTypes.Klu);

        Assert.That(actual.Count, Is.EqualTo(reference.Count));
        for (int i = 0; i < reference.Count; i++)
        {
            double tolerance = Math.Max(Math.Abs(reference[i]) * 1e-8, 1e-30);
            Assert.That(actual[i], Is.EqualTo(reference[i]).Within(tolerance), $"point {i}");
        }
    }

    private static List<double> RunNoise(SolverTypes solver)
    {
        var noise = new Noise("noise", "V1", "mid", new DecadeSweep(1.0, 1e9, 10));
        noise.BiasingParameters.Solver = solver;
        noise.FrequencyParameters.Solver = solver;
        var export = new OutputNoiseDensityExport(noise);
        var values = new List<double>();
        foreach (int _ in noise.Run(CreateRlcCircuit(), Noise.ExportNoise))
            values.Add(export.Value);
        return values;
    }

    [Test]
    public void When_TransistorAmplifierDcSweepWithKlu_Expect_SameAsSparseSolver()
    {
        // A nonlinear circuit that needs many Newton iterations, so the refactor path gets a
        // real workout and the fallback to a full factorization has to behave.
        double[] reference = RunAmplifierSweep(SolverTypes.Sparse);
        double[] actual = RunAmplifierSweep(SolverTypes.Klu);

        Assert.That(actual.Length, Is.EqualTo(reference.Length));
        for (int i = 0; i < reference.Length; i++)
            Assert.That(actual[i], Is.EqualTo(reference[i]).Within(1e-8), $"point {i}");
    }

    private static double[] RunAmplifierSweep(SolverTypes solver)
    {
        var dc = new DC("dc", "Vin", 0.5, 1.0, 0.01);
        dc.BiasingParameters.Solver = solver;
        var export = new RealVoltageExport(dc, "c");
        var values = new List<double>();
        foreach (int _ in dc.Run(CreateAmplifier(), Simulation.Exports))
            values.Add(export.Value);
        return [.. values];
    }

    [Test]
    public void When_SubcircuitWithKlu_Expect_SameAsSparseSolver()
    {
        // Subcircuits keep their own local solver, so this checks that the two kinds of solver
        // coexist in one simulation.
        double[] reference = RunSubcircuit(SolverTypes.Sparse);
        double[] actual = RunSubcircuit(SolverTypes.Klu);

        Assert.That(actual.Length, Is.EqualTo(reference.Length));
        for (int i = 0; i < reference.Length; i++)
            Assert.That(actual[i], Is.EqualTo(reference[i]).Within(1e-9), $"point {i}");
    }

    private static double[] RunSubcircuit(SolverTypes solver)
    {
        var definition = new SubcircuitDefinition(new Circuit(
            new Resistor("Ra", "a", "b", 1e3),
            new Resistor("Rb", "b", "0", 2e3)), "a", "b");

        var circuit = new Circuit(
            new VoltageSource("V1", "in", "0", 5.0),
            new Subcircuit("X1", definition, "in", "out"),
            new Resistor("Rload", "out", "0", 5e3));

        var dc = new DC("dc", "V1", 1.0, 5.0, 0.5);
        dc.BiasingParameters.Solver = solver;
        var export = new RealVoltageExport(dc, "out");
        var values = new List<double>();
        foreach (int _ in dc.Run(circuit, Simulation.Exports))
            values.Add(export.Value);
        return [.. values];
    }
}
