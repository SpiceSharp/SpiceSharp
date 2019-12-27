using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.General;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A subcircuit simulation that captures created behaviors in a local container.
    /// </summary>
    /// <seealso cref="ISimulation" />
    public class SubcircuitSimulation : SimulationWrapper
    {
        /// <summary>
        /// Gets the subcircuit.
        /// </summary>
        /// <value>
        /// The subcircuit.
        /// </value>
        protected SubcircuitDefinition Definition { get; }

        /// <summary>
        /// Gets the variables that are shared between the subcircuit simulation and the parent simulation.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        public IEnumerable<Variable> SharedVariables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation" /> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="definition">The subcircuit definition.</param>
        /// <param name="shared">The shared variables.</param>
        public SubcircuitSimulation(string name, ISimulation parent, SubcircuitDefinition definition, IEnumerable<Variable> shared)
            : base(parent,
                  new BehaviorContainerCollection(),
                  new InterfaceTypeDictionary<ISimulationState>(),
                  new SubcircuitVariableSet(name, parent?.Variables))
        {
            Definition = definition.ThrowIfNull(nameof(definition));
            SharedVariables = shared.ThrowIfNull(nameof(shared));
        }

        /// <summary>
        /// Gets the state of the specified type.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// The state, or <c>null</c> if the state isn't used.
        /// </returns>
        public override S GetState<S>()
        {
            if (LocalStates.TryGetValue(out S result))
                return result;
            return Parent.GetState<S>();
        }

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        public override P GetParameterSet<P>()
        {
            if (Definition.TryGetParameterSet(out P result))
                return result;
            return base.GetParameterSet<P>();
        }

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        public override bool TryGetParameterSet<P>(out P value)
        {
            if (Definition.TryGetParameterSet(out value))
                return true;
            return base.TryGetParameterSet(out value);
        }
    }
}
