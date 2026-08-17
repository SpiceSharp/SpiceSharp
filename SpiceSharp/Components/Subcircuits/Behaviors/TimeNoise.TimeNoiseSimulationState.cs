using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits;

public partial class TimeNoise
{
    /// <summary>
    /// An <see cref="ITimeNoiseSimulationState"/> for the entities inside a <see cref="Subcircuit"/>.
    /// Everything that is shared by the whole circuit is taken from the parent state; only the name
    /// that a noise source registers under is qualified with the name of the subcircuit instance.
    /// </summary>
    /// <seealso cref="ITimeNoiseSimulationState" />
    protected class TimeNoiseSimulationState : ITimeNoiseSimulationState
    {
        private readonly ITimeNoiseSimulationState _parent;
        private readonly string _instance;

        /// <inheritdoc/>
        public double MaximumNoiseFrequency => _parent.MaximumNoiseFrequency;

        /// <inheritdoc/>
        public int BandLimitOrder => _parent.BandLimitOrder;

        /// <inheritdoc/>
        public TimeNoisePoint Point => _parent.Point;

        /// <inheritdoc/>
        public IReadOnlyList<double> FlickerRates => _parent.FlickerRates;

        /// <inheritdoc/>
        public IReadOnlyList<TimeNoisePoint> FlickerLadder => _parent.FlickerLadder;

        /// <inheritdoc/>
        public double AmplitudeScale => _parent.AmplitudeScale;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeNoiseSimulationState"/> class.
        /// </summary>
        /// <param name="instance">The name of the subcircuit instance.</param>
        /// <param name="parent">The parent transient noise simulation state.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> or
        /// <paramref name="parent"/> is <c>null</c>.</exception>
        public TimeNoiseSimulationState(string instance, ITimeNoiseSimulationState parent)
        {
            _instance = instance.ThrowIfNull(nameof(instance));
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <inheritdoc/>
        public FlickerWeights GetFlickerWeights(double exponent) => _parent.GetFlickerWeights(exponent);

        /// <inheritdoc/>
        public void Register(TimeNoiseSource source, string name)
            => _parent.Register(source, _instance.Combine(name));
    }
}
