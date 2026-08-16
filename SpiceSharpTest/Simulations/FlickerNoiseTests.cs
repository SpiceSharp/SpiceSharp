using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for the flicker noise of a <see cref="NoiseTransient"/> analysis: the weight law that makes
/// a ladder of Ornstein-Uhlenbeck sections realize a <c>1/f^beta</c> density, the ladder that the
/// simulation state builds, and the realization that a <see cref="TimeNoiseFlicker"/> produces.
/// </summary>
[TestFixture]
public class FlickerNoiseTests
{
    /// <summary>
    /// The one-sided power spectral density of a ladder of Ornstein-Uhlenbeck sections, per unit of
    /// density coefficient.
    /// </summary>
    /// <param name="rates">The rates of the sections, in rad/s.</param>
    /// <param name="weights">The weights of the sections.</param>
    /// <param name="order">The number of poles of a section.</param>
    /// <param name="frequency">The frequency, in Hz.</param>
    private static double LadderDensity(IReadOnlyList<double> rates, FlickerWeights weights, int order, double frequency)
    {
        double omega = 2.0 * Math.PI * frequency;
        double sum = 0.0;
        for (int i = 0; i < rates.Count; i++)
        {
            double amplitude = weights.Amplitudes[i];
            double weight = amplitude * amplitude;
            double denominator = (rates[i] * rates[i]) + (omega * omega);
            sum += order == 1 ?
                4.0 * weight * rates[i] / denominator :
                8.0 * weight * rates[i] * rates[i] * rates[i] / (denominator * denominator);
        }
        return sum;
    }

    /// <summary>
    /// A ladder of <paramref name="perDecade"/> sections per decade, spanning
    /// <paramref name="decades"/> decades upward from <paramref name="minimum"/>.
    /// </summary>
    private static double[] CreateRates(double minimum, double decades, int perDecade)
    {
        int count = ((int)(decades * perDecade)) + 1;
        double ratio = Math.Pow(10.0, 1.0 / perDecade);
        double[] rates = new double[count];
        rates[0] = minimum;
        for (int i = 1; i < count; i++)
            rates[i] = rates[i - 1] * ratio;
        return rates;
    }

    [Test]
    [TestCase(0.8, 1)]
    [TestCase(1.0, 1)]
    [TestCase(1.2, 1)]
    [TestCase(1.5, 1)]
    [TestCase(0.8, 2)]
    [TestCase(1.0, 2)]
    [TestCase(1.2, 2)]
    [TestCase(1.5, 2)]
    public void When_LadderEvaluated_Expect_TargetDensity(double exponent, int order)
    {
        // The weight law is a pure function of the poles and the exponent, so the density of the
        // ladder can be evaluated analytically and compared against its 1/f^beta target. The
        // comparison is strictly in-band, and by a wide margin: the band edges of a ladder are soft,
        // and the outermost decades droop by tens of percent for a steep exponent. Total variance is
        // deliberately not compared against the band integral of the target either - section 5.3 of
        // the design shows the two differ by sin(pi*beta/2) by construction.
        const int perDecade = 2;
        const double decades = 10.0;
        const double margin = 3.0;
        var rates = CreateRates(1.0, decades, perDecade);
        var weights = new FlickerWeights(exponent, rates, order);

        double first = Math.Pow(10.0, margin) / (2.0 * Math.PI);
        double last = rates[rates.Length - 1] / (2.0 * Math.PI) / Math.Pow(10.0, margin);

        double worst = 0.0;
        int points = 400;
        for (int i = 0; i <= points; i++)
        {
            double frequency = first * Math.Pow(last / first, (double)i / points);
            double actual = LadderDensity(rates, weights, order, frequency);
            double target = Math.Pow(frequency, -exponent);
            worst = Math.Max(worst, Math.Abs((actual / target) - 1.0));
        }

        // What is left in-band is the ripple between the poles, which at two sections per decade is
        // half a percent, plus the tail of the low-edge droop, which only reaches this far in for the
        // steepest exponent tested.
        Assert.That(worst, Is.LessThan(0.03), $"ripple at beta = {exponent}, order = {order}");
    }

