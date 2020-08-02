using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// Represents the state of an integration method at a certain time point.
    /// </summary>
    public class SpiceIntegrationState
    {
        /// <summary>
        /// Gets or sets the timestep.
        /// </summary>
        /// <value>
        /// The delta.
        /// </value>
        public double Delta { get; set; }

        /// <summary>
        /// Gets the associated solution with the timepoint.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IVector<double> Solution { get; }

        /// <summary>
        /// Gets the states allocated by entities at this timepoint.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public IVector<double> State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceIntegrationState"/> class.
        /// </summary>
        /// <param name="delta">The timestep.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="states">The number of states to keep for derivatives.</param>
        public SpiceIntegrationState(double delta, IVector<double> solution, int states)
        {
            Delta = delta;
            Solution = solution.ThrowIfNull(nameof(solution));
            State = new DenseVector<double>(states);
        }
    }
}
