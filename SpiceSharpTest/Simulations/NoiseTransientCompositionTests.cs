using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.Common;
using SpiceSharp.Components.ParallelComponents;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for the composition behaviors of a <see cref="NoiseTransient"/> analysis: the noise sources
/// of the devices inside a <see cref="Subcircuit"/> or a <see cref="Parallel"/> have to take part in
/// the analysis on the same terms as the ones at the top level.
/// </summary>
[TestFixture]
public class NoiseTransientCompositionTests
{
    private const double Resistance = 1e3;
    private const double Capacitance = 1e-9;
    private const double StopTime = 2e-3;
    private const double Step = 2e-8;

    /// <summary>
    /// The time before which the samples are discarded, twenty time constants of the circuit.
    /// </summary>
    private const double BurnIn = 2e-5;

    /// <summary>
    /// The pole of the RC lowpass that every test below uses, in Hz.
    /// </summary>
    private static double PoleFrequency => 1.0 / (2.0 * Math.PI * Resistance * Capacitance);

    /// <summary>
    /// The band limit of every test below, in Hz.
    /// </summary>
    private static double MaximumFrequency => 2.0 * PoleFrequency;

    /// <summary>
    /// The output variance of the RC lowpass, at the default second-order band limit.
    /// </summary>
    private static double Expected
    {
        get
        {
            double a = MaximumFrequency, b = PoleFrequency;
            double captured = a * (b + (2.0 * a)) / (2.0 * (a + b) * (a + b));
            return Constants.Boltzmann * Constants.ReferenceTemperature / Capacitance * captured;
        }
    }

    private static NoiseTransient CreateSimulation(double stopTime = StopTime)
    {
        var tran = new NoiseTransient("tran", new Trapezoidal
        {
            InitialStep = Step,
            MaxStep = Step,
            StopTime = stopTime,

            // The injected process is rough, so the truncation error estimator would otherwise keep
            // cutting the timestep. These tests are about the injected power, not about step control.
            LteRelTol = 1e-1,
            LteAbsTol = 1e-1,
            ChargeTolerance = 1e-9
        });
        tran.NoiseParameters.MaximumNoiseFrequency = MaximumFrequency;
        tran.NoiseParameters.Seed = 0x5EED;
        return tran;
    }

