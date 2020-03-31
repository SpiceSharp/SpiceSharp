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
        /// Gets the name of the subcircuit instance.
        /// </summary>
        /// <value>
        /// The name of the instance.
        /// </value>
        public string InstanceName { get; }

        /// <summary>
        /// Gets a dictionary that maps internal nodes to nodes external to the subcircuit. Any node that isn't
        /// part of this map, is considered a local node.
        /// </summary>
        /// <value>
        /// The node map.
        /// </value>
        public IReadOnlyList<Bridge<string>> Nodes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSimulation" /> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="parent">The parent simulation.</param>
        /// <param name="definition">The subcircuit definition.</param>
        /// <param name="nodes">The nodes.</param>
        public SubcircuitSimulation(string name, ISimulation parent, SubcircuitDefinition definition, IReadOnlyList<Bridge<string>> nodes)
            : base(parent,
                  new BehaviorContainerCollection(),
                  new InterfaceTypeDictionary<ISimulationState>())
        {
            Definition = definition.ThrowIfNull(nameof(definition));
            Nodes = nodes.ThrowIfNull(nameof(nodes));
            InstanceName = name.ThrowIfNull(nameof(name));
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
