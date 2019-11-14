using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A behavior for a subcircuit component.
    /// </summary>
    /// <seealso cref="Behavior" />
    public class SubcircuitBehavior<T> : Behavior where T : IBehavior
    {
        /// <summary>
        /// Gets the simulations.
        /// </summary>
        /// <value>
        /// The simulations.
        /// </value>
        protected SubcircuitSimulation[] Simulations { get; private set; }

        /// <summary>
        /// Gets the behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        protected BehaviorList<T>[] Behaviors { get; private set; }

        /// <summary>
        /// Gets the behaviors of the subcircuit.
        /// </summary>
        /// <returns>
        /// An array of all <see cref="BehaviorContainerCollection"/>.
        /// </returns>
        [ParameterName("behaviors"), ParameterInfo("The behavior container for a specific entity", IsPrincipal = true)]
        public BehaviorContainerCollection[] GetBehaviors()
        {
            if (Simulations == null || Simulations.Length == 0)
                return null;
            var result = new BehaviorContainerCollection[Simulations.Length];
            for (var i = 0; i < Simulations.Length; i++)
                result[i] = Simulations[i].EntityBehaviors;
            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBehavior{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public SubcircuitBehavior(string name, SubcircuitBindingContext context)
            : base(name)
        {
            context.ThrowIfNull(nameof(context));

            Simulations = context.Simulations;
            Behaviors = new BehaviorList<T>[Simulations.Length];
            for (var i = 0; i < Simulations.Length; i++)
                Behaviors[i] = Simulations[i].EntityBehaviors.GetBehaviorList<T>();
        }
    }
}