    [Test]
    public void When_LadderIsCoarse_Expect_MoreRipple()
    {
        // What justifies the default of two sections per decade. A two-pole section is narrower in
        // the logarithm of its rate than a one-pole one, so it needs the denser ladder: one section
        // per decade already ripples by a sixth, which the frequency-domain rule of thumb for a
        // one-pole ladder would not have predicted.
        var sparse = MidBandRipple(1.0, 1.0, 2);
        var dense = MidBandRipple(2.0, 1.0, 2);
        Assert.Multiple(() =>
        {
            Assert.That(sparse, Is.GreaterThan(0.1), "one section per decade");
            Assert.That(dense, Is.LessThan(0.01), "two sections per decade");
        });
    }

    /// <summary>
    /// The worst relative deviation of the ladder from its target, well inside the band.
    /// </summary>
    private static double MidBandRipple(double perDecade, double exponent, int order)
    {
        var rates = CreateRates(1.0, 10.0, (int)perDecade);
        var weights = new FlickerWeights(exponent, rates, order);
        double first = 1e3 / (2.0 * Math.PI);
        double last = rates[rates.Length - 1] / (2.0 * Math.PI) / 1e3;

        double worst = 0.0;
        for (int i = 0; i <= 400; i++)
        {
            double frequency = first * Math.Pow(last / first, i / 400.0);
            worst = Math.Max(worst, Math.Abs((LadderDensity(rates, weights, order, frequency) / Math.Pow(frequency, -exponent)) - 1.0));
        }
        return worst;
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_ExponentIsOne_Expect_EqualWeights(int order)
    {
        // A 1/f density is the equal-weight ladder, whatever the order of a section: the two values
        // of the normalization constant coincide at beta = 1, and the rate enters through an exponent
        // that is exactly zero there. The generalization must not change the default by a bit.
        var rates = CreateRates(1.0, 6.0, 2);
        var weights = new FlickerWeights(1.0, rates, order);

        Assert.That(weights.Amplitudes, Has.Count.EqualTo(rates.Length));
        for (int i = 1; i < weights.Amplitudes.Count; i++)
            Assert.That(weights.Amplitudes[i], Is.EqualTo(weights.Amplitudes[0]), $"section {i}");
    }

    [Test]
    public void When_ExponentIsOne_Expect_TotalVarianceOfBandIntegral()
    {
        // At beta = 1 the log-domain kernel is symmetric, so the band-edge deficit of the ladder and
        // its out-of-band leakage cancel and the total variance is exactly the band integral. This is
        // deliberately not asserted at any other exponent: there the two differ by sin(pi*beta/2) by
        // construction, and asserting it would look like a broken weight law.
        var rates = CreateRates(1.0, 6.0, 4);
        var weights = new FlickerWeights(1.0, rates, 2);

        double variance = 0.0;
        foreach (double amplitude in weights.Amplitudes)
            variance += amplitude * amplitude;

        // The integral of 1/f from the lowest to the highest pole, plus the half section that the
        // ladder carries beyond either end of that range.
        double expected = Math.Log(rates[rates.Length - 1] / rates[0]) * rates.Length / (rates.Length - 1);
        Assert.That(variance, Is.EqualTo(expected).Within(1e-12).Percent);
    }

    [Test]
    public void When_ExponentOutOfRange_Expect_Exception()
    {
        // The weight law is the Mellin transform of the section kernel, which only converges strictly
        // between 0 and 2. Both endpoints degrade gradually rather than failing sharply, so the range
        // has to be checked rather than left to the arithmetic to blow up.
        var rates = CreateRates(1.0, 4.0, 2);
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlickerWeights(0.0, rates, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlickerWeights(2.0, rates, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlickerWeights(-1.0, rates, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlickerWeights(1.0, rates, 3));
            Assert.Throws<ArgumentException>(() => new FlickerWeights(1.0, [1.0], 1));
            Assert.Throws<ArgumentException>(() => new FlickerWeights(1.0, [0.0, 1.0], 1));
        });
    }

    [Test]
    public void When_NoFlickerSource_Expect_EmptyLadder()
    {
        // The ladder costs one exponential per section per timepoint, so it is only maintained once a
        // source has asked for weights. A circuit of resistors must not pay for it.
        var ckt = new Circuit(
            new Resistor("R1", "out", "0", 1e3),
            new Capacitor("C1", "out", "0", 1e-9));
        var tran = CreateSimulation(1e-5, 1e-8, 1e7);
        tran.RunToEnd(ckt);

        var state = tran.GetState<ITimeNoiseSimulationState>();
        Assert.Multiple(() =>
        {
            Assert.That(state.FlickerLadder, Is.Empty);
            Assert.That(state.FlickerRates, Is.Not.Empty, "the poles exist whether or not anybody walks them");
        });
    }

    [Test]
    public void When_FlickerSource_Expect_LadderOverRunLength()
    {
        // The poles are log-spaced from the reciprocal of the run length, extended downward by the
        // guard decades, up to the band limit. All three arrays are indexed in lockstep.
        const double stopTime = 1e-4;
        const double maximumFrequency = 1e7;
        const double guard = 2.0;

        var ckt = new Circuit(
            new Resistor("R1", "out", "0", 1e3),
            new Capacitor("C1", "out", "0", 1e-9),
            new FlickerInjector("N1", "out", "0", 1e-20, 1.0));
        var tran = CreateSimulation(stopTime, 1e-8, maximumFrequency);
        tran.NoiseParameters.FlickerGuardDecades = guard;
        tran.NoiseParameters.FlickerSectionsPerDecade = 3.0;
        tran.RunToEnd(ckt);

        var state = tran.GetState<ITimeNoiseSimulationState>();
        double lowest = 2.0 * Math.PI / stopTime / Math.Pow(10.0, guard);
        double highest = 2.0 * Math.PI * maximumFrequency;
        double decades = Math.Log10(highest / lowest);

        Assert.Multiple(() =>
        {
            Assert.That(state.FlickerRates[0], Is.EqualTo(lowest).Within(1e-9).Percent, "lowest pole");
            Assert.That(state.FlickerRates[state.FlickerRates.Count - 1], Is.EqualTo(highest).Within(1e-9).Percent, "highest pole");
            Assert.That(state.FlickerRates, Has.Count.EqualTo(((int)Math.Ceiling(decades * 3.0)) + 1), "sections");
            Assert.That(state.FlickerLadder, Has.Count.EqualTo(state.FlickerRates.Count), "ladder");
            Assert.That(state.GetFlickerWeights(1.0).Amplitudes, Has.Count.EqualTo(state.FlickerRates.Count), "weights");
        });
    }

    [Test]
    [TestCase(0.8)]
    [TestCase(1.0)]
    [TestCase(1.3)]
    public void When_FlickerCurrentSampled_Expect_LadderSpectrum(double exponent)
    {
        // The realization has to reproduce the analytic ladder density, which is what pins the
        // pairing between the weights and the ladder: the two arrays are indexed in lockstep and
        // nothing in the type system says so. Reversing them turns a 1/f^beta density into a
        // 1/f^(2-beta) one, which at any exponent other than 1 moves the band powers below by a
        // factor of several.
        const double stopTime = 4e-3;
        const double step = 1e-7;
        const double maximumFrequency = 1e6;
        const double coefficient = 1e-18;

        var ckt = new Circuit(
            new Resistor("R1", "out", "0", 1e3),
            new FlickerInjector("N1", "out", "0", coefficient, exponent));
        var tran = CreateSimulation(stopTime, step, maximumFrequency);

        // No guard decades: everything the ladder holds then sits inside the window that the run can
        // resolve, so the periodogram below is not asked about power the run cannot contain.
        tran.NoiseParameters.FlickerGuardDecades = 0.0;

        var times = new List<double>();
        var currents = new List<double>();
        ITimeNoiseSource source = null;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            source ??= tran.EntityBehaviors["N1"].GetValue<ITimeNoiseBehavior>();
            times.Add(tran.Time);
            currents.Add(source.Current);
        }

        // The integration method ramps up to the maximum step over the first few timepoints, and the
        // last one is cut short to land on the stop time. Everything in between is a uniform grid,
        // which is what the periodogram below needs and what a circuit without charge states gives.
        const int skip = 32;
        var samples = currents.GetRange(skip, currents.Count - skip - 1);
        double worstStep = 0.0;
        for (int i = skip + 1; i < times.Count - 1; i++)
            worstStep = Math.Max(worstStep, Math.Abs(((times[i] - times[i - 1]) / step) - 1.0));
        Assert.That(worstStep, Is.LessThan(1e-9), "sampling grid");
        Assert.That(samples, Has.Count.GreaterThan(10000));

        var state = tran.GetState<ITimeNoiseSimulationState>();
        var weights = state.GetFlickerWeights(exponent);
        int order = state.BandLimitOrder;

        // Two bands a decade apart, each averaging enough periodogram bins for the estimate to be
        // worth comparing against a closed form. Both the level and the ratio between them are
        // checked, so a wrong normalization and a wrong slope both fail.
        Assert.Multiple(() =>
        {
            foreach ((double low, double high) in new[] { (5e3, 2e4), (5e4, 2e5) })
            {
                double actual = BandPower(samples, step, low, high);
                double expected = coefficient * Integrate(f => LadderDensity(state.FlickerRates, weights, order, f), low, high);
                Assert.That(actual, Is.EqualTo(expected).Within(20.0).Percent, $"{low:G3} Hz to {high:G3} Hz");
            }
        });
    }