    /// <summary>
    /// Runs a simulation and returns the timestep-weighted variance of the output node, ignoring
    /// everything before <see cref="BurnIn"/>.
    /// </summary>
    private static double MeasureVariance(NoiseTransient tran, IEntityCollection ckt)
    {
        double weight = 0.0, sum = 0.0, squares = 0.0, previousTime = 0.0;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            double time = tran.Time;
            double dt = time - previousTime;
            previousTime = time;
            if (time < BurnIn)
                continue;

            double v = tran.GetVoltage("out");
            weight += dt;
            sum += dt * v;
            squares += dt * v * v;
        }
        Assert.That(weight, Is.GreaterThan(0.0));
        double mean = sum / weight;
        return (squares / weight) - (mean * mean);
    }

    /// <summary>
    /// Runs a simulation and returns the frozen noise current of the named entity at every exported
    /// timepoint.
    /// </summary>
    private static List<double> MeasureNoiseCurrent(NoiseTransient tran, IEntityCollection ckt, string name)
    {
        var result = new List<double>();
        ITimeNoiseSource source = null;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            source ??= tran.EntityBehaviors[name].GetValue<ITimeNoiseBehavior>();
            result.Add(source.Current);
        }
        return result;
    }

    /// <summary>
    /// A subcircuit definition holding nothing but one resistor between its two pins.
    /// </summary>
    private static SubcircuitDefinition CreateResistorDefinition(double resistance)
        => new(new Circuit(new Resistor("R1", "a", "b", resistance)), "a", "b");

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void When_SubcircuitResistor_Expect_KtOverC(bool localSolver)
    {
        // The resistor that charges the capacitor sits inside a subcircuit. Its thermal noise has to
        // reach the capacitor unchanged - with a local solver it does so through the equivalent
        // contribution that the subcircuit hands to its parent, which is only computed at the end of
        // the load of the subcircuit.
        var ckt = new Circuit(
            new Capacitor("C1", "out", "0", Capacitance),
            new Subcircuit("X1", CreateResistorDefinition(Resistance), "out", "0")
                .SetParameter("localsolver", localSolver));

        double variance = MeasureVariance(CreateSimulation(), ckt);
        Assert.That(variance, Is.EqualTo(Expected).Within(12.0).Percent);
    }

    [Test]
    public void When_Subcircuit_Expect_AggregateExports()
    {
        // A subcircuit exports the sum of the densities and of the currents of the sources inside,
        // the way a device exports the sum over its own sources.
        var ckt = new Circuit(
            new Capacitor("C1", "out", "0", Capacitance),
            new Subcircuit("X1", CreateResistorDefinition(Resistance), "out", "0"));

        var tran = CreateSimulation(1e-5);
        ITimeNoiseSource subcircuit = null, resistor = null;
        int points = 0;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            if (subcircuit is null)
            {
                subcircuit = tran.EntityBehaviors["X1"].GetValue<ITimeNoiseBehavior>();
                resistor = tran.EntityBehaviors["X1"].GetValue<IEntitiesBehavior>()
                    .LocalBehaviors["R1"].GetValue<ITimeNoiseBehavior>();
            }
            Assert.That(subcircuit.Current, Is.EqualTo(resistor.Current));
            Assert.That(subcircuit.NoiseDensity, Is.EqualTo(resistor.NoiseDensity));
            points++;
        }
        Assert.That(points, Is.GreaterThan(100));
        Assert.That(subcircuit.NoiseDensity, Is.EqualTo(4.0 * Constants.Boltzmann * Constants.ReferenceTemperature / Resistance).Within(1e-12).Percent);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void When_RepeatedSubcircuitDefinition_Expect_IndependentRealizations(bool localSolver)
    {
        // Four instances of the same definition, each holding a resistor of 4R, have the same total
        // conductance and therefore the same total noise density as a single resistor R. The
        // behaviors inside the four instances carry the same names, so a source that seeded its
        // stream from its bare name would give the four instances one and the same realization and
        // inflate the variance fourfold.
        const int count = 4;
        var definition = CreateResistorDefinition(count * Resistance);
        var ckt = new Circuit(new Capacitor("C1", "out", "0", Capacitance));
        for (int i = 0; i < count; i++)
        {
            ckt.Add(new Subcircuit($"X{i + 1}", definition, "out", "0")
                .SetParameter("localsolver", localSolver));
        }

        double variance = MeasureVariance(CreateSimulation(), ckt);
        Assert.That(variance, Is.EqualTo(Expected).Within(12.0).Percent);
    }

    [Test]
    public void When_NestedSubcircuits_Expect_IndependentRealizations()
    {
        // The same check one level deeper: the qualification of the name of a source has to compose,
        // so that "X1/Y1/R1" and "X2/Y1/R1" are two different sources.
        const int count = 4;
        var inner = CreateResistorDefinition(count * Resistance);
        var outer = new SubcircuitDefinition(new Circuit(
            new Subcircuit("Y1", inner, "a", "b"),
            new Subcircuit("Y2", inner, "a", "b")), "a", "b");

        var ckt = new Circuit(
            new Capacitor("C1", "out", "0", Capacitance),
            new Subcircuit("X1", outer, "out", "0"),
            new Subcircuit("X2", outer, "out", "0"));

        double variance = MeasureVariance(CreateSimulation(), ckt);
        Assert.That(variance, Is.EqualTo(Expected).Within(12.0).Percent);
    }

    [TestCaseSource(nameof(WorkDistributors))]
    public void When_ParallelResistor_Expect_SameRealizationAsFlat(IWorkDistributor distributor)
    {
        // A Parallel is a grouping for execution and does not rename anything, so wrapping a device
        // in one must leave its realization bit-identical to the flat netlist. The aggregate current
        // of the parallel component is the current of the single source inside it.
        var flat = new Circuit(
            new Resistor("R1", "out", "0", Resistance),
            new Capacitor("C1", "out", "0", Capacitance));
        var reference = MeasureNoiseCurrent(CreateSimulation(1e-5), flat, "R1");
        Assert.That(reference, Has.Count.GreaterThan(100));

        var parallel = new Parallel("PC1", new Resistor("R1", "out", "0", Resistance));
        AddDistributors(parallel, distributor);
        var ckt = new Circuit(parallel, new Capacitor("C1", "out", "0", Capacitance));

        var actual = MeasureNoiseCurrent(CreateSimulation(1e-5), ckt, "PC1");
        Assert.That(actual, Is.EqualTo(reference));
    }

    [TestCaseSource(nameof(WorkDistributors))]
    public void When_ParallelResistors_Expect_KtOverC(IWorkDistributor distributor)
    {
        // Four resistors of 4R inside a parallel component. Every source draws from its own stream
        // and stamps through the parallel solver, so the four have to stay independent and their
        // contributions have to survive the application of that solver to the parent.
        const int count = 4;
        var entities = new IEntity[count];
        for (int i = 0; i < count; i++)
            entities[i] = new Resistor($"R{i + 1}", "out", "0", count * Resistance);

        var parallel = new Parallel("PC1", entities);
        AddDistributors(parallel, distributor);
        var ckt = new Circuit(parallel, new Capacitor("C1", "out", "0", Capacitance));

        double variance = MeasureVariance(CreateSimulation(), ckt);
        Assert.That(variance, Is.EqualTo(Expected).Within(12.0).Percent);
    }

    [Test]
    public void When_ParallelDistributesNoiseOnly_Expect_KtOverC()
    {
        // Distributing the noise behaviors without distributing the biasing ones leaves the sources
        // writing straight into the shared right-hand side vector, so the stamping has to fall back
        // on running serially. Probing stays distributed: it touches nothing that is shared.
        const int count = 4;
        var entities = new IEntity[count];
        for (int i = 0; i < count; i++)
            entities[i] = new Resistor($"R{i + 1}", "out", "0", count * Resistance);

        var parallel = new Parallel("PC1", entities);
        parallel.SetParameter("workdistributor",
            new KeyValuePair<Type, IWorkDistributor>(typeof(ITimeNoiseBehavior), new TPLWorkDistributor()));
        var ckt = new Circuit(parallel, new Capacitor("C1", "out", "0", Capacitance));

        double variance = MeasureVariance(CreateSimulation(), ckt);
        Assert.That(variance, Is.EqualTo(Expected).Within(12.0).Percent);
    }

    [Test]
    public void When_ParallelInsideSubcircuit_Expect_KtOverC()
    {
        // The two composition behaviors have to nest: the parallel component stamps into the local
        // solver of the subcircuit, which then eliminates the internal node.
        const int count = 4;
        var entities = new IEntity[count];
        for (int i = 0; i < count; i++)
            entities[i] = new Resistor($"R{i + 1}", "a", "b", count * Resistance);

        var parallel = new Parallel("PC1", entities);
        AddDistributors(parallel, new TPLWorkDistributor());
        var definition = new SubcircuitDefinition(new Circuit(parallel), "a", "b");

        var ckt = new Circuit(
            new Capacitor("C1", "out", "0", Capacitance),
            new Subcircuit("X1", definition, "out", "0")
                .SetParameter("localsolver", true));

        double variance = MeasureVariance(CreateSimulation(), ckt);
        Assert.That(variance, Is.EqualTo(Expected).Within(12.0).Percent);
    }

    private static void AddDistributors(Parallel parallel, IWorkDistributor distributor)
    {
        if (distributor is null)
            return;
        parallel.SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(IBiasingBehavior), distributor));
        parallel.SetParameter("workdistributor", new KeyValuePair<Type, IWorkDistributor>(typeof(ITimeNoiseBehavior), distributor));
    }

    public static IEnumerable<TestCaseData> WorkDistributors
    {
        get
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new TPLWorkDistributor());
        }
    }
}
