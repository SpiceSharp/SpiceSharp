using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.General;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits
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
        protected ISubcircuitDefinition Definition { get; }

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
        /// <param name="nodes">The node bridges.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/>, <paramref name="definition"/> or <paramref name="nodes"/> is <c>null</c>.</exception>
        public SubcircuitSimulation(string name, ISimulation parent, ISubcircuitDefinition definition, IReadOnlyList<Bridge<string>> nodes)
            : base(parent,
                  new BehaviorContainerCollection(),
                  new InterfaceTypeSet<ISimulationState>())
        {
            Definition = definition.ThrowIfNull(nameof(definition));
            Nodes = nodes.ThrowIfNull(nameof(nodes));
            InstanceName = name.ThrowIfNull(nameof(name));
        }

        /// <inheritdoc/>
        public override S GetState<S>()
        {
            if (LocalStates.TryGetValue(out S result))
                return result;
            return Parent.GetState<S>();
        }
    }
}
