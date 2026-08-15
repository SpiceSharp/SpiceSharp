using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for the band-limited noise process that a transient noise analysis injects: first on its
/// own, then driven into a circuit through a hand-placed noise current source.
/// </summary>
[TestFixture]
public class TimeNoiseShapingTests
{
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_SampledOverIrregularSteps_Expect_UnitVarianceAndAutocorrelation(int order)
    {
        // The transition is exact for any step, so a deliberately irregular step sequence spanning
        // several decades of normalized timestep must still produce a stationary unit-variance
        // process with an autocorrelation of exp(-z) at order 1 and exp(-z)*(1+z) at order 2.
        const int n = 500000;
        var draws = new NoiseRandomStream(20250814ul);
        var steps = new NoiseRandomStream(11ul);

        // Start from the stationary distribution, so that there is no startup transient to average
        // away.
        double state1 = 0.0, state2 = 0.0;
        draws.NextNormals(out double normal1, out double normal2);
        double previous = TimeNoisePoint.Stationary(order).Propagate(ref state1, ref state2, normal1, normal2);

        double squares = 0.0, products = 0.0, expected = 0.0;
        for (int i = 0; i < n; i++)
        {
            // Log-uniform normalized timestep from 1e-3 to about 3.
            double z = Math.Pow(10.0, 0.5 - (3.5 * steps.NextDouble()));
            var point = new TimeNoisePoint(z, order);
            draws.NextNormals(out normal1, out normal2);
            double value = point.Propagate(ref state1, ref state2, normal1, normal2);

            squares += value * value;
            products += value * previous;
            expected += point.Decay + point.Coupling;
            previous = value;
        }

        Assert.Multiple(() =>
        {
            Assert.That(squares / n, Is.EqualTo(1.0).Within(2.0).Percent, "variance");
            Assert.That(products / n, Is.EqualTo(expected / n).Within(4.0).Percent, "autocorrelation");
        });
    }

    [Test]
    public void When_RcLowpassDrivenByShapedNoise_Expect_KtOverC()
    {
        // A resistor in parallel with a capacitor, driven by a hand-placed noise current source of
        // the thermal density of that resistor. Without the band limit the output variance would be
        // kT/C exactly; with a one-pole shaping filter at fmax it is kT/C times fmax/(fmax + fp).
        const double resistance = 1e3;
        const double capacitance = 1e-9;
        const double density = 4e-12;                                   // = 4*k*T/R for a fictitious temperature
        const double stopTime = 4e-3;
        const double step = 2e-8;
        const double burnIn = 2e-5;                                     // 20 time constants

        double poleFrequency = 1.0 / (2.0 * Math.PI * resistance * capacitance);
        double maximumFrequency = 2.0 * poleFrequency;
        double ktOverC = density * resistance / (4.0 * capacitance);
        double expected = ktOverC * maximumFrequency / (maximumFrequency + poleFrequency);

        var ckt = new Circuit(
            new CurrentSource("I1", "0", "out", new ShapedNoiseCurrent
            {
                NoiseDensity = density,
                MaximumNoiseFrequency = maximumFrequency,
                BandLimitOrder = 1,
                Seed = 0x5EEDul
            }),
            new Resistor("R1", "out", "0", resistance),
            new Capacitor("C1", "out", "0", capacitance));

        var tran = new Transient("tran", new Trapezoidal
        {
            InitialStep = step,
            MaxStep = step,
            StopTime = stopTime,

            // The injected process is rough, so the truncation error estimator would otherwise keep
            // cutting the timestep. This test is about the injected power, not about step control.
            LteRelTol = 1e-1,
            LteAbsTol = 1e-1,
            ChargeTolerance = 1e-9
        });

        double weight = 0.0, sum = 0.0, squares = 0.0, previousTime = 0.0;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            double time = tran.Time;
            double dt = time - previousTime;
            previousTime = time;
            if (time < burnIn)
                continue;

            // Weigh by the timestep: the step controller reacts to the noise, so raw timepoints are
            // not an unbiased sampling grid.
            double v = tran.GetVoltage("out");
            weight += dt;
            sum += dt * v;
            squares += dt * v * v;
        }

