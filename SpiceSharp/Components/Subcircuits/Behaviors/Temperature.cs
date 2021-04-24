using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="ITemperatureBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class Temperature : SubcircuitBehavior<ITemperatureBehavior>,
        ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Temperature(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            foreach (var behavior in Behaviors)
                behavior.Temperature();
        }
    }
}
