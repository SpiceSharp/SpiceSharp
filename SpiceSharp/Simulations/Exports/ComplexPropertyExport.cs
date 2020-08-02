using SpiceSharp.Behaviors;
using System;
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
        public string EntityName { get; }

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
            EntityName = entityName.ThrowIfNull(nameof(entityName));
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            e.ThrowIfNull(nameof(e));

            Func<Complex> extractor = null;
            var eb = Simulation.EntityBehaviors[EntityName];

            // Get the necessary behaviors in order of importance
            // 1) First the frequency analysis analysis
            if (eb.TryGetValue<IFrequencyBehavior>(out var behavior))
                extractor = behavior.CreatePropertyGetter<Complex>(PropertyName);

            // There are currently no other behaviors that export complex numbers
            Extractor = extractor;
        }
    }
}