        Assert.That(weight, Is.GreaterThan(0.0));
        double mean = sum / weight;
        double variance = (squares / weight) - (mean * mean);
        Assert.Multiple(() =>
        {
            Assert.That(mean, Is.EqualTo(0.0).Within(0.15 * Math.Sqrt(expected)), "mean");
            // A Monte-Carlo estimate over roughly 2000 correlation times, so the relative standard
            // error is a few percent. The window is wide enough for that and still narrow enough to
            // reject the unbanded kT/C, a two-sided density, or a missing noise-bandwidth factor.
            Assert.That(variance, Is.EqualTo(expected).Within(15.0).Percent, "variance");
        });
    }

    /// <summary>
    /// A waveform that produces a band-limited noise current of a given one-sided power spectral
    /// density. This is the hand-placed stand-in for the noise sources that a transient noise
    /// analysis will stamp itself.
    /// </summary>
    private class ShapedNoiseCurrent : ParameterSet<IWaveformDescription>, IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the one-sided power spectral density in A^2/Hz.
        /// </summary>
        public double NoiseDensity { get; set; }

        /// <summary>
        /// Gets or sets the band limit in Hz.
        /// </summary>
        public double MaximumNoiseFrequency { get; set; }

        /// <summary>
        /// Gets or sets the number of shaping poles.
        /// </summary>
        public int BandLimitOrder { get; set; } = 1;

        /// <summary>
        /// Gets or sets the seed of the random stream.
        /// </summary>
        public ulong Seed { get; set; }

        /// <inheritdoc/>
        public IWaveform Create(IBindingContext context)
        {
            IIntegrationMethod method = null;
            context?.TryGetState(out method);
            return new Instance(method, NoiseDensity, MaximumNoiseFrequency, BandLimitOrder, Seed);
        }

        private class Instance : IWaveform
        {
            private readonly IIntegrationMethod _method;
            private readonly NoiseRandomStream _stream;
            private readonly double _lambda, _amplitude;
            private readonly int _order;
            private double _accepted1, _accepted2;
            private double _probed1, _probed2;

            /// <inheritdoc/>
            public double Value { get; private set; }

            public Instance(IIntegrationMethod method, double density, double maximumFrequency, int order, ulong seed)
            {
                _method = method;
                _order = order;
                _lambda = 2.0 * Math.PI * maximumFrequency;
                _stream = new NoiseRandomStream(seed);

                // The equivalent noise bandwidth of an n-pole shape is k_n * fmax, with
                // k_n = (pi/2) * (2n-3)!! / (2n-2)!!.
                double k = order == 1 ? Math.PI / 2.0 : Math.PI / 4.0;
                _amplitude = Math.Sqrt(k * density * maximumFrequency);

                // Start from the stationary distribution.
                _stream.NextNormals(out double normal1, out double normal2);
                Value = _amplitude * TimeNoisePoint.Stationary(order)
                    .Propagate(ref _accepted1, ref _accepted2, normal1, normal2);
                _probed1 = _accepted1;
                _probed2 = _accepted2;
            }

            /// <inheritdoc/>
            public void Probe()
            {
                if (_method == null)
                    return;

                // Always advance the last *accepted* state: a rejected step simply consumes draws
                // that are then discarded.
                _probed1 = _accepted1;
                _probed2 = _accepted2;
                var point = new TimeNoisePoint(_lambda * (_method.Time - _method.BaseTime), _order);
                _stream.NextNormals(out double normal1, out double normal2);
                Value = _amplitude * point.Propagate(ref _probed1, ref _probed2, normal1, normal2);
            }

            /// <inheritdoc/>
            public void Accept()
            {
                _accepted1 = _probed1;
                _accepted2 = _probed2;
            }
        }
    }
}
