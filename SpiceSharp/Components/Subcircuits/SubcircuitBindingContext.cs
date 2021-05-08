using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// The binding context for a <see cref="Subcircuit"/>. 
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class SubcircuitBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the node bridges.
        /// </summary>
        /// <value>
        /// The bridges.
        /// </value>
        public IReadOnlyList<Bridge<string>> Bridges => ((SubcircuitSimulation)Simulation).Nodes;

        /// <summary>
        /// Gets the current simulation entity behaviors.
        /// </summary>
        public IBehaviorContainerCollection LocalBehaviors => Simulation.EntityBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which behaviors are created.</param>
        /// <param name="behaviors">The behaviors created by the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/>, <paramref name="simulation"/> or <paramref name="behaviors"/> is <c>null</c>.</exception>
        public SubcircuitBindingContext(IComponent component, SubcircuitSimulation simulation, IBehaviorContainer behaviors)
            : base(component, simulation, behaviors)
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
            var localSim = (SubcircuitSimulation)Simulation;
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
