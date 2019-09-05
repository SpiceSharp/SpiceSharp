﻿using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real properties.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class RealPropertyExport : Export<double>
    {
        /// <summary>
        /// Gets the identifier of the entity.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the comparer for finding the parameter.
        /// </summary>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityName">The identifier of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public RealPropertyExport(Simulation simulation, string entityName, string propertyName, IEqualityComparer<string> comparer = null)
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
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            e.ThrowIfNull(nameof(e));
            var eb = Simulation.EntityBehaviors[EntityName];
            if (eb != null)
                Extractor = eb.CreateGetter<double>(PropertyName, Comparer);
        }
    }
}
