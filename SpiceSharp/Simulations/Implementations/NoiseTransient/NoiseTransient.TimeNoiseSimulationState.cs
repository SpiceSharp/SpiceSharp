using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations;

public partial class NoiseTransient
{
    /// <summary>
    /// A class that represents the state of a <see cref="NoiseTransient"/> analysis.
    /// </summary>
    /// <seealso cref="ITimeNoiseSimulationState" />
    protected class TimeNoiseSimulationState : ITimeNoiseSimulationState
    {
        private readonly IIntegrationMethod _method;
        private readonly List<TimeNoiseSource> _sources = [];
        private readonly Dictionary<double, FlickerWeights> _weights = [];
        private readonly double[] _rates;
        private readonly double _lambda;
        private readonly NoiseTransientParameters _parameters;
        private TimeNoisePoint[] _ladder;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public double MaximumNoiseFrequency { get; }

        /// <inheritdoc/>
        public int BandLimitOrder { get; }

        /// <inheritdoc/>
        public double AmplitudeScale { get; }

        /// <inheritdoc/>
        public TimeNoisePoint Point { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<double> FlickerRates => _rates;

        /// <inheritdoc/>
        public IReadOnlyList<TimeNoisePoint> FlickerLadder => _ladder ?? [];

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeNoiseSimulationState"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The integration method that the shaping states are registered with.</param>
        /// <param name="parameters">The parameters of the transient noise analysis.</param>
        /// <param name="stopTime">The length of the run, in seconds. Its reciprocal is the lowest frequency at which a flicker noise source can carry any power.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>,
        /// <paramref name="method"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        public TimeNoiseSimulationState(string name, IIntegrationMethod method, NoiseTransientParameters parameters, double stopTime)
        {
            Name = name.ThrowIfNull(nameof(name));
            _method = method.ThrowIfNull(nameof(method));
            _parameters = parameters.ThrowIfNull(nameof(parameters));

            MaximumNoiseFrequency = parameters.MaximumNoiseFrequency;
            BandLimitOrder = parameters.BandLimitOrder;
            _lambda = 2.0 * Math.PI * MaximumNoiseFrequency;

            // The equivalent noise bandwidth of an n-pole shape is k_n * fmax, with
            // k_n = (pi/2) * (2n-3)!! / (2n-2)!!.
            double k = BandLimitOrder == 1 ? Math.PI / 2.0 : Math.PI / 4.0;
            AmplitudeScale = Math.Sqrt(k * MaximumNoiseFrequency);
            Point = TimeNoisePoint.Stationary(BandLimitOrder);
            _rates = CreateFlickerRates(stopTime);
        }

        /// <summary>
        /// Computes the rates of the flicker noise ladder, logarithmically spaced from the reciprocal
        /// of the run length - extended downward by the configured guard decades - up to the band
        /// limit.
        /// </summary>
        /// <param name="stopTime">The length of the run, in seconds.</param>
        /// <returns>The rates of the ladder sections, in rad/s.</returns>
        private double[] CreateFlickerRates(double stopTime)
        {
            double maximum = _lambda;
            double minimum = stopTime > 0.0 ? 2.0 * Math.PI / stopTime : maximum;
            minimum /= Math.Pow(10.0, _parameters.FlickerGuardDecades);

            // A run that is shorter than one period of the band limit leaves no decades to spread the
            // ladder over. Fall back on a single decade, which keeps the ladder well-formed.
            if (minimum >= maximum)
                minimum = maximum / 10.0;

            double decades = Math.Log10(maximum / minimum);
            int count = Math.Max(2, (int)Math.Ceiling(decades * _parameters.FlickerSectionsPerDecade) + 1);

            double ratio = Math.Pow(maximum / minimum, 1.0 / (count - 1));
            double[] rates = new double[count];
            rates[0] = minimum;
            for (int i = 1; i < count; i++)
                rates[i] = rates[i - 1] * ratio;

            // The top of the ladder is the band limit, exactly rather than to within the rounding of
            // count-1 multiplications.
            rates[count - 1] = maximum;
            return rates;
        }

        /// <inheritdoc/>
        public FlickerWeights GetFlickerWeights(double exponent)
        {
            if (!_weights.TryGetValue(exponent, out var weights))
            {
                weights = new FlickerWeights(exponent, _rates, BandLimitOrder);
                _weights.Add(exponent, weights);
            }

            // The ladder is only maintained once somebody reads it, so that a circuit without flicker
            // noise does not pay one exponential per section per timepoint for nothing.
            _ladder ??= new TimeNoisePoint[_rates.Length];
            return weights;
        }

        /// <inheritdoc/>
        public void Register(TimeNoiseSource source)
        {
            source.ThrowIfNull(nameof(source));
            _sources.Add(source);
            _method.RegisterState(source);
        }

        /// <summary>
        /// Restarts every registered noise source. The seed of a source is derived from the master
        /// seed and from the name of the source, so that adding an unrelated device to the circuit
        /// leaves the realization of every other source untouched.
        /// </summary>
        public void Initialize()
        {
            foreach (var source in _sources)
                source.Initialize(NoiseRandomStream.CreateSeed(_parameters.Seed, source.Name));
        }

        /// <summary>
        /// Computes the band-limit shaping coefficients for the timestep that is being probed.
        /// </summary>
        /// <param name="delta">The probed timestep, in seconds.</param>
        /// <remarks>
        /// The analogue of the frequency-domain <c>SetCurrentPoint</c> of a <see cref="Noise"/>
        /// analysis: the transcendentals that only depend on the point are evaluated once here, and
        /// every noise source of the circuit then reads them from <see cref="Point"/> and <see cref="FlickerLadder"/>.
        /// </remarks>
        public void SetCurrentPoint(double delta)
        {
            Point = new TimeNoisePoint(_lambda * delta, BandLimitOrder);
            if (_ladder != null)
            {
                for (int i = 0; i < _ladder.Length; i++)
                    _ladder[i] = new TimeNoisePoint(_rates[i] * delta, BandLimitOrder);
            }
        }
    }
}
