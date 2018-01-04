using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for noise analysis
    /// </summary>
    public abstract class NoiseBehavior: Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(Identifier name = null) : base(name) { }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public virtual void ConnectNoise()
        {
            // No noise to connect by default
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Noise(Circuit ckt);
    }
}
