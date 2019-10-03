using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A behavior for a subcircuit component.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    public class SubcircuitBehavior<T> : Behavior where T : IBehavior
    {
        /// <summary>
        /// Gets the behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        protected BehaviorList<T> Behaviors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBehavior{T}"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public SubcircuitBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            // Bind the behaviors in the subcircuit here
            var sc = (SubcircuitBindingContext)context;
            Behaviors = sc.Pool.GetBehaviorList<T>();
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            for (var i = 0; i < Behaviors.Count; i++)
                Behaviors[i].Unbind();
            Behaviors = null;
        }
    }
}
