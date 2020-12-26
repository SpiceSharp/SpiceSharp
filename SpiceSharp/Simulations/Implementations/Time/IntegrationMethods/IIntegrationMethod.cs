using SpiceSharp.Algebra;
using System;

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
        /// Creates an integrator.
        /// </summary>
        /// <param name="track">If set to <c>true</c>, the integration method will use this state to limit truncation errors.</param>
        /// <returns>The integrator.</returns>
        IIntegral CreateIntegral(bool track = true);

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
        /// At this point, all entities should have received the chance to allocate and register integration states.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Accepts a solution at the current timepoint.
        /// </summary>
        void Accept();

        /// <summary>
        /// Prepares the integration method for calculating the next timepoint.
        /// </summary>
        void Prepare();

        /// <summary>
        /// Probes a new timepoint.
        /// </summary>
        void Probe();

        /// <summary>
        /// Evaluates the current solution at the probed timepoint. If the solution is invalid,
        /// the analysis should roll back and try again.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current solution is a valid solution; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="maxTimestep"/> is not positive.</exception>
        bool Evaluate(double maxTimestep);

        /// <summary>
        /// Rejects the last probed timepoint as a valid solution. This method can be called if no solution could be found (eg. due to non-convergence).
        /// </summary>
        void Reject();

        /// <summary>
        /// Truncate the current timestep.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="maxTimestep"/> is not positive.</exception>
        void Truncate(double maxTimestep);
    }
}
