using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Implements backward Euler integration with a fixed timesep. This is the
    /// fastest way, but also the least accurate. Any changes to the timestep are
    /// ignored.
    /// </summary>
    /// <seealso cref="SpiceSharp.IntegrationMethods.IntegrationMethod" />
    public partial class FixedEuler : IntegrationMethod
    {
        private double _fixedStep;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedEuler"/> class.
        /// </summary>
        public FixedEuler()
            : base(1)
        {
        }

        /// <summary>
        /// Initializes the integration method.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Initialize(TimeSimulation simulation)
        {
            base.Initialize(simulation);
            if (simulation.Configurations.TryGet(out TimeConfiguration config))
                _fixedStep = config.Step;
        }

        /// <summary>
        /// Starts probing a new timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="delta">The timestep to be probed.</param>
        public override void Probe(TimeSimulation simulation, double delta)
        {
            base.Probe(simulation, delta);
            Slope = 1.0 / _fixedStep;
        }

        /// <summary>
        /// Continues the simulation.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        /// <param name="delta">The initial probing timestep.</param>
        public override void Continue(TimeSimulation simulation, ref double delta)
        {
            base.Continue(simulation, ref delta);
            delta = _fixedStep;
        }

        /// <summary>
        /// Evaluates whether or not the current solution can be accepted.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="newDelta">The next requested timestep in case the solution is not accepted.</param>
        /// <returns>
        /// <c>true</c> if the time point is accepted; otherwise, <c>false</c>.
        /// </returns>
        public override bool Evaluate(TimeSimulation simulation, out double newDelta)
        {
            base.Evaluate(simulation, out _);
            newDelta = _fixedStep;
            return true;
        }

        /// <summary>
        /// Creates a state for which a derivative with respect to time can be determined.
        /// </summary>
        /// <param name="track">if set to <c>false</c>, the state is considered purely informative.</param>
        /// <returns>
        /// A <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> object that is compatible with this integration method.
        /// </returns>
        /// <remarks>
        /// Tracked derivatives are used in more advanced features implemented by the integration method.
        /// For example, derived states can be used for finding a good time step by approximating the
        /// local truncation error (ie. the error made by taking discrete time steps). If you do not
        /// want the derivative to participate in these features, set <paramref name="track" /> to false.
        /// </remarks>
        public override StateDerivative CreateDerivative(bool track)
        {
            return new FixedEulerStateDerivative(this);
        }
    }
}
