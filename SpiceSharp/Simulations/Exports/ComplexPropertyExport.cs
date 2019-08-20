using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex property values.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class ComplexPropertyExport : Export<Complex>
    {
        /// <summary>
        /// Gets the identifier of the entity.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the comparer for parameter names.
        /// </summary>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityName">The identifier of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public ComplexPropertyExport(Simulation simulation, string entityName, string propertyName, IEqualityComparer<string> comparer = null)
            : base(simulation)
        {
            EntityName = entityName.ThrowIfNull(nameof(entityName));
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
            Comparer = comparer ?? EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            e.ThrowIfNull(nameof(e));

            Func<Complex> extractor = null;
            var eb = Simulation.EntityBehaviors[EntityName];

            // Get the necessary behaviors in order of importance
            // 1) First the frequency analysis analysis
            if (eb.TryGet<IFrequencyBehavior>(out var behavior))
                extractor = behavior.CreateGetter<Complex>(PropertyName, Comparer);

            // There are currently no other behaviors that export complex numbers

            Extractor = extractor;
        }
    }
}
