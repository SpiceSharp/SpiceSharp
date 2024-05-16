using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A behavior that is used by <see cref="Noise" />.
    /// </summary>
    /// <seealso cref="IBehavior" />
    [SimulationBehavior]
    public interface INoiseBehavior : INoiseSource, IBehavior
    {
        /// <summary>
        /// Loads the noise behavior. Can be used to apply the inputs of the noise simulation.
        /// </summary>
        /// <remarks>
        /// This method is run right before forward substitution of the transposed matrix 
        /// when iterating for noise.
        /// </remarks>
        void Load();

        /// <summary>
        /// Computes the noise contributions.
        /// </summary>
        /// <remarks>
        /// This method is called right after performing backward substitution
        /// and storing the solution.
        /// </remarks>
        void Compute();
    }
}
