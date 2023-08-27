using SpiceSharp.Behaviors;
using SpiceSharp.Components.Subcircuits;
using SpiceSharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex property values.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class ComplexPropertyExport : Export<IEventfulSimulation, Complex>
    {
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        public IReadOnlyList<string> EntityPath { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        public ComplexPropertyExport(IEventfulSimulation simulation, string entityName, string propertyName)
            : base(simulation)
        {
            entityName.ThrowIfNull(nameof(entityName));
            EntityPath = new[] { entityName };
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPropertyExport"/>.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityPath">The path to the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ComplexPropertyExport(IEventfulSimulation simulation, IEnumerable<string> entityPath, string propertyName)
            : base(simulation)
        {
            EntityPath = entityPath.ThrowIfEmpty(nameof(entityPath)).ToArray();
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            var behaviorContainer = Simulation.EntityBehaviors[EntityPath[0]];
            for (int i = 1; i < EntityPath.Count; i++)
            {
                var behavior = behaviorContainer.GetValue<EntitiesBehavior>();
                behaviorContainer = behavior.LocalBehaviors[EntityPath[i]];
            }
            Extractor = behaviorContainer.CreatePropertyGetter<Complex>(PropertyName);
        }
    }
}
