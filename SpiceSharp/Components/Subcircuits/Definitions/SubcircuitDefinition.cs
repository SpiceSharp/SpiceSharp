using SpiceSharp.Behaviors;
using SpiceSharp.Components.SubcircuitBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A standard implementation of a <see cref="ISubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="ISubcircuitDefinition" />
    public class SubcircuitDefinition : ISubcircuitDefinition
    {
        private string[] _pins;

        /// <summary>
        /// Gets the entities defined in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities inside the subcircuit.
        /// </value>
        public IEntityCollection Entities { get; }

        /// <summary>
        /// Gets the number of pins defined by the subcircuit.
        /// </summary>
        /// <value>
        /// The pin count.
        /// </value>
        public int PinCount => _pins.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinition"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="pins">The pins.</param>
        public SubcircuitDefinition(IEntityCollection entities, params string[] pins)
        {
            Entities = entities.ThrowIfNull(nameof(entities));
            if (pins != null)
            {
                _pins = new string[pins.Length];
                for (var i = 0; i < pins.Length; i++)
                    _pins[i] = pins[i].ThrowIfNull("node {0}".FormatString(i + 1));
            }
            else
                _pins = new string[0];
        }

        /// <summary>
        /// Creates the behaviors for the entities in the subcircuit.
        /// </summary>
        /// <param name="parentSimulation">The parent simulation.</param>
        /// <param name="behaviors">The <see cref="IBehaviorContainer" /> used for this subcircuit.</param>
        /// <param name="nodes">The nodes on the outside of the subcircuit.</param>
        /// <exception cref="CircuitException">Node mismatch: subcircuit requires {0} nodes, but {1} given".FormatString(_pins.Length, nodes?.Length ?? 0)</exception>
        public virtual void CreateBehaviors(ISimulation parentSimulation, IBehaviorContainer behaviors, string[] nodes)
        {
            if (Entities == null || Entities.Count == 0)
                return;
            if ((nodes == null && _pins.Length > 0) || nodes.Length != _pins.Length)
                throw new CircuitException("Node mismatch: subcircuit requires {0} nodes, but {1} given".FormatString(_pins.Length, nodes?.Length ?? 0));

            // We need to create behaviors for all subcircuit entities
            // So we'll make a subcircuit simulation matching the parent simulation.
        }
    }
}
