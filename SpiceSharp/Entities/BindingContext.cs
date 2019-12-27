using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> to an <see cref="ISimulation"/>.
    /// </summary>
    /// <remarks>
    /// This is an additional layer that allows to shield entities, simulations, etc. from the behavior that
    /// is being created. This makes sure that behaviors are only using the data that matters.
    /// </remarks>
    public class BindingContext
    {
        /// <summary>
        /// Gets the simulation to bind to without exposing the simulation itself.
        /// </summary>
        /// <value>
        /// The simulation.
        /// </value>
        protected ISimulation Simulation { get; }

        /// <summary>
        /// Gets the entity that provides the parameters without exposing the entity itself.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        protected IEntity Entity { get; }

        /// <summary>
        /// Gets a value indicating whether parameters should be linked. If not, then
        /// the parameters are cloned instead of referenced.
        /// </summary>
        /// <value>
        ///   <c>true</c> if parameters should be linked; otherwise, <c>false</c>.
        /// </value>
        protected bool LinkParameters { get; }

        /// <summary>
        /// Gets a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <returns>The simulation state.</returns>
        public S GetState<S>() where S : ISimulationState
        {
            var state = Simulation.GetState<S>();
            if (state == null)
                throw new StateNotDefinedException(typeof(S));
            return state;
        }

        /// <summary>
        /// Tries to get a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <param name="state">The simulation state.</param>
        /// <returns>
        /// <c>true</c> if the state was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetState<S>(out S state) where S : ISimulationState
        {
            state = Simulation.GetState<S>();
            return state != null;
        }

        /// <summary>
        /// Gets a simulation parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>The parameter set.</returns>
        public P GetSimulationParameterSet<P>() where P : IParameterSet
            => Simulation.GetParameterSet<P>();

        /// <summary>
        /// Tries to get a simulation parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The parameter set.</returns>
        public bool TryGetSimulationParameterSet<P>(out P value) where P : IParameterSet
            => Simulation.TryGetParameterSet(out value);

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            var parameters = Entity.GetParameterSet<P>();
            if (LinkParameters)
                return parameters;
            return (P)parameters.Clone();
        }

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet
        {
            if (Entity.TryGetParameterSet(out value))
            {
                if (!LinkParameters)
                    value = (P)value.Clone();
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableSet Variables => Simulation.Variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="entity">The entity creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
        public BindingContext(IEntity entity, ISimulation simulation, bool linkParameters)
        {
            Entity = entity.ThrowIfNull(nameof(entity));
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            LinkParameters = linkParameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="entity">The entity creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        public BindingContext(Entity entity, ISimulation simulation)
        {
            Entity = entity.ThrowIfNull(nameof(entity));
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            LinkParameters = entity.LinkParameters;
        }
    }
}
