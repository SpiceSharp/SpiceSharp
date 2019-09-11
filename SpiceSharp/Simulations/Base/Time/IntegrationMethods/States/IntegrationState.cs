using SpiceSharp.Algebra;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Represents the state of an integration method at a certain time point.
    /// </summary>
    public class IntegrationState
    {
        /// <summary>
        /// Gets or sets the timestep.
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// Gets the associated solution with the timepoint.
        /// </summary>
        public IVector<double> Solution { get; }

        /// <summary>
        /// Gets the states allocated by entities at this timepoint.
        /// </summary>
        public IVector<double> State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationState"/> class.
        /// </summary>
        /// <param name="delta">The timestep.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="state">The state.</param>
        public IntegrationState(double delta, IVector<double> solution, IVector<double> state)
        {
            Delta = delta;
            Solution = solution.ThrowIfNull(nameof(solution));
            State = state.ThrowIfNull(nameof(state));
        }
    }
}
