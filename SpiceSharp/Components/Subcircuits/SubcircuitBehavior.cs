using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A template for a subcircuit behavior.
    /// </summary>
    /// <typeparam name="B">The behavior type.</typeparam>
    /// <seealso cref="Behavior" />
    public abstract class SubcircuitBehavior<B> : Behavior,
        ISubcircuitBehavior
        where B : IBehavior
    {
        /// <summary>
        /// Gets the behaviors in the subcircuit.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        protected BehaviorList<B> Behaviors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBehavior{B}" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        protected SubcircuitBehavior(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(SubcircuitBindingContext context)
        {
            Behaviors = context.GetBehaviors<B>();
        }
    }
}
