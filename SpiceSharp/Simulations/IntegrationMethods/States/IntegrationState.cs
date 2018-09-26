using SpiceSharp.Algebra;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Represents the state of an integration method at a certain time point
    /// </summary>
    public class IntegrationState
    {
        /// <summary>
        /// Gets or sets the time step
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// Gets the associated solution
        /// </summary>
        public Vector<double> Solution { get; }

        /// <summary>
        /// Gets the states allocated by entities
        /// </summary>
        public Vector<double> State { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="solution"></param>
        /// <param name="state"></param>
        public IntegrationState(double delta, Vector<double> solution, Vector<double> state)
        {
            Delta = delta;
            Solution = solution;
            State = state;
        }
    }
}
