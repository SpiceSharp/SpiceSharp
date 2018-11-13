using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A template that describes noise analysis behavior.
    /// </summary>
    /// <seealso cref="Behavior" />
    public abstract class BaseNoiseBehavior : Behavior, INoiseBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseNoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseNoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Connects the noise sources.
        /// </summary>
        public virtual void ConnectNoise()
        {
            // No noise to connect by default
        }

        /// <summary>
        /// Perform noise calculations.
        /// </summary>
        /// <param name="simulation">The noise simulation.</param>
        public abstract void Noise(Noise simulation);
    }
}
