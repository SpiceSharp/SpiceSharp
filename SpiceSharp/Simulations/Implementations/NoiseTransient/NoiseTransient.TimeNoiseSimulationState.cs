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
        private readonly double _lambda;
        private readonly int _seed;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeNoiseSimulationState"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The integration method that the shaping states are registered with.</param>
        /// <param name="parameters">The parameters of the transient noise analysis.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>,
        /// <paramref name="method"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        public TimeNoiseSimulationState(string name, IIntegrationMethod method, NoiseTransientParameters parameters)
        {
            Name = name.ThrowIfNull(nameof(name));
            _method = method.ThrowIfNull(nameof(method));
            parameters.ThrowIfNull(nameof(parameters));

            MaximumNoiseFrequency = parameters.MaximumNoiseFrequency;
            BandLimitOrder = parameters.BandLimitOrder;
            _seed = parameters.Seed;
            _lambda = 2.0 * Math.PI * MaximumNoiseFrequency;

            // The equivalent noise bandwidth of an n-pole shape is k_n * fmax, with
            // k_n = (pi/2) * (2n-3)!! / (2n-2)!!.
            double k = BandLimitOrder == 1 ? Math.PI / 2.0 : Math.PI / 4.0;
            AmplitudeScale = Math.Sqrt(k * MaximumNoiseFrequency);
            Point = TimeNoisePoint.Stationary(BandLimitOrder);
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
                source.Initialize(NoiseRandomStream.CreateSeed(_seed, source.Name));
        }

        /// <summary>
        /// Computes the band-limit shaping coefficients for the timestep that is being probed.
        /// </summary>
        /// <param name="delta">The probed timestep, in seconds.</param>
        /// <remarks>
        /// The analogue of the frequency-domain <c>SetCurrentPoint</c> of a <see cref="Noise"/>
        /// analysis: the transcendentals that only depend on the point are evaluated once here, and
        /// every noise source of the circuit then reads them from <see cref="Point"/>.
        /// </remarks>
        public void SetCurrentPoint(double delta)
        {
            Point = new TimeNoisePoint(_lambda * delta, BandLimitOrder);
        }
    }
}
