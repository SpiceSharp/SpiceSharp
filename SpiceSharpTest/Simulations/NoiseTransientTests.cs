using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for the <see cref="NoiseTransient"/> analysis, driven by the thermal noise source of a
/// <see cref="Resistor"/>.
/// </summary>
[TestFixture]
public class NoiseTransientTests
{
    private const double Resistance = 1e3;
    private const double Capacitance = 1e-9;

    /// <summary>
    /// The pole of the RC lowpass that every test below uses, in Hz.
    /// </summary>
    private static double PoleFrequency => 1.0 / (2.0 * Math.PI * Resistance * Capacitance);

    /// <summary>
    /// The output variance of an RC lowpass driven by the thermal noise of its own resistor, if the
    /// noise were not band-limited. It does not depend on the resistance.
    /// </summary>
    private static double KtOverC => Constants.Boltzmann * Constants.ReferenceTemperature / Capacitance;

    /// <summary>
    /// The fraction of <see cref="KtOverC"/> that survives the band limit, cascading the n-pole
    /// shaping filter at <paramref name="maximumFrequency"/> with the pole of the circuit.
    /// </summary>
    private static double CapturedFraction(double maximumFrequency, int order)
    {
        double a = maximumFrequency, b = PoleFrequency;
        if (order == 1)
            return a / (a + b);
        return a * (b + (2.0 * a)) / (2.0 * (a + b) * (a + b));
    }

    private static Trapezoidal CreateTimeParameters(double stopTime, double step)
        => new()
        {
            InitialStep = step,
            MaxStep = step,
            StopTime = stopTime,

            // The injected process is rough, so the truncation error estimator would otherwise keep
            // cutting the timestep. These tests are about the injected power, not about step control.
            LteRelTol = 1e-1,
            LteAbsTol = 1e-1,
            ChargeTolerance = 1e-9
        };

