using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Components.SubcircuitBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can contain other entities and group them.
    /// </summary>
    /// <seealso cref="Component" />
    public class Subcircuit : Component
    {
        /// <summary>
        /// Gets or sets the entities in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEntityCollection[] Entities { get; set; }

        /// <summary>
        /// Gets the local pin names. These will globally look like other pin names.
        /// </summary>
        /// <value>
        /// The local pin names.
        /// </value>
        public string[] Pins { get; }

        /// <summary>
        /// The mock simulation used to create behaviors inside the subcircuit
        /// </summary>
        private SubcircuitSimulation[] _simulations;
        private Variable[] _nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="pins">The local node names in the subcircuit that will be connected outside.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        public Subcircuit(string name, IEntityCollection[] entities, params string[] pins)
            : base(name, pins.Length)
        {
            Entities = entities;
            Pins = pins;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="pins">The pins.</param>
        public Subcircuit(string name, IEntityCollection entities, params string[] pins)
            : base(name, pins.Length)
        {
            Entities = new[] { entities };
            Pins = pins;
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            if (Entities == null || Entities.Length == 0)
                return;

            _simulations = new SubcircuitSimulation[Entities.Length];
            for (var i = 0; i < Entities.Length; i++)
            {
                _simulations[i] = new SubcircuitSimulation(Name, simulation);
                var ec = new SubcircuitEntityCollection(simulation, Entities[i], entities);
                _simulations[i].Run(ec);
            }
        }
    }
}
