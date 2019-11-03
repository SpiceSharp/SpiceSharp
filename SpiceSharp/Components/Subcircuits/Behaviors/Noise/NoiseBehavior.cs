using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Noise behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SubcircuitBehaviors.SubcircuitBehavior{T}" />
    /// <seealso cref="SpiceSharp.Behaviors.INoiseBehavior" />
    public class NoiseBehavior : SubcircuitBehavior<INoiseBehavior>, INoiseBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public NoiseBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        public void Noise()
        {
            foreach (var bs in Behaviors)
            {
                foreach (var behavior in bs)
                    behavior.Noise();
            }
        }
    }
}
