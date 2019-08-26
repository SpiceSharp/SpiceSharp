using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A temperature behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITemperatureBehavior" />
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        private BehaviorList<ITemperatureBehavior> _behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            var sc = (SubcircuitBindingContext)context;
            // _behaviors = sc.Pool.GetBehaviorList<ITemperatureBehavior>();

            for (var i = 0; i < _behaviors.Count; i++)
                _behaviors[i].Bind(context);
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Temperature()
        {
            for (var i = 0; i < _behaviors.Count; i++)
                _behaviors[i].Temperature();
        }
    }
}