    /// <summary>
    /// Runs a simulation and returns the timestep-weighted mean and variance of the output node,
    /// ignoring everything before <paramref name="burnIn"/>.
    /// </summary>
    private static (double Mean, double Variance) Measure(NoiseTransient tran, IEntityCollection ckt, double burnIn)
    {
        double weight = 0.0, sum = 0.0, squares = 0.0, previousTime = 0.0;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            double time = tran.Time;
            double dt = time - previousTime;
            previousTime = time;
            if (time < burnIn)
                continue;

            double v = tran.GetVoltage("out");
            weight += dt;
            sum += dt * v;
            squares += dt * v * v;
        }
        Assert.That(weight, Is.GreaterThan(0.0));
        double mean = sum / weight;
        return (mean, (squares / weight) - (mean * mean));
    }

    /// <summary>
    /// Runs a simulation and returns the frozen noise current of the named device at every exported
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

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_RcLowpass_Expect_KtOverC(int order)
    {
        // A resistor in parallel with a capacitor. Its own thermal noise charges the capacitor to
        // kT/C, reduced by the fraction of the noise power that survives the band limit.
        const double stopTime = 2e-3;
        const double step = 2e-8;
        const double burnIn = 2e-5;                                     // 20 time constants
        double maximumFrequency = 2.0 * PoleFrequency;
        double expected = KtOverC * CapturedFraction(maximumFrequency, order);

        var ckt = new Circuit(
            new Resistor("R1", "out", "0", Resistance),
            new Capacitor("C1", "out", "0", Capacitance));

        var tran = new NoiseTransient("tran", CreateTimeParameters(stopTime, step));
        tran.NoiseParameters.MaximumNoiseFrequency = maximumFrequency;
        tran.NoiseParameters.BandLimitOrder = order;
        tran.NoiseParameters.Seed = 0x5EED;

        var (mean, variance) = Measure(tran, ckt, burnIn);
        Assert.Multiple(() =>
        {
            Assert.That(mean, Is.EqualTo(0.0).Within(0.15 * Math.Sqrt(expected)), "mean");

            // A Monte-Carlo estimate over roughly 2000 correlation times, so the relative standard
            // error is a few percent. The window is wide enough for that and still narrow enough to
            // reject the unbanded kT/C, a two-sided density, or a missing noise-bandwidth factor.
            Assert.That(variance, Is.EqualTo(expected).Within(12.0).Percent, "variance");
        });
    }

    [Test]
    public void When_ParallelResistors_Expect_IndependentRealizations()
    {
        // Four resistors of 4R in parallel have the same conductance, and therefore the same total
        // noise density, as a single resistor R. The output variance must therefore be the same as
        // in the test above. If the four sources shared a random stream or a shaping state they
        // would be perfectly correlated instead of independent, and the variance would come out
        // four times too large.
        const int count = 4;
        const double stopTime = 2e-3;
        const double step = 2e-8;
        const double burnIn = 2e-5;
        double maximumFrequency = 2.0 * PoleFrequency;
        double expected = KtOverC * CapturedFraction(maximumFrequency, 2);

        var ckt = new Circuit(new Capacitor("C1", "out", "0", Capacitance));
        for (int i = 0; i < count; i++)
            ckt.Add(new Resistor($"R{i + 1}", "out", "0", count * Resistance));

        var tran = new NoiseTransient("tran", CreateTimeParameters(stopTime, step));
        tran.NoiseParameters.MaximumNoiseFrequency = maximumFrequency;
        tran.NoiseParameters.Seed = 0x5EED;

        var (_, variance) = Measure(tran, ckt, burnIn);
        Assert.That(variance, Is.EqualTo(expected).Within(12.0).Percent);
    }

    [Test]
    public void When_SameSeed_Expect_SameRealization()
    {
        var reference = MeasureNoiseCurrent(CreateSimulation(0x5EED), CreateRcCircuit(), "R1");
        var same = MeasureNoiseCurrent(CreateSimulation(0x5EED), CreateRcCircuit(), "R1");
        var other = MeasureNoiseCurrent(CreateSimulation(0x5EEE), CreateRcCircuit(), "R1");

        Assert.That(reference, Has.Count.GreaterThan(100));
        Assert.Multiple(() =>
        {
            Assert.That(same, Is.EqualTo(reference), "same seed");
            Assert.That(other, Is.Not.EqualTo(reference), "different seed");
        });
    }

    [Test]
    public void When_UnrelatedDeviceAdded_Expect_SameRealization()
    {
        // The stream of a noise source is seeded from its name, not from the order in which it was
        // registered, so an unrelated device elsewhere in the circuit must not shift the realization
        // of any other source.
        var extended = CreateRcCircuit();
        extended.Add(new VoltageSource("V1", "b", "0", 1.0));
        extended.Add(new Resistor("R2", "b", "0", Resistance));

        var reference = MeasureNoiseCurrent(CreateSimulation(0x5EED), CreateRcCircuit(), "R1");
        var actual = MeasureNoiseCurrent(CreateSimulation(0x5EED), extended, "R1");
        Assert.That(actual, Is.EqualTo(reference));
    }

    [Test]
    public void When_Rerun_Expect_SameRealization()
    {
        // A Monte-Carlo driver reuses the setup of a single simulation over many runs, so a rerun
        // has to restart the streams from the master seed rather than continue where it left off.
        var tran = CreateSimulation(0x5EED);
        var ckt = CreateRcCircuit();

        var reference = new List<double>();
        ITimeNoiseSource source = null;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            source ??= tran.EntityBehaviors["R1"].GetValue<ITimeNoiseBehavior>();
            reference.Add(source.Current);
        }
        Assert.That(reference, Has.Count.GreaterThan(100));

        var same = new List<double>();
        foreach (int _ in tran.Rerun(Transient.ExportTransient))
            same.Add(source.Current);

        // Picking a new master seed in between two runs is what makes the runs an ensemble
        tran.NoiseParameters.Seed = 0x5EEE;
        var other = new List<double>();
        foreach (int _ in tran.Rerun(Transient.ExportTransient))
            other.Add(source.Current);

        Assert.Multiple(() =>
        {
            Assert.That(same, Is.EqualTo(reference), "same seed");
            Assert.That(other, Has.Count.EqualTo(reference.Count), "different seed, count");
            Assert.That(other, Is.Not.EqualTo(reference), "different seed");
        });
    }

    [Test]
    public void When_NoMaximumNoiseFrequency_Expect_Exception()
    {
        // A band limit of zero would make every source silent rather than white, which is a much
        // worse outcome than a refusal to run.
        var tran = new NoiseTransient("tran", CreateTimeParameters(1e-5, 2e-8));
        var ckt = CreateRcCircuit();
        Assert.Throws<SpiceSharpException>(() =>
        {
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
            }
        });
    }

    private static Circuit CreateRcCircuit()
        => new(
            new Resistor("R1", "out", "0", Resistance),
            new Capacitor("C1", "out", "0", Capacitance));

    private static NoiseTransient CreateSimulation(int seed)
    {
        var tran = new NoiseTransient("tran", CreateTimeParameters(1e-5, 2e-8));
        tran.NoiseParameters.MaximumNoiseFrequency = 2.0 * PoleFrequency;
        tran.NoiseParameters.Seed = seed;
        return tran;
    }
}
