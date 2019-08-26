using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can contain other entities and group them.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    public class Subcircuit : Component
    {
        /// <summary>
        /// Gets the entities in the subcircuit.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public EntityCollection Entities { get; }

        /// <summary>
        /// Gets the local pin names. These will globally look like other pin names.
        /// </summary>
        /// <value>
        /// The local pin names.
        /// </value>
        public string[] LocalPinNames { get; }

        private BehaviorPool _behaviors;
        private ParameterPool _parameters;
        private VariableSet _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name of the subcircuit.</param>
        /// <param name="pins">The local pin names in the subcircuit that will be connected outside.</param>
        /// <param name="entities">The entities in the subcircuit.</param>
        public Subcircuit(string name, string[] pins, EntityCollection entities)
            : base(name, pins.Length)
        {
            Entities = entities.ThrowIfNull(nameof(entities));
            LocalPinNames = new string[pins.Length];
            for (var i = 0; i < pins.Length; i++)
                LocalPinNames[i] = pins[i].ThrowIfNull("pin");
        }

        public override void CreateBehaviors(Type[] types, Simulation simulation, EntityCollection entities)
        {

        }

        protected override void BindBehavior(IBehavior behavior, Simulation simulation)
        {
            base.BindBehavior(behavior, simulation);
        }

        protected override ComponentBindingContext BuildBindingContext(Simulation simulation)
        {
            return base.BuildBindingContext(simulation);
        }
    }
}