    [Test]
    public void When_NoFlickerCoefficient_Expect_SilentSource()
    {
        // Every SPICE model defaults its flicker coefficient to zero, so this is the common case and
        // not an edge one. The source has to stay exactly silent, which is what lets it skip its
        // ladder rather than walk a dozen sections per timepoint to multiply the result by zero.
        var ckt = new Circuit(
            new CurrentSource("I1", "0", "out", 1e-3),
            new Diode("D1", "out", "0", "DM"),
            new Capacitor("C1", "out", "0", 1e-9),
            new DiodeModel("DM"));

        var tran = CreateSimulation(1e-6, 1e-9, 1e8);
        ITimeNoiseBehavior behavior = null;
        double worst = 0.0;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            behavior ??= tran.EntityBehaviors["D1"].GetValue<ITimeNoiseBehavior>();
            worst = Math.Max(worst, Math.Abs(behavior.GetProperty<ITimeNoiseSource>("flicker").Current));
        }

        Assert.Multiple(() =>
        {
            Assert.That(worst, Is.Zero, "current");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("flicker").NoiseDensity, Is.Zero, "density");
        });
    }

    [Test]
    public void When_Diode_Expect_FlickerDensity()
    {
        // The coefficient that the device hands to its flicker source has to be KF*|I|^AF, evaluated
        // at the conduction current of the junction.
        const double kf = 1e-14;
        const double af = 1.2;
        var model = new DiodeModel("DM");
        model.SetParameter("kf", kf);
        model.SetParameter("af", af);
        var ckt = new Circuit(
            new CurrentSource("I1", "0", "out", 1e-3),
            new Diode("D1", "out", "0", "DM"),
            new Capacitor("C1", "out", "0", 1e-9),
            model);

        var tran = CreateSimulation(1e-6, 1e-9, 1e8);
        tran.RunToEnd(ckt);

        var behavior = tran.EntityBehaviors["D1"].GetValue<ITimeNoiseBehavior>();
        double current = tran.EntityBehaviors["D1"].GetProperty<double>("id");
        Assert.That(behavior.GetProperty<ITimeNoiseSource>("flicker").NoiseDensity,
            Is.EqualTo(kf * Math.Pow(Math.Abs(current), af)).Within(0.1).Percent);
    }

    [Test]
    public void When_Bipolar_Expect_FlickerDensity()
    {
        const double kf = 1e-14;
        const double af = 1.5;
        var q = new BipolarJunctionTransistor("Q1") { Model = "QM" };
        q.Connect("out", "out", "0", "0");
        var model = new BipolarJunctionTransistorModel("QM");
        model.SetParameter("kf", kf);
        model.SetParameter("af", af);
        var ckt = new Circuit(
            new CurrentSource("I1", "0", "out", 1e-3),
            q,
            new Capacitor("C1", "out", "0", 1e-9),
            model);

        var tran = CreateSimulation(1e-6, 1e-9, 1e8);
        tran.RunToEnd(ckt);

        var behavior = tran.EntityBehaviors["Q1"].GetValue<ITimeNoiseBehavior>();
        double current = tran.EntityBehaviors["Q1"].GetProperty<double>("ib");
        Assert.That(behavior.GetProperty<ITimeNoiseSource>("flicker").NoiseDensity,
            Is.EqualTo(kf * Math.Pow(Math.Abs(current), af)).Within(0.1).Percent);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void When_Mosfet_Expect_FlickerDensity(int level)
    {
        // The flicker noise of a mosfet is referred to the gate oxide, so the coefficient carries the
        // channel area and the oxide capacitance on top of KF and AF.
        const double kf = 1e-26;
        const double af = 1.4;
        const double oxideThickness = 2e-8;
        const double width = 2e-5;
        const double length = 1e-5;

        IComponent mosfet = level switch
        {
            1 => new Mosfet1("M1") { Model = "MM" },
            2 => new Mosfet2("M1") { Model = "MM" },
            _ => new Mosfet3("M1") { Model = "MM" }
        };
        mosfet.Connect("out", "out", "0", "0");
        mosfet.SetParameter("w", width);
        mosfet.SetParameter("l", length);

        IEntity model = level switch
        {
            1 => new Mosfet1Model("MM"),
            2 => new Mosfet2Model("MM"),
            _ => new Mosfet3Model("MM")
        };
        model.SetParameter("kp", 1e-3);
        model.SetParameter("vto", 1.0);
        model.SetParameter("tox", oxideThickness);
        model.SetParameter("kf", kf);
        model.SetParameter("af", af);

        var ckt = new Circuit(
            new CurrentSource("I1", "0", "out", 1e-3),
            mosfet,
            new Capacitor("C1", "out", "0", 1e-9),
            model);

        var tran = CreateSimulation(1e-6, 1e-9, 1e8);
        tran.RunToEnd(ckt);

        var behavior = tran.EntityBehaviors["M1"].GetValue<ITimeNoiseBehavior>();
        double current = tran.EntityBehaviors["M1"].GetProperty<double>("id");
        double cox = 3.9 * 8.854214871e-12 / oxideThickness;
        double expected = kf * Math.Pow(Math.Abs(current), af) / (width * length * cox * cox);
        Assert.That(behavior.GetProperty<ITimeNoiseSource>("flicker").NoiseDensity,
            Is.EqualTo(expected).Within(0.1).Percent);
    }

    [Test]
    public void When_FlickerDominates_Expect_SameRealizationAcrossRuns()
    {
        // A flicker source holds one shaping section per ladder pole rather than one, so its rollback
        // and its reseeding are worth checking on their own: a section that leaked across a rejected
        // timepoint or across a rerun would break reproducibility without changing the statistics
        // enough to be visible anywhere else.
        var tran = CreateSimulation(1e-5, 1e-8, 1e7);
        var ckt = new Circuit(
            new Resistor("R1", "out", "0", 1e3),
            new FlickerInjector("N1", "out", "0", 1e-18, 1.0));

        var reference = new List<double>();
        ITimeNoiseSource source = null;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            source ??= tran.EntityBehaviors["N1"].GetValue<ITimeNoiseBehavior>();
            reference.Add(source.Current);
        }
        Assert.That(reference, Has.Count.GreaterThan(100));

        var same = new List<double>();
        foreach (int _ in tran.Rerun(Transient.ExportTransient))
            same.Add(source.Current);

        tran.NoiseParameters.Seed = 0x5EEE;
        var other = new List<double>();
        foreach (int _ in tran.Rerun(Transient.ExportTransient))
            other.Add(source.Current);

        Assert.Multiple(() =>
        {
            Assert.That(same, Is.EqualTo(reference), "same seed");
            Assert.That(other, Is.Not.EqualTo(reference), "different seed");
        });
    }

    /// <summary>
    /// Estimates the power that a uniformly sampled signal carries between two frequencies, by
    /// summing the periodogram over the bins that fall in the band.
    /// </summary>
    private static double BandPower(IReadOnlyList<double> samples, double step, double low, double high)
    {
        int n = samples.Count;
        double resolution = 1.0 / (n * step);
        int first = Math.Max(1, (int)Math.Ceiling(low / resolution));
        int last = Math.Min((n / 2) - 1, (int)Math.Floor(high / resolution));
        Assert.That(last - first, Is.GreaterThan(20), "periodogram bins");

        // Everything is accumulated relative to the mean of the run. A flicker process wanders, so
        // its mean over a finite window is not zero and would otherwise leak into the low bins.
        double mean = 0.0;
        for (int i = 0; i < n; i++)
            mean += samples[i];
        mean /= n;

        double power = 0.0;
        for (int k = first; k <= last; k++)
        {
            double real = 0.0, imaginary = 0.0;
            double angle = -2.0 * Math.PI * k / n;
            for (int i = 0; i < n; i++)
            {
                double value = samples[i] - mean;
                real += value * Math.Cos(angle * i);
                imaginary += value * Math.Sin(angle * i);
            }

            // The one-sided density of bin k is 2*dt/n times its squared magnitude, and the power in
            // the band is that density times the width 1/(n*dt) of a bin.
            power += 2.0 / ((double)n * n) * ((real * real) + (imaginary * imaginary));
        }
        return power;
    }

    /// <summary>
    /// Integrates a function over a band on a logarithmic grid.
    /// </summary>
    private static double Integrate(Func<double, double> density, double low, double high)
    {
        const int points = 2000;
        double ratio = Math.Pow(high / low, 1.0 / points);
        double sum = 0.0;
        double frequency = low;
        for (int i = 0; i < points; i++)
        {
            double next = frequency * ratio;
            sum += 0.5 * (density(frequency) + density(next)) * (next - frequency);
            frequency = next;
        }
        return sum;
    }

    private static NoiseTransient CreateSimulation(double stopTime, double step, double maximumFrequency)
    {
        var tran = new NoiseTransient("tran", new Trapezoidal
        {
            InitialStep = step,
            MaxStep = step,
            StopTime = stopTime,

            // The injected process is rough, so the truncation error estimator would otherwise keep
            // cutting the timestep. These tests are about the injected density, not about step
            // control.
            LteRelTol = 1e-1,
            LteAbsTol = 1e-1,
            ChargeTolerance = 1e-9
        });
        tran.NoiseParameters.MaximumNoiseFrequency = maximumFrequency;
        tran.NoiseParameters.Seed = 0x5EED;
        return tran;
    }
}
