using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for noise analysis
    /// </summary>
    public abstract class BaseNoiseBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected BaseNoiseBehavior(Identifier name) : base(name) { }

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
        /// <param name="simulation">Noise simulation</param>
        public abstract void Noise(Noise simulation);
    }
}
