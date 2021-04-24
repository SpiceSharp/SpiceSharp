using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="IFrequencyUpdateBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyUpdateBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class FrequencyUpdate : SubcircuitBehavior<IFrequencyUpdateBehavior>,
        IFrequencyUpdateBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyUpdate" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyUpdate(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        void IFrequencyUpdateBehavior.Update()
        {
            foreach (var behavior in Behaviors)
                behavior.Update();
        }
    }
}
