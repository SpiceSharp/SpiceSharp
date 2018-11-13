using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A behavior that is used by <see cref="Noise" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.IBehavior" />
    public interface INoiseBehavior : IBehavior
    {
        /// <summary>
        /// Connects the noise generators in the circuit.
        /// </summary>
        void ConnectNoise();

        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        /// <param name="simulation">The noise simulation.</param>
        void Noise(Noise simulation);
    }
}
