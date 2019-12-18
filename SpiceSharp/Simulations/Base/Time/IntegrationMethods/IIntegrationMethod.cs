using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface describing an integration method.
    /// </summary>
    /// <seealso cref="ISimulationState"/>.
    public interface IIntegrationMethod : ISimulationState
    {
        /// <summary>
        /// Gets the maximum order of the integration method.
        /// </summary>
        /// <value>
        /// The maximum order.
        /// </value>
        int MaxOrder { get; }

        /// <summary>
        /// Gets or sets the current order of the integration method.
        /// </summary>
        /// <value>
        /// The current order.
        /// </value>
        int Order { get; set; }

        /// <summary>
        /// Gets the base timepoint in seconds from which the current timepoint is being probed.
        /// </summary>
        /// <value>
        /// The base time.
        /// </value>
        double BaseTime { get; }

        /// <summary>
        /// Gets the currently probed timepoint in seconds.
        /// </summary>
        /// <value>
        /// The current time.
        /// </value>
        double Time { get; }

        /// <summary>
        /// Gets the derivative factor of any quantity that is being derived
        /// by the integration method.
        /// </summary>
        /// <value>
        /// The slope.
        /// </value>
        double Slope { get; }

        /// <summary>
        /// Registers an integration state with the integration method.
        /// </summary>
        /// <param name="state">The integration state.</param>
        void RegisterState(IIntegrationState state);

        /// <summary>
        /// Creates a derivative.
        /// </summary>
        /// <param name="track">If set to <c>true</c>, the integration method will use this state to limit truncation errors.</param>
        /// <returns>The derivative.</returns>
        IDerivative CreateDerivative(bool track = true);

        /// <summary>
        /// Gets a previous solution used by the integration method. An index of 0 indicates the last accepted solution.
        /// </summary>
        /// <param name="index">The number of points to go back.</param>
        /// <returns>
        /// The previous solution.
        /// </returns>
        IVector<double> GetPreviousSolution(int index);

        /// <summary>
        /// Gets a previous timestep. An index of 0 indicates the current timestep.
        /// </summary>
        /// <param name="index">The number of points to go back.</param>
        /// <returns>
        /// The previous timestep.
        /// </returns>
        double GetPreviousTimestep(int index);

        /// <summary>
        /// Initializes the integration method using the allocated biasing state.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Initializes the integration states.
        /// </summary>
        void InitializeStates();

        /// <summary>
        /// Prepares the integration method for calculating the next timepoint.
        /// The integration method may change the suggested timestep if needed.
        /// </summary>
        /// <param name="delta">The initial timestep to try.</param>
        void Prepare(ref double delta);

        /// <summary>
        /// Probes a new timepoint.
        /// </summary>
        /// <param name="delta">The timestep to probe.</param>
        void Probe(double delta);

        /// <summary>
        /// Evaluates the solution at the probed timepoint. If the solution is invalid,
        /// the analysis should roll back and try a smaller timestep. 
        /// </summary>
        /// <param name="newDelta">A new timestep suggested by the method if the probed timepoint is invalid.</param>
        /// <returns>
        /// <c>true</c> if the solution is a valid solution; otherwise, <c>false</c>.
        /// </returns>
        bool Evaluate(out double newDelta);

        /// <summary>
        /// Accepts the last probed timepoint.
        /// </summary>
        void Accept();

        /// <summary>
        /// Rejects the last probed timepoint. This method can be called if no
        /// solution could be found.
        /// </summary>
        /// <param name="newDelta">A new timestep suggested by the method.</param>
        void Reject(out double newDelta);
    }
}
