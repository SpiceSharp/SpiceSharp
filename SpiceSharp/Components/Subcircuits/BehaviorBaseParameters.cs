using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <summary>
    /// This parameter set is ignored on the entity, but created automatically for the <see cref="SpiceSharp.Behaviors.BehaviorContainer"/>.
    /// </summary>
    public class BehaviorBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        [ParameterName("behaviors"), ParameterName("b"), ParameterInfo("The behaviors associated with the subcircuit instance")]
        public BehaviorContainerCollection Behaviors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorBaseParameters"/> class.
        /// </summary>
        /// <param name="behaviors">The behaviors.</param>
        public BehaviorBaseParameters(BehaviorContainerCollection behaviors)
        {
            Behaviors = behaviors.ThrowIfNull(nameof(behaviors));
        }
    }
}
