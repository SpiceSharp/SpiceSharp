using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SubcircuitBehaviors.SubcircuitBehavior{T}" />
    /// <seealso cref="SpiceSharp.Behaviors.IFrequencyBehavior" />
    public class FrequencyBehavior : SubcircuitBehavior<IFrequencyBehavior>, IFrequencyBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        public void InitializeParameters()
        {
            for (var i = 0; i < Behaviors.Count; i++)
                Behaviors[i].InitializeParameters();
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        public void Load()
        {
            for (var i = 0; i < Behaviors.Count; i++)
                Behaviors[i].Load();
        }
    }
}
