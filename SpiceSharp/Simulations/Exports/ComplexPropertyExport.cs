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
        /// <value>
        /// The identifier of the entity.
        /// </value>
        public string EntityName { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the comparer for parameter names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityName">The identifier of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// entityName
        /// or
        /// propertyName
        /// </exception>
        public ComplexPropertyExport(Simulation simulation, string entityName, string propertyName, IEqualityComparer<string> comparer = null)
            : base(simulation)
        {
            EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Comparer = comparer ?? EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="ArgumentNullException">e</exception>
        protected override void Initialize(object sender, EventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            var simulation = (Simulation) sender;
            Func<Complex> extractor = null;
            var eb = simulation.EntityBehaviors[EntityName];

            // Get the necessary behavior in order:
            // 1) First try transient analysis
            if (eb.TryGet<IFrequencyBehavior>(out var behavior) && behavior is IPropertyExporter exporter)
                exporter.CreateGetter(Simulation, PropertyName, Comparer, out extractor);

            Extractor = extractor;
        }
    }
}
