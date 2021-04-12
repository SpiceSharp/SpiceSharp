using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="IAcceptBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class Accept : SubcircuitBehavior<IAcceptBehavior>,
        IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Accept" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Accept(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Accept()
        {
            foreach (var behavior in Behaviors)
                behavior.Accept();
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Probe()
        {
            foreach (var behavior in Behaviors)
                behavior.Probe();
        }
    }
}
