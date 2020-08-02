namespace SpiceSharp.Simulations
{
    public abstract partial class BiasingSimulation
    {
        /// <summary>
        /// An <see cref="IIterationSimulationState"/> for a <see cref="BiasingSimulation"/>.
        /// </summary>
        /// <seealso cref="IIterationSimulationState" />
        protected class IterationState : IIterationSimulationState
        {
            /// <inheritdoc/>
            public IterationModes Mode { get; set; }

            /// <inheritdoc/>
            public double SourceFactor { get; set; } = 1.0;

            /// <inheritdoc/>
            public double Gmin { get; set; } = 1e-12;

            /// <inheritdoc/>
            public bool IsConvergent { get; set; } = true;
        }
    }
}
