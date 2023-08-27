using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="ITimeBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITimeBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class Time : Biasing,
        ITimeBehavior
    {
        private BehaviorList<ITimeBehavior> _behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Time(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override void FetchBehaviors(SubcircuitBindingContext context)
        {
            base.FetchBehaviors(context);
            _behaviors = context.GetBehaviors<ITimeBehavior>();
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            foreach (var behavior in _behaviors)
                behavior.InitializeStates();
        }
    }
}
