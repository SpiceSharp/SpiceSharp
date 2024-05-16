using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Context for binding an <see cref="IBehavior"/> to an <see cref="ISimulation"/>.
    /// </summary>
    /// <remarks>
    /// This is an additional layer that allows to shield entities, simulations, etc. from the behavior that
    /// is being created. This makes sure that behaviors are only using the data that matters.
    /// </remarks>
    /// <seealso cref="IBindingContext"/>
    [BindingContextFor(typeof(Entity))]
    public class BindingContext : IBindingContext
    {
        private readonly Dictionary<IParameterSet, IParameterSet> _cloned;

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

        /// <inheritdoc/>
        public IBehaviorContainer Behaviors { get; }

        /// <inheritdoc/>
        public S GetState<S>() where S : ISimulationState => Simulation.GetState<S>();

        /// <inheritdoc/>
        public bool TryGetState<S>(out S state) where S : ISimulationState => Simulation.TryGetState(out state);

        /// <inheritdoc/>
        public bool UsesState<S>() where S : ISimulationState => Simulation.UsesState<S>();

        /// <summary>
        /// Gets a simulation parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>The parameter set.</returns>
        public P GetSimulationParameterSet<P>() where P : IParameterSet, ICloneable<P> => Simulation.GetParameterSet<P>();

        /// <summary>
        /// Tries to get a simulation parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The parameter set.</returns>
        public bool TryGetSimulationParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P> => Simulation.TryGetParameterSet(out value);

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        public P GetParameterSet<P>() where P : IParameterSet, ICloneable<P>
        {
            var value = Entity.GetParameterSet<P>();

            // Are we using cloned parameter sets?
            if (_cloned != null)
            {
                if (!_cloned.TryGetValue(value, out var result))
                {
                    result = (IParameterSet)value.Clone();
                    _cloned.Add(value, result);
                }
                return (P)result;
            }

            // Linking parameters by reference
            return value;
        }

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P>
        {
            if (Entity.TryGetParameterSet(out value))
            {
                // Are we using cloned parameter sets?
                if (_cloned != null)
                {
                    if (!_cloned.TryGetValue(value, out var result))
                    {
                        result = (IParameterSet)value.Clone();
                        _cloned.Add(value, result);
                    }
                    value = (P)result;
                    return true;
                }

                // Return the original
                value = (P)value.Clone();
                return true;
            }

            // Linking parameters by reference
            value = default;
            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingContext"/> class.
        /// </summary>
        /// <param name="entity">The entity creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The behavior container.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> or <paramref name="simulation"/> is <c>null</c>.</exception>
        public BindingContext(IEntity entity, ISimulation simulation, IBehaviorContainer behaviors)
        {
            Entity = entity.ThrowIfNull(nameof(entity));
            Simulation = simulation.ThrowIfNull(nameof(simulation));
            Behaviors = behaviors;
            _cloned = entity.LinkParameters ? null : [];
        }
    }
}
