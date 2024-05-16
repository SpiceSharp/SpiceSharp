using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// The binding context for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="BindingContext" />
    public class ParallelBindingContext : BindingContext
    {
        /// <summary>
        /// Gets the current simulation entity behaviors.
        /// </summary>
        public IBehaviorContainerCollection LocalBehaviors => Simulation.EntityBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelBindingContext"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="simulation">The simulation for which behaviors are created.</param>
        /// <param name="behaviors">The behaviors created by the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/>, <paramref name="simulation"/> or <paramref name="behaviors"/> is <c>null</c>.</exception>
        public ParallelBindingContext(IEntity entity, ParallelSimulation simulation, IBehaviorContainer behaviors)
            : base(entity, simulation, behaviors)
        {
        }


        /// <summary>
        /// Sets the state of the local simulation to another one.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The state.</param>
        public void AddLocalState<TState>(TState state)
            where TState : ISimulationState
        {
            var localSim = (ParallelSimulation)Simulation;
            localSim.LocalStates.Add(state);
        }

        /// <summary>
        /// Gets the behaviors from the local simulation.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>The list of behaviors.</returns>
        public BehaviorList<B> GetBehaviors<B>()
            where B : IBehavior
        {
            return Simulation.EntityBehaviors.GetBehaviorList<B>();
        }
    }
}
