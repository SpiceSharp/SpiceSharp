using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Noise behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="INoiseBehavior" />
    public class NoiseBehavior : SubcircuitBehavior<INoiseBehavior>, INoiseBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public NoiseBehavior(string name, SubcircuitBindingContext context) : base(name, context)
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
