using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Interface that describes frequency-dependent behaviors.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface IFrequencyBehavior : IBehavior
    {
        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void InitializeParameters();

        /// <summary>
        /// Load the Y-matrix and right hand side vector for frequency domain analysis.
        /// </summary>
        void Load();
    }
}
