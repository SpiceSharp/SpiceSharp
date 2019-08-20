using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface for time-dependent behaviors.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.IBehavior" />
    public interface ITimeBehavior : IBehavior
    {
        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="StateDerivative" /> or <see cref="StateHistory" />.
        /// </remarks>
        void InitializeStates();

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void Load();
    }
}
