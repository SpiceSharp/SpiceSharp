using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A behavior that allows access to the entity behaviors inside the subcircuit.
    /// </summary>
    [GeneratedParameters]
    public partial class EntitiesBehavior : Behavior
    {
        private readonly SubcircuitBindingContext _context;

        /// <summary>
        /// Gets a simulation state from the used simulation.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>The simulation state.</returns>
        public S GetState<S>() where S : ISimulationState => _context.GetState<S>();

        /// <summary>
        /// Tries to get a simulation state from the used simulation.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <param name="state">The simulation state.</param>
        /// <returns><c>true</c> if the state was found; otherwise, <c>false</c>.</returns>
        public bool TryGetState<S>(out S state) where S : ISimulationState => _context.TryGetState(out state);

        /// <summary>
        /// Gets the local simulation behaviors.
        /// </summary>
        /// <value>
        /// The local behaviors.
        /// </value>
        [ParameterName("behaviors"), ParameterInfo("The behaviors inside the subcircuit", IsPrincipal = true)]
        public IBehaviorContainerCollection LocalBehaviors => _context.LocalBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitiesBehavior"/> class.
        /// </summary>
        /// <param name="context"></param>
        public EntitiesBehavior(SubcircuitBindingContext context)
            : base(context)
        {
            _context = context;
        }
    }
}
