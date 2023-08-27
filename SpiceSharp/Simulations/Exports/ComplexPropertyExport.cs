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
            var behaviors = new HashSet<IBehavior>();
            foreach (var behavior in Simulation.EntityBehaviors[EntityPath[0]])
                behaviors.Add(behavior);

            for (int i = 1; i < EntityPath.Count; i++)
            {
                string nextComponentName = EntityPath[i];

                // Keep track of the behaviors one level deeper
                var subBehaviors = new HashSet<IBehavior>();

                // Go through all the behaviors of the previously found level, and collect the new set of behaviors.
                foreach (var behavior in behaviors)
                {
                    if (behavior is EntitiesBehavior subcktBehavior &&
                        subcktBehavior.LocalBehaviors.TryGetBehaviors(nextComponentName, out var behaviorContainer))
                    {
                        foreach (var subBehavior in behaviorContainer)
                            subBehaviors.Add(subBehavior);
                    }
                }

                // If we didn't find any new behaviors, then that means that the last level was not a subcircuit
                if (subBehaviors.Count == 0)
                    throw new SpiceSharpException($"Entity {EntityPath[i - 1]} is not a subcircuit.");
                behaviors = subBehaviors;
            }


            // We have found all the behaviors for the relevant entity path, let's now find the property
            // This code is basically identical to how it is implemented for BehaviorContainer
            foreach (var behavior in behaviors)
            {
                Extractor = behavior.CreatePropertyGetter<Complex>(PropertyName);
                if (Extractor != null)
                    return; // Success
            }

            // If we reached this part, then none of the behaviors have defined the property...
            throw new ParameterNotFoundException(this, PropertyName, typeof(Complex));
        }
    }
}
