using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A behavior that allows access to the entity behaviors inside the subcircuit.
    /// </summary>
    [GeneratedParameters]
    public partial class EntitiesBehavior : Behavior, IEntitiesBehavior
    {
        private readonly SubcircuitBindingContext _context;

        /// <inheritdoc />
        public S GetState<S>() where S : ISimulationState => _context.GetState<S>();

        /// <inheritdoc />
        public bool TryGetState<S>(out S state) where S : ISimulationState => _context.TryGetState(out state);

        /// <inheritdoc />
        [ParameterName("behaviors"), ParameterInfo("The behaviors inside the subcircuit", IsPrincipal = true)]
        public IBehaviorContainerCollection LocalBehaviors => _context.LocalBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitiesBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public EntitiesBehavior(SubcircuitBindingContext context)
            : base(context)
        {
            _context = context;
        }
    }
}
